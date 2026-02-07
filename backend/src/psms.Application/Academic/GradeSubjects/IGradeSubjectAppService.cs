using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Academic.GradeSubjects.Dto;
using psms.Academic.Subjects.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Academic.GradeSubjects;

/// <summary>
/// Service for managing grade-subject assignments.
/// </summary>
public interface IGradeSubjectAppService : IApplicationService
{
    /// <summary>Gets all subjects assigned to a grade.</summary>
    Task<ListResultDto<GradeSubjectDto>> GetByGradeAsync(Guid gradeId);

    /// <summary>Gets all grades a subject is assigned to.</summary>
    Task<ListResultDto<GradeSubjectDto>> GetBySubjectAsync(Guid subjectId);

    /// <summary>Assigns a subject to a grade.</summary>
    Task<GradeSubjectDto> AssignAsync(AssignSubjectToGradeDto input);

    /// <summary>Removes a grade-subject assignment.</summary>
    Task UnassignAsync(Guid id);

    /// <summary>Gets active subjects not yet assigned to the specified grade.</summary>
    Task<ListResultDto<SubjectListDto>> GetUnassignedSubjectsForGradeAsync(Guid gradeId);
}
