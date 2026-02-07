using psms.Domain.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace psms.Admissions.ApplicantParents.Dto;

/// <summary>
/// DTO for updating parent/guardian information.
/// Aligned with ApplicantParent entity.
/// </summary>
public class UpdateApplicantParentDto
{
    // Relationship
    public RelationshipType? Relationship { get; set; }

    // Personal Information
    [StringLength(100, MinimumLength = 2)]
    public string FirstName { get; set; }

    [StringLength(100, MinimumLength = 2)]
    public string LastName { get; set; }

    [StringLength(13)]
    public string IdNumber { get; set; }

    // Contact Information
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; }

    [Phone]
    [StringLength(20)]
    public string PhoneNumber { get; set; }

    [Phone]
    [StringLength(20)]
    public string AlternatePhone { get; set; }

    // Address
    [StringLength(200)]
    public string StreetAddress { get; set; }

    [StringLength(100)]
    public string Suburb { get; set; }

    [StringLength(100)]
    public string City { get; set; }

    [StringLength(50)]
    public string Province { get; set; }

    [StringLength(10)]
    public string PostalCode { get; set; }

    // Employment
    [StringLength(100)]
    public string Occupation { get; set; }

    [StringLength(200)]
    public string Employer { get; set; }

    // Flags
    public bool? IsPrimaryContact { get; set; }
    public bool? IsFinanciallyResponsible { get; set; }
}
