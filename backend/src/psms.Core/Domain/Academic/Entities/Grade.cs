using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Shared.Enums;

namespace psms.Domain.Academic.Entities
{
    /// <summary>
    /// Represents a grade/year level with SA-specific fields
    /// </summary>
    [Table("Grades")]
    public class Grade : FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
    {
        public const int MaxGradeNameLength = 100;
        public const int MaxDescriptionLength = 500;

        /// <summary>
        /// Tenant identifier for multi-tenancy
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// South African grade level (GradeR, Grade1-12)
        /// </summary>
        [Required]
        public SouthAfricanGradeLevel GradeLevel { get; set; }

        /// <summary>
        /// Grade name (e.g., "Grade 1", "Grade R")
        /// </summary>
        [Required]
        [StringLength(MaxGradeNameLength)]
        public string GradeName { get; set; }

        /// <summary>
        /// South African school phase
        /// </summary>
        [Required]
        public SouthAfricanSchoolPhase SchoolPhase { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [StringLength(MaxDescriptionLength)]
        public string Description { get; set; }

        /// <summary>
        /// Indicates if grade is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Soft delete flag
        /// </summary>
        public bool IsDeleted { get; set; }

        // Navigation Properties
        public virtual ICollection<Class> Classes { get; set; }
        public virtual ICollection<GradeSubject> GradeSubjects { get; set; }
        public virtual ICollection<Student> Students { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected Grade()
        {
            Classes = new HashSet<Class>();
            GradeSubjects = new HashSet<GradeSubject>();
            Students = new HashSet<Student>();
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public Grade(
            Guid id,
            int? tenantId,
            SouthAfricanGradeLevel gradeLevel,
            string gradeName,
            SouthAfricanSchoolPhase schoolPhase) : this()
        {
            Id = id;
            TenantId = tenantId;
            GradeLevel = gradeLevel;
            GradeName = gradeName;
            SchoolPhase = schoolPhase;
            IsActive = true;
            IsDeleted = false;
        }
    }
}
