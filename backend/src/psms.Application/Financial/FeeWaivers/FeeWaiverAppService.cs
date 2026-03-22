using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using psms.Domain.Financial.Entities;
using psms.Domain.Shared.Enums;
using psms.Financial.FeeWaivers.Dto;
using psms.Financial.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.Financial.FeeWaivers;

/// <summary>
/// Service for managing fee waivers.
/// </summary>
[AbpAuthorize(PermissionNames.Financial_FeeWaivers)]
public class FeeWaiverAppService : ApplicationService, IFeeWaiverAppService
{
    private readonly IRepository<FeeWaiver, Guid> _feeWaiverRepository;
    private readonly IRepository<Student, Guid> _studentRepository;

    public FeeWaiverAppService(
        IRepository<FeeWaiver, Guid> feeWaiverRepository,
        IRepository<Student, Guid> studentRepository)
    {
        _feeWaiverRepository = feeWaiverRepository;
        _studentRepository = studentRepository;
    }

    [AbpAuthorize(PermissionNames.Financial_FeeWaivers_View)]
    public async Task<FeeWaiverDto> GetAsync(Guid id)
    {
        var feeWaiver = await _feeWaiverRepository
            .GetAll()
            .Include(fw => fw.Student)
            .Include(fw => fw.AcademicYear)
            .FirstOrDefaultAsync(fw => fw.Id == id && fw.TenantId == AbpSession.TenantId);

        if (feeWaiver == null)
            throw new UserFriendlyException(FinancialExceptionCodes.FeeWaiverNotFound,
                "Fee waiver not found.");

        return ObjectMapper.Map<FeeWaiverDto>(feeWaiver);
    }

