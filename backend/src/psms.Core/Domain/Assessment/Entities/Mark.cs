using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Academic.Entities;
using psms.Domain.Shared.Enums;

namespace psms.Domain.Assessment.Entities
{
    /// <summary>
    /// Represents a student's mark for an assessment
    /// </summary>
    [Table("Marks")]
    public class Mark : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public const int MaxCommentLength = 1000;
        public const int MaxFeedbackLength = 2000;

        /// <summary>
        /// Tenant identifier for multi-tenancy
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Reference to the assessment
        /// </summary>
        [Required]
        public Guid AssessmentId { get; set; }

        /// <summary>
        /// Reference to the student
        /// </summary>
        [Required]
        public Guid StudentId { get; set; }

        /// <summary>
        /// Raw mark achieved
        /// </summary>
        [Column(TypeName = "decimal(8,2)")]
        public decimal? RawMark { get; set; }

        /// <summary>
        /// Percentage achieved
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? Percentage { get; set; }

        /// <summary>
        /// Achievement level (CAPS 1-7 scale)
        /// </summary>
        public CapsAchievementLevel? AchievementLevel { get; set; }

        /// <summary>
        /// Status of the mark
        /// </summary>
        [Required]
        public MarkStatus Status { get; set; }

        /// <summary>
        /// Whether the student was absent
        /// </summary>
        public bool WasAbsent { get; set; }

        /// <summary>
        /// Whether this is a re-assessment mark
        /// </summary>
        public bool IsReassessment { get; set; }

        /// <summary>
        /// Teacher's comment on the mark
        /// </summary>
        [StringLength(MaxCommentLength)]
        public string TeacherComment { get; set; }

        /// <summary>
        /// Detailed feedback for the student
        /// </summary>
        [StringLength(MaxFeedbackLength)]
        public string Feedback { get; set; }

        /// <summary>
        /// Date when the mark was recorded
        /// </summary>
        public DateTime? MarkedDate { get; set; }

        /// <summary>
        /// Teacher who marked the assessment
        /// </summary>
        public long? MarkedByTeacherUserId { get; set; }

        /// <summary>
        /// Whether the mark has been moderated
        /// </summary>
        public bool IsModerated { get; set; }

        /// <summary>
        /// Moderation adjustment applied
        /// </summary>
        [Column(TypeName = "decimal(8,2)")]
        public decimal? ModerationAdjustment { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(AssessmentId))]
        public virtual Assessment Assessment { get; set; }

        [ForeignKey(nameof(StudentId))]
        public virtual Student Student { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected Mark()
        {
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public Mark(
            Guid id,
            int? tenantId,
            Guid assessmentId,
            Guid studentId) : this()
        {
            Id = id;
            TenantId = tenantId;
            AssessmentId = assessmentId;
            StudentId = studentId;
            Status = MarkStatus.Pending;
            WasAbsent = false;
            IsReassessment = false;
            IsModerated = false;
        }

        /// <summary>
        /// Records the mark for the student
        /// </summary>
        public void RecordMark(decimal rawMark, decimal maxMarks, long teacherUserId, string comment = null)
        {
            RawMark = rawMark;
            Percentage = maxMarks > 0 ? (rawMark / maxMarks) * 100 : 0;
            AchievementLevel = CalculateAchievementLevel(Percentage.Value);
            MarkedDate = DateTime.UtcNow;
            MarkedByTeacherUserId = teacherUserId;
            TeacherComment = comment;
            Status = MarkStatus.Completed;
        }

        /// <summary>
        /// Marks the student as absent
        /// </summary>
        public void MarkAsAbsent()
        {
            WasAbsent = true;
            Status = MarkStatus.Absent;
        }

        /// <summary>
        /// Applies moderation adjustment
        /// </summary>
        public void ApplyModeration(decimal adjustment)
        {
            ModerationAdjustment = adjustment;
            IsModerated = true;

            if (RawMark.HasValue)
            {
                // Recalculate percentage with moderation
                // Note: This is a simplified approach - actual moderation logic may vary
            }
        }

        /// <summary>
        /// Calculates CAPS achievement level from percentage
        /// </summary>
        private CapsAchievementLevel CalculateAchievementLevel(decimal percentage)
        {
            if (percentage >= 80) return CapsAchievementLevel.Level7;
            if (percentage >= 70) return CapsAchievementLevel.Level6;
            if (percentage >= 60) return CapsAchievementLevel.Level5;
            if (percentage >= 50) return CapsAchievementLevel.Level4;
            if (percentage >= 40) return CapsAchievementLevel.Level3;
            if (percentage >= 30) return CapsAchievementLevel.Level2;
            return CapsAchievementLevel.Level1;
        }
    }
}
