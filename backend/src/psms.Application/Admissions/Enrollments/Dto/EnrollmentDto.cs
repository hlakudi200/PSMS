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

    // Dates
    public DateTime? OfferAcceptedDate { get; set; }
    public DateTime? FormsSubmittedDate { get; set; }
    public DateTime? EnrollmentCompletedDate { get; set; }

    // Created Student
    public Guid? CreatedStudentId { get; set; }
    public string AdmissionNumber { get; set; }
}
