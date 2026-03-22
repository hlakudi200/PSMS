using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Workflow.Enums;

namespace psms.Domain.Workflow.Entities;

/// <summary>
/// Allows a user to delegate their workflow responsibilities to another user
/// for a specific date range (e.g., when a principal goes on leave).
/// </summary>
[Table("WorkflowDelegations")]
public class WorkflowDelegation : FullAuditedEntity<Guid>, IMayHaveTenant
{
    public const int MaxReasonLength = 500;
    public const int MaxUserNameLength = 256;
    public const int MaxAssignedRoleLength = 100;

    public int? TenantId { get; set; }

    /// <summary>
    /// The user delegating their responsibilities.
    /// </summary>
    [Required]
    public long DelegatorUserId { get; set; }

    /// <summary>
    /// The user receiving the delegated responsibilities.
    /// </summary>
    [Required]
    public long DelegateUserId { get; set; }

    [Required]
    [StringLength(MaxUserNameLength)]
    public string DelegatorUserName { get; set; }

    [Required]
    [StringLength(MaxUserNameLength)]
    public string DelegateUserName { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [StringLength(MaxReasonLength)]
    public string Reason { get; set; }

    /// <summary>
    /// Whether this delegation is active. Can be manually revoked.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// If null, delegation applies to all entity types.
    /// If set, delegation is scoped to a specific workflow entity type.
    /// </summary>
    public WorkflowEntityType? EntityType { get; set; }

    /// <summary>
    /// If null, delegation applies to all roles.
    /// If set, delegation is scoped to a specific role.
    /// </summary>
    [StringLength(MaxAssignedRoleLength)]
    public string AssignedRole { get; set; }

    /// <summary>
    /// Revokes this delegation, setting IsActive to false.
    /// </summary>
    public void Revoke()
    {
        if (!IsActive)
            throw new InvalidOperationException("Delegation has already been revoked.");

        IsActive = false;
    }

    /// <summary>
    /// Returns true if this delegation is both active and within its date range.
    /// </summary>
    [NotMapped]
    public bool IsEffective => IsActive && StartDate <= DateTime.UtcNow && EndDate >= DateTime.UtcNow;
}
