using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Authorization;
using psms.Authorization.Users;
using psms.Domain.Workflow.Entities;
using psms.Workflow.Shared;
using psms.Workflow.WorkflowDelegations.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.Workflow.WorkflowDelegations;

[AbpAuthorize(PermissionNames.Workflow_Delegations)]
public class WorkflowDelegationAppService : ApplicationService, IWorkflowDelegationAppService
{

    private readonly IRepository<WorkflowDelegation, Guid> _delegationRepository;
    private readonly UserManager _userManager;

    public WorkflowDelegationAppService(
        IRepository<WorkflowDelegation, Guid> delegationRepository,
        UserManager userManager)
    {
        _delegationRepository = delegationRepository;
        _userManager = userManager;
    }

    [AbpAuthorize(PermissionNames.Workflow_Delegations_View)]
    public async Task<WorkflowDelegationDto> GetAsync(Guid id)
    {
        var delegation = await _delegationRepository
            .GetAll()
            .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == AbpSession.TenantId);

        if (delegation == null)
            throw new UserFriendlyException(WorkflowExceptionCodes.DelegationNotFound,
                "Workflow delegation not found.");

        return ObjectMapper.Map<WorkflowDelegationDto>(delegation);
    }

    [AbpAuthorize(PermissionNames.Workflow_Delegations_View)]
    public async Task<PagedResultDto<WorkflowDelegationDto>> GetAllAsync(GetWorkflowDelegationsInput input)
    {
        var query = _delegationRepository
            .GetAll()
            .Where(d => d.TenantId == AbpSession.TenantId)
            .WhereIf(input.DelegatorUserId.HasValue, d => d.DelegatorUserId == input.DelegatorUserId.Value)
            .WhereIf(input.DelegateUserId.HasValue, d => d.DelegateUserId == input.DelegateUserId.Value)
            .WhereIf(input.IsActive.HasValue, d => d.IsActive == input.IsActive.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Search),
                d => d.DelegatorUserName.ToLower().Contains(input.Search.Trim().ToLower())
                    || d.DelegateUserName.ToLower().Contains(input.Search.Trim().ToLower())
                    || (d.Reason != null && d.Reason.ToLower().Contains(input.Search.Trim().ToLower())));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "CreationTime DESC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<WorkflowDelegationDto>(
            totalCount,
            ObjectMapper.Map<List<WorkflowDelegationDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Workflow_Delegations_View)]
    public async Task<PagedResultDto<WorkflowDelegationDto>> GetMyDelegationsAsync(PagedAndSortedResultRequestDto input)
    {
        var currentUserId = AbpSession.UserId.Value;

        var query = _delegationRepository
            .GetAll()
            .Where(d => d.TenantId == AbpSession.TenantId
                && (d.DelegatorUserId == currentUserId || d.DelegateUserId == currentUserId));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "CreationTime DESC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<WorkflowDelegationDto>(
            totalCount,
            ObjectMapper.Map<List<WorkflowDelegationDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Workflow_Delegations_Create)]
    public async Task<WorkflowDelegationDto> CreateAsync(CreateWorkflowDelegationDto input)
    {
        var currentUserId = AbpSession.UserId.Value;

        // Validate dates
        if (input.EndDate <= input.StartDate)
            throw new UserFriendlyException(WorkflowExceptionCodes.DelegationInvalidDates,
                "End date must be after start date.");

        if (input.StartDate.Date < DateTime.UtcNow.Date)
            throw new UserFriendlyException(WorkflowExceptionCodes.DelegationInvalidDates,
                "Start date cannot be in the past.");

        // Cannot delegate to yourself
        if (input.DelegateUserId == currentUserId)
            throw new UserFriendlyException(WorkflowExceptionCodes.DelegationSelfDelegate,
                "You cannot delegate responsibilities to yourself.");

        // Validate delegate user exists
        var delegateUser = await _userManager.FindByIdAsync(input.DelegateUserId.ToString());
        if (delegateUser == null)
            throw new UserFriendlyException(WorkflowExceptionCodes.DelegationNotFound,
                "The delegate user was not found.");

        // Check for overlapping active delegation
        var hasOverlap = await _delegationRepository
            .GetAll()
            .AnyAsync(d => d.TenantId == AbpSession.TenantId
                && d.DelegatorUserId == currentUserId
                && d.DelegateUserId == input.DelegateUserId
                && d.IsActive
                && d.StartDate < input.EndDate
                && d.EndDate > input.StartDate
                && (!input.EntityType.HasValue || !d.EntityType.HasValue || d.EntityType == input.EntityType));

        if (hasOverlap)
            throw new UserFriendlyException(WorkflowExceptionCodes.DelegationOverlap,
                "An overlapping active delegation already exists for this user and date range.");

        // Lookup delegator username
        var delegatorUser = await _userManager.FindByIdAsync(currentUserId.ToString());
        var delegatorUserName = delegatorUser?.UserName ?? "Unknown";

        var delegation = new WorkflowDelegation
        {
            Id = Guid.NewGuid(),
            TenantId = AbpSession.TenantId,
            DelegatorUserId = currentUserId,
            DelegateUserId = input.DelegateUserId,
            DelegatorUserName = delegatorUserName,
            DelegateUserName = delegateUser.UserName,
            StartDate = input.StartDate,
            EndDate = input.EndDate,
            Reason = input.Reason,
            IsActive = true,
            EntityType = input.EntityType,
            AssignedRole = input.AssignedRole
        };

        await _delegationRepository.InsertAsync(delegation);
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<WorkflowDelegationDto>(delegation);
    }

    [AbpAuthorize(PermissionNames.Workflow_Delegations_Revoke)]
    public async Task RevokeAsync(Guid id)
    {
        var delegation = await _delegationRepository
            .GetAll()
            .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == AbpSession.TenantId);

        if (delegation == null)
            throw new UserFriendlyException(WorkflowExceptionCodes.DelegationNotFound,
                "Workflow delegation not found.");

        // Only the delegator or an admin can revoke
        var currentUserId = AbpSession.UserId.Value;
        if (delegation.DelegatorUserId != currentUserId)
        {
            var currentUser = await _userManager.FindByIdAsync(currentUserId.ToString());
            var roles = await _userManager.GetRolesAsync(currentUser);
            if (!roles.Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase)))
                throw new UserFriendlyException(WorkflowExceptionCodes.DelegationCannotRevokeOthers,
                    "Only the delegator or an admin can revoke this delegation.");
        }

        try
        {
            delegation.Revoke();
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(WorkflowExceptionCodes.DelegationNotFound, ex.Message);
        }

        await _delegationRepository.UpdateAsync(delegation);
        await CurrentUnitOfWork.SaveChangesAsync();
    }
}