    [AbpAuthorize(PermissionNames.Financial_FeeWaivers_View)]
    public async Task<PagedResultDto<FeeWaiverListDto>> GetAllAsync(GetFeeWaiversInput input)
    {
        var query = _feeWaiverRepository
            .GetAll()
            .Include(fw => fw.Student)
            .Where(fw => fw.TenantId == AbpSession.TenantId)
            .WhereIf(input.StudentId.HasValue, fw => fw.StudentId == input.StudentId.Value)
            .WhereIf(input.AcademicYearId.HasValue, fw => fw.AcademicYearId == input.AcademicYearId.Value)
            .WhereIf(input.WaiverType.HasValue, fw => (int)fw.WaiverType == input.WaiverType.Value)
            .WhereIf(input.Status.HasValue, fw => (int)fw.Status == input.Status.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Search),
                fw => fw.Reason.ToLower().Contains(input.Search.Trim().ToLower())
                    || (fw.Student != null && (fw.Student.FirstName + " " + fw.Student.LastName)
                        .ToLower().Contains(input.Search.Trim().ToLower())));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "CreationTime DESC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<FeeWaiverListDto>(
            totalCount,
            ObjectMapper.Map<List<FeeWaiverListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Financial_FeeWaivers_Create)]
    public async Task<FeeWaiverDto> CreateAsync(CreateFeeWaiverDto input)
    {
        // Validate Student exists
        var student = await _studentRepository
            .FirstOrDefaultAsync(s => s.Id == input.StudentId && s.TenantId == AbpSession.TenantId);

        if (student == null)
            throw new UserFriendlyException(FinancialExceptionCodes.StudentNotFound,
                "Student not found.");

        var feeWaiver = new FeeWaiver(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.StudentId,
            input.AcademicYearId,
            (FeeWaiverType)input.WaiverType,
            input.RequestedAmount,
            input.Reason.Trim());

        if (input.StudentFeeId.HasValue)
            feeWaiver.StudentFeeId = input.StudentFeeId.Value;

        if (!string.IsNullOrWhiteSpace(input.SupportingDocumentUrl))
            feeWaiver.SupportingDocumentUrl = input.SupportingDocumentUrl.Trim();

        await _feeWaiverRepository.InsertAsync(feeWaiver);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(feeWaiver.Id);
    }

    [AbpAuthorize(PermissionNames.Financial_FeeWaivers_Edit)]
    public async Task<FeeWaiverDto> UpdateAsync(Guid id, UpdateFeeWaiverDto input)
    {
        var feeWaiver = await _feeWaiverRepository
            .FirstOrDefaultAsync(fw => fw.Id == id && fw.TenantId == AbpSession.TenantId);

        if (feeWaiver == null)
            throw new UserFriendlyException(FinancialExceptionCodes.FeeWaiverNotFound,
                "Fee waiver not found.");

        if (feeWaiver.Status != FeeWaiverStatus.Draft)
            throw new UserFriendlyException(FinancialExceptionCodes.InvalidFeeWaiverStatusTransition,
                "Only draft fee waivers can be updated.");

        if (input.Reason != null) feeWaiver.Reason = input.Reason.Trim();
        if (input.RequestedAmount.HasValue) feeWaiver.RequestedAmount = input.RequestedAmount.Value;
        if (input.SupportingDocumentUrl != null) feeWaiver.SupportingDocumentUrl = input.SupportingDocumentUrl.Trim();

        await _feeWaiverRepository.UpdateAsync(feeWaiver);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Financial_FeeWaivers_Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var feeWaiver = await _feeWaiverRepository
            .FirstOrDefaultAsync(fw => fw.Id == id && fw.TenantId == AbpSession.TenantId);

        if (feeWaiver == null)
            throw new UserFriendlyException(FinancialExceptionCodes.FeeWaiverNotFound,
                "Fee waiver not found.");

        if (feeWaiver.Status != FeeWaiverStatus.Draft)
            throw new UserFriendlyException(FinancialExceptionCodes.InvalidFeeWaiverStatusTransition,
                "Only draft fee waivers can be deleted.");

        await _feeWaiverRepository.DeleteAsync(feeWaiver);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    [AbpAuthorize(PermissionNames.Financial_FeeWaivers_Edit)]
    public async Task<FeeWaiverDto> SubmitAsync(Guid id)
    {
        var feeWaiver = await _feeWaiverRepository
            .FirstOrDefaultAsync(fw => fw.Id == id && fw.TenantId == AbpSession.TenantId);

        if (feeWaiver == null)
            throw new UserFriendlyException(FinancialExceptionCodes.FeeWaiverNotFound,
                "Fee waiver not found.");

        try
        {
            feeWaiver.Submit();
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(FinancialExceptionCodes.InvalidFeeWaiverStatusTransition, ex.Message);
        }

        await _feeWaiverRepository.UpdateAsync(feeWaiver);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Financial_FeeWaivers_Approve)]
    public async Task<FeeWaiverDto> ApproveAsync(Guid id, decimal approvedAmount, string notes)
    {
        var feeWaiver = await _feeWaiverRepository
            .FirstOrDefaultAsync(fw => fw.Id == id && fw.TenantId == AbpSession.TenantId);

        if (feeWaiver == null)
            throw new UserFriendlyException(FinancialExceptionCodes.FeeWaiverNotFound,
                "Fee waiver not found.");

        try
        {
            feeWaiver.Approve(AbpSession.UserId.Value, approvedAmount, notes);
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(FinancialExceptionCodes.InvalidFeeWaiverStatusTransition, ex.Message);
        }

        await _feeWaiverRepository.UpdateAsync(feeWaiver);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Financial_FeeWaivers_Approve)]
    public async Task<FeeWaiverDto> RejectAsync(Guid id, string notes)
    {
        var feeWaiver = await _feeWaiverRepository
            .FirstOrDefaultAsync(fw => fw.Id == id && fw.TenantId == AbpSession.TenantId);

        if (feeWaiver == null)
            throw new UserFriendlyException(FinancialExceptionCodes.FeeWaiverNotFound,
                "Fee waiver not found.");

        try
        {
            feeWaiver.Reject(AbpSession.UserId.Value, notes);
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(FinancialExceptionCodes.InvalidFeeWaiverStatusTransition, ex.Message);
        }

        await _feeWaiverRepository.UpdateAsync(feeWaiver);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }
}
