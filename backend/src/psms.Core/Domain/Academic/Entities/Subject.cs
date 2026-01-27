using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace psms.Domain.Academic.Entities
{
    /// <summary>
    /// Represents an academic subject
    /// </summary>
    [Table("Subjects")]
    public class Subject : FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
    {
        public const int MaxSubjectNameLength = 100;
        public const int MaxSubjectCodeLength = 20;
        public const int MaxDescriptionLength = 500;

        /// <summary>
        /// Tenant identifier for multi-tenancy
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Subject name (e.g., "Mathematics", "English")
        /// </summary>
        [Required]
        [StringLength(MaxSubjectNameLength)]
        public string SubjectName { get; set; }

        /// <summary>
        /// Subject code (e.g., "MATH", "ENG")
        /// </summary>
        [Required]
        [StringLength(MaxSubjectCodeLength)]
        public string SubjectCode { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [StringLength(MaxDescriptionLength)]
        public string Description { get; set; }

        /// <summary>
        /// Indicates if subject is core (required)
        /// </summary>
        public bool IsCore { get; set; }

        /// <summary>
        /// Indicates if subject is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Soft delete flag
        /// </summary>
        public bool IsDeleted { get; set; }

        // Navigation Properties
        public virtual ICollection<GradeSubject> GradeSubjects { get; set; }
        public virtual ICollection<TeacherSubject> TeacherSubjects { get; set; }
        public virtual ICollection<StudentSubject> StudentSubjects { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected Subject()
        {
            GradeSubjects = new HashSet<GradeSubject>();
            TeacherSubjects = new HashSet<TeacherSubject>();
            StudentSubjects = new HashSet<StudentSubject>();
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public Subject(
            Guid id,
            int? tenantId,
            string subjectName,
            string subjectCode,
            bool isCore) : this()
        {
            Id = id;
            TenantId = tenantId;
            SubjectName = subjectName;
            SubjectCode = subjectCode;
            IsCore = isCore;
            IsActive = true;
            IsDeleted = false;
        }
    }
}
