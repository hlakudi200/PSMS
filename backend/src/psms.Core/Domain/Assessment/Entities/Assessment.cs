using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Academic.Entities;
using psms.Domain.Shared.Enums;

namespace psms.Domain.Assessment.Entities
{
    /// <summary>
    /// Represents an assessment (test, exam, assignment) for a subject
    /// </summary>
    [Table("Assessments")]
    public class Assessment : FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
    {
        public const int MaxNameLength = 200;
        public const int MaxDescriptionLength = 2000;
        public const int MaxInstructionsLength = 4000;

        /// <summary>
        /// Tenant identifier for multi-tenancy
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Reference to the class-subject
        /// </summary>
        [Required]
        public Guid ClassSubjectId { get; set; }

        /// <summary>
        /// Reference to the term
        /// </summary>
        [Required]
        public Guid TermId { get; set; }

        /// <summary>
        /// Name of the assessment
        /// </summary>
        [Required]
        [StringLength(MaxNameLength)]
        public string Name { get; set; }

        /// <summary>
        /// Description of the assessment
        /// </summary>
        [StringLength(MaxDescriptionLength)]
        public string Description { get; set; }

        /// <summary>
        /// Type of assessment
        /// </summary>
        [Required]
        public AssessmentType AssessmentType { get; set; }

        /// <summary>
        /// CAPS assessment category
        /// </summary>
        public CapsAssessmentCategory? CapsCategory { get; set; }

        /// <summary>
        /// Maximum marks for this assessment
        /// </summary>
        [Required]
        public decimal MaxMarks { get; set; }

        /// <summary>
        /// Weight of this assessment in the term (percentage)
        /// </summary>
        public decimal Weight { get; set; }

        /// <summary>
        /// Pass mark percentage
        /// </summary>
        public decimal PassPercentage { get; set; } = 50;

        /// <summary>
        /// Date the assessment is scheduled
        /// </summary>
        public DateTime? ScheduledDate { get; set; }

        /// <summary>
        /// Due date for submission (assignments)
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Duration in minutes (for exams/tests)
        /// </summary>
        public int? DurationMinutes { get; set; }

        /// <summary>
        /// Instructions for the assessment
        /// </summary>
        [StringLength(MaxInstructionsLength)]
        public string Instructions { get; set; }

        /// <summary>
        /// Whether the assessment is published and visible to students
        /// </summary>
        public bool IsPublished { get; set; }

        /// <summary>
        /// Whether marks have been released
        /// </summary>
        public bool MarksReleased { get; set; }

        /// <summary>
        /// Teacher who created the assessment
        /// </summary>
        [Required]
        public long CreatedByTeacherUserId { get; set; }

        /// <summary>
        /// Soft delete flag
        /// </summary>
        public bool IsDeleted { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(ClassSubjectId))]
        public virtual ClassSubject ClassSubject { get; set; }

        [ForeignKey(nameof(TermId))]
        public virtual Term Term { get; set; }

        // Collections
        public virtual ICollection<Mark> Marks { get; set; }
        public virtual ICollection<AssessmentQuestion> Questions { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected Assessment()
        {
            Marks = new HashSet<Mark>();
            Questions = new HashSet<AssessmentQuestion>();
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public Assessment(
            Guid id,
            int? tenantId,
            Guid classSubjectId,
            Guid termId,
            string name,
            AssessmentType assessmentType,
            decimal maxMarks,
            long createdByTeacherUserId) : this()
        {
            Id = id;
            TenantId = tenantId;
            ClassSubjectId = classSubjectId;
            TermId = termId;
            Name = name;
            AssessmentType = assessmentType;
            MaxMarks = maxMarks;
            CreatedByTeacherUserId = createdByTeacherUserId;
            IsPublished = false;
            MarksReleased = false;
            IsDeleted = false;
        }

        /// <summary>
        /// Publishes the assessment to students
        /// </summary>
        public void Publish()
        {
            IsPublished = true;
        }

        /// <summary>
        /// Unpublishes the assessment
        /// </summary>
        public void Unpublish()
        {
            if (MarksReleased)
                throw new InvalidOperationException("Cannot unpublish an assessment with released marks.");

            IsPublished = false;
        }

        /// <summary>
        /// Releases marks to students
        /// </summary>
        public void ReleaseMarks()
        {
            if (!IsPublished)
                throw new InvalidOperationException("Cannot release marks for an unpublished assessment.");

            MarksReleased = true;
        }

        /// <summary>
        /// Calculates percentage from raw mark
        /// </summary>
        public decimal CalculatePercentage(decimal rawMark)
        {
            return MaxMarks > 0 ? (rawMark / MaxMarks) * 100 : 0;
        }
    }
}
