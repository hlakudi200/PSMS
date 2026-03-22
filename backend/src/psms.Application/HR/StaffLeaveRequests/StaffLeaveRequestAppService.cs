using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Authorization;
using psms.Authorization.Users;
using psms.Domain.HR.Entities;
using psms.Domain.Shared.Enums;
using psms.HR.Shared;
using psms.HR.StaffLeaveRequests.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.HR.StaffLeaveRequests;

/// <summary>
/// Service for managing staff leave requests.
/// </summary>
[AbpAuthorize(PermissionNames.HR_Leave)]
public class StaffLeaveRequestAppService : ApplicationService, IStaffLeaveRequestAppService
{
    private readonly IRepository<StaffLeaveRequest, Guid> _leaveRepository;
    private readonly UserManager _userManager;

    public StaffLeaveRequestAppService(
        IRepository<StaffLeaveRequest, Guid> leaveRepository,
        UserManager userManager)
    {
        _leaveRepository = leaveRepository;
        _userManager = userManager;
    }

    [AbpAuthorize(PermissionNames.HR_Leave_View)]
    public async Task<StaffLeaveRequestDto> GetAsync(Guid id)
    {
        var leave = await _leaveRepository
            .GetAll()
            .Include(l => l.SubstituteTeacher)
            .FirstOrDefaultAsync(l => l.Id == id && l.TenantId == AbpSession.TenantId);

        if (leave == null)
            throw new UserFriendlyException(HRExceptionCodes.LeaveNotFound,
                "Leave request not found.");

        return ObjectMapper.Map<StaffLeaveRequestDto>(leave);
    }

