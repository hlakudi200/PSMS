using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Admissions.Enrollments.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace psms.Admissions.Enrollments;

/// <summary>
/// Service for managing student enrollments.
/// Implements ADM-025 to ADM-029.
/// </summary>
public interface IEnrollmentAppService : IApplicationService
{
    /// <summary>
    /// Gets enrollment status for an application.
    /// </summary>
    Task<EnrollmentDto> GetByApplicationAsync(Guid applicationId);

    /// <summary>
    /// Gets all applications ready for enrollment.
    /// </summary>
    Task<PagedResultDto<EnrollmentDto>> GetPendingEnrollmentsAsync(GetPendingEnrollmentsInput input);

    /// <summary>
    /// Gets available classes for a grade.
    /// </summary>
    Task<List<ClassAvailabilityDto>> GetAvailableClassesAsync(Guid gradeId);

    /// <summary>
    /// Accepts an admission offer.
    /// </summary>
    Task<EnrollmentDto> AcceptOfferAsync(AcceptOfferDto input);

    /// <summary>
    /// Assigns a class to an enrolling student.
    /// </summary>
    Task<EnrollmentDto> AssignClassAsync(AssignClassDto input);

    /// <summary>
    /// Completes enrollment and creates student record.
    /// </summary>
    Task<EnrollmentDto> CompleteEnrollmentAsync(CompleteEnrollmentDto input);
}
