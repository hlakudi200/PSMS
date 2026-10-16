using Abp.Dependency;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using psms.Admissions.AdmissionSettings.Dto;
using psms.Domain.Admissions.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Admissions.AdmissionSettings;

/// <summary>
/// How full a grade is, by the one arithmetic everything must agree on.
/// <para>
/// The screens warned about capacity (ADM-375) and the numbers were right, but
/// the decision itself never consulted them — a principal could accept ten
/// applicants for two places, all ten were told yes, and the eight who could
/// not be enrolled found out after being offered a place. Anything that acts
/// on capacity now reads it from here, so the warning on the screen and the
/// refusal at the decision cannot drift apart.
/// </para>
/// <para>
/// A grade with no capacity set is not full — <c>MaxCapacity</c> is optional,
/// and a school that has not said how many places exist has not said there are
/// none.
/// </para>
/// </summary>
public class GradeCapacityReader : ITransientDependency
{
    private readonly IRepository<Domain.Admissions.Entities.AdmissionSettings, Guid> _settings;
    private readonly IRepository<Application, Guid> _applications;
    private readonly IRepository<Waitlist, Guid> _waitlists;

    public GradeCapacityReader(
        IRepository<Domain.Admissions.Entities.AdmissionSettings, Guid> settings,
        IRepository<Application, Guid> applications,
        IRepository<Waitlist, Guid> waitlists)
    {
        _settings = settings;
        _applications = applications;
        _waitlists = waitlists;
    }

    /// <summary>
    /// The grade's own settings, falling back to the year's default row the way
    /// the settings screen does.
    /// </summary>
    public async Task<Domain.Admissions.Entities.AdmissionSettings> SettingsForAsync(
        Guid academicYearId, Guid gradeId, bool withNames = false)
    {
        var query = _settings.GetAll();
        if (withNames)
            query = query.Include(s => s.AcademicYear).Include(s => s.Grade);

        var forGrade = await query
            .FirstOrDefaultAsync(s => s.AcademicYearId == academicYearId && s.GradeId == gradeId);

        return forGrade ?? await query
            .FirstOrDefaultAsync(s => s.AcademicYearId == academicYearId && s.GradeId == null);
    }

    /// <summary>
    /// Counts for a grade, or null when the school has no admission settings
    /// for it at all — which is a different thing from a grade with room.
    /// </summary>
    public async Task<CapacityStatusDto> StatusAsync(Guid academicYearId, Guid gradeId)
    {
        var settings = await SettingsForAsync(academicYearId, gradeId, withNames: true);
        if (settings == null) return null;

        var applications = _applications
            .GetAll()
            .Where(a => a.AcademicYearId == academicYearId && a.AppliedGradeId == gradeId);

        return new CapacityStatusDto
        {
            GradeId = gradeId,
            GradeName = settings.Grade?.GradeName,
            AcademicYearId = academicYearId,
            AcademicYearName = settings.AcademicYear?.YearName,
            Capacity = settings.MaxCapacity ?? 0,
            CurrentEnrollment = settings.CurrentEnrolledCount,

            ApprovedPendingEnrollment = await applications
                .CountAsync(a => a.Status == ApplicationStatus.Approved),

            UnderConsideration = await applications
                .CountAsync(a => a.Status == ApplicationStatus.UnderConsideration
                    || a.Status == ApplicationStatus.UnderReview
                    || a.Status == ApplicationStatus.InterviewScheduled
                    || a.Status == ApplicationStatus.AssessmentScheduled),

            WaitlistCount = await _waitlists
                .GetAll()
                .CountAsync(w => w.GradeId == gradeId
                    && w.Status == WaitlistStatus.Active
                    && w.Application.AcademicYearId == academicYearId),
        };
    }

    /// <summary>
    /// Whether one more offer in this grade would be an offer past the last
    /// place, and the sentence to say about it.
    /// <para>
    /// Returns false when no capacity is set: an unset limit is not a limit of
    /// zero, and refusing every decision in a school that never filled the
    /// field in would be worse than the silence this replaces.
    /// </para>
    /// </summary>
    public async Task<(bool WouldOverfill, string Explanation)> WouldOverfillAsync(
        Guid academicYearId, Guid gradeId)
    {
        var status = await StatusAsync(academicYearId, gradeId);

        if (status == null || status.Capacity <= 0) return (false, null);
        if (status.AvailableSpots > 0) return (false, null);

        var taken = status.CurrentEnrollment + status.ApprovedPendingEnrollment;
        var grade = string.IsNullOrWhiteSpace(status.GradeName) ? "this grade" : status.GradeName;

        return (true,
            $"{grade} has {status.Capacity} place{(status.Capacity == 1 ? "" : "s")} and "
            + $"{taken} {(taken == 1 ? "is" : "are")} already taken "
            + $"({status.CurrentEnrollment} enrolled, {status.ApprovedPendingEnrollment} offered "
            + "and not yet enrolled).");
    }
}
