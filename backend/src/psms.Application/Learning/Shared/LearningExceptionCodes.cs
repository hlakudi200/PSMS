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
    public const string InvalidLessonStatusTransition = "LRN_INVALID_LESSON_STATUS_TRANSITION";
    public const string CannotUpdateNonScheduledLesson = "LRN_CANNOT_UPDATE_NON_SCHEDULED_LESSON";
    public const string CannotDeleteActiveLesson = "LRN_CANNOT_DELETE_ACTIVE_LESSON";
    // OL-001 scheduling guard rails — must schedule >=24h ahead, within
    // 07:00-17:00 SA time, 30-180 minutes long, and not overlap an
    // already-scheduled lesson on the same class-subject.
    public const string LessonTooSoon = "LRN_LESSON_TOO_SOON";
    public const string LessonOutsideSchoolHours = "LRN_LESSON_OUTSIDE_SCHOOL_HOURS";
    public const string LessonOverlapsExisting = "LRN_LESSON_OVERLAPS_EXISTING";
    public const string LessonDurationOutOfRange = "LRN_LESSON_DURATION_OUT_OF_RANGE";
    public const string LessonNotOwnedByTeacher = "LRN_LESSON_NOT_OWNED_BY_TEACHER";
    public const string TeacherNotFoundForCurrentUser = "LRN_TEACHER_NOT_FOUND_FOR_CURRENT_USER";
    // OL-006 host-window guard — Start can only fire from 15 min before
    // the scheduled start until the scheduled end time. Outside that
    // window we either tell the teacher to wait or ask them to reschedule.
    public const string LessonStartTooEarly = "LRN_LESSON_START_TOO_EARLY";
    public const string LessonStartTooLate = "LRN_LESSON_START_TOO_LATE";
    // Distinct codes for non-time validation failures so audit-log / i18n
    // consumers don't have to disambiguate "invalid lesson times" between
    // three semantically different errors.
    public const string AttendeeCountOutOfRange = "LRN_ATTENDEE_COUNT_OUT_OF_RANGE";
    public const string InvalidMeetingLink = "LRN_INVALID_MEETING_LINK";
    // OL-003 recording upload guards: type whitelist, 5 GB size cap, and
    // status guard (only Completed lessons may have a recording attached).
    public const string RecordingFileTypeInvalid = "LRN_RECORDING_FILE_TYPE_INVALID";
    public const string RecordingFileTooLarge = "LRN_RECORDING_FILE_TOO_LARGE";
    public const string RecordingFileMissing = "LRN_RECORDING_FILE_MISSING";
    public const string LessonNotCompleted = "LRN_LESSON_NOT_COMPLETED";
    public const string InvalidRecordingUrl = "LRN_INVALID_RECORDING_URL";
    public const string RecordingUploadConflict = "LRN_RECORDING_UPLOAD_CONFLICT";
    // Direct-upload reference (object key) failed validation — it didn't
    // belong to this tenant/lesson, or the file wasn't found in storage.
    public const string InvalidRecordingUpload = "LRN_INVALID_RECORDING_UPLOAD";
    // LC-01 in-app live classroom: the lesson isn't an in-app live class, the
    // class hasn't started yet (for students), or LiveKit isn't configured.
    public const string LiveClassNotAvailable = "LRN_LIVE_CLASS_NOT_AVAILABLE";
    // US-TCH-004: the lesson date must fall inside the current term, and
    // pre-lesson materials must be published materials on the lesson's own
    // class-subject.
    public const string LessonOutsideCurrentTerm = "LRN_LESSON_OUTSIDE_CURRENT_TERM";
    public const string InvalidLessonMaterials = "LRN_INVALID_LESSON_MATERIALS";
}
