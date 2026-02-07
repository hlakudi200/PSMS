namespace psms.Admissions.Shared;

/// <summary>
/// Exception codes for the Admissions module.
/// Maps to business rules ADM-001 through ADM-031.
/// </summary>
public static class AdmissionsExceptionCodes
{
    // Application Submission (ADM-001 to ADM-005)
    public const string InvalidApplicationNumber = "ADM001_INVALID_APPLICATION_NUMBER";
    public const string InvalidProspectiveStudentInfo = "ADM002_INVALID_PROSPECTIVE_STUDENT_INFO";
    public const string ParentInformationRequired = "ADM003_PARENT_INFORMATION_REQUIRED";
    public const string InvalidGradeSelection = "ADM004_INVALID_GRADE_SELECTION";
    public const string InvalidStatusTransition = "ADM005_INVALID_STATUS_TRANSITION";

    // Application Fee (ADM-006, ADM-007)
    public const string ApplicationFeeNotPaid = "ADM006_APPLICATION_FEE_NOT_PAID";
    public const string RefundNotAllowed = "ADM007_REFUND_NOT_ALLOWED";

    // Documents (ADM-008 to ADM-010)
    public const string RequiredDocumentsMissing = "ADM008_REQUIRED_DOCUMENTS_MISSING";
    public const string InvalidDocumentUpload = "ADM009_INVALID_DOCUMENT_UPLOAD";
    public const string DocumentsNotVerified = "ADM010_DOCUMENTS_NOT_VERIFIED";

    // Interview (ADM-011 to ADM-013)
    public const string InterviewRequired = "ADM011_INTERVIEW_REQUIRED";
    public const string InvalidInterviewSchedule = "ADM012_INVALID_INTERVIEW_SCHEDULE";
    public const string InterviewNotCompleted = "ADM013_INTERVIEW_NOT_COMPLETED";

    // Assessment (ADM-014 to ADM-016)
    public const string AssessmentRequired = "ADM014_ASSESSMENT_REQUIRED";
    public const string InvalidAssessmentSchedule = "ADM015_INVALID_ASSESSMENT_SCHEDULE";
    public const string AssessmentBelowPassMark = "ADM016_ASSESSMENT_BELOW_PASS_MARK";

    // Decision (ADM-017 to ADM-020)
    public const string UnauthorizedAdmissionDecision = "ADM017_UNAUTHORIZED_ADMISSION_DECISION";
    public const string OfferExpired = "ADM018_OFFER_EXPIRED";
    public const string RejectionReasonRequired = "ADM020_REJECTION_REASON_REQUIRED";

    // Waitlist (ADM-021 to ADM-024)
    public const string WaitlistOfferExpired = "ADM023_WAITLIST_OFFER_EXPIRED";

    // Enrollment (ADM-025 to ADM-029)
    public const string EnrollmentNotComplete = "ADM025_ENROLLMENT_NOT_COMPLETE";
    public const string StudentCreationFailed = "ADM026_STUDENT_CREATION_FAILED";
    public const string GradeCapacityFull = "ADM028_GRADE_CAPACITY_FULL";
    public const string ClassAssignmentRequired = "ADM029_CLASS_ASSIGNMENT_REQUIRED";

