using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace psms.Domain.Academic.Entities
{
    /// <summary>
    /// Represents the association between a class and a subject, including the assigned teacher
    /// </summary>
    [Table("ClassSubjects")]
    public class ClassSubject : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        /// <summary>
        /// Tenant identifier for multi-tenancy
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Reference to the class
        /// </summary>
        [Required]
        public Guid ClassId { get; set; }

        /// <summary>
        /// Reference to the subject
        /// </summary>
        [Required]
        public Guid SubjectId { get; set; }

        /// <summary>
        /// Reference to the teacher assigned to teach this subject in this class
        /// </summary>
        public Guid? TeacherId { get; set; }

        /// <summary>
        /// Number of periods per week for this subject
        /// </summary>
        public int PeriodsPerWeek { get; set; }

        /// <summary>
        /// Whether this subject is active for the class
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        [ForeignKey(nameof(ClassId))]
        public virtual Class Class { get; set; }

        [ForeignKey(nameof(SubjectId))]
        public virtual Subject Subject { get; set; }

        [ForeignKey(nameof(TeacherId))]
        public virtual Teacher Teacher { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected ClassSubject()
        {
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public ClassSubject(
            Guid id,
            int? tenantId,
            Guid classId,
            Guid subjectId,
            Guid? teacherId = null) : this()
        {
            Id = id;
            TenantId = tenantId;
            ClassId = classId;
            SubjectId = subjectId;
            TeacherId = teacherId;
            IsActive = true;
        }

        /// <summary>
        /// Assigns a teacher to this class subject
        /// </summary>
        public void AssignTeacher(Guid teacherId)
        {
            TeacherId = teacherId;
        }

        /// <summary>
        /// Removes the teacher assignment
        /// </summary>
        public void RemoveTeacher()
        {
            TeacherId = null;
        }

        /// <summary>
        /// Deactivates the class subject
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
        }

        /// <summary>
        /// Activates the class subject
        /// </summary>
        public void Activate()
        {
            IsActive = true;
        }
    }
}
