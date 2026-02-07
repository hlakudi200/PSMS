using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Admissions.Applications.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Admissions.Applications;

/// <summary>
/// Service for managing admission applications.
/// Implements ADM-001 to ADM-005.
/// </summary>
public interface IApplicationAppService : IApplicationService
{
    /// <summary>
    /// Gets an application by ID.
    /// </summary>
    Task<ApplicationDto> GetAsync(Guid id);

    /// <summary>
    /// Gets an application by application number.
    /// </summary>
    Task<ApplicationDto> GetByApplicationNumberAsync(string applicationNumber);

    /// <summary>
    /// Gets paginated list of applications with filters.
    /// </summary>
    Task<PagedResultDto<ApplicationListDto>> GetAllAsync(GetApplicationsInput input);

    /// <summary>
    /// Creates a new application (starts in Draft status).
    /// </summary>
    Task<ApplicationDto> CreateAsync(CreateApplicationDto input);

    /// <summary>
    /// Updates an application (only allowed when in Draft status).
    /// </summary>
    Task<ApplicationDto> UpdateAsync(Guid id, UpdateApplicationDto input);

    /// <summary>
    /// Deletes an application (only allowed when in Draft status).
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Submits an application for processing.
    /// Transitions from Draft → Submitted → PaymentPending.
    /// </summary>
    Task<ApplicationDto> SubmitAsync(Guid id);

    /// <summary>
    /// Withdraws an application.
    /// Can be done at any status except Enrolled.
    /// </summary>
    Task<ApplicationDto> WithdrawAsync(Guid id, string reason = null);

    /// <summary>
    /// Gets application statistics for a specific academic year and grade.
    /// </summary>
    Task<ApplicationStatisticsDto> GetStatisticsAsync(Guid academicYearId, Guid? gradeId = null);
}
