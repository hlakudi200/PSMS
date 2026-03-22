using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Academic.Shared;
using psms.Academic.StudentTransfers.Dto;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.Academic.StudentTransfers;

/// <summary>
/// Service for managing student transfer requests.
/// </summary>
[AbpAuthorize(PermissionNames.Academic_Transfers)]
public class StudentTransferAppService : ApplicationService, IStudentTransferAppService
{
    private readonly IRepository<StudentTransferRequest, Guid> _transferRepository;
    private readonly IRepository<Student, Guid> _studentRepository;
    private readonly IRepository<AcademicYear, Guid> _academicYearRepository;

    public StudentTransferAppService(
        IRepository<StudentTransferRequest, Guid> transferRepository,
        IRepository<Student, Guid> studentRepository,
        IRepository<AcademicYear, Guid> academicYearRepository)
    {
        _transferRepository = transferRepository;
        _studentRepository = studentRepository;
        _academicYearRepository = academicYearRepository;
    }

    [AbpAuthorize(PermissionNames.Academic_Transfers_View)]
    public async Task<StudentTransferDto> GetAsync(Guid id)
    {
        var transfer = await _transferRepository
            .GetAll()
            .Include(t => t.Student)
            .Include(t => t.AcademicYear)
            .Include(t => t.TransferGrade)
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == AbpSession.TenantId);

        if (transfer == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TransferNotFound,
                "Student transfer request not found.");