    // Additional validation codes
    public const string ApplicationNotFound = "ADM_APPLICATION_NOT_FOUND";
    public const string ApplicationAlreadySubmitted = "ADM_APPLICATION_ALREADY_SUBMITTED";
    public const string ApplicationCannotBeEdited = "ADM_APPLICATION_CANNOT_BE_EDITED";
    public const string ApplicationCannotBeWithdrawn = "ADM_APPLICATION_CANNOT_BE_WITHDRAWN";
    public const string DuplicateApplication = "ADM_DUPLICATE_APPLICATION";
    public const string InvalidAgeForGrade = "ADM_INVALID_AGE_FOR_GRADE";
    public const string InvalidSaIdNumber = "ADM_INVALID_SA_ID_NUMBER";
    public const string MaxParentsExceeded = "ADM_MAX_PARENTS_EXCEEDED";
    public const string PrimaryContactRequired = "ADM_PRIMARY_CONTACT_REQUIRED";
    public const string FinanciallyResponsibleRequired = "ADM_FINANCIALLY_RESPONSIBLE_REQUIRED";
    public const string DocumentAlreadyVerified = "ADM_DOCUMENT_ALREADY_VERIFIED";
    public const string MaxReschedulesExceeded = "ADM_MAX_RESCHEDULES_EXCEEDED";
    public const string InterviewAlreadyCompleted = "ADM_INTERVIEW_ALREADY_COMPLETED";
    public const string AssessmentAlreadyCompleted = "ADM_ASSESSMENT_ALREADY_COMPLETED";
    public const string WaitlistPositionNotAvailable = "ADM_WAITLIST_POSITION_NOT_AVAILABLE";
    public const string OfferAlreadyAccepted = "ADM_OFFER_ALREADY_ACCEPTED";
    public const string OfferAlreadyDeclined = "ADM_OFFER_ALREADY_DECLINED";
    public const string AdmissionSettingsNotFound = "ADM_SETTINGS_NOT_FOUND";
    public const string ApplicationsNotOpen = "ADM_APPLICATIONS_NOT_OPEN";
    public const string FeeAlreadyPaid = "ADM_FEE_ALREADY_PAID";
    public const string InterviewNotFound = "ADM_INTERVIEW_NOT_FOUND";
    public const string InterviewAlreadyScheduled = "ADM_INTERVIEW_ALREADY_SCHEDULED";
    public const string InsufficientInterviewNotice = "ADM_INSUFFICIENT_INTERVIEW_NOTICE";
    public const string CannotRescheduleInterview = "ADM_CANNOT_RESCHEDULE_INTERVIEW";
    public const string CannotCancelInterview = "ADM_CANNOT_CANCEL_INTERVIEW";
    public const string InterviewNotScheduled = "ADM_INTERVIEW_NOT_SCHEDULED";
    public const string AssessmentNotFound = "ADM_ASSESSMENT_NOT_FOUND";
    public const string AssessmentAlreadyScheduled = "ADM_ASSESSMENT_ALREADY_SCHEDULED";
    public const string InsufficientAssessmentNotice = "ADM_INSUFFICIENT_ASSESSMENT_NOTICE";
    public const string CannotRecordResults = "ADM_CANNOT_RECORD_RESULTS";
    public const string CannotCancelAssessment = "ADM_CANNOT_CANCEL_ASSESSMENT";
    public const string WaitlistNotFound = "ADM_WAITLIST_NOT_FOUND";
    public const string WaitlistAlreadyExists = "ADM_WAITLIST_ALREADY_EXISTS";
    public const string CannotOfferPosition = "ADM_CANNOT_OFFER_POSITION";
    public const string CannotAcceptOffer = "ADM_CANNOT_ACCEPT_OFFER";
    public const string CannotDeclineOffer = "ADM_CANNOT_DECLINE_OFFER";
    public const string CannotWithdrawFromWaitlist = "ADM_CANNOT_WITHDRAW_FROM_WAITLIST";
    public const string DocumentNotFound = "ADM_DOCUMENT_NOT_FOUND";
    public const string DuplicateAdmissionSettings = "ADM_DUPLICATE_ADMISSION_SETTINGS";
    public const string CannotDeleteSettingsWithApplications = "ADM_CANNOT_DELETE_SETTINGS_WITH_APPLICATIONS";
    public const string ParentNotFound = "ADM_PARENT_NOT_FOUND";
    public const string InvalidGradeForAge = "ADM_INVALID_GRADE_FOR_AGE";
    public const string InvalidCapacityUpdate = "ADM_INVALID_CAPACITY_UPDATE";
    public const string CannotDeleteVerifiedDocument = "ADM_CANNOT_DELETE_VERIFIED_DOCUMENT";
    public const string DocumentRejectionReasonRequired = "ADM_DOCUMENT_REJECTION_REASON_REQUIRED";
    public const string DocumentTooLarge = "ADM_DOCUMENT_TOO_LARGE";
    public const string InvalidDocumentFormat = "ADM_INVALID_DOCUMENT_FORMAT";
}
