using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Admissions.AdmissionSettings.Dto;
using psms.Admissions.Shared;
using psms.Authorization;
using psms.Domain.Admissions.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Admissions.AdmissionSettings;

/// <summary>
/// Service for managing admission settings per academic year and grade.
/// Implements ADM-011, ADM-014, ADM-028.
/// </summary>
[AbpAuthorize(PermissionNames.Admissions_Settings)]
public class AdmissionSettingsAppService : ApplicationService, IAdmissionSettingsAppService
{
    private readonly IRepository<Domain.Admissions.Entities.AdmissionSettings, Guid> _settingsRepository;
    private readonly IRepository<Application, Guid> _applicationRepository;
    private readonly IRepository<Waitlist, Guid> _waitlistRepository;
    private readonly IRepository<psms.Domain.Academic.Entities.AcademicYear, Guid> _academicYearRepository;

    public AdmissionSettingsAppService(
        IRepository<Domain.Admissions.Entities.AdmissionSettings, Guid> settingsRepository,
        IRepository<Application, Guid> applicationRepository,
        IRepository<Waitlist, Guid> waitlistRepository,
        IRepository<psms.Domain.Academic.Entities.AcademicYear, Guid> academicYearRepository)
    {
        _settingsRepository = settingsRepository;
        _applicationRepository = applicationRepository;
        _waitlistRepository = waitlistRepository;
        _academicYearRepository = academicYearRepository;
    }

    [AbpAuthorize(PermissionNames.Admissions_Settings_View)]
    public async Task<AdmissionSettingsDto> GetAsync(Guid id)
    {
        var settings = await _settingsRepository
            .GetAll()
            .Include(s => s.AcademicYear)
            .Include(s => s.Grade)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (settings == null)
            throw new UserFriendlyException(AdmissionsExceptionCodes.AdmissionSettingsNotFound, "Admission settings not found.");

        return ObjectMapper.Map<AdmissionSettingsDto>(settings);
    }

    [AbpAuthorize(PermissionNames.Admissions_Settings_View)]
    public async Task<AdmissionSettingsDto> GetByGradeAsync(Guid academicYearId, Guid gradeId)
    {
        // Try to get grade-specific settings
        var settings = await _settingsRepository
            .GetAll()
            .Include(s => s.AcademicYear)
            .Include(s => s.Grade)
            .FirstOrDefaultAsync(s => s.AcademicYearId == academicYearId && s.GradeId == gradeId);

        // Fall back to default settings (where GradeId is null)
        if (settings == null)
        {
            settings = await _settingsRepository
                .GetAll()
                .Include(s => s.AcademicYear)
                .Include(s => s.Grade)
                .FirstOrDefaultAsync(s => s.AcademicYearId == academicYearId && s.GradeId == null);
        }

        if (settings == null)
            throw new UserFriendlyException(AdmissionsExceptionCodes.AdmissionSettingsNotFound,
                "Admission settings not found for this academic year. Please configure admission settings first.");

        return ObjectMapper.Map<AdmissionSettingsDto>(settings);
    }

