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
    /// Represents an after-care program
    /// </summary>
    [Table("AfterCares")]
    public class AfterCare : FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
    {
        public const int MaxProgramNameLength = 200;
        public const int MaxDescriptionLength = 2000;
        public const int MaxLocationLength = 200;
        public const int MaxSupervisorNameLength = 100;
        public const int MaxPhoneLength = 20;
        public const int MaxActivitiesLength = 2000;

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
        /// Name of the after-care program
        /// </summary>
        [Required]
        [StringLength(MaxProgramNameLength)]
        public string ProgramName { get; set; }

        /// <summary>
        /// Description of the program
        /// </summary>
        [StringLength(MaxDescriptionLength)]
        public string Description { get; set; }

        /// <summary>
        /// Type of after-care
        /// </summary>
        [Required]
        public AfterCareType AfterCareType { get; set; }

        /// <summary>
        /// Location where after-care is held
        /// </summary>
        [StringLength(MaxLocationLength)]
        public string Location { get; set; }

        /// <summary>
        /// Start time of after-care
        /// </summary>
        [Required]
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// End time of after-care
        /// </summary>
        [Required]
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// Days of week available (JSON array or flags)
        /// </summary>
        public string DaysAvailable { get; set; }

        /// <summary>
        /// Maximum capacity
        /// </summary>
        public int Capacity { get; set; }

        /// <summary>
        /// Current enrollment
        /// </summary>
        public int CurrentEnrollment { get; set; }

        /// <summary>
        /// Supervisor name
        /// </summary>
        [StringLength(MaxSupervisorNameLength)]
        public string SupervisorName { get; set; }

        /// <summary>
        /// Contact phone
        /// </summary>
        [StringLength(MaxPhoneLength)]
        public string ContactPhone { get; set; }

        /// <summary>
        /// Activities included (JSON array)
        /// </summary>
        [StringLength(MaxActivitiesLength)]
        public string ActivitiesIncluded { get; set; }

        /// <summary>
        /// Whether meals are provided
        /// </summary>
        public bool IncludesMeals { get; set; }

        /// <summary>
        /// Whether homework supervision is provided
        /// </summary>
        public bool IncludesHomeworkSupervision { get; set; }

        /// <summary>
        /// Monthly fee (ZAR)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyFee { get; set; }

        /// <summary>
        /// Whether the program is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Soft delete flag
        /// </summary>
        public bool IsDeleted { get; set; }

        // Collections
        public virtual ICollection<StudentAfterCare> StudentEnrollments { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected AfterCare()
        {
            StudentEnrollments = new HashSet<StudentAfterCare>();
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public AfterCare(
            Guid id,
            int? tenantId,
            Guid academicYearId,
            string programName,
            AfterCareType afterCareType,
            TimeSpan startTime,
            TimeSpan endTime,
            decimal monthlyFee) : this()
        {
            Id = id;
            TenantId = tenantId;
            AcademicYearId = academicYearId;
            ProgramName = programName;
            AfterCareType = afterCareType;
            StartTime = startTime;
            EndTime = endTime;
            MonthlyFee = monthlyFee;
            CurrentEnrollment = 0;
            IsActive = true;
            IsDeleted = false;
        }

        /// <summary>
        /// Checks if program has capacity
        /// </summary>
        public bool HasCapacity()
        {
            return Capacity == 0 || CurrentEnrollment < Capacity;
        }

        /// <summary>
        /// Enrolls a student
        /// </summary>
        public void EnrollStudent()
        {
            if (!HasCapacity())
                throw new InvalidOperationException("After-care program is at full capacity.");

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
    }
}
