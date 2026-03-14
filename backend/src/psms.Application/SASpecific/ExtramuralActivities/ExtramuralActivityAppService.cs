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
using psms.Domain.Shared.Enums;
using psms.SASpecific.ExtramuralActivities.Dto;
using psms.SASpecific.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.SASpecific.ExtramuralActivities;

/// <summary>
/// Service for managing extramural activities.
/// </summary>
[AbpAuthorize(PermissionNames.SASpecific_Extramurals)]
public class ExtramuralActivityAppService : ApplicationService, IExtramuralActivityAppService
{
    private readonly IRepository<ExtramuralActivity, Guid> _extramuralActivityRepository;
    private readonly IRepository<AcademicYear, Guid> _academicYearRepository;

    public ExtramuralActivityAppService(
        IRepository<ExtramuralActivity, Guid> extramuralActivityRepository,
        IRepository<AcademicYear, Guid> academicYearRepository)
    {
        _extramuralActivityRepository = extramuralActivityRepository;
        _academicYearRepository = academicYearRepository;
    }

    [AbpAuthorize(PermissionNames.SASpecific_Extramurals_View)]
    public async Task<ExtramuralActivityDto> GetAsync(Guid id)
    {
        var activity = await _extramuralActivityRepository
            .GetAll()
            .Include(ea => ea.StudentEnrollments)
            .FirstOrDefaultAsync(ea => ea.Id == id && ea.TenantId == AbpSession.TenantId);

        if (activity == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.ExtramuralActivityNotFound,
                "Extramural activity not found.");

        return ObjectMapper.Map<ExtramuralActivityDto>(activity);
    }

