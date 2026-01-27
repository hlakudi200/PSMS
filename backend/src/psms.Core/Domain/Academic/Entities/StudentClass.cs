using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace psms.Domain.Academic.Entities
{
    /// <summary>
    /// Represents a student's enrollment in a class for a specific academic year
    /// </summary>
    [Table("StudentClasses")]
    public class StudentClass : FullAuditedEntity<Guid>, IMayHaveTenant
    {
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
        /// Reference to the class
        /// </summary>
        [Required]
        public Guid ClassId { get; set; }

        /// <summary>
        /// Reference to the academic year
        /// </summary>
        [Required]
        public Guid AcademicYearId { get; set; }

        /// <summary>
        /// Date when student was enrolled in this class
        /// </summary>
        [Required]
        public DateTime EnrollmentDate { get; set; }

        /// <summary>
        /// Date when student left this class (if applicable)
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Whether this is the student's current class
        /// </summary>
        public bool IsCurrent { get; set; }

        /// <summary>
        /// Whether the enrollment is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        [ForeignKey(nameof(StudentId))]
        public virtual Student Student { get; set; }

        [ForeignKey(nameof(ClassId))]
        public virtual Class Class { get; set; }

        [ForeignKey(nameof(AcademicYearId))]
        public virtual AcademicYear AcademicYear { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected StudentClass()
        {
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public StudentClass(
            Guid id,
            int? tenantId,
            Guid studentId,
            Guid classId,
            Guid academicYearId,
            DateTime enrollmentDate) : this()
        {
            Id = id;
            TenantId = tenantId;
            StudentId = studentId;
            ClassId = classId;
            AcademicYearId = academicYearId;
            EnrollmentDate = enrollmentDate;
            IsCurrent = true;
            IsActive = true;
        }

        /// <summary>
        /// Ends the enrollment in this class
        /// </summary>
        public void EndEnrollment(DateTime endDate)
        {
            EndDate = endDate;
            IsCurrent = false;
            IsActive = false;
        }

        /// <summary>
        /// Marks this as the current class
        /// </summary>
        public void SetAsCurrent()
        {
            IsCurrent = true;
            IsActive = true;
        }
    }
}