    [AbpAuthorize(PermissionNames.HR_Leave_ViewAll)]
    public async Task<PagedResultDto<StaffLeaveRequestListDto>> GetAllAsync(GetStaffLeaveRequestsInput input)
    {
        var query = _leaveRepository
            .GetAll()
            .Where(l => l.TenantId == AbpSession.TenantId)
            .WhereIf(input.UserId.HasValue, l => l.UserId == input.UserId.Value)
            .WhereIf(input.LeaveType.HasValue, l => (int)l.LeaveType == input.LeaveType.Value)
            .WhereIf(input.Status.HasValue, l => (int)l.Status == input.Status.Value)
            .WhereIf(input.StartDate.HasValue, l => l.StartDate >= input.StartDate.Value)
            .WhereIf(input.EndDate.HasValue, l => l.EndDate <= input.EndDate.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Search),
                l => l.LeaveNumber.ToLower().Contains(input.Search.Trim().ToLower())
                    || l.UserName.ToLower().Contains(input.Search.Trim().ToLower()));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "CreationTime DESC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<StaffLeaveRequestListDto>(
            totalCount,
            ObjectMapper.Map<List<StaffLeaveRequestListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.HR_Leave_Create)]
    public async Task<StaffLeaveRequestDto> CreateAsync(CreateStaffLeaveRequestDto input)
    {
        // Validate date range
        if (input.EndDate < input.StartDate)
            throw new UserFriendlyException(HRExceptionCodes.InvalidDateRange,
                "End date must be on or after start date.");

        // Check for overlapping leave requests
        var hasOverlap = await _leaveRepository
            .GetAll()
            .Where(l => l.TenantId == AbpSession.TenantId
                && l.UserId == AbpSession.UserId.Value
                && l.Status != StaffLeaveStatus.Cancelled
                && l.Status != StaffLeaveStatus.Rejected)
            .AnyAsync(l => l.StartDate <= input.EndDate && l.EndDate >= input.StartDate);

        if (hasOverlap)
            throw new UserFriendlyException(HRExceptionCodes.OverlappingLeave,
                "You already have a leave request that overlaps with the requested dates.");

        // Get current user name
        var user = await _userManager.GetUserByIdAsync(AbpSession.UserId.Value);
        var userName = user.FullName ?? $"{user.Name} {user.Surname}";

        // Generate leave number: LR-{tenantId:D3}-{yyyy}-{sequential}
        var leaveNumber = await GenerateLeaveNumberAsync();

        // Get substitute teacher name if provided
        string substituteTeacherName = null;
        if (input.SubstituteTeacherId.HasValue)
        {
            // SubstituteTeacher name will be populated via navigation property mapping
        }

        var leave = new StaffLeaveRequest(
            Guid.NewGuid(),
            AbpSession.TenantId,
            leaveNumber,
            AbpSession.UserId.Value,
            userName,
            (StaffLeaveType)input.LeaveType,
            input.StartDate,
            input.EndDate,
            input.Reason)
        {
            SubstituteTeacherId = input.SubstituteTeacherId,
            SupportingDocumentUrl = input.SupportingDocumentUrl
        };

        await _leaveRepository.InsertAsync(leave);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(leave.Id);
    }

    [AbpAuthorize(PermissionNames.HR_Leave_Create)]
    public async Task<StaffLeaveRequestDto> UpdateAsync(Guid id, UpdateStaffLeaveRequestDto input)
    {
        var leave = await _leaveRepository
            .FirstOrDefaultAsync(l => l.Id == id && l.TenantId == AbpSession.TenantId);

        if (leave == null)
            throw new UserFriendlyException(HRExceptionCodes.LeaveNotFound,
                "Leave request not found.");

        if (input.LeaveType.HasValue) leave.LeaveType = (StaffLeaveType)input.LeaveType.Value;
        if (input.StartDate.HasValue) leave.StartDate = input.StartDate.Value;
        if (input.EndDate.HasValue) leave.EndDate = input.EndDate.Value;
        if (input.Reason != null) leave.Reason = input.Reason;
        if (input.SubstituteTeacherId.HasValue) leave.SubstituteTeacherId = input.SubstituteTeacherId.Value;
        if (input.SupportingDocumentUrl != null) leave.SupportingDocumentUrl = input.SupportingDocumentUrl;

        // Validate date range after update
        if (leave.EndDate < leave.StartDate)
            throw new UserFriendlyException(HRExceptionCodes.InvalidDateRange,
                "End date must be on or after start date.");

        await _leaveRepository.UpdateAsync(leave);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.HR_Leave_Create)]
    public async Task DeleteAsync(Guid id)
    {
        var leave = await _leaveRepository
            .FirstOrDefaultAsync(l => l.Id == id && l.TenantId == AbpSession.TenantId);

        if (leave == null)
            throw new UserFriendlyException(HRExceptionCodes.LeaveNotFound,
                "Leave request not found.");

        await _leaveRepository.DeleteAsync(leave);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    [AbpAuthorize(PermissionNames.HR_Leave_Create)]
    public async Task<StaffLeaveRequestDto> SubmitAsync(Guid id)
    {
        var leave = await _leaveRepository
            .FirstOrDefaultAsync(l => l.Id == id && l.TenantId == AbpSession.TenantId);

        if (leave == null)
            throw new UserFriendlyException(HRExceptionCodes.LeaveNotFound,
                "Leave request not found.");

        try
        {
            leave.Submit();
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(HRExceptionCodes.InvalidStatus, ex.Message);
        }

        await _leaveRepository.UpdateAsync(leave);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.HR_Leave_Approve)]
    public async Task<StaffLeaveRequestDto> ApproveAsync(Guid id)
    {
        var leave = await _leaveRepository
            .FirstOrDefaultAsync(l => l.Id == id && l.TenantId == AbpSession.TenantId);

        if (leave == null)
            throw new UserFriendlyException(HRExceptionCodes.LeaveNotFound,
                "Leave request not found.");

        try
        {
            leave.Approve(AbpSession.UserId.Value);
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(HRExceptionCodes.InvalidStatus, ex.Message);
        }

        await _leaveRepository.UpdateAsync(leave);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.HR_Leave_Approve)]
    public async Task<StaffLeaveRequestDto> RejectAsync(Guid id, string reason)
    {
        var leave = await _leaveRepository
            .FirstOrDefaultAsync(l => l.Id == id && l.TenantId == AbpSession.TenantId);

        if (leave == null)
            throw new UserFriendlyException(HRExceptionCodes.LeaveNotFound,
                "Leave request not found.");

        try
        {
            leave.Reject(AbpSession.UserId.Value, reason);
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(HRExceptionCodes.InvalidStatus, ex.Message);
        }

        await _leaveRepository.UpdateAsync(leave);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.HR_Leave_Create)]
    public async Task<StaffLeaveRequestDto> CancelAsync(Guid id)
    {
        var leave = await _leaveRepository
            .FirstOrDefaultAsync(l => l.Id == id && l.TenantId == AbpSession.TenantId);

        if (leave == null)
            throw new UserFriendlyException(HRExceptionCodes.LeaveNotFound,
                "Leave request not found.");

        try
        {
            leave.Cancel();
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(HRExceptionCodes.InvalidStatus, ex.Message);
        }

        await _leaveRepository.UpdateAsync(leave);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.HR_Leave_View)]
    public async Task<PagedResultDto<StaffLeaveRequestListDto>> GetMyLeavesAsync(PagedAndSortedResultRequestDto input)
    {
        var query = _leaveRepository
            .GetAll()
            .Where(l => l.TenantId == AbpSession.TenantId && l.UserId == AbpSession.UserId.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "CreationTime DESC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<StaffLeaveRequestListDto>(
            totalCount,
            ObjectMapper.Map<List<StaffLeaveRequestListDto>>(items));
    }

    private async Task<string> GenerateLeaveNumberAsync()
    {
        var tenantId = AbpSession.TenantId ?? 0;
        var year = DateTime.UtcNow.Year;
        var prefix = $"LR-{tenantId:D3}-{year}-";

        var existingNumbers = await _leaveRepository
            .GetAll()
            .Where(l => l.TenantId == AbpSession.TenantId && l.LeaveNumber.StartsWith(prefix))
            .Select(l => l.LeaveNumber)
            .ToListAsync();

        int maxSequential = 0;
        foreach (var num in existingNumbers)
        {
            var suffix = num.Substring(prefix.Length);
            if (int.TryParse(suffix, out int parsed) && parsed > maxSequential)
            {
                maxSequential = parsed;
            }
        }

        return $"{prefix}{maxSequential + 1:D4}";
    }
}
