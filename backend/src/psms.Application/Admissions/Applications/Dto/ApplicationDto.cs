using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Admissions.Applications.Dto;

/// <summary>
/// Full application details DTO.
/// Aligned with Application entity.
/// </summary>
public class ApplicationDto : FullAuditedEntityDto<Guid>
{
    // Application Identity
    public string ApplicationNumber { get; set; }
    public Guid AcademicYearId { get; set; }
    public string AcademicYearName { get; set; }
    public Guid ApplyingForGradeId { get; set; }
    public string ApplyingForGradeName { get; set; }

    // Prospective Student Information (from Application entity)
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }
    public string FullName => string.IsNullOrWhiteSpace(MiddleName)
        ? $"{FirstName} {LastName}"
        : $"{FirstName} {MiddleName} {LastName}";
    public DateTime DateOfBirth { get; set; }
    public int Age => CalculateAge(DateOfBirth);
    public Gender Gender { get; set; }
    public string GenderDisplayName => Gender.ToString();
    public string IdNumber { get; set; }
    public string PassportNumber { get; set; }
    public bool IsSACitizen { get; set; }

    // Previous School
    public string PreviousSchool { get; set; }

    // Contact (creator's email from entity)
    public string CreatorEmailAddress { get; set; }

    // Application Status
    public ApplicationStatus Status { get; set; }
    public string StatusDisplayName => Status.ToString();
    public DateTime ApplicationDate { get; set; }
    public DateTime? SubmittedDate { get; set; }

    // Decision Information
    public AdmissionDecision? Decision { get; set; }
    public string DecisionDisplayName => Decision?.ToString();
    public string DecisionReason { get; set; }
    public DateTime? DecisionDate { get; set; }
    public DateTime? ReviewedDate { get; set; }
    public long? ReviewedByUserId { get; set; }
    public string ReviewedByUserName { get; set; }

    // Offer Expiry
    public DateTime? OfferExpiryDate { get; set; }
    public bool IsOfferExpired => OfferExpiryDate.HasValue && OfferExpiryDate.Value < DateTime.UtcNow;

    // Enrollment
    public Guid? CreatedStudentId { get; set; }

    // Related Data Counts (populated via navigation properties)
    public int ParentCount { get; set; }
    public int DocumentCount { get; set; }
    public bool HasInterview { get; set; }
    public bool HasAssessment { get; set; }
    public int? WaitlistPosition { get; set; }

    // Fee Status (from ApplicationFee navigation)
    public bool IsFeePaid { get; set; }

    // Workflow Flags
    public bool CanEdit => Status == ApplicationStatus.Draft;
    public bool CanSubmit => Status == ApplicationStatus.Draft && ParentCount > 0;
    public bool CanWithdraw => Status != ApplicationStatus.Enrolled && Status != ApplicationStatus.Withdrawn && Status != ApplicationStatus.Expired;
    public bool CanMakeDecision => Status == ApplicationStatus.UnderConsideration;

    private static int CalculateAge(DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age)) age--;
        return age;
    }
}
