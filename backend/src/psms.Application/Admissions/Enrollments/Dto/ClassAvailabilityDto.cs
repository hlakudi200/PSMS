using System;

namespace psms.Admissions.Enrollments.Dto;

/// <summary>
/// DTO for class availability information.
/// </summary>
public class ClassAvailabilityDto
{
    public Guid ClassId { get; set; }
    public string ClassName { get; set; }
    public Guid GradeId { get; set; }
    public string GradeName { get; set; }
    public int MaxCapacity { get; set; }
    public int CurrentEnrolled { get; set; }
    public int AvailableSpots => MaxCapacity - CurrentEnrolled;
    public bool IsAvailable => AvailableSpots > 0;
    public string TeacherName { get; set; }
}
