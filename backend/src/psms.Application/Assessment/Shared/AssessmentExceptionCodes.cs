namespace psms.Assessment.Shared;

/// <summary>
/// Exception codes for the Assessment module.
/// </summary>
public static class AssessmentExceptionCodes
{
    // Assessment
    public const string AssessmentNotFound = "ASM_ASSESSMENT_NOT_FOUND";
    public const string ClassSubjectNotFound = "ASM_CLASS_SUBJECT_NOT_FOUND";
    public const string TermNotFound = "ASM_TERM_NOT_FOUND";
    public const string TermInFuture = "ASM_TERM_IN_FUTURE";
    // QA-002: a due date must not fall before the scheduled date.
    public const string DueDateBeforeScheduled = "ASM_DUE_BEFORE_SCHEDULED";
    public const string NotAssignedTeacher = "ASM_NOT_ASSIGNED_TEACHER";
    public const string CannotEditPublished = "ASM_CANNOT_EDIT_PUBLISHED";
    public const string CannotDeleteWithMarks = "ASM_CANNOT_DELETE_WITH_MARKS";
    public const string CannotUnpublishReleasedMarks = "ASM_CANNOT_UNPUBLISH_RELEASED_MARKS";
    public const string InvalidMaxMarks = "ASM_INVALID_MAX_MARKS";
    public const string AssessmentAlreadyPublished = "ASM_ALREADY_PUBLISHED";

    // AssessmentQuestion
    public const string QuestionNotFound = "ASM_QUESTION_NOT_FOUND";
    public const string DuplicateQuestionNumber = "ASM_DUPLICATE_QUESTION_NUMBER";
    public const string QuestionTotalExceedsMax = "ASM_QUESTION_TOTAL_EXCEEDS_MAX";
    public const string CannotEditPublishedQuestion = "ASM_CANNOT_EDIT_PUBLISHED_QUESTION";
    public const string InvalidOptionsForType = "ASM_INVALID_OPTIONS_FOR_TYPE";
    // QA-001: structure rules enforced by CreateWithQuestionsAsync.
    public const string InsufficientQuestions = "ASM_INSUFFICIENT_QUESTIONS";
    public const string TooManyQuestions = "ASM_TOO_MANY_QUESTIONS";
    public const string InvalidQuestionOptions = "ASM_INVALID_QUESTION_OPTIONS";
    public const string InvalidCorrectOption = "ASM_INVALID_CORRECT_OPTION";

    // Mark
    public const string MarkNotFound = "ASM_MARK_NOT_FOUND";
    public const string DuplicateMark = "ASM_DUPLICATE_MARK";
    public const string MarkExceedsMax = "ASM_MARK_EXCEEDS_MAX";
    public const string MarksLocked = "ASM_MARKS_LOCKED";
    public const string MarksAlreadyReleased = "ASM_MARKS_ALREADY_RELEASED";
    public const string StudentNotInClass = "ASM_STUDENT_NOT_IN_CLASS";
    public const string CannotEditLockedMark = "ASM_CANNOT_EDIT_LOCKED_MARK";
    public const string NoRawMarkToModerate = "ASM_NO_RAW_MARK_TO_MODERATE";
    public const string AssessmentNotPublished = "ASM_ASSESSMENT_NOT_PUBLISHED";
    public const string StudentNotFound = "ASM_STUDENT_NOT_FOUND";
    public const string DuplicateStudentInBatch = "ASM_DUPLICATE_STUDENT_IN_BATCH";
    // TF-002/004: post-publish feedback edit guards.
    public const string FeedbackTooLong = "ASM_FEEDBACK_TOO_LONG";
    public const string FeedbackEditWindowExpired = "ASM_FEEDBACK_EDIT_WINDOW_EXPIRED";
    public const string FeedbackInappropriateLanguage = "ASM_FEEDBACK_INAPPROPRIATE_LANGUAGE";

    // Report
    public const string ReportNotFound = "ASM_REPORT_NOT_FOUND";
    public const string DuplicateReport = "ASM_DUPLICATE_REPORT";
    public const string IncompleteMarksForReport = "ASM_INCOMPLETE_MARKS_FOR_REPORT";
    public const string InvalidReportStatusTransition = "ASM_INVALID_REPORT_STATUS_TRANSITION";
    public const string CannotPublishUnapproved = "ASM_CANNOT_PUBLISH_UNAPPROVED";
    /// <summary>
    /// A school tried to save an SBA / examination split that does not total
    /// 100%, or supplied the same band twice.
    /// </summary>
    public const string InvalidAssessmentWeighting = "ASM_INVALID_ASSESSMENT_WEIGHTING";

