using Abp.Application.Services.Dto;
using System;

namespace psms.Academic.Teachers.Dto;

/// <summary>
/// Lightweight DTO for teacher lists and dropdowns.
/// </summary>
public class TeacherListDto : EntityDto<Guid>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName { get; set; }
    public string EmployeeNumber { get; set; }
    public string Email { get; set; }
    public string EmploymentStatus { get; set; }
    public bool IsActive { get; set; }
    public int SubjectAssignmentCount { get; set; }
    public int ClassAssignmentCount { get; set; }
}
