using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Admissions.Applications.Dto;

/// <summary>
/// Input DTO for filtering and paginating application list.
/// </summary>
public class GetApplicationsInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// Filter by application number (partial match).
    /// </summary>
    public string ApplicationNumber { get; set; }

    /// <summary>
    /// Filter by applicant name (partial match on first or last name).
    /// </summary>
    public string ApplicantName { get; set; }

    /// <summary>
    /// Filter by specific status.
    /// </summary>
    public ApplicationStatus? Status { get; set; }

    /// <summary>
    /// Filter by academic year.
    /// </summary>
    public Guid? AcademicYearId { get; set; }

    /// <summary>
    /// Filter by grade.
    /// </summary>
    public Guid? GradeId { get; set; }

    /// <summary>
    /// Filter applications submitted after this date.
    /// </summary>
    public DateTime? SubmittedDateFrom { get; set; }

    /// <summary>
    /// Filter applications submitted before this date.
    /// </summary>
    public DateTime? SubmittedDateTo { get; set; }

    /// <summary>
    /// General keyword search across applicant name and application number.
    /// </summary>
    public string Keyword { get; set; }

    /// <summary>
    /// Filter by fee payment status.
    /// </summary>
    public bool? IsFeePaid { get; set; }

    /// <summary>
    /// Filter by waitlist status.
    /// </summary>
    public bool? IsOnWaitlist { get; set; }

    /// <summary>
    /// Include only applications with expired offers.
    /// </summary>
    public bool? HasExpiredOffer { get; set; }
}
