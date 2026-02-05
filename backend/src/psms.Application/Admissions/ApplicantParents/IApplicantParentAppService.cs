using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Admissions.ApplicantParents.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Admissions.ApplicantParents;

/// <summary>
/// Service for managing parents/guardians on admission applications.
/// Implements ADM-003.
/// </summary>
public interface IApplicantParentAppService : IApplicationService
{
    /// <summary>
    /// Gets a parent by ID.
    /// </summary>
    Task<ApplicantParentDto> GetAsync(Guid id);

    /// <summary>
    /// Gets all parents for an application.
    /// </summary>
    Task<ListResultDto<ApplicantParentDto>> GetAllByApplicationAsync(Guid applicationId);

    /// <summary>
    /// Adds a parent to an application.
    /// </summary>
    Task<ApplicantParentDto> CreateAsync(CreateApplicantParentDto input);

    /// <summary>
    /// Updates parent information.
    /// </summary>
    Task<ApplicantParentDto> UpdateAsync(Guid id, UpdateApplicantParentDto input);

    /// <summary>
    /// Removes a parent from an application.
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Sets a parent as the primary contact.
    /// </summary>
    Task SetAsPrimaryContactAsync(Guid id);

    /// <summary>
    /// Sets a parent as financially responsible.
    /// </summary>
    Task SetAsFinanciallyResponsibleAsync(Guid id);
}
