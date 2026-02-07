using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Academic.GradeSubjects.Dto;

/// <summary>
/// DTO for a grade-subject assignment.
/// </summary>
public class GradeSubjectDto : EntityDto<Guid>
{
    public Guid GradeId { get; set; }
    public Guid SubjectId { get; set; }
    public bool IsRequired { get; set; }

    // Flattened from Grade
    public string GradeName { get; set; }
    public SouthAfricanGradeLevel GradeLevel { get; set; }

    // Flattened from Subject
    public string SubjectName { get; set; }
    public string SubjectCode { get; set; }
    public bool IsCore { get; set; }
}
