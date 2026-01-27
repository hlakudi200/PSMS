using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace psms.Domain.Academic.Entities
{
    /// <summary>
    /// Represents medical information for a student
    /// </summary>
    [Table("MedicalInfos")]
    public class MedicalInfo : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public const int MaxConditionLength = 500;
        public const int MaxDoctorNameLength = 200;
        public const int MaxPhoneLength = 20;
        public const int MaxMedicalAidLength = 100;
        public const int MaxMemberNumberLength = 50;
        public const int MaxNotesLength = 2000;

        /// <summary>
        /// Tenant identifier for multi-tenancy
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Reference to the student
        /// </summary>
        [Required]
        public Guid StudentId { get; set; }

        /// <summary>
        /// Blood type of the student
        /// </summary>
        [StringLength(10)]
        public string BloodType { get; set; }

        /// <summary>
        /// Known allergies
        /// </summary>
        [StringLength(MaxConditionLength)]
        public string Allergies { get; set; }

        /// <summary>
        /// Chronic conditions
        /// </summary>
        [StringLength(MaxConditionLength)]
        public string ChronicConditions { get; set; }

        /// <summary>
        /// Current medications
        /// </summary>
        [StringLength(MaxConditionLength)]
        public string CurrentMedications { get; set; }

        /// <summary>
        /// Special dietary requirements
        /// </summary>
        [StringLength(MaxConditionLength)]
        public string DietaryRequirements { get; set; }

        /// <summary>
        /// Physical disabilities or special needs
        /// </summary>
        [StringLength(MaxConditionLength)]
        public string SpecialNeeds { get; set; }

        /// <summary>
        /// Family doctor name
        /// </summary>
        [StringLength(MaxDoctorNameLength)]
        public string DoctorName { get; set; }

        /// <summary>
        /// Doctor's phone number
        /// </summary>
        [StringLength(MaxPhoneLength)]
        public string DoctorPhone { get; set; }

        /// <summary>
        /// Medical aid provider name
        /// </summary>
        [StringLength(MaxMedicalAidLength)]
        public string MedicalAidProvider { get; set; }

        /// <summary>
        /// Medical aid membership number
        /// </summary>
        [StringLength(MaxMemberNumberLength)]
        public string MedicalAidNumber { get; set; }

        /// <summary>
        /// Medical aid plan type
        /// </summary>
        [StringLength(MaxMedicalAidLength)]
        public string MedicalAidPlan { get; set; }

        /// <summary>
        /// Main member name on medical aid
        /// </summary>
        [StringLength(MaxDoctorNameLength)]
        public string MedicalAidMainMember { get; set; }

        /// <summary>
        /// Immunization status (e.g., "Up to date", "Incomplete")
        /// </summary>
        [StringLength(100)]
        public string ImmunizationStatus { get; set; }

        /// <summary>
        /// Date of last physical examination
        /// </summary>
        public DateTime? LastPhysicalExamDate { get; set; }

        /// <summary>
        /// Whether the student has permission to receive over-the-counter medication
        /// </summary>
        public bool CanReceiveOTCMedication { get; set; }

        /// <summary>
        /// Additional medical notes
        /// </summary>
        [StringLength(MaxNotesLength)]
        public string AdditionalNotes { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(StudentId))]
        public virtual Student Student { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected MedicalInfo()
        {
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public MedicalInfo(
            Guid id,
            int? tenantId,
            Guid studentId) : this()
        {
            Id = id;
            TenantId = tenantId;
            StudentId = studentId;
        }
    }
}
