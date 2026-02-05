using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Admissions.AdmissionSettings.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Admissions.AdmissionSettings;

/// <summary>
/// Service for managing admission settings per academic year and grade.
/// Implements ADM-011, ADM-014, ADM-028.
/// </summary>
public interface IAdmissionSettingsAppService : IApplicationService
{
    /// <summary>
    /// Gets admission settings by ID.
    /// </summary>
    Task<AdmissionSettingsDto> GetAsync(Guid id);

    /// <summary>
    /// Gets admission settings for a specific grade and academic year.
    /// Falls back to default settings if grade-specific not found.
    /// </summary>
    Task<AdmissionSettingsDto> GetByGradeAsync(Guid academicYearId, Guid gradeId);

    /// <summary>
    /// Gets all admission settings for an academic year.
    /// </summary>
    Task<ListResultDto<AdmissionSettingsDto>> GetAllByAcademicYearAsync(Guid academicYearId);

    /// <summary>
    /// Creates new admission settings.
    /// </summary>
    Task<AdmissionSettingsDto> CreateAsync(CreateAdmissionSettingsDto input);

    /// <summary>
    /// Updates existing admission settings.
    /// </summary>
    Task<AdmissionSettingsDto> UpdateAsync(Guid id, UpdateAdmissionSettingsDto input);

    /// <summary>
    /// Deletes admission settings.
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Gets capacity status for a specific grade.
    /// </summary>
    Task<CapacityStatusDto> GetCapacityStatusAsync(Guid academicYearId, Guid gradeId);

    /// <summary>
    /// Opens applications for a grade (or all grades if gradeId is null).
    /// </summary>
    Task OpenApplicationsAsync(Guid academicYearId, Guid? gradeId = null);

    /// <summary>
    /// Closes applications for a grade (or all grades if gradeId is null).
    /// </summary>
    Task CloseApplicationsAsync(Guid academicYearId, Guid? gradeId = null);

    /// <summary>
    /// Updates the capacity for a grade.
    /// </summary>
    Task UpdateCapacityAsync(Guid academicYearId, Guid gradeId, int newCapacity);
}
