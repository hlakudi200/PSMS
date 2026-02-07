using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using psms.Domain.SASpecific.Entities;
using psms.SASpecific.AfterCares.Dto;
using psms.SASpecific.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.SASpecific.AfterCares;

/// <summary>
/// Service for managing after-care programs.
/// </summary>
[AbpAuthorize(PermissionNames.SASpecific_AfterCare)]
public class AfterCareAppService : ApplicationService, IAfterCareAppService
{
    private readonly IRepository<AfterCare, Guid> _afterCareRepository;
    private readonly IRepository<AcademicYear, Guid> _academicYearRepository;

    public AfterCareAppService(
        IRepository<AfterCare, Guid> afterCareRepository,
        IRepository<AcademicYear, Guid> academicYearRepository)
    {
        _afterCareRepository = afterCareRepository;
        _academicYearRepository = academicYearRepository;
    }

    [AbpAuthorize(PermissionNames.SASpecific_AfterCare_View)]
    public async Task<AfterCareDto> GetAsync(Guid id)
    {
        var afterCare = await _afterCareRepository
            .GetAll()
            .Include(ac => ac.StudentEnrollments)
            .FirstOrDefaultAsync(ac => ac.Id == id && ac.TenantId == AbpSession.TenantId);

        if (afterCare == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.AfterCareNotFound,
                "After-care program not found.");

        return ObjectMapper.Map<AfterCareDto>(afterCare);
    }

