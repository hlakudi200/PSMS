using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Academic.Entities;

namespace psms.Domain.Assessment.Entities
{
    /// <summary>
    /// Represents a student's answer to an assessment question
    /// </summary>
    [Table("StudentAnswers")]
    public class StudentAnswer : CreationAuditedEntity<Guid>
    {
        public const int MaxAnswerTextLength = 4000;
        public const int MaxFeedbackLength = 2000;

        /// <summary>
        /// Reference to the assessment question
        /// </summary>
        [Required]
        public Guid AssessmentQuestionId { get; set; }

        /// <summary>
        /// Reference to the student
        /// </summary>
        [Required]
        public Guid StudentId { get; set; }

        /// <summary>
        /// The student's answer text
        /// </summary>
        [StringLength(MaxAnswerTextLength)]
        public string AnswerText { get; set; }

        /// <summary>
        /// Selected option index (for multiple choice)
        /// </summary>
        public int? SelectedOption { get; set; }

        /// <summary>
        /// Whether the answer is correct (for auto-marked questions)
        /// </summary>
        public bool? IsCorrect { get; set; }

        /// <summary>
        /// Marks awarded for this answer
        /// </summary>
        [Column(TypeName = "decimal(8,2)")]
        public decimal? MarksAwarded { get; set; }

        /// <summary>
        /// Teacher's feedback on the answer
        /// </summary>
        [StringLength(MaxFeedbackLength)]
        public string Feedback { get; set; }

        /// <summary>
        /// Date the answer was submitted
        /// </summary>
        public DateTime? SubmittedDate { get; set; }

        /// <summary>
        /// Date the answer was marked
        /// </summary>
        public DateTime? MarkedDate { get; set; }

        /// <summary>
        /// Teacher who marked the answer
        /// </summary>
        public long? MarkedByTeacherUserId { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(AssessmentQuestionId))]
        public virtual AssessmentQuestion AssessmentQuestion { get; set; }

        [ForeignKey(nameof(StudentId))]
        public virtual Student Student { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected StudentAnswer()
        {
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public StudentAnswer(
            Guid id,
            Guid assessmentQuestionId,
            Guid studentId) : this()
        {
            Id = id;
            AssessmentQuestionId = assessmentQuestionId;
            StudentId = studentId;
        }

        /// <summary>
        /// Submits the answer
        /// </summary>
        public void Submit(string answerText, int? selectedOption = null)
        {
            AnswerText = answerText;
            SelectedOption = selectedOption;
            SubmittedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Auto-marks the answer (for multiple choice, true/false)
        /// </summary>
        public void AutoMark(bool isCorrect, decimal maxMarks)
        {
            IsCorrect = isCorrect;
            MarksAwarded = isCorrect ? maxMarks : 0;
            MarkedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Manually marks the answer
        /// </summary>
        public void ManualMark(decimal marks, long teacherUserId, string feedback = null)
        {
            MarksAwarded = marks;
            MarkedByTeacherUserId = teacherUserId;
            Feedback = feedback;
            MarkedDate = DateTime.UtcNow;
        }
    }
}
