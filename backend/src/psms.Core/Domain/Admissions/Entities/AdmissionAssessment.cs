using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Academic.Entities;
using psms.Domain.Shared.Enums;

namespace psms.Domain.Admissions.Entities
{
    /// <summary>
    /// Represents a placement assessment for an application
    /// </summary>
    [Table("AdmissionAssessments")]
    public class AdmissionAssessment : CreationAuditedEntity<Guid>
    {
        public const int MaxSubjectsLength = 500;
        public const int MaxFeedbackLength = 2000;

        [Required]
        public Guid ApplicationId { get; set; }

        [Required]
        public AssessmentType Type { get; set; }

        [Required]
        public DateTime ScheduledDate { get; set; }

        [Required]
        public Guid AssessedGradeId { get; set; }

        /// <summary>
        /// JSON array of subjects assessed
        /// </summary>
        [StringLength(MaxSubjectsLength)]
        public string Subjects { get; set; }

        public decimal TotalScore { get; set; }

        public decimal MaxScore { get; set; }

        public decimal Percentage { get; set; }

        public bool Passed { get; set; }

        [StringLength(MaxFeedbackLength)]
        public string Feedback { get; set; }

        [Required]
        public long AssessorUserId { get; set; }

        public DateTime? CompletedDate { get; set; }

        [ForeignKey(nameof(ApplicationId))]
        public virtual Application Application { get; set; }

        [ForeignKey(nameof(AssessedGradeId))]
        public virtual Grade AssessedGrade { get; set; }

        protected AdmissionAssessment() { }

        public AdmissionAssessment(Guid id, Guid applicationId, AssessmentType type,
            DateTime scheduledDate, Guid assessedGradeId, long assessorUserId)
        {
            Id = id;
            ApplicationId = applicationId;
            Type = type;
            ScheduledDate = scheduledDate;
            AssessedGradeId = assessedGradeId;
            AssessorUserId = assessorUserId;
        }

        public void RecordResults(decimal totalScore, decimal maxScore, decimal passPercentage, string feedback = null)
        {
            TotalScore = totalScore;
            MaxScore = maxScore;
            Percentage = maxScore > 0 ? (totalScore / maxScore) * 100 : 0;
            Passed = Percentage >= passPercentage;
            Feedback = feedback;
            CompletedDate = DateTime.UtcNow;
        }
    }
}
