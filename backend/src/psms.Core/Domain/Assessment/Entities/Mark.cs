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
        /// RC-23. Why this task was not required of this learner.
        /// <para>
        /// National Protocol §8(9): a learner who cannot offer the Physical
        /// Education Task "may be exempted … provided a valid medical reason is
        /// submitted", and their Life Orientation marks are then "recalculated
        /// in terms of four tasks". An exempted task is left out of the average
        /// rather than scored zero, which is what makes that recalculation
        /// happen.
        /// </para>
        /// <para>
        /// This is the school's note that an exemption was granted and on what
        /// basis — a reference to the documentation it holds. It is <b>not</b> a
        /// place to transcribe a learner's medical detail: that is special
        /// personal information under POPIA §26, and the school's own file is
        /// where it belongs. The field is deliberately kept off learner- and
        /// parent-facing payloads for the same reason.
        /// </para>
        /// </summary>
        public string ExemptionReason { get; set; }

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
        /// JSON log of post-publish feedback edits (TF-005): an array of
        /// { at, byUserId, previous } entries. Written by the application
        /// service so the edit history can be shown to the teacher.
        /// </summary>
        public string FeedbackHistory { get; set; }

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
            AchievementLevel = CapsAchievementScale.LevelFor(Percentage.Value);
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
        /// RC-23. Excuses this learner from this task, on a stated basis.
        /// <para>
        /// National Protocol §8(9). An exempted task carries no mark and is left
        /// out of the subject average — which is how a Life Orientation mark
        /// comes to be "recalculated in terms of four tasks" rather than five.
        /// Scoring it zero would punish the learner for an absence the policy
        /// excuses.
        /// </para>
        /// <para>
        /// <paramref name="reason"/> is required: §8(9) grants the exemption
        /// "provided a valid medical reason is submitted", so an exemption with
        /// nothing recorded against it is not one the policy describes.
        /// </para>
        /// <para>
        /// RawMark is left as it was. It is the audit record of what was
        /// actually recorded, the same way moderation leaves it alone — the
        /// percentage and the level are what stop counting.
        /// </para>
        /// </summary>
        public void Exempt(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException(
                    "An exemption has to say what it was granted on.", nameof(reason));

            ExemptionReason = reason.Trim();
            Status = MarkStatus.Exempted;
            WasAbsent = false;
            Percentage = null;
            AchievementLevel = null;
        }

        /// <summary>
        /// RC-23. Withdraws an exemption, putting the task back in the learner's
        /// programme with no mark yet recorded.
        /// </summary>
        public void ClearExemption()
        {
            if (Status != MarkStatus.Exempted) return;

            ExemptionReason = null;
            Status = MarkStatus.Pending;
        }

        /// <summary>
        /// Applies a moderation adjustment on top of the recorded raw mark
        /// (e.g. a moderator's review credits or deducts marks after the
        /// original submission). RawMark itself is left untouched — it's the
        /// audit record of what was actually recorded — while Percentage/
        /// AchievementLevel are recalculated from the adjusted effective
        /// mark, clamped to the assessment's valid range.
        /// </summary>
        public void ApplyModeration(decimal adjustment, decimal maxMarks)
        {
            ModerationAdjustment = adjustment;
            IsModerated = true;

            if (RawMark.HasValue)
            {
                var effectiveMark = RawMark.Value + adjustment;
                if (effectiveMark < 0) effectiveMark = 0;
                if (maxMarks > 0 && effectiveMark > maxMarks) effectiveMark = maxMarks;

                Percentage = maxMarks > 0 ? (effectiveMark / maxMarks) * 100 : Percentage;
                if (Percentage.HasValue)
                    AchievementLevel = CapsAchievementScale.LevelFor(Percentage.Value);
            }
        }

    }
}
