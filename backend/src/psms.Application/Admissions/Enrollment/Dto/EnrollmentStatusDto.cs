using System;

namespace psms.Admissions.Enrollment.Dto;

/// <summary>
/// DTO for enrollment status and checklist (ADM-025).
/// </summary>
public class EnrollmentStatusDto
{
    public Guid ApplicationId { get; set; }
    public string ApplicationNumber { get; set; }
    public string ApplicantName { get; set; }
    public string GradeName { get; set; }

    // Checklist Items (ADM-025)
    public bool OfferAccepted { get; set; }
    public DateTime? OfferAcceptedDate { get; set; }

    public bool RegistrationFeePaid { get; set; }
    public DateTime? RegistrationFeePaidDate { get; set; }
    public decimal? RegistrationFeeAmount { get; set; }

    public bool EnrollmentFormCompleted { get; set; }
    public DateTime? EnrollmentFormCompletedDate { get; set; }

    public bool MedicalFormsSubmitted { get; set; }
    public DateTime? MedicalFormsSubmittedDate { get; set; }

    public bool EnrollmentContractSigned { get; set; }
    public DateTime? EnrollmentContractSignedDate { get; set; }

    public bool POPIAConsentSigned { get; set; }
    public DateTime? POPIAConsentSignedDate { get; set; }

    // Class Assignment (ADM-029)
    public Guid? AssignedClassId { get; set; }
    public string AssignedClassName { get; set; }
    public bool IsClassAssigned => AssignedClassId.HasValue;

    // Completion Status
    public bool CanCompleteEnrollment => OfferAccepted
        && RegistrationFeePaid
        && EnrollmentFormCompleted
        && MedicalFormsSubmitted
        && EnrollmentContractSigned
        && POPIAConsentSigned
        && IsClassAssigned;

    public int CompletedSteps
    {
        get
        {
            var count = 0;
            if (OfferAccepted) count++;
            if (RegistrationFeePaid) count++;
            if (EnrollmentFormCompleted) count++;
            if (MedicalFormsSubmitted) count++;
            if (EnrollmentContractSigned) count++;
            if (POPIAConsentSigned) count++;
            if (IsClassAssigned) count++;
            return count;
        }
    }

    public int TotalSteps => 7;
    public decimal CompletionPercentage => (decimal)CompletedSteps / TotalSteps * 100;
}
