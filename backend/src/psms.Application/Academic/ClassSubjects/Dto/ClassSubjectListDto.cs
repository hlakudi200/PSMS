using Abp.Application.Services.Dto;
using System;

namespace psms.Academic.ClassSubjects.Dto;

/// <summary>
/// Lightweight DTO for class-subject assignment lists.
/// </summary>
public class ClassSubjectListDto : EntityDto<Guid>
{
    public Guid ClassId { get; set; }
    public Guid SubjectId { get; set; }
    public Guid? TeacherId { get; set; }
    public int PeriodsPerWeek { get; set; }
    public bool IsActive { get; set; }

    // Flattened
    public string ClassName { get; set; }
    public string SubjectName { get; set; }
    public string SubjectCode { get; set; }
    public string TeacherName { get; set; }
}
