using Abp.Application.Services.Dto;
using System;

namespace psms.Academic.ClassSubjects.Dto;

/// <summary>
/// Full DTO for a class-subject assignment.
/// </summary>
public class ClassSubjectDto : FullAuditedEntityDto<Guid>
{
    public Guid ClassId { get; set; }
    public Guid SubjectId { get; set; }
    public Guid? TeacherId { get; set; }
    public int PeriodsPerWeek { get; set; }
    public bool IsActive { get; set; }

    // Flattened from Class
    public string ClassName { get; set; }
    public Guid GradeId { get; set; }
    public string GradeName { get; set; }

    // Flattened from Subject
    public string SubjectName { get; set; }
    public string SubjectCode { get; set; }

    // Flattened from Teacher
    public string TeacherName { get; set; }
}
