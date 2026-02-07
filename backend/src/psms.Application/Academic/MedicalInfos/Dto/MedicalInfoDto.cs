using Abp.Application.Services.Dto;
using System;

namespace psms.Academic.MedicalInfos.Dto;

public class MedicalInfoDto : FullAuditedEntityDto<Guid>
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; }
    public string BloodType { get; set; }
    public string Allergies { get; set; }
    public string ChronicConditions { get; set; }
    public string CurrentMedications { get; set; }
    public string DietaryRequirements { get; set; }
    public string SpecialNeeds { get; set; }
    public string DoctorName { get; set; }
    public string DoctorPhone { get; set; }
    public string MedicalAidProvider { get; set; }
    public string MedicalAidNumber { get; set; }
    public string MedicalAidPlan { get; set; }
    public string MedicalAidMainMember { get; set; }
    public string ImmunizationStatus { get; set; }
    public DateTime? LastPhysicalExamDate { get; set; }
    public bool CanReceiveOTCMedication { get; set; }
    public string AdditionalNotes { get; set; }
}
