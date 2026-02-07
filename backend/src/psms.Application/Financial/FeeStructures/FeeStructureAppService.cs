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
using psms.Financial.FeeStructures.Dto;
using psms.Financial.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.Financial.FeeStructures;

/// <summary>
/// Service for managing fee structures.
/// </summary>
[AbpAuthorize(PermissionNames.Financial_FeeStructures)]
public class FeeStructureAppService : ApplicationService, IFeeStructureAppService
{
    private readonly IRepository<FeeStructure, Guid> _feeStructureRepository;
    private readonly IRepository<Grade, Guid> _gradeRepository;
    private readonly IRepository<AcademicYear, Guid> _academicYearRepository;

    public FeeStructureAppService(
        IRepository<FeeStructure, Guid> feeStructureRepository,
        IRepository<Grade, Guid> gradeRepository,
        IRepository<AcademicYear, Guid> academicYearRepository)
    {
        _feeStructureRepository = feeStructureRepository;
        _gradeRepository = gradeRepository;
        _academicYearRepository = academicYearRepository;
    }

    [AbpAuthorize(PermissionNames.Financial_FeeStructures_View)]
    public async Task<FeeStructureDto> GetAsync(Guid id)
    {
        var feeStructure = await _feeStructureRepository
            .GetAll()
            .Include(fs => fs.Grade)
            .Include(fs => fs.AcademicYear)
            .Include(fs => fs.StudentFees)
            .FirstOrDefaultAsync(fs => fs.Id == id && fs.TenantId == AbpSession.TenantId);

        if (feeStructure == null)
            throw new UserFriendlyException(FinancialExceptionCodes.FeeStructureNotFound,
                "Fee structure not found.");

        return ObjectMapper.Map<FeeStructureDto>(feeStructure);
    }

