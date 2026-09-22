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

        /// <summary>
        /// RC-15. The mark on this row is the School-Based Assessment component
        /// only, because the examination for this subject is set and marked
        /// outside the school and has not happened here — Grade 12's National
        /// Senior Certificate paper (NPPPPR §31(1)).
        /// <para>
        /// The card has to say so. A 25% SBA mark presented as a final mark
        /// reads as an NSC result and is not one.
        /// </para>
        /// </summary>
        public bool AwaitsExternalExamination { get; set; }

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
        /// Whether a term/examination split is one a final mark can be computed
        /// from: each side within 0-100, and the two adding to exactly 100.
        /// <para>
        /// RC-07: the DTO ranges each side independently, so 80/80 used to be
        /// accepted and produced a final mark of 160.
        /// </para>
        /// </summary>
        public static bool IsValidWeighting(decimal termWeight, decimal examWeight) =>
            termWeight >= 0 && termWeight <= 100 &&
            examWeight >= 0 && examWeight <= 100 &&
            termWeight + examWeight == 100;

        /// <summary>
        /// Records the marks for this subject.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// The weights do not add to 100, so no meaningful final mark exists.
        /// Callers reachable from the API validate first and raise a friendly
        /// error; this is the backstop for everything else.
        /// </exception>
        public void RecordMarks(decimal? termMark, decimal? examMark, decimal termWeight = 40, decimal examWeight = 60)
        {
            if (!IsValidWeighting(termWeight, examWeight))
                throw new ArgumentException(
                    $"Term and examination weights must each be 0-100 and add to 100; got {termWeight}/{examWeight}.",
                    nameof(termWeight));

            TermMark = termMark;
            ExamMark = examMark;
            TermWeight = termWeight;
            ExamWeight = examWeight;

            // Calculate final mark. Clearing both marks clears the final mark
            // too — it used to keep the previous one, so a mark entered by
            // mistake and then blanked stayed on the card.
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
            else
            {
                FinalMark = null;
            }

            // Rounded to what the column holds, so the value in memory is the
            // value in the database. Generation reads these straight back out of
            // memory while an edit re-reads them from Postgres; unrounded, the
            // same report could produce an overall a cent apart depending on
            // which path last touched it.
            if (FinalMark.HasValue)
                FinalMark = System.Math.Round(FinalMark.Value, 2, System.MidpointRounding.AwayFromZero);

            // Calculate achievement level
            AchievementLevel = CapsAchievementScale.LevelFor(FinalMark);
        }

        /// <summary>
        /// RC-05 / RC-15. Records an aggregated subject mark: the two
        /// components, the split actually applied to them, and whether the
        /// examination is still to come externally.
        /// </summary>
        /// <param name="result">What <see cref="SubjectMarkAggregator"/> worked out.</param>
        /// <param name="asPromotionMark">
        /// True for a year-end card, whose final mark is the promotion mark and
        /// is a whole number under NPPPPR §31(3). A term card keeps the two
        /// decimals it is reported to.
        /// </param>
        public void RecordAggregate(SubjectMarkResult result, bool asPromotionMark)
        {
            RecordMarks(
                result.SchoolBasedMark,
                result.ExaminationMark,
                result.AppliedSbaWeight,
                result.AppliedExamWeight);

            AwaitsExternalExamination = result.AwaitsExternalExamination;

            if (asPromotionMark && FinalMark.HasValue)
            {
                FinalMark = CapsRounding.PromotionMark(FinalMark.Value);
                AchievementLevel = CapsAchievementScale.LevelFor(FinalMark);
            }
        }

        /// <summary>
        /// RC-06. Stamps this row with how the learner did against the rest of
        /// the class in this subject. Computed as a cohort pass once every
        /// report in the class exists, not while generating one learner — there
        /// is nothing to compare a learner to on their own.
        /// </summary>
        public void SetClassStatistics(int? position, decimal? classAverage, decimal? highest, decimal? lowest)
        {
            SubjectPosition = position;
            ClassAverage = classAverage;
            HighestInClass = highest;
            LowestInClass = lowest;
        }

        /// <summary>
        /// Clears the cohort statistics, for when this row no longer has a mark
        /// to rank or the cohort it was ranked against has gone.
        /// </summary>
        public void ClearClassStatistics() => SetClassStatistics(null, null, null, null);
    }
}
