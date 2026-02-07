using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.MedicalInfos.Dto;

public class CreateUpdateMedicalInfoDto
{
    [Required]
    public Guid StudentId { get; set; }

    [StringLength(10)]
    public string BloodType { get; set; }

    [StringLength(500)]
    public string Allergies { get; set; }

    [StringLength(500)]
    public string ChronicConditions { get; set; }

    [StringLength(500)]
    public string CurrentMedications { get; set; }

    [StringLength(500)]
    public string DietaryRequirements { get; set; }

    [StringLength(500)]
    public string SpecialNeeds { get; set; }

    [StringLength(200)]
    public string DoctorName { get; set; }

    [StringLength(20)]
    public string DoctorPhone { get; set; }

    [StringLength(100)]
    public string MedicalAidProvider { get; set; }

    [StringLength(50)]
    public string MedicalAidNumber { get; set; }

    [StringLength(100)]
    public string MedicalAidPlan { get; set; }

    [StringLength(200)]
    public string MedicalAidMainMember { get; set; }

    [StringLength(100)]
    public string ImmunizationStatus { get; set; }

    public DateTime? LastPhysicalExamDate { get; set; }

    public bool CanReceiveOTCMedication { get; set; }

    [StringLength(2000)]
    public string AdditionalNotes { get; set; }
}
