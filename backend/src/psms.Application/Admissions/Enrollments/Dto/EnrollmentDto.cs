using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Admissions.Enrollments.Dto;

/// <summary>
/// DTO for enrollment information.
/// Implements ADM-025 to ADM-027.
/// </summary>
public class EnrollmentDto : EntityDto<Guid>
{
    public Guid ApplicationId { get; set; }
    public string ApplicationNumber { get; set; }
    public string ApplicantName { get; set; }

    // Student Info (from Application)
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }

    // Grade/Class Assignment
    public Guid GradeId { get; set; }
    public string GradeName { get; set; }
    public Guid? AssignedClassId { get; set; }
    public string AssignedClassName { get; set; }

    // Enrollment Status
    public bool IsOfferAccepted { get; set; }
    public bool IsFormsSubmitted { get; set; }
    public bool IsClassAssigned { get; set; }
    public bool IsEnrollmentComplete { get; set; }

    /// <summary>Who recorded the acceptance: the family, or staff taking their word.</summary>
    public long? OfferAcceptedByUserId { get; set; }

    /// <summary>Set when the family turned the place down, with their reason.</summary>
    public DateTime? OfferDeclinedDate { get; set; }
    public string OfferDeclineReason { get; set; }

    /// <summary>The deadline the family is working to, and whether it has passed.</summary>
    public DateTime? OfferExpiryDate { get; set; }
    public bool IsOfferExpired { get; set; }

    // Dates
    public DateTime? OfferAcceptedDate { get; set; }
    public DateTime? FormsSubmittedDate { get; set; }
    public DateTime? EnrollmentCompletedDate { get; set; }

    // Created Student
    public Guid? CreatedStudentId { get; set; }
    public string AdmissionNumber { get; set; }

    // LC-07: the student's freshly-provisioned login. Populated ONLY in the
    // CompleteEnrollment response so the officer can hand the credentials over;
    // never returned on reads (the password isn't stored in plaintext).
    public string LoginUserName { get; set; }
    public string TemporaryPassword { get; set; }

    /// <summary>
    /// #437. How many parents or guardians enrolment linked to the new learner,
    /// from the application. Zero means nobody can be contacted about them yet
    /// and no report card can be shown to anyone, so the officer is warned.
    /// Only on the CompleteEnrollment response.
    /// </summary>
    public int? GuardiansLinked { get; set; }
}
