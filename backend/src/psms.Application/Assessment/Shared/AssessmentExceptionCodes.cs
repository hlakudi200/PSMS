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

    // Mark
    public const string MarkNotFound = "ASM_MARK_NOT_FOUND";
    public const string DuplicateMark = "ASM_DUPLICATE_MARK";
    public const string MarkExceedsMax = "ASM_MARK_EXCEEDS_MAX";
    public const string MarksLocked = "ASM_MARKS_LOCKED";
    public const string MarksAlreadyReleased = "ASM_MARKS_ALREADY_RELEASED";
    public const string StudentNotInClass = "ASM_STUDENT_NOT_IN_CLASS";
    public const string CannotEditLockedMark = "ASM_CANNOT_EDIT_LOCKED_MARK";
    public const string AssessmentNotPublished = "ASM_ASSESSMENT_NOT_PUBLISHED";
    public const string StudentNotFound = "ASM_STUDENT_NOT_FOUND";
    public const string DuplicateStudentInBatch = "ASM_DUPLICATE_STUDENT_IN_BATCH";

    // Report
    public const string ReportNotFound = "ASM_REPORT_NOT_FOUND";
    public const string DuplicateReport = "ASM_DUPLICATE_REPORT";
    public const string IncompleteMarksForReport = "ASM_INCOMPLETE_MARKS_FOR_REPORT";
    public const string InvalidReportStatusTransition = "ASM_INVALID_REPORT_STATUS_TRANSITION";
    public const string CannotPublishUnapproved = "ASM_CANNOT_PUBLISH_UNAPPROVED";
    public const string ReportNotEditable = "ASM_REPORT_NOT_EDITABLE";
    public const string ReportNotPublished = "ASM_REPORT_NOT_PUBLISHED";
    public const string MaxMarksReductionInvalid = "ASM_MAX_MARKS_REDUCTION_INVALID";

    // ReportSubject
    public const string ReportSubjectNotFound = "ASM_REPORT_SUBJECT_NOT_FOUND";
    public const string DuplicateReportSubject = "ASM_DUPLICATE_REPORT_SUBJECT";

    // Report PDF
    public const string PdfNotGenerated = "ASM_PDF_NOT_GENERATED";
    public const string PdfGenerationFailed = "ASM_PDF_GENERATION_FAILED";
    public const string ReportNotGeneratedForPdf = "ASM_REPORT_NOT_GENERATED_FOR_PDF";
}