    [AbpAuthorize(PermissionNames.SASpecific_Extramurals_View)]
    public async Task<PagedResultDto<ExtramuralActivityListDto>> GetAllAsync(GetExtramuralActivitiesInput input)
    {
        var query = _extramuralActivityRepository
            .GetAll()
            .Include(ea => ea.StudentEnrollments)
            .Where(ea => ea.TenantId == AbpSession.TenantId)
            .WhereIf(input.AcademicYearId.HasValue, ea => ea.AcademicYearId == input.AcademicYearId.Value)
            .WhereIf(input.Category.HasValue, ea => ea.Category == input.Category.Value)
            .WhereIf(input.ActivityType.HasValue, ea => ea.ActivityType == input.ActivityType.Value)
            .WhereIf(input.IsActive.HasValue, ea => ea.IsActive == input.IsActive.Value)
            .WhereIf(input.IsRegistrationOpen.HasValue, ea => ea.IsRegistrationOpen == input.IsRegistrationOpen.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.ActivityName),
                ea => ea.ActivityName.ToLower().Contains(input.ActivityName.Trim().ToLower()))
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                ea => ea.ActivityName.ToLower().Contains(input.Keyword.ToLower()));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "ActivityName ASC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<ExtramuralActivityListDto>(
            totalCount,
            ObjectMapper.Map<List<ExtramuralActivityListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.SASpecific_Extramurals_View)]
    public async Task<ListResultDto<ExtramuralActivityListDto>> GetByAcademicYearAsync(Guid academicYearId)
    {
        var items = await _extramuralActivityRepository
            .GetAll()
            .Include(ea => ea.StudentEnrollments)
            .Where(ea => ea.TenantId == AbpSession.TenantId
                && ea.AcademicYearId == academicYearId)
            .OrderBy(ea => ea.ActivityName)
            .ToListAsync();

        return new ListResultDto<ExtramuralActivityListDto>(
            ObjectMapper.Map<List<ExtramuralActivityListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.SASpecific_Extramurals_Create)]
    public async Task<ExtramuralActivityDto> CreateAsync(CreateExtramuralActivityDto input)
    {
        // Validate AcademicYear exists
        var academicYear = await _academicYearRepository
            .FirstOrDefaultAsync(ay => ay.Id == input.AcademicYearId && ay.TenantId == AbpSession.TenantId);

        if (academicYear == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.AcademicYearNotFound,
                "Academic year not found.");

        // Validate MinGrade <= MaxGrade
        if (input.MinGrade.HasValue && input.MaxGrade.HasValue && input.MinGrade.Value > input.MaxGrade.Value)
            throw new UserFriendlyException(SASpecificExceptionCodes.InvalidGradeRange,
                "Minimum grade must be less than or equal to maximum grade.");

        // Duplicate check: (AcademicYearId, ActivityName, Category) per tenant
        var duplicateExists = await _extramuralActivityRepository
            .GetAll()
            .AnyAsync(ea => ea.TenantId == AbpSession.TenantId
                && ea.AcademicYearId == input.AcademicYearId
                && ea.ActivityName.ToLower() == input.ActivityName.Trim().ToLower()
                && ea.Category == input.Category);

        if (duplicateExists)
            throw new UserFriendlyException(SASpecificExceptionCodes.DuplicateExtramuralActivity,
                "An extramural activity with this name and category already exists for the selected academic year.");

        var activity = new ExtramuralActivity(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.AcademicYearId,
            input.ActivityName.Trim(),
            input.Category,
            input.ActivityType)
        {
            Description = input.Description?.Trim(),
            Venue = input.Venue?.Trim(),
            DayOfWeek = input.DayOfWeek,
            StartTime = input.StartTime,
            EndTime = input.EndTime,
            Season = input.Season,
            TermNumber = input.TermNumber,
            MinGrade = input.MinGrade,
            MaxGrade = input.MaxGrade,
            GenderRestriction = input.GenderRestriction,
            CoachName = input.CoachName?.Trim(),
            CoachPhone = input.CoachPhone?.Trim(),
            MaxCapacity = input.MaxCapacity,
            Requirements = input.Requirements?.Trim(),
            FeePerTerm = input.FeePerTerm
        };

        await _extramuralActivityRepository.InsertAsync(activity);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(activity.Id);
    }

    [AbpAuthorize(PermissionNames.SASpecific_Extramurals_Edit)]
    public async Task<ExtramuralActivityDto> UpdateAsync(Guid id, UpdateExtramuralActivityDto input)
    {
        var activity = await _extramuralActivityRepository
            .FirstOrDefaultAsync(ea => ea.Id == id && ea.TenantId == AbpSession.TenantId);

        if (activity == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.ExtramuralActivityNotFound,
                "Extramural activity not found.");

        // Validate MinGrade <= MaxGrade
        var newMinGrade = input.MinGrade ?? activity.MinGrade;
        var newMaxGrade = input.MaxGrade ?? activity.MaxGrade;
        if (newMinGrade.HasValue && newMaxGrade.HasValue && newMinGrade.Value > newMaxGrade.Value)
            throw new UserFriendlyException(SASpecificExceptionCodes.InvalidGradeRange,
                "Minimum grade must be less than or equal to maximum grade.");

        // Duplicate check if ActivityName or Category is changing
        var newName = input.ActivityName?.Trim() ?? activity.ActivityName;
        var newCategory = input.Category ?? activity.Category;
        if ((input.ActivityName != null && newName.ToLower() != activity.ActivityName.ToLower())
            || (input.Category.HasValue && newCategory != activity.Category))
        {
            var duplicateExists = await _extramuralActivityRepository
                .GetAll()
                .AnyAsync(ea => ea.TenantId == AbpSession.TenantId
                    && ea.AcademicYearId == activity.AcademicYearId
                    && ea.ActivityName.ToLower() == newName.ToLower()
                    && ea.Category == newCategory
                    && ea.Id != id);

            if (duplicateExists)
                throw new UserFriendlyException(SASpecificExceptionCodes.DuplicateExtramuralActivity,
                    "An extramural activity with this name and category already exists for the selected academic year.");
        }

        if (input.ActivityName != null) activity.ActivityName = input.ActivityName.Trim();
        if (input.Description != null) activity.Description = input.Description.Trim();
        if (input.Category.HasValue) activity.Category = input.Category.Value;
        if (input.ActivityType.HasValue) activity.ActivityType = input.ActivityType.Value;
        if (input.Venue != null) activity.Venue = input.Venue.Trim();
        if (input.DayOfWeek.HasValue) activity.DayOfWeek = input.DayOfWeek.Value;
        if (input.StartTime.HasValue) activity.StartTime = input.StartTime.Value;
        if (input.EndTime.HasValue) activity.EndTime = input.EndTime.Value;
        if (input.Season.HasValue) activity.Season = input.Season.Value;
        if (input.TermNumber.HasValue) activity.TermNumber = input.TermNumber.Value;
        if (input.MinGrade.HasValue) activity.MinGrade = input.MinGrade.Value;
        if (input.MaxGrade.HasValue) activity.MaxGrade = input.MaxGrade.Value;
        if (input.GenderRestriction.HasValue) activity.GenderRestriction = input.GenderRestriction.Value;
        if (input.CoachName != null) activity.CoachName = input.CoachName.Trim();
        if (input.CoachPhone != null) activity.CoachPhone = input.CoachPhone.Trim();
        if (input.MaxCapacity.HasValue) activity.MaxCapacity = input.MaxCapacity.Value;
        if (input.Requirements != null) activity.Requirements = input.Requirements.Trim();
        if (input.FeePerTerm.HasValue) activity.FeePerTerm = input.FeePerTerm.Value;

        await _extramuralActivityRepository.UpdateAsync(activity);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.SASpecific_Extramurals_Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var activity = await _extramuralActivityRepository
            .GetAll()
            .Include(ea => ea.StudentEnrollments)
            .FirstOrDefaultAsync(ea => ea.Id == id && ea.TenantId == AbpSession.TenantId);

        if (activity == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.ExtramuralActivityNotFound,
                "Extramural activity not found.");

        if (activity.StudentEnrollments != null && activity.StudentEnrollments.Any(se => se.Status == EnrollmentStatus.Active))
            throw new UserFriendlyException(SASpecificExceptionCodes.CannotDeleteExtramuralWithEnrollments,
                "Cannot delete an extramural activity that has active enrollments.");

        await _extramuralActivityRepository.DeleteAsync(activity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    [AbpAuthorize(PermissionNames.SASpecific_Extramurals_Manage)]
    public async Task<ExtramuralActivityDto> ActivateAsync(Guid id)
    {
        var activity = await _extramuralActivityRepository
            .FirstOrDefaultAsync(ea => ea.Id == id && ea.TenantId == AbpSession.TenantId);

        if (activity == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.ExtramuralActivityNotFound,
                "Extramural activity not found.");

        activity.IsActive = true;
        await _extramuralActivityRepository.UpdateAsync(activity);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.SASpecific_Extramurals_Manage)]
    public async Task<ExtramuralActivityDto> DeactivateAsync(Guid id)
    {
        var activity = await _extramuralActivityRepository
            .FirstOrDefaultAsync(ea => ea.Id == id && ea.TenantId == AbpSession.TenantId);

        if (activity == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.ExtramuralActivityNotFound,
                "Extramural activity not found.");

        activity.IsActive = false;
        await _extramuralActivityRepository.UpdateAsync(activity);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.SASpecific_Extramurals_Manage)]
    public async Task<ExtramuralActivityDto> OpenRegistrationAsync(Guid id)
    {
        var activity = await _extramuralActivityRepository
            .FirstOrDefaultAsync(ea => ea.Id == id && ea.TenantId == AbpSession.TenantId);

        if (activity == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.ExtramuralActivityNotFound,
                "Extramural activity not found.");

        activity.OpenRegistration();
        await _extramuralActivityRepository.UpdateAsync(activity);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.SASpecific_Extramurals_Manage)]
    public async Task<ExtramuralActivityDto> CloseRegistrationAsync(Guid id)
    {
        var activity = await _extramuralActivityRepository
            .FirstOrDefaultAsync(ea => ea.Id == id && ea.TenantId == AbpSession.TenantId);

        if (activity == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.ExtramuralActivityNotFound,
                "Extramural activity not found.");

        activity.CloseRegistration();
        await _extramuralActivityRepository.UpdateAsync(activity);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }
}
