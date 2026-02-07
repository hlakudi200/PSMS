using Abp.Application.Services.Dto;
using System;

namespace psms.Academic.Classes.Dto;

/// <summary>
/// Lightweight list DTO for Class entity.
/// </summary>
public class ClassListDto : EntityDto<Guid>
{
    public string ClassName { get; set; }
    public Guid GradeId { get; set; }
    public string GradeName { get; set; }
    public Guid AcademicYearId { get; set; }
    public string AcademicYearName { get; set; }
    public int MaxCapacity { get; set; }
    public string ClassTeacherName { get; set; }
    public bool IsActive { get; set; }
    public int StudentCount { get; set; }
    public int AvailableCapacity => MaxCapacity - StudentCount;
}
