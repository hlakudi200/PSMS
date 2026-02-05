using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Admissions.Enrollments.Dto;

/// <summary>
/// DTO for completing enrollment and creating student record.
/// Implements ADM-026, ADM-027.
/// </summary>
public class CompleteEnrollmentDto
{
    /// <summary>
    /// Application ID.
    /// </summary>
    [Required]
    public Guid ApplicationId { get; set; }

    /// <summary>
    /// Class ID to assign the student to.
    /// </summary>
    [Required]
    public Guid ClassId { get; set; }

    /// <summary>
    /// Optional custom admission number (auto-generated if not provided).
    /// </summary>
    [StringLength(50)]
    public string AdmissionNumber { get; set; }

    /// <summary>
    /// Confirmation that all enrollment forms are submitted.
    /// </summary>
    public bool FormsSubmitted { get; set; }

    /// <summary>
    /// Emergency contact name.
    /// </summary>
    [StringLength(100)]
    public string EmergencyContactName { get; set; }

    /// <summary>
    /// Emergency contact phone.
    /// </summary>
    [StringLength(20)]
    public string EmergencyContactPhone { get; set; }

    /// <summary>
    /// Medical conditions or allergies.
    /// </summary>
    [StringLength(2000)]
    public string MedicalConditions { get; set; }

    /// <summary>
    /// POPIA consent confirmation.
    /// </summary>
    public bool POPIAConsentGiven { get; set; }
}
