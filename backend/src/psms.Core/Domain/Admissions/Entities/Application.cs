using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Academic.Entities;
using psms.Domain.Shared.Enums;

namespace psms.Domain.Admissions.Entities
{
    /// <summary>
    /// Represents a school admission application from prospective parents
    /// </summary>
    [Table("Applications")]
    public class Application : FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
    {
        public const int MaxApplicationNumberLength = 50;
        public const int MaxNameLength = 100;
        public const int MaxIdNumberLength = 13;
        public const int MaxPassportNumberLength = 50;
        public const int MaxSchoolNameLength = 200;
        public const int MaxEmailLength = 256;
        public const int MaxReasonLength = 2000;

        /// <summary>
        /// Tenant identifier for multi-tenancy
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Unique application number (format: APP-{TenantId}-{Year}-{Sequence})
        /// </summary>
        [Required]
        [StringLength(MaxApplicationNumberLength)]
        public string ApplicationNumber { get; set; }

        /// <summary>
        /// First name of the prospective student
        /// </summary>
        [Required]
        [StringLength(MaxNameLength)]
        public string ProspectiveStudentFirstName { get; set; }

        /// <summary>
        /// Last name of the prospective student
        /// </summary>
        [Required]
        [StringLength(MaxNameLength)]
        public string ProspectiveStudentLastName { get; set; }

        /// <summary>
        /// Middle name of the prospective student
        /// </summary>
        [StringLength(MaxNameLength)]
        public string ProspectiveStudentMiddleName { get; set; }

        /// <summary>
        /// Date of birth of the prospective student
        /// </summary>
        [Required]
        public DateTime DateOfBirth { get; set; }

        /// <summary>
        /// Gender of the prospective student
        /// </summary>
        [Required]
        public Gender Gender { get; set; }

        /// <summary>
        /// South African ID number (for SA citizens)
        /// </summary>
        [StringLength(MaxIdNumberLength)]
        public string IdNumber { get; set; }

        /// <summary>
        /// Passport number (for non-SA citizens)
        /// </summary>
        [StringLength(MaxPassportNumberLength)]
        public string PassportNumber { get; set; }

        /// <summary>
        /// Indicates if the prospective student is a South African citizen
        /// </summary>
        public bool IsSACitizen { get; set; }

        /// <summary>
        /// Grade the student is applying for
        /// </summary>
        [Required]
        public Guid AppliedGradeId { get; set; }

        /// <summary>
        /// Academic year the application is for (e.g., 2025)
        /// </summary>
        [Required]
        public Guid AcademicYearId { get; set; }

        /// <summary>
        /// Name of previous school attended
        /// </summary>
        [StringLength(MaxSchoolNameLength)]
        public string PreviousSchool { get; set; }

        /// <summary>
        /// Date when application was created
        /// </summary>
        [Required]
        public DateTime ApplicationDate { get; set; }

        /// <summary>
        /// Current status of the application
        /// </summary>
        [Required]
        public ApplicationStatus Status { get; set; }

        /// <summary>
        /// Date when application was submitted
        /// </summary>
        public DateTime? SubmissionDate { get; set; }

        /// <summary>
        /// Date when application was reviewed
        /// </summary>
        public DateTime? ReviewedDate { get; set; }

        /// <summary>
        /// User ID of the reviewer (ABP User.Id is long)
        /// </summary>
        public long? ReviewedByUserId { get; set; }

        /// <summary>
        /// Admission decision
        /// </summary>
        public AdmissionDecision? Decision { get; set; }

        /// <summary>
        /// Reason for the admission decision
        /// </summary>
        [StringLength(MaxReasonLength)]
        public string DecisionReason { get; set; }

        /// <summary>
        /// Date when decision was made
        /// </summary>
        public DateTime? DecisionDate { get; set; }

        /// <summary>
        /// Date when the acceptance offer expires
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Student ID if application was converted to student
        /// </summary>
        public Guid? CreatedStudentId { get; set; }

