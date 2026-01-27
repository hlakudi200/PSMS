using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using psms.Domain.Academic.Entities;
using psms.Domain.Shared.Enums;

namespace psms.Domain.Assessment.Entities
{
    /// <summary>
    /// Represents subject-level information in a student report
    /// </summary>
    [Table("ReportSubjects")]
    public class ReportSubject : Entity<Guid>
    {
        public const int MaxCommentLength = 1000;

        /// <summary>
        /// Reference to the report
        /// </summary>
        [Required]
        public Guid ReportId { get; set; }

        /// <summary>
        /// Reference to the subject
        /// </summary>
        [Required]
        public Guid SubjectId { get; set; }

        /// <summary>
        /// Reference to the teacher who taught this subject
        /// </summary>
        public Guid? TeacherId { get; set; }

        /// <summary>
        /// Term mark percentage
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? TermMark { get; set; }

        /// <summary>
        /// Exam mark percentage
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? ExamMark { get; set; }

        /// <summary>
        /// Final mark percentage (weighted average of term and exam)
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? FinalMark { get; set; }

        /// <summary>
        /// Achievement level (CAPS 1-7)
        /// </summary>
        public CapsAchievementLevel? AchievementLevel { get; set; }

        /// <summary>
        /// Subject teacher's comment
        /// </summary>
        [StringLength(MaxCommentLength)]
        public string TeacherComment { get; set; }

        /// <summary>
        /// Student's position in class for this subject
        /// </summary>
        public int? SubjectPosition { get; set; }

        /// <summary>
        /// Class average for this subject
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? ClassAverage { get; set; }

        /// <summary>
        /// Highest mark in class for this subject
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? HighestInClass { get; set; }

        /// <summary>
        /// Lowest mark in class for this subject
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? LowestInClass { get; set; }

        /// <summary>
        /// Weight of term work in final mark
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal TermWeight { get; set; } = 40;

        /// <summary>
        /// Weight of exam in final mark
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal ExamWeight { get; set; } = 60;

        // Navigation Properties
        [ForeignKey(nameof(ReportId))]
        public virtual Report Report { get; set; }

        [ForeignKey(nameof(SubjectId))]
        public virtual Subject Subject { get; set; }

        [ForeignKey(nameof(TeacherId))]
        public virtual Teacher Teacher { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected ReportSubject()
        {
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public ReportSubject(
            Guid id,
            Guid reportId,
            Guid subjectId) : this()
        {
            Id = id;
            ReportId = reportId;
            SubjectId = subjectId;
        }

        /// <summary>
        /// Records the marks for this subject
        /// </summary>
        public void RecordMarks(decimal? termMark, decimal? examMark, decimal termWeight = 40, decimal examWeight = 60)
        {
            TermMark = termMark;
            ExamMark = examMark;
            TermWeight = termWeight;
            ExamWeight = examWeight;

            // Calculate final mark
            if (termMark.HasValue && examMark.HasValue)
            {
                FinalMark = (termMark.Value * termWeight / 100) + (examMark.Value * examWeight / 100);
            }
            else if (termMark.HasValue)
            {
                FinalMark = termMark.Value;
            }
            else if (examMark.HasValue)
            {
                FinalMark = examMark.Value;
            }

            // Calculate achievement level
            if (FinalMark.HasValue)
            {
                AchievementLevel = CalculateAchievementLevel(FinalMark.Value);
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