    [AbpAuthorize(PermissionNames.SASpecific_AfterCare_View)]
    public async Task<PagedResultDto<AfterCareListDto>> GetAllAsync(GetAfterCaresInput input)
    {
        var query = _afterCareRepository
            .GetAll()
            .Include(ac => ac.StudentEnrollments)
            .Where(ac => ac.TenantId == AbpSession.TenantId)
            .WhereIf(input.AcademicYearId.HasValue, ac => ac.AcademicYearId == input.AcademicYearId.Value)
            .WhereIf(input.AfterCareType.HasValue, ac => ac.AfterCareType == input.AfterCareType.Value)
            .WhereIf(input.IsActive.HasValue, ac => ac.IsActive == input.IsActive.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.ProgramName),
                ac => ac.ProgramName.ToLower().Contains(input.ProgramName.Trim().ToLower()));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "ProgramName ASC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<AfterCareListDto>(
            totalCount,
            ObjectMapper.Map<List<AfterCareListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.SASpecific_AfterCare_View)]
    public async Task<ListResultDto<AfterCareListDto>> GetByAcademicYearAsync(Guid academicYearId)
    {
        var items = await _afterCareRepository
            .GetAll()
            .Include(ac => ac.StudentEnrollments)
            .Where(ac => ac.TenantId == AbpSession.TenantId
                && ac.AcademicYearId == academicYearId)
            .OrderBy(ac => ac.ProgramName)
            .ToListAsync();

        return new ListResultDto<AfterCareListDto>(
            ObjectMapper.Map<List<AfterCareListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.SASpecific_AfterCare_Create)]
    public async Task<AfterCareDto> CreateAsync(CreateAfterCareDto input)
    {
        // Validate AcademicYear exists
        var academicYear = await _academicYearRepository
            .FirstOrDefaultAsync(ay => ay.Id == input.AcademicYearId && ay.TenantId == AbpSession.TenantId);

        if (academicYear == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.AcademicYearNotFound,
                "Academic year not found.");

        // Validate EndTime > StartTime
        if (input.EndTime <= input.StartTime)
            throw new UserFriendlyException(SASpecificExceptionCodes.InvalidAfterCareTimes,
                "End time must be after start time.");

        // Duplicate check: (AcademicYearId, ProgramName, AfterCareType) per tenant
        var duplicateExists = await _afterCareRepository
            .GetAll()
            .AnyAsync(ac => ac.TenantId == AbpSession.TenantId
                && ac.AcademicYearId == input.AcademicYearId
                && ac.ProgramName.ToLower() == input.ProgramName.Trim().ToLower()
                && ac.AfterCareType == input.AfterCareType);

        if (duplicateExists)
            throw new UserFriendlyException(SASpecificExceptionCodes.DuplicateAfterCare,
                "An after-care program with this name and type already exists for the selected academic year.");

        var afterCare = new AfterCare(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.AcademicYearId,
            input.ProgramName.Trim(),
            input.AfterCareType,
            input.StartTime,
            input.EndTime,
            input.MonthlyFee)
        {
            Description = input.Description?.Trim(),
            Location = input.Location?.Trim(),
            DaysAvailable = input.DaysAvailable,
            Capacity = input.Capacity,
            SupervisorName = input.SupervisorName?.Trim(),
            ContactPhone = input.ContactPhone?.Trim(),
            ActivitiesIncluded = input.ActivitiesIncluded,
            IncludesMeals = input.IncludesMeals,
            IncludesHomeworkSupervision = input.IncludesHomeworkSupervision
        };

        await _afterCareRepository.InsertAsync(afterCare);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(afterCare.Id);
    }

    [AbpAuthorize(PermissionNames.SASpecific_AfterCare_Edit)]
    public async Task<AfterCareDto> UpdateAsync(Guid id, UpdateAfterCareDto input)
    {
        var afterCare = await _afterCareRepository
            .FirstOrDefaultAsync(ac => ac.Id == id && ac.TenantId == AbpSession.TenantId);

        if (afterCare == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.AfterCareNotFound,
                "After-care program not found.");

        // Validate times if either is changing
        var newStartTime = input.StartTime ?? afterCare.StartTime;
        var newEndTime = input.EndTime ?? afterCare.EndTime;
        if (newEndTime <= newStartTime)
            throw new UserFriendlyException(SASpecificExceptionCodes.InvalidAfterCareTimes,
                "End time must be after start time.");

        // Duplicate check if ProgramName or AfterCareType is changing
        var newName = input.ProgramName?.Trim() ?? afterCare.ProgramName;
        var newType = input.AfterCareType ?? afterCare.AfterCareType;
        if ((input.ProgramName != null && newName.ToLower() != afterCare.ProgramName.ToLower())
            || (input.AfterCareType.HasValue && newType != afterCare.AfterCareType))
        {
            var duplicateExists = await _afterCareRepository
                .GetAll()
                .AnyAsync(ac => ac.TenantId == AbpSession.TenantId
                    && ac.AcademicYearId == afterCare.AcademicYearId
                    && ac.ProgramName.ToLower() == newName.ToLower()
                    && ac.AfterCareType == newType
                    && ac.Id != id);

            if (duplicateExists)
                throw new UserFriendlyException(SASpecificExceptionCodes.DuplicateAfterCare,
                    "An after-care program with this name and type already exists for the selected academic year.");
        }

        if (input.ProgramName != null) afterCare.ProgramName = input.ProgramName.Trim();
        if (input.Description != null) afterCare.Description = input.Description.Trim();
        if (input.AfterCareType.HasValue) afterCare.AfterCareType = input.AfterCareType.Value;
        if (input.Location != null) afterCare.Location = input.Location.Trim();
        if (input.StartTime.HasValue) afterCare.StartTime = input.StartTime.Value;
        if (input.EndTime.HasValue) afterCare.EndTime = input.EndTime.Value;
        if (input.DaysAvailable != null) afterCare.DaysAvailable = input.DaysAvailable;
        if (input.Capacity.HasValue) afterCare.Capacity = input.Capacity.Value;
        if (input.SupervisorName != null) afterCare.SupervisorName = input.SupervisorName.Trim();
        if (input.ContactPhone != null) afterCare.ContactPhone = input.ContactPhone.Trim();
        if (input.ActivitiesIncluded != null) afterCare.ActivitiesIncluded = input.ActivitiesIncluded;
        if (input.IncludesMeals.HasValue) afterCare.IncludesMeals = input.IncludesMeals.Value;
        if (input.IncludesHomeworkSupervision.HasValue) afterCare.IncludesHomeworkSupervision = input.IncludesHomeworkSupervision.Value;
        if (input.MonthlyFee.HasValue) afterCare.MonthlyFee = input.MonthlyFee.Value;

        await _afterCareRepository.UpdateAsync(afterCare);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.SASpecific_AfterCare_Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var afterCare = await _afterCareRepository
            .GetAll()
            .Include(ac => ac.StudentEnrollments)
            .FirstOrDefaultAsync(ac => ac.Id == id && ac.TenantId == AbpSession.TenantId);

        if (afterCare == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.AfterCareNotFound,
                "After-care program not found.");

        if (afterCare.StudentEnrollments != null && afterCare.StudentEnrollments.Any(se => se.Status == Domain.Shared.Enums.EnrollmentStatus.Active))
            throw new UserFriendlyException(SASpecificExceptionCodes.CannotDeleteAfterCareWithEnrollments,
                "Cannot delete an after-care program that has active enrollments.");

        await _afterCareRepository.DeleteAsync(afterCare);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    [AbpAuthorize(PermissionNames.SASpecific_AfterCare_Manage)]
    public async Task<AfterCareDto> ActivateAsync(Guid id)
    {
        var afterCare = await _afterCareRepository
            .FirstOrDefaultAsync(ac => ac.Id == id && ac.TenantId == AbpSession.TenantId);

        if (afterCare == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.AfterCareNotFound,
                "After-care program not found.");

        afterCare.IsActive = true;
        await _afterCareRepository.UpdateAsync(afterCare);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.SASpecific_AfterCare_Manage)]
    public async Task<AfterCareDto> DeactivateAsync(Guid id)
    {
        var afterCare = await _afterCareRepository
            .FirstOrDefaultAsync(ac => ac.Id == id && ac.TenantId == AbpSession.TenantId);

        if (afterCare == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.AfterCareNotFound,
                "After-care program not found.");

        afterCare.IsActive = false;
        await _afterCareRepository.UpdateAsync(afterCare);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }
}
