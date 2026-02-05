using Abp.Application.Services.Dto;
using System;

namespace psms.Academic.Classes.Dto;

/// <summary>
/// Full DTO for Class entity with computed properties.
/// </summary>
public class ClassDto : FullAuditedEntityDto<Guid>
{
    public string ClassName { get; set; }
    public Guid GradeId { get; set; }
    public string GradeName { get; set; }
    public Guid AcademicYearId { get; set; }
    public string AcademicYearName { get; set; }
    public int MaxCapacity { get; set; }
    public Guid? ClassTeacherId { get; set; }
    public string ClassTeacherName { get; set; }
    public bool IsActive { get; set; }

    /// <summary>Number of active students enrolled.</summary>
    public int StudentCount { get; set; }

    /// <summary>Number of teacher assignments.</summary>
    public int TeacherAssignmentCount { get; set; }

    /// <summary>Available capacity (MaxCapacity - StudentCount).</summary>
    public int AvailableCapacity => MaxCapacity - StudentCount;
}