    /// <summary>
    /// Every admission settings row the school has, newest year first.
    /// <para>
    /// The only way to read these was one academic year at a time, and the
    /// screen that lists them defaults its year filter to "All Years" — so the
    /// list asked for nothing and showed nothing, and a school that had
    /// configured an intake was told it had none.
    /// </para>
    /// <para>
    /// The year and grade narrow the result when given, so the filters on that
    /// screen mean what they say rather than deciding whether anything loads
    /// at all.
    /// </para>
    /// </summary>
    /// <summary>
    /// What a prospective parent may apply for at this school, and on what
    /// terms.
    /// <para>
    /// The application form needs an academic year and a grade. An applicant
    /// holds neither <c>Academic.Calendar.View</c> nor
    /// <c>Academic.Grades.View</c>, and should not: a parent applying has no
    /// business reading the school's whole timetable of years and grades. So
    /// the question is answered in admissions terms instead, and only for
    /// intakes that are actually open.
    /// </para>
    /// <para>
    /// Guarded by the permission to create an application rather than the one
    /// to manage settings: this is for the person applying, not the person
    /// configuring.
    /// </para>
    /// </summary>
    [AbpAuthorize(PermissionNames.Admissions_Applications_Create)]
    public async Task<ListResultDto<OpenIntakeDto>> GetOpenIntakesAsync()
    {
        var settings = await _settingsRepository
            .GetAll()
            .Include(s => s.AcademicYear)
            .Include(s => s.Grade)
            .ToListAsync();

        var open = settings
            .Where(x => x.AreApplicationsOpen() && !x.IsAtCapacity())
            .OrderBy(x => x.AcademicYear.Year)
            .ThenBy(x => x.GradeId == null ? 0 : 1)
            .ThenBy(x => x.Grade == null ? 0 : (int)x.Grade.GradeLevel)
            .Select(x => new OpenIntakeDto
            {
                AcademicYearId = x.AcademicYearId,
                AcademicYearName = x.AcademicYear?.YearName,
                GradeId = x.GradeId,
                GradeName = x.Grade?.GradeName,
                ApplicationCloseDate = x.ApplicationCloseDate,
                FeeRequired = x.RequiresApplicationFee(),
                FeeAmount = x.ApplicationFeeAmount,
                IsInterviewRequired = x.IsInterviewRequired,
                IsAssessmentRequired = x.IsAssessmentRequired,
                MinimumAge = x.MinimumAge,
                MaximumAge = x.MaximumAge,
                RequiredDocuments = x.RequiredDocuments,
                AvailableSpots = x.MaxCapacity.HasValue ? x.GetAvailableSpots() : (int?)null,
            })
            .ToList();

        return new ListResultDto<OpenIntakeDto>(open);
    }

    [AbpAuthorize(PermissionNames.Admissions_Settings_View)]
    public async Task<ListResultDto<AdmissionSettingsDto>> GetAllAsync(
        Guid? academicYearId = null, Guid? gradeId = null)
    {
        var settings = await _settingsRepository
            .GetAll()
            .Include(s => s.AcademicYear)
            .Include(s => s.Grade)
            .WhereIf(academicYearId.HasValue, s => s.AcademicYearId == academicYearId.Value)
            .WhereIf(gradeId.HasValue, s => s.GradeId == gradeId.Value)
            .OrderByDescending(s => s.AcademicYear.Year)
            // The default that covers every grade belongs above the grades it
            // is the default for.
            .ThenBy(s => s.GradeId == null ? 0 : 1)
            .ThenBy(s => s.Grade.GradeLevel)
            .ToListAsync();

        return new ListResultDto<AdmissionSettingsDto>(
            ObjectMapper.Map<List<AdmissionSettingsDto>>(settings));
    }

    [AbpAuthorize(PermissionNames.Admissions_Settings_View)]
    public async Task<ListResultDto<AdmissionSettingsDto>> GetAllByAcademicYearAsync(Guid academicYearId)
    {
        var settingsList = await _settingsRepository
            .GetAll()
            .Include(s => s.AcademicYear)
            .Include(s => s.Grade)
            .Where(s => s.AcademicYearId == academicYearId)
            .OrderBy(s => s.GradeId == null) // Default settings first
            .ThenBy(s => s.Grade != null ? s.Grade.GradeName : "")
            .ToListAsync();

        return new ListResultDto<AdmissionSettingsDto>(
            ObjectMapper.Map<List<AdmissionSettingsDto>>(settingsList));
    }

    [AbpAuthorize(PermissionNames.Admissions_Settings_Manage)]
    public async Task<AdmissionSettingsDto> CreateAsync(CreateAdmissionSettingsDto input)
    {
        // Check if settings already exist for this academic year and grade
        var existingSettings = await _settingsRepository
            .FirstOrDefaultAsync(s => s.AcademicYearId == input.AcademicYearId && s.GradeId == input.GradeId);

        if (existingSettings != null)
        {
            throw new UserFriendlyException(AdmissionsExceptionCodes.DuplicateAdmissionSettings,
                input.GradeId.HasValue
                    ? "Admission settings already exist for this grade and academic year."
                    : "Default admission settings already exist for this academic year.");
        }

        var settings = ObjectMapper.Map<Domain.Admissions.Entities.AdmissionSettings>(input);
        settings.Id = Guid.NewGuid();
        settings.TenantId = AbpSession.TenantId;

        await AssertTheWindowMakesSenseAsync(settings);

        await _settingsRepository.InsertAsync(settings);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(settings.Id);
    }

