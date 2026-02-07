using psms.Academic.Shared;
using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.Students.Dto;

/// <summary>
/// Input DTO for creating a student.
/// </summary>
public class CreateStudentDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FirstName { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string LastName { get; set; }

    [StringLength(100)]
    public string MiddleName { get; set; }

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    public Gender Gender { get; set; }

    [StringLength(13)]
    public string IdNumber { get; set; }

    [StringLength(50)]
    public string PassportNumber { get; set; }

    public bool IsSACitizen { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string AdmissionNumber { get; set; }

    [Required]
    public DateTime AdmissionDate { get; set; }

    [Required]
    public Guid CurrentGradeId { get; set; }

    [Required]
    public Guid CurrentClassId { get; set; }

    // Contact (optional)
    [StringLength(20)]
    public string Phone { get; set; }

    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; }

    public AddressDto PhysicalAddress { get; set; }
    public AddressDto PostalAddress { get; set; }

    // Emergency (optional)
    [StringLength(100)]
    public string EmergencyContactName { get; set; }

    [StringLength(20)]
    public string EmergencyContactPhone { get; set; }

    // Medical (optional)
    [StringLength(2000)]
    public string MedicalConditions { get; set; }

    public bool POPIAConsentGiven { get; set; }
}
