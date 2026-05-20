namespace psms.Learning.Shared;

/// <summary>
/// Exception codes for the Learning module.
/// </summary>
public static class LearningExceptionCodes
{
    // LearningMaterial
    public const string LearningMaterialNotFound = "LRN_LEARNING_MATERIAL_NOT_FOUND";
    public const string DuplicateLearningMaterialTitle = "LRN_DUPLICATE_LEARNING_MATERIAL_TITLE";
    public const string MaterialAlreadyPublished = "LRN_MATERIAL_ALREADY_PUBLISHED";
    public const string MaterialNotPublished = "LRN_MATERIAL_NOT_PUBLISHED";
    public const string ClassSubjectNotFound = "LRN_CLASS_SUBJECT_NOT_FOUND";
    public const string TermNotFound = "LRN_TERM_NOT_FOUND";
    public const string InvalidLearningMaterialUpload = "LRN_INVALID_LEARNING_MATERIAL_UPLOAD";
    public const string LearningMaterialTooLarge = "LRN_LEARNING_MATERIAL_TOO_LARGE";
    public const string VersionConflict = "LRN_VERSION_CONFLICT";

    // OnlineLesson
    public const string OnlineLessonNotFound = "LRN_ONLINE_LESSON_NOT_FOUND";
    public const string InvalidLessonTimes = "LRN_INVALID_LESSON_TIMES";
    public const string LessonStartTimeInPast = "LRN_LESSON_START_TIME_IN_PAST";
    public const string InvalidLessonStatusTransition = "LRN_INVALID_LESSON_STATUS_TRANSITION";
    public const string CannotUpdateNonScheduledLesson = "LRN_CANNOT_UPDATE_NON_SCHEDULED_LESSON";
    public const string CannotDeleteActiveLesson = "LRN_CANNOT_DELETE_ACTIVE_LESSON";
}
