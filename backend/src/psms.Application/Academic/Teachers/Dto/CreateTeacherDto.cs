using psms.Academic.Shared;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.Teachers.Dto;

/// <summary>
/// Input DTO for creating a new teacher.
/// </summary>
public class CreateTeacherDto
{
    [Required]
    public long UserId { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FirstName { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string LastName { get; set; }

    [StringLength(100)]
    public string MiddleName { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string EmployeeNumber { get; set; }

    [Required]
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
}