        /// <summary>
        /// Email address of the applicant (prospective parent)
        /// </summary>
        [Required]
        [StringLength(MaxEmailLength)]
        public string CreatorEmailAddress { get; set; }

        /// <summary>
        /// Concurrency stamp for optimistic concurrency control
        /// </summary>
        [StringLength(40)]
        public string ConcurrencyStamp { get; set; }

        /// <summary>
        /// Soft delete flag
        /// </summary>
        public bool IsDeleted { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(AppliedGradeId))]
        public virtual Grade AppliedGrade { get; set; }

        [ForeignKey(nameof(AcademicYearId))]
        public virtual AcademicYear AcademicYear { get; set; }

        [ForeignKey(nameof(CreatedStudentId))]
        public virtual Student CreatedStudent { get; set; }

        // Collections
        public virtual ICollection<ApplicantParent> ApplicantParents { get; set; }
        public virtual ICollection<ApplicationDocument> ApplicationDocuments { get; set; }
        public virtual ApplicationFee ApplicationFee { get; set; }
        public virtual AdmissionInterview AdmissionInterview { get; set; }
        public virtual AdmissionAssessment AdmissionAssessment { get; set; }
        public virtual Waitlist Waitlist { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected Application()
        {
            ApplicantParents = new HashSet<ApplicantParent>();
            ApplicationDocuments = new HashSet<ApplicationDocument>();
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public Application(
            Guid id,
            int? tenantId,
            string applicationNumber,
            string prospectiveStudentFirstName,
            string prospectiveStudentLastName,
            DateTime dateOfBirth,
            Gender gender,
            Guid appliedGradeId,
            Guid academicYearId,
            string creatorEmailAddress) : this()
        {
            Id = id;
            TenantId = tenantId;
            ApplicationNumber = applicationNumber;
            ProspectiveStudentFirstName = prospectiveStudentFirstName;
            ProspectiveStudentLastName = prospectiveStudentLastName;
            DateOfBirth = dateOfBirth;
            Gender = gender;
            AppliedGradeId = appliedGradeId;
            AcademicYearId = academicYearId;
            CreatorEmailAddress = creatorEmailAddress;
            ApplicationDate = DateTime.UtcNow;
            Status = ApplicationStatus.Draft;
            ConcurrencyStamp = Guid.NewGuid().ToString("N");
            IsDeleted = false;
        }

        /// <summary>
        /// Gets the full name of the prospective student
        /// </summary>
        public string GetProspectiveStudentFullName()
        {
            return string.IsNullOrWhiteSpace(ProspectiveStudentMiddleName)
                ? $"{ProspectiveStudentFirstName} {ProspectiveStudentLastName}"
                : $"{ProspectiveStudentFirstName} {ProspectiveStudentMiddleName} {ProspectiveStudentLastName}";
        }

        #region Workflow Methods (ADM-005)

        /// <summary>
        /// Submits the application - moves to PaymentPending (ADM-005)
        /// </summary>
        public void Submit()
        {
            if (Status != ApplicationStatus.Draft)
                throw new InvalidOperationException("Only draft applications can be submitted.");

            Status = ApplicationStatus.PaymentPending;
            SubmissionDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Marks payment as received - moves to UnderReview (ADM-006)
        /// </summary>
        public void MarkPaymentReceived()
        {
            if (Status != ApplicationStatus.PaymentPending)
                throw new InvalidOperationException("Application must be in PaymentPending status.");

            Status = ApplicationStatus.UnderReview;
        }

        /// <summary>
        /// Requests additional documents from parent (ADM-008)
        /// </summary>
        public void RequestDocuments()
        {
            if (Status != ApplicationStatus.UnderReview)
                throw new InvalidOperationException("Application must be under review to request documents.");

            Status = ApplicationStatus.DocumentsRequired;
        }

        /// <summary>
        /// Marks documents as complete - returns to UnderReview
        /// </summary>
        public void MarkDocumentsComplete()
        {
            if (Status != ApplicationStatus.DocumentsRequired)
                throw new InvalidOperationException("Application must be in DocumentsRequired status.");

            Status = ApplicationStatus.UnderReview;
        }

        /// <summary>
        /// Schedules interview (ADM-012)
        /// </summary>
        public void ScheduleInterview()
        {
            if (Status != ApplicationStatus.UnderReview)
                throw new InvalidOperationException("Application must be under review to schedule interview.");

            Status = ApplicationStatus.InterviewScheduled;
        }

        /// <summary>
        /// Marks interview as complete - moves to UnderConsideration
        /// </summary>
        public void CompleteInterview()
        {
            if (Status != ApplicationStatus.InterviewScheduled)
                throw new InvalidOperationException("Application must have scheduled interview.");

            Status = ApplicationStatus.UnderConsideration;
        }

        /// <summary>
        /// Schedules assessment (ADM-015)
        /// </summary>
        public void ScheduleAssessment()
        {
            if (Status != ApplicationStatus.UnderReview)
                throw new InvalidOperationException("Application must be under review to schedule assessment.");

            Status = ApplicationStatus.AssessmentScheduled;
        }

        /// <summary>
        /// Marks assessment as complete - moves to UnderConsideration
        /// </summary>
        public void CompleteAssessment()
        {
            if (Status != ApplicationStatus.AssessmentScheduled)
                throw new InvalidOperationException("Application must have scheduled assessment.");

            Status = ApplicationStatus.UnderConsideration;
        }

        /// <summary>
        /// Moves directly to UnderConsideration (when no interview/assessment required)
        /// </summary>
        public void MoveToConsideration()
        {
            if (Status != ApplicationStatus.UnderReview)
                throw new InvalidOperationException("Application must be under review.");

            Status = ApplicationStatus.UnderConsideration;
        }

        /// <summary>
        /// Approves the application (ADM-017, ADM-018)
        /// </summary>
        public void Approve(long reviewedByUserId, string reason = null, int offerExpiryDays = 14)
        {
            if (Status != ApplicationStatus.UnderConsideration)
                throw new InvalidOperationException("Application must be under consideration to approve.");

            Status = ApplicationStatus.Approved;
            Decision = AdmissionDecision.Accepted;
            DecisionReason = reason;
            DecisionDate = DateTime.UtcNow;
            ReviewedByUserId = reviewedByUserId;
            ReviewedDate = DateTime.UtcNow;
            ExpiryDate = DateTime.UtcNow.AddDays(offerExpiryDays);
        }

        /// <summary>
        /// Approves with conditions (ADM-019)
        /// </summary>
        public void ApproveWithConditions(long reviewedByUserId, string conditions, int offerExpiryDays = 14)
        {
            if (Status != ApplicationStatus.UnderConsideration)
                throw new InvalidOperationException("Application must be under consideration to approve.");

            if (string.IsNullOrWhiteSpace(conditions))
                throw new ArgumentException("Conditions must be specified for conditional approval.");

            Status = ApplicationStatus.Approved;
            Decision = AdmissionDecision.ConditionalAcceptance;
            DecisionReason = conditions;
            DecisionDate = DateTime.UtcNow;
            ReviewedByUserId = reviewedByUserId;
            ReviewedDate = DateTime.UtcNow;
            ExpiryDate = DateTime.UtcNow.AddDays(offerExpiryDays);
        }

        /// <summary>
        /// Rejects the application (ADM-020)
        /// </summary>
        public void Reject(long reviewedByUserId, string reason)
        {
            if (Status != ApplicationStatus.UnderConsideration)
                throw new InvalidOperationException("Application must be under consideration to reject.");

            if (string.IsNullOrWhiteSpace(reason) || reason.Length < 50)
                throw new ArgumentException("Rejection reason must be at least 50 characters.");

            Status = ApplicationStatus.Rejected;
            Decision = AdmissionDecision.Rejected;
            DecisionReason = reason;
            DecisionDate = DateTime.UtcNow;
            ReviewedByUserId = reviewedByUserId;
            ReviewedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Places the application on waitlist (ADM-021)
        /// </summary>
        public void PlaceOnWaitlist(long reviewedByUserId, string reason = null)
        {
            if (Status != ApplicationStatus.UnderConsideration)
                throw new InvalidOperationException("Application must be under consideration to waitlist.");

            Status = ApplicationStatus.Waitlisted;
            Decision = AdmissionDecision.Waitlisted;
            DecisionReason = reason;
            DecisionDate = DateTime.UtcNow;
            ReviewedByUserId = reviewedByUserId;
            ReviewedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Marks the application as enrolled and links to created student (ADM-026)
        /// </summary>
        public void MarkAsEnrolled(Guid studentId)
        {
            if (Status != ApplicationStatus.Approved)
                throw new InvalidOperationException("Only approved applications can be marked as enrolled.");

            Status = ApplicationStatus.Enrolled;
            CreatedStudentId = studentId;
        }

        /// <summary>
        /// Withdraws the application (ADM-031)
        /// </summary>
        public void Withdraw(string reason = null)
        {
            if (Status == ApplicationStatus.Enrolled)
                throw new InvalidOperationException("Cannot withdraw an enrolled application.");

            if (Status == ApplicationStatus.Withdrawn || Status == ApplicationStatus.Expired)
                throw new InvalidOperationException("Application is already withdrawn or expired.");

            Status = ApplicationStatus.Withdrawn;
            DecisionReason = reason;
        }

        /// <summary>
        /// Reverts application status to UnderReview (when interview/assessment is cancelled)
        /// </summary>
        public void RevertToUnderReview()
        {
            if (Status != ApplicationStatus.InterviewScheduled && Status != ApplicationStatus.AssessmentScheduled)
                throw new InvalidOperationException("Can only revert from InterviewScheduled or AssessmentScheduled.");

            Status = ApplicationStatus.UnderReview;
        }

        /// <summary>
        /// Approves the application from the waitlist (ADM-023)
        /// </summary>
        public void ApproveFromWaitlist(int offerExpiryDays = 14)
        {
            if (Status != ApplicationStatus.Waitlisted)
                throw new InvalidOperationException("Only waitlisted applications can be approved from waitlist.");

            Status = ApplicationStatus.Approved;
            Decision = AdmissionDecision.Accepted;
            ExpiryDate = DateTime.UtcNow.AddDays(offerExpiryDays);
        }

        /// <summary>
        /// Expires the application (ADM-030)
        /// </summary>
        public void Expire()
        {
            if (Status == ApplicationStatus.Enrolled || Status == ApplicationStatus.Withdrawn || Status == ApplicationStatus.Expired)
                throw new InvalidOperationException("Cannot expire this application.");

            Status = ApplicationStatus.Expired;
        }

        /// <summary>
        /// Extends the offer expiry date (ADM-018) - maximum 1 extension of 7 days
        /// </summary>
        public void ExtendOfferExpiry(int additionalDays = 7)
        {
            if (Status != ApplicationStatus.Approved)
                throw new InvalidOperationException("Can only extend expiry for approved applications.");

            if (additionalDays > 7)
                throw new ArgumentException("Maximum extension is 7 days.");

            ExpiryDate = ExpiryDate?.AddDays(additionalDays) ?? DateTime.UtcNow.AddDays(additionalDays);
        }

        #endregion

        #region Validation Methods

        /// <summary>
        /// Checks if the offer has expired
        /// </summary>
        public bool IsOfferExpired()
        {
            return Status == ApplicationStatus.Approved && ExpiryDate.HasValue && DateTime.UtcNow > ExpiryDate.Value;
        }

        /// <summary>
        /// Checks if application can be submitted
        /// </summary>
        public bool CanSubmit()
        {
            return Status == ApplicationStatus.Draft;
        }

        /// <summary>
        /// Checks if a decision can be made on this application
        /// </summary>
        public bool CanMakeDecision()
        {
            return Status == ApplicationStatus.UnderConsideration;
        }

        #endregion
    }
}
