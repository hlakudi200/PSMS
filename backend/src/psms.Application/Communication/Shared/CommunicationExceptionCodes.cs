namespace psms.Communication.Shared;

public static class CommunicationExceptionCodes
{
    // Announcement
    public const string AnnouncementNotFound = "COMM_ANNOUNCEMENT_NOT_FOUND";
    public const string ExpiryDateBeforePublishDate = "COMM_EXPIRY_DATE_BEFORE_PUBLISH_DATE";
    public const string TargetGradeRequired = "COMM_TARGET_GRADE_REQUIRED";
    public const string TargetClassRequired = "COMM_TARGET_CLASS_REQUIRED";
    public const string AnnouncementAlreadyPublished = "COMM_ANNOUNCEMENT_ALREADY_PUBLISHED";
    public const string AnnouncementNotPublished = "COMM_ANNOUNCEMENT_NOT_PUBLISHED";

    // AnnouncementRead
    public const string AnnouncementReadNotFound = "COMM_ANNOUNCEMENT_READ_NOT_FOUND";
    public const string AlreadyMarkedAsRead = "COMM_ALREADY_MARKED_AS_READ";

    // Document
    public const string DocumentNotFound = "COMM_DOCUMENT_NOT_FOUND";
    public const string DocumentAlreadyPublished = "COMM_DOCUMENT_ALREADY_PUBLISHED";
    public const string DocumentNotPublished = "COMM_DOCUMENT_NOT_PUBLISHED";
    public const string AcademicYearNotFound = "COMM_ACADEMIC_YEAR_NOT_FOUND";

    // Message
    public const string MessageNotFound = "COMM_MESSAGE_NOT_FOUND";
    public const string CannotMessageSelf = "COMM_CANNOT_MESSAGE_SELF";
    public const string MessageNotVisible = "COMM_MESSAGE_NOT_VISIBLE";
    public const string RecipientNotFound = "COMM_RECIPIENT_NOT_FOUND";
    public const string OnlySenderCanDelete = "COMM_ONLY_SENDER_CAN_DELETE";
    public const string OnlyRecipientCanDelete = "COMM_ONLY_RECIPIENT_CAN_DELETE";
    public const string OnlyRecipientCanMarkRead = "COMM_ONLY_RECIPIENT_CAN_MARK_READ";

    // Notification
    public const string NotificationNotFound = "COMM_NOTIFICATION_NOT_FOUND";
    public const string NotificationNotForCurrentUser = "COMM_NOTIFICATION_NOT_FOR_CURRENT_USER";
    public const string NotificationDispatchFailed = "COMM_NOTIFICATION_DISPATCH_FAILED";
}
