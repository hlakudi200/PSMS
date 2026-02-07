using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Admissions.ApplicantParents.Dto;

/// <summary>
/// DTO for parent/guardian information on an application.
/// Aligned with ApplicantParent entity.
/// </summary>
public class ApplicantParentDto : EntityDto<Guid>
{
    public Guid ApplicationId { get; set; }

    // Relationship
    public RelationshipType Relationship { get; set; }
    public string RelationshipDisplayName => Relationship.ToString();

    // Personal Information
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public string IdNumber { get; set; }

    // Contact Information
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string AlternatePhone { get; set; }

    // Address
    public string StreetAddress { get; set; }
    public string Suburb { get; set; }
    public string City { get; set; }
    public string Province { get; set; }
    public string PostalCode { get; set; }

    // Employment
    public string Occupation { get; set; }
    public string Employer { get; set; }

    // Flags
    public bool IsPrimaryContact { get; set; }
    public bool IsFinanciallyResponsible { get; set; }
}
