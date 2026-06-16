using Abp.Application.Services.Dto;
using psms.Academic.Shared;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Academic.Students.Dto;

/// <summary>
/// Full DTO for Student entity with computed properties.
/// </summary>
public class StudentDto : FullAuditedEntityDto<Guid>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MiddleName { get; set; }
    public string FullName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public int Age { get; set; }
    public Gender Gender { get; set; }

    // Identification
    public string IdNumber { get; set; }
    public string PassportNumber { get; set; }
    public bool IsSACitizen { get; set; }

    // Academic
    public string AdmissionNumber { get; set; }
    public DateTime AdmissionDate { get; set; }
    public Guid CurrentGradeId { get; set; }
    public string CurrentGradeName { get; set; }
    public Guid CurrentClassId { get; set; }
    public string CurrentClassName { get; set; }

    // Contact
    public string Phone { get; set; }
    public string Email { get; set; }
    public AddressDto PhysicalAddress { get; set; }
    public AddressDto PostalAddress { get; set; }
    public string ProfilePhotoUrl { get; set; }

    // Emergency
    public string EmergencyContactName { get; set; }
    public string EmergencyContactPhone { get; set; }

    // Medical / POPIA
    public string MedicalConditions { get; set; }
    public bool POPIAConsentGiven { get; set; }
    public DateTime? POPIAConsentDate { get; set; }

    public bool IsActive { get; set; }

    /// <summary>Number of linked parents/guardians.</summary>
    public int ParentCount { get; set; }

    /// <summary>Number of subject enrollments.</summary>
    public int SubjectCount { get; set; }

    /// <summary>
    /// Login username for the student's portal account (LC-07). Populated only
    /// in the CreateAsync response (alongside TemporaryPassword); null on reads.
    /// </summary>
    public string LoginUserName { get; set; }

    /// <summary>
    /// One-time temporary password — populated ONLY in the response to
    /// CreateAsync when a login account is freshly provisioned, so the admin
    /// can hand it to the student. Never returned on reads (it isn't stored in
    /// plaintext). The student must change it on first login.
    /// </summary>
    public string TemporaryPassword { get; set; }
}