    /// <summary>
    /// RC-07. Marks were recorded against a term/examination split that does
    /// not add to 100%, which would give a final mark over 100.
    /// </summary>
    public const string InvalidSubjectMarkWeighting = "ASM_INVALID_SUBJECT_MARK_WEIGHTING";

    /// <summary>
    /// RC-05. A report was asked for over an academic year that has no terms,
    /// so there is nothing to aggregate.
    /// </summary>
    public const string NoTermsInScope = "ASM_NO_TERMS_IN_SCOPE";

    /// <summary>
    /// RC-05. A term report was asked for without naming the term it covers.
    /// </summary>
    public const string TermRequiredForReport = "ASM_TERM_REQUIRED_FOR_REPORT";

    /// <summary>
    /// RC-05. The term named does not belong to the academic year the report is
    /// being written for.
    /// </summary>
    public const string TermNotInAcademicYear = "ASM_TERM_NOT_IN_ACADEMIC_YEAR";

    /// <summary>
    /// RC-05. A class is not linked to a grade, so there is no band and no way
    /// to tell how its marks should be weighted.
    /// </summary>
    public const string ClassGradeNotResolved = "ASM_CLASS_GRADE_NOT_RESOLVED";

    /// <summary>
    /// RC-16. A promotion decision was recorded on something other than the
    /// year-end report card, which is the one it belongs on.
    /// </summary>
    public const string PromotionNotOnThisReport = "ASM_PROMOTION_NOT_ON_THIS_REPORT";

    /// <summary>
    /// RC-16. A learner was promoted or progressed without saying which grade
    /// they move into.
    /// </summary>
    public const string PromotionGradeRequired = "ASM_PROMOTION_GRADE_REQUIRED";

    /// <summary>
    /// RC-16. A promoted learner was sent to a grade that is not the next one.
    /// </summary>
    public const string PromotionGradeNotNext = "ASM_PROMOTION_GRADE_NOT_NEXT";

    /// <summary>
    /// RE-002 / RC-05 / RC-12. A report card with no subjects, or with no marks
    /// in any subject, was sent for approval or publication. A blank card must
    /// never reach a parent.
    /// </summary>
    public const string BlankReportCard = "ASM_BLANK_REPORT_CARD";

    /// <summary>
    /// RC-17. A report card was published without one of the fields RE-002
    /// requires a South African report card to carry.
    /// </summary>
    public const string ReportCardIncomplete = "ASM_REPORT_CARD_INCOMPLETE";

    /// <summary>
    /// RC-17. A report card was published before the class teacher and the
    /// principal had signed it, which RE-003 requires.
    /// </summary>
    public const string ReportCardNotSigned = "ASM_REPORT_CARD_NOT_SIGNED";

    public const string ReportNotEditable = "ASM_REPORT_NOT_EDITABLE";
    public const string ReportNotPublished = "ASM_REPORT_NOT_PUBLISHED";

    /// <summary>
    /// Someone who is not a parent of the learner tried to record the parent's
    /// acknowledgement of a report card.
    /// </summary>
    public const string NotTheParent = "ASM_NOT_THE_PARENT";

    /// <summary>
    /// A report was submitted for approval in a school with no active report
    /// approval workflow. Approval only happens through the engine, so there
    /// would be nobody to review it.
    /// </summary>
    public const string NoApprovalWorkflowConfigured = "ASM_NO_APPROVAL_WORKFLOW_CONFIGURED";
    public const string MaxMarksReductionInvalid = "ASM_MAX_MARKS_REDUCTION_INVALID";

    // ReportSubject
    public const string ReportSubjectNotFound = "ASM_REPORT_SUBJECT_NOT_FOUND";
    public const string DuplicateReportSubject = "ASM_DUPLICATE_REPORT_SUBJECT";

    // Report PDF
    public const string PdfNotGenerated = "ASM_PDF_NOT_GENERATED";
    public const string PdfGenerationFailed = "ASM_PDF_GENERATION_FAILED";
    public const string ReportNotGeneratedForPdf = "ASM_REPORT_NOT_GENERATED_FOR_PDF";
}
