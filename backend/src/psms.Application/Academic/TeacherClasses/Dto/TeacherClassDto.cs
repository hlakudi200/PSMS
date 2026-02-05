using Abp.Application.Services.Dto;
using System;

namespace psms.Academic.TeacherClasses.Dto;

/// <summary>
/// DTO for teacher-class assignment with flattened navigation properties.
/// </summary>
public class TeacherClassDto : EntityDto<Guid>
{
    public Guid TeacherId { get; set; }
    public string TeacherName { get; set; }
    public Guid ClassId { get; set; }
    public string ClassName { get; set; }
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; }
    public string SubjectCode { get; set; }
    public bool IsClassTeacher { get; set; }
}
