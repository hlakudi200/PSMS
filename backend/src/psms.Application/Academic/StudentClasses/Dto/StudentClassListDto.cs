using Abp.Application.Services.Dto;
using System;

namespace psms.Academic.StudentClasses.Dto;

public class StudentClassListDto : EntityDto<Guid>
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; }
    public Guid ClassId { get; set; }
    public string ClassName { get; set; }
    public string AcademicYearName { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsActive { get; set; }
}
