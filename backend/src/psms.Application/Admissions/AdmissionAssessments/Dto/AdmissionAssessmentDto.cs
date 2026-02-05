using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Admissions.AdmissionAssessments.Dto;

/// <summary>
/// DTO for admission assessment/placement test information.
/// Aligned with AdmissionAssessment entity.
/// </summary>
public class AdmissionAssessmentDto : CreationAuditedEntityDto<Guid>
{
    public Guid ApplicationId { get; set; }
    public string ApplicationNumber { get; set; }
    public string ApplicantName { get; set; }

    // Assessment Type
    public AssessmentType Type { get; set; }
    public string TypeDisplayName => Type.ToString();

    // Schedule Information (ADM-015)
    public DateTime ScheduledDate { get; set; }
    public Guid AssessedGradeId { get; set; }
    public string GradeName { get; set; }

    // Subjects to be assessed (JSON)
    public string Subjects { get; set; }

    // Assessor
    public long AssessorUserId { get; set; }
    public string AssessorName { get; set; }

    // Results (ADM-016)
    public DateTime? CompletedDate { get; set; }
    public decimal TotalScore { get; set; }
    public decimal MaxScore { get; set; }
    public decimal Percentage { get; set; }
    public string PercentageDisplay => $"{Percentage:N1}%";
    public bool Passed { get; set; }
    public string Feedback { get; set; }

    // Flags
    public bool IsCompleted => CompletedDate.HasValue;
    public bool IsUpcoming => !CompletedDate.HasValue && ScheduledDate >= DateTime.Today;
}
