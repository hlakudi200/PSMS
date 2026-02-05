using psms.Academic.Shared;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.Teachers.Dto;

/// <summary>
/// Input DTO for updating a teacher. All fields nullable (partial update).
/// </summary>
public class UpdateTeacherDto
{
    [StringLength(100, MinimumLength = 2)]
    public string FirstName { get; set; }

    [StringLength(100, MinimumLength = 2)]
    public string LastName { get; set; }

    [StringLength(100)]
    public string MiddleName { get; set; }

    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; }

    [StringLength(20)]
    public string Phone { get; set; }

    public DateTime? DateOfJoining { get; set; }

    public AddressDto Address { get; set; }

    [StringLength(500)]
    public string Qualifications { get; set; }

    [StringLength(500)]
    public string QualifiedSubjects { get; set; }

    [StringLength(50)]
    public string EmploymentStatus { get; set; }

    public bool? IsActive { get; set; }
}
