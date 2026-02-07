using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Academic.StudentParents.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Academic.StudentParents;

/// <summary>
/// Application service interface for managing student-parent links.
/// </summary>
public interface IStudentParentAppService : IApplicationService
{
    /// <summary>Gets all parents linked to a student.</summary>
    Task<ListResultDto<StudentParentDto>> GetByStudentAsync(Guid studentId);

    /// <summary>Gets all students linked to a parent.</summary>
    Task<ListResultDto<StudentParentDto>> GetByParentAsync(Guid parentId);

    /// <summary>Links a student to a parent.</summary>
    Task<StudentParentDto> LinkAsync(LinkStudentParentDto input);

    /// <summary>Updates a student-parent link.</summary>
    Task<StudentParentDto> UpdateLinkAsync(Guid id, LinkStudentParentDto input);

    /// <summary>Removes a student-parent link.</summary>
    Task UnlinkAsync(Guid id);
}
