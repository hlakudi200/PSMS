using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Microsoft.EntityFrameworkCore;
using psms.Authorization.Users;
using psms.Domain.Workflow.Entities;
using psms.Domain.Workflow.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Workflow.Shared;

/// <summary>
/// WF-34: the single answer to "may this user act on this step?", used by the act
/// check, the My Approvals list and the dashboard count so the three can never
/// disagree again.
///
/// A user may act when:
///  - the step is pinned to them and they still hold the step's role; or
///  - the step is pinned to someone who has an active delegation to them whose
///    scope (entity type, role) covers the step; or
///  - the step is role-based and they hold the role; or
///  - the step is role-based and someone who HOLDS that role has an active
///    delegation to them whose scope covers the step.
/// Role comparison is case-insensitive throughout.
/// </summary>
public class WorkflowActorResolver : ITransientDependency
{
    private readonly IRepository<WorkflowDelegation, Guid> _delegations;
    private readonly UserManager _userManager;
    private readonly IAbpSession _session;

    public WorkflowActorResolver(
        IRepository<WorkflowDelegation, Guid> delegations,
        UserManager userManager,
        IAbpSession session)
    {
        _delegations = delegations;
        _userManager = userManager;
        _session = session;
    }

    /// <summary>Everything needed to decide act rights for one user, loaded once per request.</summary>
    public sealed class ActorContext
    {
        public long UserId { get; init; }
        public IReadOnlyCollection<string> Roles { get; init; }
        public IReadOnlyList<Delegation> Delegations { get; init; }

        /// <summary>Roles the user holds directly or can act for through a delegation (used to pre-filter queries).</summary>
        public IReadOnlyCollection<string> ActionableRoles { get; init; }

        public bool HasRole(string role) =>
            role != null && Roles.Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
    }

    public sealed class Delegation
    {
        public long DelegatorUserId { get; init; }
        public WorkflowEntityType? EntityType { get; init; }
        public string AssignedRole { get; init; }
        public IReadOnlyCollection<string> DelegatorRoles { get; init; }

        public bool Covers(WorkflowEntityType entityType, string role) =>
            (!EntityType.HasValue || EntityType.Value == entityType)
            && (string.IsNullOrEmpty(AssignedRole) || AssignedRole.Equals(role, StringComparison.OrdinalIgnoreCase));

        public bool DelegatorHolds(string role) =>
            role != null && DelegatorRoles.Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
    }

    public Task<ActorContext> GetCurrentAsync() =>
        _session.UserId.HasValue ? GetContextAsync(_session.UserId.Value) : Task.FromResult<ActorContext>(null);

    public async Task<ActorContext> GetContextAsync(long userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        var roles = user != null ? (await _userManager.GetRolesAsync(user)).ToList() : new List<string>();

        var now = DateTime.UtcNow;
        var rows = await _delegations.GetAll()
            .Where(d => d.TenantId == _session.TenantId
                && d.DelegateUserId == userId
                && d.IsActive
                && d.StartDate <= now
                && d.EndDate >= now)
            .Select(d => new { d.DelegatorUserId, d.EntityType, d.AssignedRole })
            .ToListAsync();

        var delegatorRoles = new Dictionary<long, IReadOnlyCollection<string>>();
        foreach (var delegatorId in rows.Select(r => r.DelegatorUserId).Distinct())
        {
            var delegator = await _userManager.FindByIdAsync(delegatorId.ToString());
            delegatorRoles[delegatorId] = delegator != null
                ? (await _userManager.GetRolesAsync(delegator)).ToList()
                : new List<string>();
        }

        var delegations = rows.Select(r => new Delegation
        {
            DelegatorUserId = r.DelegatorUserId,
            EntityType = r.EntityType,
            AssignedRole = r.AssignedRole,
            DelegatorRoles = delegatorRoles[r.DelegatorUserId],
        }).ToList();

        // A delegation lets the delegate act for the roles the DELEGATOR holds
        // (narrowed to the delegation's role scope when it has one).
        var actionable = new HashSet<string>(roles, StringComparer.OrdinalIgnoreCase);
        foreach (var d in delegations)
            foreach (var role in d.DelegatorRoles)
                if (string.IsNullOrEmpty(d.AssignedRole) || d.AssignedRole.Equals(role, StringComparison.OrdinalIgnoreCase))
                    actionable.Add(role);

        return new ActorContext
        {
            UserId = userId,
            Roles = roles,
            Delegations = delegations,
            ActionableRoles = actionable,
        };
    }

    public static bool CanAct(ActorContext actor, WorkflowStep step, WorkflowEntityType entityType)
    {
        if (actor == null || step == null || string.IsNullOrEmpty(step.AssignedRole)) return false;

        if (step.AssignedUserId.HasValue)
        {
            if (step.AssignedUserId.Value == actor.UserId)
                // WF-05: a pinned assignee who has since lost the role may not act.
                return actor.HasRole(step.AssignedRole);

            return actor.Delegations.Any(d =>
                d.DelegatorUserId == step.AssignedUserId.Value
                && d.Covers(entityType, step.AssignedRole));
        }

        if (actor.HasRole(step.AssignedRole)) return true;

        return actor.Delegations.Any(d =>
            d.Covers(entityType, step.AssignedRole)
            && d.DelegatorHolds(step.AssignedRole));
    }

    /// <summary>Why the user cannot act, for the error message.</summary>
    public static string ExplainDenial(ActorContext actor, WorkflowStep step)
    {
        if (step.AssignedUserId.HasValue)
        {
            if (step.AssignedUserId.Value == actor.UserId)
                return $"You no longer hold the required role '{step.AssignedRole}' for this step.";
            return "This step is assigned to a specific user. You are not authorized to act on it.";
        }
        return $"You do not have the required role '{step.AssignedRole}' to act on this step.";
    }
}
