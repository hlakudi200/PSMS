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
    /// Represents a student's term or year-end report
    /// </summary>
    [Table("Reports")]
    public class Report : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public const int MaxTeacherCommentLength = 2000;
        public const int MaxPrincipalCommentLength = 1000;
        public const int MaxParentCommentLength = 1000;

        /// <summary>
        /// Tenant identifier for multi-tenancy
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Reference to the student
        /// </summary>
        [Required]
        public Guid StudentId { get; set; }

        /// <summary>
        /// Reference to the class
        /// </summary>
        [Required]
        public Guid ClassId { get; set; }

        /// <summary>
        /// Reference to the term (null for year-end reports)
        /// </summary>
        public Guid? TermId { get; set; }

        /// <summary>
        /// Reference to the academic year
        /// </summary>
        [Required]
        public Guid AcademicYearId { get; set; }

        /// <summary>
        /// Type of report
        /// </summary>
        [Required]
        public ReportType ReportType { get; set; }

        /// <summary>
        /// Overall average percentage
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? OverallPercentage { get; set; }

        /// <summary>
        /// Overall achievement level
        /// </summary>
        public CapsAchievementLevel? OverallAchievementLevel { get; set; }

        /// <summary>
        /// Student's position in class
        /// </summary>
        public int? ClassPosition { get; set; }

        /// <summary>
        /// Total students in class
        /// </summary>
        public int? TotalStudentsInClass { get; set; }

        /// <summary>
        /// Days present in term/year
        /// </summary>
        public int DaysPresent { get; set; }

        /// <summary>
        /// Days absent in term/year
        /// </summary>
        public int DaysAbsent { get; set; }

        /// <summary>
        /// Days late in term/year
        /// </summary>
        public int DaysLate { get; set; }

        /// <summary>
        /// Class teacher's comment
        /// </summary>
        [StringLength(MaxTeacherCommentLength)]
        public string TeacherComment { get; set; }

        /// <summary>
        /// Principal's comment
        /// </summary>
        [StringLength(MaxPrincipalCommentLength)]
        public string PrincipalComment { get; set; }

        /// <summary>
        /// Parent acknowledgement comment
        /// </summary>
        [StringLength(MaxParentCommentLength)]
        public string ParentComment { get; set; }

        /// <summary>
        /// Date parent acknowledged the report
        /// </summary>
        public DateTime? ParentAcknowledgedDate { get; set; }

        /// <summary>
        /// Promotion decision
        /// </summary>
        public PromotionDecision? PromotionDecision { get; set; }

        /// <summary>
        /// Next grade promoted to
        /// </summary>
        public Guid? PromotedToGradeId { get; set; }

        /// <summary>
        /// Status of the report
        /// </summary>
        [Required]
        public ReportStatus Status { get; set; }

        /// <summary>
        /// Date the report was generated
        /// </summary>
        public DateTime? GeneratedDate { get; set; }

        /// <summary>
        /// Date the report was published to parent
        /// </summary>
        public DateTime? PublishedDate { get; set; }

        /// <summary>
        /// User who approved the report
        /// </summary>
        public long? ApprovedByUserId { get; set; }

        /// <summary>
        /// Date the report was approved
        /// </summary>
        public DateTime? ApprovedDate { get; set; }

        /// <summary>
        /// URL to the generated PDF stored in Supabase Storage
        /// </summary>
        [StringLength(2048)]
        public string PdfUrl { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(StudentId))]
        public virtual Student Student { get; set; }

        [ForeignKey(nameof(ClassId))]
        public virtual Class Class { get; set; }

        [ForeignKey(nameof(TermId))]
        public virtual Term Term { get; set; }

        [ForeignKey(nameof(AcademicYearId))]
        public virtual AcademicYear AcademicYear { get; set; }

        [ForeignKey(nameof(PromotedToGradeId))]
        public virtual Grade PromotedToGrade { get; set; }

        // Collections
        public virtual ICollection<ReportSubject> SubjectReports { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected Report()
        {
            SubjectReports = new HashSet<ReportSubject>();
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public Report(
            Guid id,
            int? tenantId,
            Guid studentId,
            Guid classId,
            Guid academicYearId,
            ReportType reportType,
            Guid? termId = null) : this()
        {
            Id = id;
            TenantId = tenantId;
            StudentId = studentId;
            ClassId = classId;
            AcademicYearId = academicYearId;
            ReportType = reportType;
            TermId = termId;
            Status = ReportStatus.Draft;
        }

        /// <summary>
        /// Generates the report (calculates averages, etc.)
        /// </summary>
        public void Generate()
        {
            GeneratedDate = DateTime.UtcNow;
            Status = ReportStatus.Generated;
        }

        /// <summary>
        /// Submits the report for approval
        /// </summary>
        public void SubmitForApproval()
        {
            if (Status != ReportStatus.Generated)
                throw new InvalidOperationException("Report must be generated before submission.");

            Status = ReportStatus.PendingApproval;
        }

        /// <summary>
        /// Approves the report
        /// </summary>
        public void Approve(long approvedByUserId)
        {
            if (Status != ReportStatus.PendingApproval)
                throw new InvalidOperationException("Report must be pending approval.");

            ApprovedByUserId = approvedByUserId;
            ApprovedDate = DateTime.UtcNow;
            Status = ReportStatus.Approved;
        }

        /// <summary>
        /// WF-31: the approver sent the report back. It returns to Generated so the
        /// teacher can correct it and resubmit; the reason is kept as the
        /// principal's comment so it travels with the report.
        /// </summary>
        public void ReturnForRevision(string reason)
        {
            if (Status != ReportStatus.PendingApproval)
                throw new InvalidOperationException("Only reports pending approval can be returned for revision.");

            Status = ReportStatus.Generated;
            ApprovedByUserId = null;
            ApprovedDate = null;
            if (!string.IsNullOrWhiteSpace(reason))
                PrincipalComment = reason.Length > 1000 ? reason.Substring(0, 1000) : reason;
        }

        /// <summary>
        /// Publishes the report to parent
        /// </summary>
        public void Publish()
        {
            if (Status != ReportStatus.Approved)
                throw new InvalidOperationException("Report must be approved before publishing.");

            PublishedDate = DateTime.UtcNow;
            Status = ReportStatus.Published;
        }

        /// <summary>
        /// Records parent acknowledgement
        /// </summary>
        public void AcknowledgeByParent(string comment = null)
        {
            ParentAcknowledgedDate = DateTime.UtcNow;
            ParentComment = comment;
        }

        /// <summary>
        /// Sets the URL to the generated PDF
        /// </summary>
        public void SetPdfUrl(string url)
        {
            PdfUrl = url;
        }

        /// <summary>
        /// Records promotion decision
        /// </summary>
        public void RecordPromotion(PromotionDecision decision, Guid? promotedToGradeId = null)
        {
            PromotionDecision = decision;
            PromotedToGradeId = promotedToGradeId;
        }
    }
}