        return ObjectMapper.Map<StudentTransferDto>(transfer);
    }

    [AbpAuthorize(PermissionNames.Academic_Transfers_View)]
    public async Task<PagedResultDto<StudentTransferListDto>> GetAllAsync(GetStudentTransfersInput input)
    {
        var query = _transferRepository
            .GetAll()
            .Include(t => t.Student)
            .Where(t => t.TenantId == AbpSession.TenantId)
            .WhereIf(input.StudentId.HasValue, t => t.StudentId == input.StudentId.Value)
            .WhereIf(input.AcademicYearId.HasValue, t => t.AcademicYearId == input.AcademicYearId.Value)
            .WhereIf(input.TransferType.HasValue, t => (int)t.TransferType == input.TransferType.Value)
            .WhereIf(input.Status.HasValue, t => (int)t.Status == input.Status.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Search),
                t => t.TransferNumber.ToLower().Contains(input.Search.Trim().ToLower())
                    || (t.Student != null && (t.Student.FirstName + " " + t.Student.LastName).ToLower()
                        .Contains(input.Search.Trim().ToLower()))
                    || t.FromSchoolName.ToLower().Contains(input.Search.Trim().ToLower())
                    || t.ToSchoolName.ToLower().Contains(input.Search.Trim().ToLower()));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "RequestedDate DESC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<StudentTransferListDto>(
            totalCount,
            ObjectMapper.Map<List<StudentTransferListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Academic_Transfers_Create)]
    public async Task<StudentTransferDto> CreateAsync(CreateStudentTransferDto input)
    {
        // Validate student exists
        var student = await _studentRepository
            .FirstOrDefaultAsync(s => s.Id == input.StudentId && s.TenantId == AbpSession.TenantId);

        if (student == null)
            throw new UserFriendlyException(AcademicExceptionCodes.StudentNotFound,
                "Student not found.");

        // Validate academic year exists
        var academicYear = await _academicYearRepository
            .FirstOrDefaultAsync(ay => ay.Id == input.AcademicYearId && ay.TenantId == AbpSession.TenantId);

        if (academicYear == null)
            throw new UserFriendlyException(AcademicExceptionCodes.AcademicYearNotFound,
                "Academic year not found.");

        // Generate transfer number: TR-{tenantId:D3}-{yyyy}-{sequential}
        var transferNumber = await GenerateTransferNumberAsync();

        var transfer = new StudentTransferRequest(
            Guid.NewGuid(),
            AbpSession.TenantId,
            transferNumber,
            input.StudentId,
            input.AcademicYearId,
            (TransferType)input.TransferType,
            input.Reason)
        {
            FromSchoolName = input.FromSchoolName,
            ToSchoolName = input.ToSchoolName,
            EffectiveDate = input.EffectiveDate,
            TransferGradeId = input.TransferGradeId,
            PreviousReportUrl = input.PreviousReportUrl
        };

        await _transferRepository.InsertAsync(transfer);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(transfer.Id);
    }

    [AbpAuthorize(PermissionNames.Academic_Transfers_Create)]
    public async Task<StudentTransferDto> UpdateAsync(Guid id, UpdateStudentTransferDto input)
    {
        var transfer = await _transferRepository
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == AbpSession.TenantId);

        if (transfer == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TransferNotFound,
                "Student transfer request not found.");

        if (input.TransferType.HasValue) transfer.TransferType = (TransferType)input.TransferType.Value;
        if (input.Reason != null) transfer.Reason = input.Reason;
        if (input.FromSchoolName != null) transfer.FromSchoolName = input.FromSchoolName;
        if (input.ToSchoolName != null) transfer.ToSchoolName = input.ToSchoolName;
        if (input.EffectiveDate.HasValue) transfer.EffectiveDate = input.EffectiveDate.Value;
        if (input.TransferGradeId.HasValue) transfer.TransferGradeId = input.TransferGradeId.Value;
        if (input.PreviousReportUrl != null) transfer.PreviousReportUrl = input.PreviousReportUrl;

        await _transferRepository.UpdateAsync(transfer);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Academic_Transfers_Create)]
    public async Task DeleteAsync(Guid id)
    {
        var transfer = await _transferRepository
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == AbpSession.TenantId);

        if (transfer == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TransferNotFound,
                "Student transfer request not found.");

        await _transferRepository.DeleteAsync(transfer);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    [AbpAuthorize(PermissionNames.Academic_Transfers_Create)]
    public async Task<StudentTransferDto> SubmitAsync(Guid id)
    {
        var transfer = await _transferRepository
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == AbpSession.TenantId);

        if (transfer == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TransferNotFound,
                "Student transfer request not found.");

        try
        {
            transfer.Submit();
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(AcademicExceptionCodes.TransferInvalidStatus, ex.Message);
        }

        await _transferRepository.UpdateAsync(transfer);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Academic_Transfers_Approve)]
    public async Task<StudentTransferDto> ApproveAsync(Guid id)
    {
        var transfer = await _transferRepository
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == AbpSession.TenantId);

        if (transfer == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TransferNotFound,
                "Student transfer request not found.");

        try
        {
            transfer.Approve(AbpSession.UserId.Value);
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(AcademicExceptionCodes.TransferInvalidStatus, ex.Message);
        }

        await _transferRepository.UpdateAsync(transfer);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Academic_Transfers_Approve)]
    public async Task<StudentTransferDto> RejectAsync(Guid id, string reason)
    {
        var transfer = await _transferRepository
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == AbpSession.TenantId);

        if (transfer == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TransferNotFound,
                "Student transfer request not found.");

        try
        {
            transfer.Reject(AbpSession.UserId.Value, reason);
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(AcademicExceptionCodes.TransferInvalidStatus, ex.Message);
        }

        await _transferRepository.UpdateAsync(transfer);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Academic_Transfers_Approve)]
    public async Task<StudentTransferDto> CompleteAsync(Guid id, string certificateUrl = null)
    {
        var transfer = await _transferRepository
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == AbpSession.TenantId);

        if (transfer == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TransferNotFound,
                "Student transfer request not found.");

        try
        {
            transfer.Complete(certificateUrl);
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(AcademicExceptionCodes.TransferInvalidStatus, ex.Message);
        }

        await _transferRepository.UpdateAsync(transfer);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Academic_Transfers_Create)]
    public async Task<StudentTransferDto> CancelAsync(Guid id)
    {
        var transfer = await _transferRepository
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == AbpSession.TenantId);

        if (transfer == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TransferNotFound,
                "Student transfer request not found.");

        try
        {
            transfer.Cancel();
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(AcademicExceptionCodes.TransferInvalidStatus, ex.Message);
        }

        await _transferRepository.UpdateAsync(transfer);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    private async Task<string> GenerateTransferNumberAsync()
    {
        var tenantId = AbpSession.TenantId ?? 0;
        var year = DateTime.UtcNow.Year;
        var prefix = $"TR-{tenantId:D3}-{year}-";

        var existingNumbers = await _transferRepository
            .GetAll()
            .Where(t => t.TenantId == AbpSession.TenantId && t.TransferNumber.StartsWith(prefix))
            .Select(t => t.TransferNumber)
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
