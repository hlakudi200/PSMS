using psms.Academic.Shared;
using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.Students.Dto;

/// <summary>
/// Input DTO for updating a student. All fields nullable (partial update).
/// </summary>
public class UpdateStudentDto
{
    [StringLength(100, MinimumLength = 2)]
    public string FirstName { get; set; }

    [StringLength(100, MinimumLength = 2)]
    public string LastName { get; set; }

    [StringLength(100)]
    public string MiddleName { get; set; }

    public DateTime? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }

    [StringLength(13)]
    public string IdNumber { get; set; }

    [StringLength(50)]
    public string PassportNumber { get; set; }

    public bool? IsSACitizen { get; set; }

    // Contact
    [StringLength(20)]
    public string Phone { get; set; }

    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; }

    public AddressDto PhysicalAddress { get; set; }
    public AddressDto PostalAddress { get; set; }

    [StringLength(500)]
    public string ProfilePhotoUrl { get; set; }

    // Emergency
    [StringLength(100)]
    public string EmergencyContactName { get; set; }

    [StringLength(20)]
    public string EmergencyContactPhone { get; set; }

    // Medical / POPIA
    [StringLength(2000)]
    public string MedicalConditions { get; set; }

    public bool? POPIAConsentGiven { get; set; }

    public bool? IsActive { get; set; }
}
