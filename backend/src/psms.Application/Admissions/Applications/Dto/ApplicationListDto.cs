using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Admissions.Applications.Dto;

/// <summary>
/// Lightweight DTO for application list views.
/// </summary>
public class ApplicationListDto : EntityDto<Guid>
{
    public string ApplicationNumber { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public DateTime DateOfBirth { get; set; }
    public string GradeName { get; set; }
    public string AcademicYearName { get; set; }
    public ApplicationStatus Status { get; set; }
    public string StatusDisplayName => Status.ToString();
    public DateTime? SubmittedDate { get; set; }
    public DateTime CreationTime { get; set; }
    public bool IsFeePaid { get; set; }
    public int ParentCount { get; set; }
    public int DocumentCount { get; set; }
    public bool HasInterview { get; set; }
    public bool HasAssessment { get; set; }
    public int? WaitlistPosition { get; set; }
    public DateTime? OfferExpiryDate { get; set; }
    public bool IsOfferExpired => OfferExpiryDate.HasValue && OfferExpiryDate.Value < DateTime.UtcNow;
}