    [AbpAuthorize(PermissionNames.Financial_FeeStructures_View)]
    public async Task<PagedResultDto<FeeStructureListDto>> GetAllAsync(GetFeeStructuresInput input)
    {
        var query = _feeStructureRepository
            .GetAll()
            .Include(fs => fs.Grade)
            .Include(fs => fs.AcademicYear)
            .Include(fs => fs.StudentFees)
            .Where(fs => fs.TenantId == AbpSession.TenantId)
            .WhereIf(input.GradeId.HasValue, fs => fs.GradeId == input.GradeId.Value)
            .WhereIf(input.AcademicYearId.HasValue, fs => fs.AcademicYearId == input.AcademicYearId.Value)
            .WhereIf(input.FeeType.HasValue, fs => fs.FeeType == input.FeeType.Value)
            .WhereIf(input.IsActive.HasValue, fs => fs.IsActive == input.IsActive.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.FeeName),
                fs => fs.FeeName.ToLower().Contains(input.FeeName.Trim().ToLower()));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "FeeName ASC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<FeeStructureListDto>(
            totalCount,
            ObjectMapper.Map<List<FeeStructureListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Financial_FeeStructures_View)]
    public async Task<ListResultDto<FeeStructureListDto>> GetByGradeAndYearAsync(Guid gradeId, Guid academicYearId)
    {
        var items = await _feeStructureRepository
            .GetAll()
            .Include(fs => fs.Grade)
            .Include(fs => fs.AcademicYear)
            .Include(fs => fs.StudentFees)
            .Where(fs => fs.TenantId == AbpSession.TenantId
                && fs.GradeId == gradeId
                && fs.AcademicYearId == academicYearId)
            .OrderBy(fs => fs.FeeName)
            .ToListAsync();

        return new ListResultDto<FeeStructureListDto>(
            ObjectMapper.Map<List<FeeStructureListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Financial_FeeStructures_Create)]
    public async Task<FeeStructureDto> CreateAsync(CreateFeeStructureDto input)
    {
        // Validate Grade exists
        var grade = await _gradeRepository
            .FirstOrDefaultAsync(g => g.Id == input.GradeId && g.TenantId == AbpSession.TenantId);

        if (grade == null)
            throw new UserFriendlyException(FinancialExceptionCodes.GradeNotFound,
                "Grade not found.");

        // Validate AcademicYear exists
        var academicYear = await _academicYearRepository
            .FirstOrDefaultAsync(ay => ay.Id == input.AcademicYearId && ay.TenantId == AbpSession.TenantId);

        if (academicYear == null)
            throw new UserFriendlyException(FinancialExceptionCodes.AcademicYearNotFound,
                "Academic year not found.");

        // Duplicate check: (GradeId, AcademicYearId, FeeType) per tenant
        var duplicateExists = await _feeStructureRepository
            .GetAll()
            .AnyAsync(fs => fs.TenantId == AbpSession.TenantId
                && fs.GradeId == input.GradeId
                && fs.AcademicYearId == input.AcademicYearId
                && fs.FeeType == input.FeeType);

        if (duplicateExists)
            throw new UserFriendlyException(FinancialExceptionCodes.DuplicateFeeStructure,
                "A fee structure with this fee type already exists for the selected grade and academic year.");

        var feeStructure = new FeeStructure(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.GradeId,
            input.AcademicYearId,
            input.FeeType,
            input.FeeName.Trim(),
            input.Amount)
        {
            Currency = string.IsNullOrWhiteSpace(input.Currency) ? "ZAR" : input.Currency.Trim(),
            BillingFrequency = input.BillingFrequency?.Trim(),
            DueDay = input.DueDay
        };

        await _feeStructureRepository.InsertAsync(feeStructure);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(feeStructure.Id);
    }

    [AbpAuthorize(PermissionNames.Financial_FeeStructures_Edit)]
    public async Task<FeeStructureDto> UpdateAsync(Guid id, UpdateFeeStructureDto input)
    {
        var feeStructure = await _feeStructureRepository
            .FirstOrDefaultAsync(fs => fs.Id == id && fs.TenantId == AbpSession.TenantId);

        if (feeStructure == null)
            throw new UserFriendlyException(FinancialExceptionCodes.FeeStructureNotFound,
                "Fee structure not found.");

        // Duplicate check if FeeType is changing
        if (input.FeeType.HasValue && input.FeeType.Value != feeStructure.FeeType)
        {
            var duplicateExists = await _feeStructureRepository
                .GetAll()
                .AnyAsync(fs => fs.TenantId == AbpSession.TenantId
                    && fs.GradeId == feeStructure.GradeId
                    && fs.AcademicYearId == feeStructure.AcademicYearId
                    && fs.FeeType == input.FeeType.Value
                    && fs.Id != id);

            if (duplicateExists)
                throw new UserFriendlyException(FinancialExceptionCodes.DuplicateFeeStructure,
                    "A fee structure with this fee type already exists for the selected grade and academic year.");

            feeStructure.FeeType = input.FeeType.Value;
        }

        if (input.FeeName != null) feeStructure.FeeName = input.FeeName.Trim();
        if (input.Amount.HasValue) feeStructure.Amount = input.Amount.Value;
        if (input.Currency != null) feeStructure.Currency = input.Currency.Trim();
        if (input.BillingFrequency != null) feeStructure.BillingFrequency = input.BillingFrequency.Trim();
        if (input.DueDay.HasValue) feeStructure.DueDay = input.DueDay.Value;

        await _feeStructureRepository.UpdateAsync(feeStructure);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Financial_FeeStructures_Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var feeStructure = await _feeStructureRepository
            .GetAll()
            .Include(fs => fs.StudentFees)
            .FirstOrDefaultAsync(fs => fs.Id == id && fs.TenantId == AbpSession.TenantId);

        if (feeStructure == null)
            throw new UserFriendlyException(FinancialExceptionCodes.FeeStructureNotFound,
                "Fee structure not found.");

        if (feeStructure.StudentFees != null && feeStructure.StudentFees.Count > 0)
            throw new UserFriendlyException(FinancialExceptionCodes.CannotDeleteFeeStructureWithStudentFees,
                "Cannot delete a fee structure that has student fees assigned.");

        await _feeStructureRepository.DeleteAsync(feeStructure);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    [AbpAuthorize(PermissionNames.Financial_FeeStructures_Approve)]
    public async Task<FeeStructureDto> ActivateAsync(Guid id)
    {
        var feeStructure = await _feeStructureRepository
            .FirstOrDefaultAsync(fs => fs.Id == id && fs.TenantId == AbpSession.TenantId);

        if (feeStructure == null)
            throw new UserFriendlyException(FinancialExceptionCodes.FeeStructureNotFound,
                "Fee structure not found.");

        feeStructure.Activate();
        await _feeStructureRepository.UpdateAsync(feeStructure);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Financial_FeeStructures_Approve)]
    public async Task<FeeStructureDto> DeactivateAsync(Guid id)
    {
        var feeStructure = await _feeStructureRepository
            .FirstOrDefaultAsync(fs => fs.Id == id && fs.TenantId == AbpSession.TenantId);

        if (feeStructure == null)
            throw new UserFriendlyException(FinancialExceptionCodes.FeeStructureNotFound,
                "Fee structure not found.");

        feeStructure.Deactivate();
        await _feeStructureRepository.UpdateAsync(feeStructure);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }
}