    [AbpAuthorize(PermissionNames.Admissions_Settings_Manage)]
    public async Task<AdmissionSettingsDto> UpdateAsync(Guid id, UpdateAdmissionSettingsDto input)
    {
        var settings = await _settingsRepository.GetAsync(id);

        ObjectMapper.Map(input, settings);

        await AssertTheWindowMakesSenseAsync(settings);

        await _settingsRepository.UpdateAsync(settings);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    /// <summary>
    /// The application window has to describe a period a parent could actually
    /// apply in.
    /// <para>
    /// Nothing checked it. A window could close before it opened, could run
    /// past the end of the school year it was for, and could have closed years
    /// ago while the switch still said the school was accepting applications —
    /// which is exactly what one of these rows said: a 2027 intake whose
    /// window opened and closed in 2025.
    /// </para>
    /// <para>
    /// That last one is not cosmetic. Whether a school appears to a prospective
    /// parent at all is <c>AreApplicationsOpen()</c>, which weighs the switch
    /// against these dates. A principal could set this up, read "Open" on their
    /// own screen, and be invisible to every applicant with nothing anywhere
    /// saying why.
    /// </para>
    /// <para>
    /// Deliberately not checked: that the window falls inside the academic year.
    /// Applications for a year open during the year before it — that is the
    /// normal case, not an error.
    /// </para>
    /// </summary>
    private async Task AssertTheWindowMakesSenseAsync(Domain.Admissions.Entities.AdmissionSettings settings)
    {
        var opens = settings.ApplicationOpenDate;
        var closes = settings.ApplicationCloseDate;

        if (opens.HasValue && closes.HasValue && closes.Value.Date < opens.Value.Date)
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidApplicationWindow,
                "Applications cannot close before they open.");

        if (closes.HasValue)
        {
            var year = await _academicYearRepository
                .FirstOrDefaultAsync(y => y.Id == settings.AcademicYearId);

            if (year != null && closes.Value.Date > year.EndDate.Date)
                throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidApplicationWindow,
                    $"Applications would close on {closes.Value:dd MMM yyyy}, after the "
                    + $"{year.YearName} school year has already ended on {year.EndDate:dd MMM yyyy}. "
                    + "Nobody can apply for a year that is over.");
        }

