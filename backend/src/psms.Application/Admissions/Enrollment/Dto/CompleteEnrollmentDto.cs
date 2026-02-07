using System.ComponentModel.DataAnnotations;

namespace psms.Admissions.Enrollment.Dto;

/// <summary>
/// DTO for completing enrollment form.
/// </summary>
public class CompleteEnrollmentDto
{
    // Additional Student Details (not in application)
    [StringLength(100)]
    public string PreferredName { get; set; }

    [StringLength(50)]
    public string Religion { get; set; }

    [StringLength(100)]
    public string ChurchName { get; set; }

    // Transport
    public bool RequiresTransport { get; set; }

    [StringLength(200)]
    public string TransportPickupAddress { get; set; }

    // Aftercare
    public bool RequiresAftercare { get; set; }

    [StringLength(10)]
    public string AftercareEndTime { get; set; }

    // Medical Details
    [StringLength(100)]
    public string DoctorName { get; set; }

    [Phone]
    [StringLength(20)]
    public string DoctorPhone { get; set; }

    [StringLength(200)]
    public string MedicalAidName { get; set; }

    [StringLength(50)]
    public string MedicalAidNumber { get; set; }

    [StringLength(100)]
    public string MedicalAidMainMember { get; set; }

    // Dietary Requirements
    public bool HasDietaryRequirements { get; set; }

    [StringLength(500)]
    public string DietaryRequirements { get; set; }

    // Additional Emergency Contacts
    [StringLength(100)]
    public string EmergencyContact2Name { get; set; }

    [StringLength(50)]
    public string EmergencyContact2Relationship { get; set; }

    [Phone]
    [StringLength(20)]
    public string EmergencyContact2Phone { get; set; }

    [StringLength(100)]
    public string EmergencyContact3Name { get; set; }

    [StringLength(50)]
    public string EmergencyContact3Relationship { get; set; }

    [Phone]
    [StringLength(20)]
    public string EmergencyContact3Phone { get; set; }

    // Authorized Persons to Collect
    [StringLength(500)]
    public string AuthorizedToCollect { get; set; }

    // Sibling Information (for class assignment)
    public bool HasSiblingsAtSchool { get; set; }

    [StringLength(500)]
    public string SiblingDetails { get; set; }

    // Additional Notes
    [StringLength(1000)]
    public string AdditionalNotes { get; set; }
}
