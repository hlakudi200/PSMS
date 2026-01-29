using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Academic.Entities;

namespace psms.Domain.Admissions.Entities
{
    /// <summary>
    /// Represents admission settings/configuration per grade and academic year
    /// </summary>
    [Table("AdmissionSettings")]
    public class AdmissionSettings : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public const int MaxRequiredDocumentsLength = 1000;
        public const int MaxNotesLength = 2000;

        /// <summary>
        /// Tenant identifier for multi-tenancy
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Academic year these settings apply to
        /// </summary>
        [Required]
        public Guid AcademicYearId { get; set; }

        /// <summary>
        /// Grade these settings apply to (null = all grades)
        /// </summary>
        public Guid? GradeId { get; set; }

        /// <summary>
        /// Application fee amount in ZAR
        /// </summary>
        [Required]
        public decimal ApplicationFeeAmount { get; set; }

        /// <summary>
        /// Maximum capacity for the grade (for waitlist logic)
        /// </summary>
        public int? MaxCapacity { get; set; }

        /// <summary>
        /// Current enrolled count (updated when students enroll)
        /// </summary>
        public int CurrentEnrolledCount { get; set; }

        /// <summary>
        /// Date when applications open
        /// </summary>
        public DateTime? ApplicationOpenDate { get; set; }

        /// <summary>
        /// Date when applications close
        /// </summary>
        public DateTime? ApplicationCloseDate { get; set; }

        /// <summary>
        /// Whether applications are currently being accepted
        /// </summary>
        public bool IsAcceptingApplications { get; set; }

        /// <summary>
        /// JSON array of required document categories (e.g., ["BirthCertificate", "PreviousSchoolReport"])
        /// </summary>
        [StringLength(MaxRequiredDocumentsLength)]
        public string RequiredDocuments { get; set; }

        /// <summary>
        /// Whether interview is required for this grade
        /// </summary>
        public bool IsInterviewRequired { get; set; }

        /// <summary>
        /// Whether placement assessment is required for this grade
        /// </summary>
        public bool IsAssessmentRequired { get; set; }

        /// <summary>
        /// Minimum age in years for admission (calculated from DOB)
        /// </summary>
        public int? MinimumAge { get; set; }

        /// <summary>
        /// Maximum age in years for admission (calculated from DOB)
        /// </summary>
        public int? MaximumAge { get; set; }

        /// <summary>
        /// Number of days before approved application offer expires
        /// </summary>
        public int OfferExpiryDays { get; set; } = 14;

        /// <summary>
        /// Additional notes or instructions for applicants
        /// </summary>
        [StringLength(MaxNotesLength)]
        public string Notes { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(AcademicYearId))]
        public virtual AcademicYear AcademicYear { get; set; }

        [ForeignKey(nameof(GradeId))]
        public virtual Grade Grade { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected AdmissionSettings()
        {
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public AdmissionSettings(
            Guid id,
            int? tenantId,
            Guid academicYearId,
            decimal applicationFeeAmount) : this()
        {
            Id = id;
            TenantId = tenantId;
            AcademicYearId = academicYearId;
            ApplicationFeeAmount = applicationFeeAmount;
            IsAcceptingApplications = false;
            CurrentEnrolledCount = 0;
            OfferExpiryDays = 14;
        }

        /// <summary>
        /// Checks if the grade has reached capacity
        /// </summary>
        public bool IsAtCapacity()
        {
            return MaxCapacity.HasValue && CurrentEnrolledCount >= MaxCapacity.Value;
        }

        /// <summary>
        /// Gets the number of available spots
        /// </summary>
        public int GetAvailableSpots()
        {
            if (!MaxCapacity.HasValue) return int.MaxValue;
            return Math.Max(0, MaxCapacity.Value - CurrentEnrolledCount);
        }

        /// <summary>
        /// Checks if applications are currently open based on dates
        /// </summary>
        public bool AreApplicationsOpen()
        {
            if (!IsAcceptingApplications) return false;

            var now = DateTime.UtcNow;

            if (ApplicationOpenDate.HasValue && now < ApplicationOpenDate.Value)
                return false;

            if (ApplicationCloseDate.HasValue && now > ApplicationCloseDate.Value)
                return false;

            return true;
        }

        /// <summary>
        /// Increments the enrolled count
        /// </summary>
        public void IncrementEnrolledCount()
        {
            CurrentEnrolledCount++;
        }

        /// <summary>
        /// Decrements the enrolled count (e.g., if student withdraws)
        /// </summary>
        public void DecrementEnrolledCount()
        {
            if (CurrentEnrolledCount > 0)
                CurrentEnrolledCount--;
        }
    }
}
