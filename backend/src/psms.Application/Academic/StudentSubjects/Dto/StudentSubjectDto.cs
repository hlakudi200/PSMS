using Abp.Application.Services.Dto;
using System;

namespace psms.Academic.StudentSubjects.Dto;

/// <summary>
/// DTO for student-subject enrollment with flattened navigation properties.
/// </summary>
public class StudentSubjectDto : EntityDto<Guid>
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; }
    public string StudentAdmissionNumber { get; set; }
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; }
    public string SubjectCode { get; set; }
    public Guid AcademicYearId { get; set; }
    public string AcademicYearName { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public bool IsActive { get; set; }
}
