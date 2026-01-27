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
    /// Represents a student's enrollment in after-care
    /// </summary>
    [Table("StudentAfterCares")]
    public class StudentAfterCare : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public const int MaxNotesLength = 1000;
        public const int MaxDietaryRequirementsLength = 500;
        public const int MaxMedicalNotesLength = 1000;
        public const int MaxAuthorizedPickupLength = 2000;

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
        /// Reference to the after-care program
        /// </summary>
        [Required]
        public Guid AfterCareId { get; set; }

        /// <summary>
        /// Reference to the academic year
        /// </summary>
        [Required]
        public Guid AcademicYearId { get; set; }

        /// <summary>
        /// Days enrolled (JSON array: ["Monday", "Tuesday", etc.])
        /// </summary>
        public string DaysEnrolled { get; set; }

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
        /// Dietary requirements
        /// </summary>
        [StringLength(MaxDietaryRequirementsLength)]
        public string DietaryRequirements { get; set; }

        /// <summary>
        /// Medical notes
        /// </summary>
        [StringLength(MaxMedicalNotesLength)]
        public string MedicalNotes { get; set; }

        /// <summary>
        /// Authorized persons for pickup (JSON array)
        /// </summary>
        [StringLength(MaxAuthorizedPickupLength)]
        public string AuthorizedPickupPersons { get; set; }

        /// <summary>
        /// Usual pickup time
        /// </summary>
        public TimeSpan? UsualPickupTime { get; set; }

        /// <summary>
        /// Additional notes
        /// </summary>
        [StringLength(MaxNotesLength)]
        public string Notes { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(StudentId))]
        public virtual Student Student { get; set; }

        [ForeignKey(nameof(AfterCareId))]
        public virtual AfterCare AfterCare { get; set; }

        [ForeignKey(nameof(AcademicYearId))]
        public virtual AcademicYear AcademicYear { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected StudentAfterCare()
        {
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public StudentAfterCare(
            Guid id,
            int? tenantId,
            Guid studentId,
            Guid afterCareId,
            Guid academicYearId,
            DateTime startDate) : this()
        {
            Id = id;
            TenantId = tenantId;
            StudentId = studentId;
            AfterCareId = afterCareId;
            AcademicYearId = academicYearId;
            StartDate = startDate;
            Status = EnrollmentStatus.Active;
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
    }
}
