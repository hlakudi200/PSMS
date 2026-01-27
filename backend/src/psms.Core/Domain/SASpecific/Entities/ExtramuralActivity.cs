using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Shared.Enums;

namespace psms.Domain.SASpecific.Entities
{
    /// <summary>
    /// Represents an extramural/extra-curricular activity
    /// </summary>
    [Table("ExtramuralActivities")]
    public class ExtramuralActivity : FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
    {
        public const int MaxActivityNameLength = 200;
        public const int MaxDescriptionLength = 2000;
        public const int MaxVenueLength = 200;
        public const int MaxCoachNameLength = 100;
        public const int MaxPhoneLength = 20;
        public const int MaxRequirementsLength = 1000;

        /// <summary>
        /// Tenant identifier for multi-tenancy
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Reference to the academic year
        /// </summary>
        [Required]
        public Guid AcademicYearId { get; set; }

        /// <summary>
        /// Name of the activity
        /// </summary>
        [Required]
        [StringLength(MaxActivityNameLength)]
        public string ActivityName { get; set; }

        /// <summary>
        /// Description of the activity
        /// </summary>
        [StringLength(MaxDescriptionLength)]
        public string Description { get; set; }

        /// <summary>
        /// Category of activity
        /// </summary>
        [Required]
        public ExtramuralCategory Category { get; set; }

        /// <summary>
        /// Type of activity
        /// </summary>
        [Required]
        public ExtramuralType ActivityType { get; set; }

        /// <summary>
        /// Venue where activity takes place
        /// </summary>
        [StringLength(MaxVenueLength)]
        public string Venue { get; set; }

        /// <summary>
        /// Day of week
        /// </summary>
        public DayOfWeek? DayOfWeek { get; set; }

        /// <summary>
        /// Start time
        /// </summary>
        public TimeSpan? StartTime { get; set; }

        /// <summary>
        /// End time
        /// </summary>
        public TimeSpan? EndTime { get; set; }

        /// <summary>
        /// Season (for sports)
        /// </summary>
        public SchoolSeason? Season { get; set; }

        /// <summary>
        /// Term the activity runs in (null for year-round)
        /// </summary>
        public int? TermNumber { get; set; }

        /// <summary>
        /// Minimum grade level
        /// </summary>
        public int? MinGrade { get; set; }

        /// <summary>
        /// Maximum grade level
        /// </summary>
        public int? MaxGrade { get; set; }

        /// <summary>
        /// Gender restriction (if any)
        /// </summary>
        public Gender? GenderRestriction { get; set; }

        /// <summary>
        /// Coach/instructor name
        /// </summary>
        [StringLength(MaxCoachNameLength)]
        public string CoachName { get; set; }

        /// <summary>
        /// Coach's phone number
        /// </summary>
        [StringLength(MaxPhoneLength)]
        public string CoachPhone { get; set; }

        /// <summary>
        /// Maximum capacity
        /// </summary>
        public int? MaxCapacity { get; set; }

        /// <summary>
        /// Current enrollment
        /// </summary>
        public int CurrentEnrollment { get; set; }

        /// <summary>
        /// Equipment/requirements needed
        /// </summary>
        [StringLength(MaxRequirementsLength)]
        public string Requirements { get; set; }

        /// <summary>
        /// Fee per term (ZAR)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal FeePerTerm { get; set; }

        /// <summary>
        /// Whether the activity is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Whether registration is open
        /// </summary>
        public bool IsRegistrationOpen { get; set; }

        /// <summary>
        /// Soft delete flag
        /// </summary>
        public bool IsDeleted { get; set; }

        // Collections
        public virtual ICollection<StudentExtramural> StudentEnrollments { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected ExtramuralActivity()
        {
            StudentEnrollments = new HashSet<StudentExtramural>();
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public ExtramuralActivity(
            Guid id,
            int? tenantId,
            Guid academicYearId,
            string activityName,
            ExtramuralCategory category,
            ExtramuralType activityType) : this()
        {
            Id = id;
            TenantId = tenantId;
            AcademicYearId = academicYearId;
            ActivityName = activityName;
            Category = category;
            ActivityType = activityType;
            CurrentEnrollment = 0;
            IsActive = true;
            IsRegistrationOpen = true;
            IsDeleted = false;
        }

        /// <summary>
        /// Checks if the activity has capacity
        /// </summary>
        public bool HasCapacity()
        {
            return !MaxCapacity.HasValue || CurrentEnrollment < MaxCapacity.Value;
        }

        /// <summary>
        /// Enrolls a student
        /// </summary>
        public void EnrollStudent()
        {
            if (!HasCapacity())
                throw new InvalidOperationException("Activity is at full capacity.");

            CurrentEnrollment++;
        }

        /// <summary>
        /// Removes a student
        /// </summary>
        public void RemoveStudent()
        {
            if (CurrentEnrollment > 0)
                CurrentEnrollment--;
        }

        /// <summary>
        /// Opens registration
        /// </summary>
        public void OpenRegistration()
        {
            IsRegistrationOpen = true;
        }

        /// <summary>
        /// Closes registration
        /// </summary>
        public void CloseRegistration()
        {
            IsRegistrationOpen = false;
        }

        /// <summary>
        /// Checks if a student's grade is eligible
        /// </summary>
        public bool IsGradeEligible(int gradeLevel)
        {
            if (MinGrade.HasValue && gradeLevel < MinGrade.Value)
                return false;
            if (MaxGrade.HasValue && gradeLevel > MaxGrade.Value)
                return false;
            return true;
        }
    }
}
