namespace psms.Domain.Shared.Enums
{
    /// <summary>
    /// Application status in the admissions process
    /// Based on ADM-005: Application Status Workflow
    /// </summary>
    public enum ApplicationStatus
    {
        /// <summary>
        /// Initial state - application being filled out
        /// </summary>
        Draft = 1,

        /// <summary>
        /// Application submitted by parent, awaiting fee payment
        /// </summary>
        Submitted = 2,

        /// <summary>
        /// Awaiting application fee payment (ADM-006)
        /// </summary>
        PaymentPending = 3,

        /// <summary>
        /// Payment received, application under initial review
        /// </summary>
        UnderReview = 4,

        /// <summary>
        /// Additional documents required from parent (ADM-008)
        /// </summary>
        DocumentsRequired = 5,

        /// <summary>
        /// Interview has been scheduled (ADM-012)
        /// </summary>
        InterviewScheduled = 6,

        /// <summary>
        /// Placement assessment has been scheduled (ADM-015)
        /// </summary>
        AssessmentScheduled = 7,

        /// <summary>
        /// All requirements met, awaiting admission decision (ADM-017)
        /// </summary>
        UnderConsideration = 8,

        /// <summary>
        /// Application approved, offer sent to parent (ADM-018)
        /// </summary>
        Approved = 9,

        /// <summary>
        /// Application rejected (ADM-020)
        /// </summary>
        Rejected = 10,

        /// <summary>
        /// Placed on waitlist due to capacity (ADM-021)
        /// </summary>
        Waitlisted = 11,

        /// <summary>
        /// Student enrolled and record created (ADM-026)
        /// </summary>
        Enrolled = 12,

        /// <summary>
        /// Application withdrawn by parent (ADM-031)
        /// </summary>
        Withdrawn = 13,

        /// <summary>
        /// Application expired due to inactivity (ADM-030)
        /// </summary>
        Expired = 14
    }
}
