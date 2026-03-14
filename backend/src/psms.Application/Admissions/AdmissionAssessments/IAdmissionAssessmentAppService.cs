using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Admissions.AdmissionAssessments.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Admissions.AdmissionAssessments;

/// <summary>
/// Service for managing admission assessments.
/// Implements ADM-014 to ADM-016.
/// </summary>
public interface IAdmissionAssessmentAppService : IApplicationService
{
    /// <summary>
    /// Gets an assessment by ID.
    /// </summary>
    Task<AdmissionAssessmentDto> GetAsync(Guid id);

    /// <summary>
    /// Gets the assessment for an application.
    /// </summary>
    Task<AdmissionAssessmentDto> GetByApplicationAsync(Guid applicationId);

    /// <summary>
    /// Gets all assessments with filters.
    /// </summary>
    Task<PagedResultDto<AdmissionAssessmentDto>> GetAllAsync(GetAdmissionAssessmentsInput input);

    /// <summary>
    /// Schedules an assessment for an application.
    /// </summary>
    Task<AdmissionAssessmentDto> ScheduleAsync(ScheduleAssessmentDto input);

    /// <summary>
    /// Records assessment results.
    /// </summary>
    Task<AdmissionAssessmentDto> RecordResultsAsync(Guid id, RecordAssessmentResultsDto input);

    /// <summary>
    /// Cancels an assessment.
    /// </summary>
    Task CancelAsync(Guid id, string reason);
}
