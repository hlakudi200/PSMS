using Abp.Application.Services.Dto;
using psms.Academic.Shared;
using System;

namespace psms.Academic.Teachers.Dto;

/// <summary>
/// Full DTO for Teacher entity including computed properties.
/// </summary>
public class TeacherDto : FullAuditedEntityDto<Guid>
{
    public long UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MiddleName { get; set; }
    public string FullName { get; set; }
    public string EmployeeNumber { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public DateTime DateOfJoining { get; set; }
    public AddressDto Address { get; set; }
    public string Qualifications { get; set; }
    public string QualifiedSubjects { get; set; }
    public string EmploymentStatus { get; set; }
    public string ProfilePhotoUrl { get; set; }
    public bool IsActive { get; set; }

    /// <summary>Number of subject assignments</summary>
    public int SubjectAssignmentCount { get; set; }

    /// <summary>Number of class assignments</summary>
    public int ClassAssignmentCount { get; set; }
}
