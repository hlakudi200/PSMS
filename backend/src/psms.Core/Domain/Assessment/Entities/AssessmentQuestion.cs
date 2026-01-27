using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Shared.Enums;

namespace psms.Domain.Assessment.Entities
{
    /// <summary>
    /// Represents a question within an assessment
    /// </summary>
    [Table("AssessmentQuestions")]
    public class AssessmentQuestion : CreationAuditedEntity<Guid>
    {
        public const int MaxQuestionTextLength = 4000;
        public const int MaxCorrectAnswerLength = 2000;
        public const int MaxOptionsLength = 4000;
        public const int MaxExplanationLength = 2000;

        /// <summary>
        /// Reference to the assessment
        /// </summary>
        [Required]
        public Guid AssessmentId { get; set; }

        /// <summary>
        /// Question number/order in the assessment
        /// </summary>
        [Required]
        public int QuestionNumber { get; set; }

        /// <summary>
        /// Type of question
        /// </summary>
        [Required]
        public QuestionType QuestionType { get; set; }

        /// <summary>
        /// The question text
        /// </summary>
        [Required]
        [StringLength(MaxQuestionTextLength)]
        public string QuestionText { get; set; }

        /// <summary>
        /// Marks allocated to this question
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(8,2)")]
        public decimal Marks { get; set; }

        /// <summary>
        /// JSON array of options (for multiple choice questions)
        /// </summary>
        [StringLength(MaxOptionsLength)]
        public string Options { get; set; }

        /// <summary>
        /// The correct answer
        /// </summary>
        [StringLength(MaxCorrectAnswerLength)]
        public string CorrectAnswer { get; set; }

        /// <summary>
        /// Explanation of the correct answer
        /// </summary>
        [StringLength(MaxExplanationLength)]
        public string Explanation { get; set; }

        /// <summary>
        /// Cognitive level (Bloom's taxonomy)
        /// </summary>
        public CognitiveLevel? CognitiveLevel { get; set; }

        /// <summary>
        /// Whether this question is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        [ForeignKey(nameof(AssessmentId))]
        public virtual Assessment Assessment { get; set; }

        // Collections
        public virtual ICollection<StudentAnswer> StudentAnswers { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected AssessmentQuestion()
        {
            StudentAnswers = new HashSet<StudentAnswer>();
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public AssessmentQuestion(
            Guid id,
            Guid assessmentId,
            int questionNumber,
            QuestionType questionType,
            string questionText,
            decimal marks) : this()
        {
            Id = id;
            AssessmentId = assessmentId;
            QuestionNumber = questionNumber;
            QuestionType = questionType;
            QuestionText = questionText;
            Marks = marks;
            IsActive = true;
        }

        /// <summary>
        /// Sets options for multiple choice questions
        /// </summary>
        public void SetOptions(string optionsJson)
        {
            if (QuestionType != QuestionType.MultipleChoice && QuestionType != QuestionType.TrueFalse)
                throw new InvalidOperationException("Options can only be set for multiple choice or true/false questions.");

            Options = optionsJson;
        }
    }
}
