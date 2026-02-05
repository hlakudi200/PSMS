using Abp.Application.Services.Dto;
using System;

namespace psms.Academic.TeacherSubjects.Dto;

/// <summary>
/// DTO for teacher-subject assignment with flattened navigation properties.
/// </summary>
public class TeacherSubjectDto : EntityDto<Guid>
{
    public Guid TeacherId { get; set; }
    public string TeacherName { get; set; }
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; }
    public string SubjectCode { get; set; }
    public Guid GradeId { get; set; }
    public string GradeName { get; set; }
    public bool IsPrimary { get; set; }
}