        // The contradiction that makes a school invisible: open for business,
        // with a window that shut. Turning the switch off is how a closed
        // window is recorded.
        if (settings.IsAcceptingApplications && closes.HasValue && closes.Value.Date < DateTime.UtcNow.Date)
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidApplicationWindow,
                $"Applications are switched on, but the window closed on {closes.Value:dd MMM yyyy}, "
                + "so no parent can see this school. Move the closing date, or switch applications off.");
    }

    [AbpAuthorize(PermissionNames.Admissions_Settings_Manage)]
    public async Task DeleteAsync(Guid id)
    {
        var settings = await _settingsRepository.GetAsync(id);

        // Check if there are any applications using these settings
        var hasApplications = await _applicationRepository
            .GetAll()
            .AnyAsync(a => a.AcademicYearId == settings.AcademicYearId
                && (settings.GradeId == null || a.AppliedGradeId == settings.GradeId));

        if (hasApplications)
        {
            throw new UserFriendlyException(AdmissionsExceptionCodes.CannotDeleteSettingsWithApplications,
                "Cannot delete admission settings that have associated applications.");
        }

        await _settingsRepository.DeleteAsync(settings);
    }

    [AbpAuthorize(PermissionNames.Admissions_Settings_View)]
    public async Task<CapacityStatusDto> GetCapacityStatusAsync(Guid academicYearId, Guid gradeId)
    {
        var settings = await _settingsRepository
            .GetAll()
            .Include(s => s.AcademicYear)
            .Include(s => s.Grade)
            .FirstOrDefaultAsync(s => s.AcademicYearId == academicYearId && s.GradeId == gradeId);

        // Fall back to default settings
        if (settings == null)
        {
            settings = await _settingsRepository
                .GetAll()
                .Include(s => s.AcademicYear)
                .Include(s => s.Grade)
                .FirstOrDefaultAsync(s => s.AcademicYearId == academicYearId && s.GradeId == null);
        }

        if (settings == null)
            throw new UserFriendlyException(AdmissionsExceptionCodes.AdmissionSettingsNotFound,
                "Admission settings not found for this grade.");

        // Count applications in different states
        var applicationsQuery = _applicationRepository
            .GetAll()
            .Where(a => a.AcademicYearId == academicYearId && a.AppliedGradeId == gradeId);

        var approvedPendingEnrollment = await applicationsQuery
            .CountAsync(a => a.Status == ApplicationStatus.Approved);

        var underConsideration = await applicationsQuery
            .CountAsync(a => a.Status == ApplicationStatus.UnderConsideration
                || a.Status == ApplicationStatus.UnderReview
                || a.Status == ApplicationStatus.InterviewScheduled
                || a.Status == ApplicationStatus.AssessmentScheduled);

        var waitlistCount = await _waitlistRepository
            .GetAll()
            .CountAsync(w => w.GradeId == gradeId
                && w.Status == WaitlistStatus.Active
                && w.Application.AcademicYearId == academicYearId);

        return new CapacityStatusDto
        {
            GradeId = gradeId,
            GradeName = settings.Grade?.GradeName,
            AcademicYearId = academicYearId,
            AcademicYearName = settings.AcademicYear?.YearName,
            Capacity = settings.MaxCapacity ?? 0,
            CurrentEnrollment = settings.CurrentEnrolledCount,
            ApprovedPendingEnrollment = approvedPendingEnrollment,
            UnderConsideration = underConsideration,
            WaitlistCount = waitlistCount
        };
    }

    [AbpAuthorize(PermissionNames.Admissions_Settings_OpenApplications)]
    public async Task OpenApplicationsAsync(Guid academicYearId, Guid? gradeId = null)
    {
        var query = _settingsRepository
            .GetAll()
            .Where(s => s.AcademicYearId == academicYearId);

        if (gradeId.HasValue)
            query = query.Where(s => s.GradeId == gradeId.Value);

        var settingsList = await query.ToListAsync();

        foreach (var settings in settingsList)
        {
            settings.IsAcceptingApplications = true;
            settings.ApplicationOpenDate = DateTime.UtcNow;
        }

        await CurrentUnitOfWork.SaveChangesAsync();
    }

    [AbpAuthorize(PermissionNames.Admissions_Settings_CloseApplications)]
    public async Task CloseApplicationsAsync(Guid academicYearId, Guid? gradeId = null)
    {
        var query = _settingsRepository
            .GetAll()
            .Where(s => s.AcademicYearId == academicYearId);

        if (gradeId.HasValue)
            query = query.Where(s => s.GradeId == gradeId.Value);

        var settingsList = await query.ToListAsync();

        foreach (var settings in settingsList)
        {
            settings.IsAcceptingApplications = false;
            settings.ApplicationCloseDate = DateTime.UtcNow;
        }

        await CurrentUnitOfWork.SaveChangesAsync();
    }

    [AbpAuthorize(PermissionNames.Admissions_Settings_UpdateCapacity)]
    public async Task UpdateCapacityAsync(Guid academicYearId, Guid gradeId, int newCapacity)
    {
        var settings = await _settingsRepository
            .FirstOrDefaultAsync(s => s.AcademicYearId == academicYearId && s.GradeId == gradeId);

        if (settings == null)
            throw new UserFriendlyException(AdmissionsExceptionCodes.AdmissionSettingsNotFound,
                "Admission settings not found for this grade.");

        if (newCapacity < settings.CurrentEnrolledCount)
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidCapacityUpdate,
                $"Cannot set capacity below current enrollment ({settings.CurrentEnrolledCount}).");

        settings.MaxCapacity = newCapacity;

        await _settingsRepository.UpdateAsync(settings);
    }
}
