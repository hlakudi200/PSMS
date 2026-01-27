using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Academic.Entities;
using psms.Domain.Shared.Enums;

namespace psms.Domain.SASpecific.Entities
{
    /// <summary>
    /// Represents a student's enrollment in an extramural activity
    /// </summary>
    [Table("StudentExtramurals")]
    public class StudentExtramural : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public const int MaxNotesLength = 1000;
        public const int MaxAchievementsLength = 2000;
        public const int MaxMedicalNotesLength = 1000;
        public const int MaxEmergencyContactLength = 100;
        public const int MaxEmergencyPhoneLength = 20;

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
        /// Reference to the extramural activity
        /// </summary>
        [Required]
        public Guid ExtramuralActivityId { get; set; }

        /// <summary>
        /// Reference to the academic year
        /// </summary>
        [Required]
        public Guid AcademicYearId { get; set; }

        /// <summary>
        /// Term number (if termly enrollment)
        /// </summary>
        public int? TermNumber { get; set; }

        /// <summary>
        /// Status of enrollment
        /// </summary>
        [Required]
        public EnrollmentStatus Status { get; set; }

        /// <summary>
        /// Start date of enrollment
        /// </summary>
        [Required]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date of enrollment
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Team/group assignment (e.g., "A Team", "Junior Squad")
        /// </summary>
        public string TeamAssignment { get; set; }

        /// <summary>
        /// Position/role in the activity
        /// </summary>
        public string PositionRole { get; set; }

        /// <summary>
        /// Medical notes relevant to the activity
        /// </summary>
        [StringLength(MaxMedicalNotesLength)]
        public string MedicalNotes { get; set; }

        /// <summary>
        /// Emergency contact name
        /// </summary>
        [StringLength(MaxEmergencyContactLength)]
        public string EmergencyContactName { get; set; }

        /// <summary>
        /// Emergency contact phone
        /// </summary>
        [StringLength(MaxEmergencyPhoneLength)]
        public string EmergencyContactPhone { get; set; }

        /// <summary>
        /// Whether consent form is signed
        /// </summary>
        public bool ConsentFormSigned { get; set; }

        /// <summary>
        /// Date consent form was signed
        /// </summary>
        public DateTime? ConsentFormDate { get; set; }

        /// <summary>
        /// Achievements in this activity (JSON array)
        /// </summary>
        [StringLength(MaxAchievementsLength)]
        public string Achievements { get; set; }

        /// <summary>
        /// Attendance rate (percentage)
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? AttendanceRate { get; set; }

        /// <summary>
        /// Additional notes
        /// </summary>
        [StringLength(MaxNotesLength)]
        public string Notes { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(StudentId))]
        public virtual Student Student { get; set; }

        [ForeignKey(nameof(ExtramuralActivityId))]
        public virtual ExtramuralActivity ExtramuralActivity { get; set; }

        [ForeignKey(nameof(AcademicYearId))]
        public virtual AcademicYear AcademicYear { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected StudentExtramural()
        {
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public StudentExtramural(
            Guid id,
            int? tenantId,
            Guid studentId,
            Guid extramuralActivityId,
            Guid academicYearId,
            DateTime startDate) : this()
        {
            Id = id;
            TenantId = tenantId;
            StudentId = studentId;
            ExtramuralActivityId = extramuralActivityId;
            AcademicYearId = academicYearId;
            StartDate = startDate;
            Status = EnrollmentStatus.Active;
            ConsentFormSigned = false;
        }

        /// <summary>
        /// Records consent form signing
        /// </summary>
        public void SignConsentForm()
        {
            ConsentFormSigned = true;
            ConsentFormDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Suspends enrollment
        /// </summary>
        public void Suspend()
        {
            Status = EnrollmentStatus.Suspended;
        }

        /// <summary>
        /// Reactivates enrollment
        /// </summary>
        public void Reactivate()
        {
            Status = EnrollmentStatus.Active;
        }

        /// <summary>
        /// Terminates enrollment
        /// </summary>
        public void Terminate()
        {
            Status = EnrollmentStatus.Terminated;
            EndDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Records an achievement
        /// </summary>
        public void AddAchievement(string achievement)
        {
            // Append to achievements JSON - in real implementation, deserialize, add, serialize
            if (string.IsNullOrEmpty(Achievements))
                Achievements = $"[\"{achievement}\"]";
            else
                Achievements = Achievements.TrimEnd(']') + $",\"{achievement}\"]";
        }
    }
}
