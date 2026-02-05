namespace psms.Authorization;

/// <summary>
/// Permission names for the PSMS (Private School Management System).
/// Permission pattern: {Module}.{Entity}.{Action}
/// See PSMS-Permissions-Matrix.md for complete role-permission mappings.
/// </summary>
public static class PermissionNames
{
    #region System/Pages Permissions (ABP Default)

    public const string Pages_Tenants = "Pages.Tenants";
    public const string Pages_Users = "Pages.Users";
    public const string Pages_Users_Activation = "Pages.Users.Activation";
    public const string Pages_Roles = "Pages.Roles";

    #endregion

    #region Admissions Module

    public const string Admissions = "Admissions";

    // Applications
    public const string Admissions_Applications = "Admissions.Applications";
    public const string Admissions_Applications_View = "Admissions.Applications.View";
    public const string Admissions_Applications_ViewAll = "Admissions.Applications.ViewAll";
    public const string Admissions_Applications_Create = "Admissions.Applications.Create";
    public const string Admissions_Applications_Edit = "Admissions.Applications.Edit";
    public const string Admissions_Applications_Delete = "Admissions.Applications.Delete";
    public const string Admissions_Applications_Submit = "Admissions.Applications.Submit";
    public const string Admissions_Applications_Withdraw = "Admissions.Applications.Withdraw";

    // Review
    public const string Admissions_Review = "Admissions.Review";
    public const string Admissions_Review_StartReview = "Admissions.Review.StartReview";
    public const string Admissions_Review_RequestDocuments = "Admissions.Review.RequestDocuments";
    public const string Admissions_Review_MarkDocumentsComplete = "Admissions.Review.MarkDocumentsComplete";

    // Decision
    public const string Admissions_Decision = "Admissions.Decision";
    public const string Admissions_Decision_Approve = "Admissions.Decision.Approve";
    public const string Admissions_Decision_Reject = "Admissions.Decision.Reject";
    public const string Admissions_Decision_Waitlist = "Admissions.Decision.Waitlist";
    public const string Admissions_Decision_ExtendOffer = "Admissions.Decision.ExtendOffer";

    // Documents
    public const string Admissions_Documents = "Admissions.Documents";
    public const string Admissions_Documents_Upload = "Admissions.Documents.Upload";
    public const string Admissions_Documents_View = "Admissions.Documents.View";
    public const string Admissions_Documents_Download = "Admissions.Documents.Download";
    public const string Admissions_Documents_Verify = "Admissions.Documents.Verify";
    public const string Admissions_Documents_Reject = "Admissions.Documents.Reject";
    public const string Admissions_Documents_Delete = "Admissions.Documents.Delete";

    // Interviews
    public const string Admissions_Interviews = "Admissions.Interviews";
    public const string Admissions_Interviews_View = "Admissions.Interviews.View";
    public const string Admissions_Interviews_Schedule = "Admissions.Interviews.Schedule";
    public const string Admissions_Interviews_Reschedule = "Admissions.Interviews.Reschedule";
    public const string Admissions_Interviews_Cancel = "Admissions.Interviews.Cancel";
    public const string Admissions_Interviews_Conduct = "Admissions.Interviews.Conduct";
    public const string Admissions_Interviews_RecordOutcome = "Admissions.Interviews.RecordOutcome";

    // Assessments
    public const string Admissions_Assessments = "Admissions.Assessments";
    public const string Admissions_Assessments_View = "Admissions.Assessments.View";
    public const string Admissions_Assessments_Schedule = "Admissions.Assessments.Schedule";
    public const string Admissions_Assessments_Cancel = "Admissions.Assessments.Cancel";
    public const string Admissions_Assessments_Conduct = "Admissions.Assessments.Conduct";
    public const string Admissions_Assessments_RecordResults = "Admissions.Assessments.RecordResults";

    // Waitlist
    public const string Admissions_Waitlist = "Admissions.Waitlist";
    public const string Admissions_Waitlist_View = "Admissions.Waitlist.View";
    public const string Admissions_Waitlist_ViewAll = "Admissions.Waitlist.ViewAll";
    public const string Admissions_Waitlist_OfferPosition = "Admissions.Waitlist.OfferPosition";
    public const string Admissions_Waitlist_AcceptOffer = "Admissions.Waitlist.AcceptOffer";
    public const string Admissions_Waitlist_DeclineOffer = "Admissions.Waitlist.DeclineOffer";
    public const string Admissions_Waitlist_Withdraw = "Admissions.Waitlist.Withdraw";

    // Enrollment
    public const string Admissions_Enrollment = "Admissions.Enrollment";
    public const string Admissions_Enrollment_View = "Admissions.Enrollment.View";
    public const string Admissions_Enrollment_AcceptOffer = "Admissions.Enrollment.AcceptOffer";
    public const string Admissions_Enrollment_SubmitForms = "Admissions.Enrollment.SubmitForms";
    public const string Admissions_Enrollment_AssignClass = "Admissions.Enrollment.AssignClass";
    public const string Admissions_Enrollment_Complete = "Admissions.Enrollment.Complete";

    // Settings
    public const string Admissions_Settings = "Admissions.Settings";
    public const string Admissions_Settings_View = "Admissions.Settings.View";
    public const string Admissions_Settings_Manage = "Admissions.Settings.Manage";
    public const string Admissions_Settings_OpenApplications = "Admissions.Settings.OpenApplications";
    public const string Admissions_Settings_CloseApplications = "Admissions.Settings.CloseApplications";
    public const string Admissions_Settings_UpdateCapacity = "Admissions.Settings.UpdateCapacity";

    #endregion

    #region Academic Module

    public const string Academic = "Academic";

    // Students
    public const string Academic_Students = "Academic.Students";
    public const string Academic_Students_View = "Academic.Students.View";
    public const string Academic_Students_ViewAll = "Academic.Students.ViewAll";
    public const string Academic_Students_Create = "Academic.Students.Create";
    public const string Academic_Students_Edit = "Academic.Students.Edit";
    public const string Academic_Students_Delete = "Academic.Students.Delete";
    public const string Academic_Students_AssignClass = "Academic.Students.AssignClass";

    // Teachers
    public const string Academic_Teachers = "Academic.Teachers";
    public const string Academic_Teachers_View = "Academic.Teachers.View";
    public const string Academic_Teachers_ViewAll = "Academic.Teachers.ViewAll";
    public const string Academic_Teachers_Create = "Academic.Teachers.Create";
    public const string Academic_Teachers_Edit = "Academic.Teachers.Edit";
    public const string Academic_Teachers_Delete = "Academic.Teachers.Delete";
    public const string Academic_Teachers_AssignSubject = "Academic.Teachers.AssignSubject";

    // Parents
    public const string Academic_Parents = "Academic.Parents";
    public const string Academic_Parents_View = "Academic.Parents.View";
    public const string Academic_Parents_ViewAll = "Academic.Parents.ViewAll";
    public const string Academic_Parents_Create = "Academic.Parents.Create";
    public const string Academic_Parents_Edit = "Academic.Parents.Edit";
    public const string Academic_Parents_Delete = "Academic.Parents.Delete";
    public const string Academic_Teachers_AssignClass = "Academic.Teachers.AssignClass";

    // Grades
    public const string Academic_Grades = "Academic.Grades";
    public const string Academic_Grades_View = "Academic.Grades.View";
    public const string Academic_Grades_Manage = "Academic.Grades.Manage";

    // Subjects
    public const string Academic_Subjects = "Academic.Subjects";
    public const string Academic_Subjects_View = "Academic.Subjects.View";
    public const string Academic_Subjects_Manage = "Academic.Subjects.Manage";

    // Classes
    public const string Academic_Classes = "Academic.Classes";
    public const string Academic_Classes_View = "Academic.Classes.View";
    public const string Academic_Classes_Create = "Academic.Classes.Create";
    public const string Academic_Classes_Edit = "Academic.Classes.Edit";
    public const string Academic_Classes_Delete = "Academic.Classes.Delete";

    // Timetables
    public const string Academic_Timetables = "Academic.Timetables";
    public const string Academic_Timetables_View = "Academic.Timetables.View";
    public const string Academic_Timetables_Create = "Academic.Timetables.Create";
    public const string Academic_Timetables_Edit = "Academic.Timetables.Edit";
    public const string Academic_Timetables_Delete = "Academic.Timetables.Delete";
    public const string Academic_Timetables_Publish = "Academic.Timetables.Publish";
    public const string Academic_Timetables_MakeVariations = "Academic.Timetables.MakeVariations";

    // Attendance
    public const string Academic_Attendance = "Academic.Attendance";
    public const string Academic_Attendance_View = "Academic.Attendance.View";
    public const string Academic_Attendance_ViewAll = "Academic.Attendance.ViewAll";
    public const string Academic_Attendance_Capture = "Academic.Attendance.Capture";
    public const string Academic_Attendance_Edit = "Academic.Attendance.Edit";
    public const string Academic_Attendance_Reports = "Academic.Attendance.Reports";

    // Calendar
    public const string Academic_Calendar = "Academic.Calendar";
    public const string Academic_Calendar_View = "Academic.Calendar.View";
    public const string Academic_Calendar_Manage = "Academic.Calendar.Manage";
    public const string Academic_AcademicYears_Manage = "Academic.AcademicYears.Manage";
    public const string Academic_Terms_Manage = "Academic.Terms.Manage";

    #endregion

    #region Financial Module

    public const string Financial = "Financial";

    // Fee Structures
    public const string Financial_FeeStructures = "Financial.FeeStructures";
    public const string Financial_FeeStructures_View = "Financial.FeeStructures.View";
    public const string Financial_FeeStructures_Create = "Financial.FeeStructures.Create";
    public const string Financial_FeeStructures_Edit = "Financial.FeeStructures.Edit";
    public const string Financial_FeeStructures_Delete = "Financial.FeeStructures.Delete";
    public const string Financial_FeeStructures_Approve = "Financial.FeeStructures.Approve";

    // Payments
    public const string Financial_Payments = "Financial.Payments";
    public const string Financial_Payments_View = "Financial.Payments.View";
    public const string Financial_Payments_ViewAll = "Financial.Payments.ViewAll";
    public const string Financial_Payments_Process = "Financial.Payments.Process";
    public const string Financial_Payments_RecordManual = "Financial.Payments.RecordManual";
    public const string Financial_Payments_Reconcile = "Financial.Payments.Reconcile";
    public const string Financial_Payments_Void = "Financial.Payments.Void";

    // Receipts
    public const string Financial_Receipts = "Financial.Receipts";
    public const string Financial_Receipts_View = "Financial.Receipts.View";
    public const string Financial_Receipts_Download = "Financial.Receipts.Download";
    public const string Financial_Receipts_Void = "Financial.Receipts.Void";
    public const string Financial_Receipts_Resend = "Financial.Receipts.Resend";

    // Reports
    public const string Financial_Reports = "Financial.Reports";
    public const string Financial_Reports_OutstandingFees = "Financial.Reports.OutstandingFees";
    public const string Financial_Reports_Income = "Financial.Reports.Income";
    public const string Financial_Reports_Collections = "Financial.Reports.Collections";
    public const string Financial_Reports_Export = "Financial.Reports.Export";

    // Statements
    public const string Financial_Statements = "Financial.Statements";
    public const string Financial_Statements_View = "Financial.Statements.View";
    public const string Financial_Statements_Generate = "Financial.Statements.Generate";

    #endregion

    #region Assessment Module

    public const string Assessment = "Assessment";

    // Marks
    public const string Assessment_Marks = "Assessment.Marks";
    public const string Assessment_Marks_View = "Assessment.Marks.View";
    public const string Assessment_Marks_ViewAll = "Assessment.Marks.ViewAll";
    public const string Assessment_Marks_Create = "Assessment.Marks.Create";
    public const string Assessment_Marks_Edit = "Assessment.Marks.Edit";
    public const string Assessment_Marks_Delete = "Assessment.Marks.Delete";
    public const string Assessment_Marks_Publish = "Assessment.Marks.Publish";
    public const string Assessment_Marks_Unlock = "Assessment.Marks.Unlock";
    public const string Assessment_Marks_Import = "Assessment.Marks.Import";

    // Quizzes
    public const string Assessment_Quizzes = "Assessment.Quizzes";
    public const string Assessment_Quizzes_View = "Assessment.Quizzes.View";
    public const string Assessment_Quizzes_Create = "Assessment.Quizzes.Create";
    public const string Assessment_Quizzes_Edit = "Assessment.Quizzes.Edit";
    public const string Assessment_Quizzes_Delete = "Assessment.Quizzes.Delete";
    public const string Assessment_Quizzes_Publish = "Assessment.Quizzes.Publish";
    public const string Assessment_Quizzes_Attempt = "Assessment.Quizzes.Attempt";
    public const string Assessment_Quizzes_ViewResults = "Assessment.Quizzes.ViewResults";

    // Report Cards
    public const string Assessment_ReportCards = "Assessment.ReportCards";
    public const string Assessment_ReportCards_View = "Assessment.ReportCards.View";
    public const string Assessment_ReportCards_Generate = "Assessment.ReportCards.Generate";
    public const string Assessment_ReportCards_Publish = "Assessment.ReportCards.Publish";
    public const string Assessment_ReportCards_Download = "Assessment.ReportCards.Download";

    // Feedback
    public const string Assessment_Feedback = "Assessment.Feedback";
    public const string Assessment_Feedback_View = "Assessment.Feedback.View";
    public const string Assessment_Feedback_Create = "Assessment.Feedback.Create";
    public const string Assessment_Feedback_Edit = "Assessment.Feedback.Edit";
    public const string Assessment_Feedback_Delete = "Assessment.Feedback.Delete";

    #endregion

    #region Communication Module

    public const string Communication = "Communication";

    // Announcements
    public const string Communication_Announcements = "Communication.Announcements";
    public const string Communication_Announcements_View = "Communication.Announcements.View";
    public const string Communication_Announcements_Create = "Communication.Announcements.Create";
    public const string Communication_Announcements_Edit = "Communication.Announcements.Edit";
    public const string Communication_Announcements_Delete = "Communication.Announcements.Delete";
    public const string Communication_Announcements_SendSchoolWide = "Communication.Announcements.SendSchoolWide";

    // Messages
    public const string Communication_Messages = "Communication.Messages";
    public const string Communication_Messages_View = "Communication.Messages.View";
    public const string Communication_Messages_Send = "Communication.Messages.Send";
    public const string Communication_Messages_SendToAll = "Communication.Messages.SendToAll";
    public const string Communication_Messages_Delete = "Communication.Messages.Delete";

    // Notifications
    public const string Communication_Notifications = "Communication.Notifications";
    public const string Communication_Notifications_View = "Communication.Notifications.View";
    public const string Communication_Notifications_ManagePreferences = "Communication.Notifications.ManagePreferences";
    public const string Communication_Notifications_Configure = "Communication.Notifications.Configure";

    // Documents (shared documents, not application documents)
    public const string Communication_Documents = "Communication.Documents";
    public const string Communication_Documents_View = "Communication.Documents.View";
    public const string Communication_Documents_Upload = "Communication.Documents.Upload";
    public const string Communication_Documents_Download = "Communication.Documents.Download";
    public const string Communication_Documents_Delete = "Communication.Documents.Delete";
    public const string Communication_Documents_Approve = "Communication.Documents.Approve";

    #endregion

    #region Learning Module

    public const string Learning = "Learning";

    // Materials
    public const string Learning_Materials = "Learning.Materials";
    public const string Learning_Materials_View = "Learning.Materials.View";
    public const string Learning_Materials_Upload = "Learning.Materials.Upload";
    public const string Learning_Materials_Edit = "Learning.Materials.Edit";
    public const string Learning_Materials_Delete = "Learning.Materials.Delete";
    public const string Learning_Materials_Download = "Learning.Materials.Download";
    public const string Learning_Materials_ManageVersions = "Learning.Materials.ManageVersions";

    // Lessons
    public const string Learning_Lessons = "Learning.Lessons";
    public const string Learning_Lessons_View = "Learning.Lessons.View";
    public const string Learning_Lessons_Schedule = "Learning.Lessons.Schedule";
    public const string Learning_Lessons_Host = "Learning.Lessons.Host";
    public const string Learning_Lessons_Join = "Learning.Lessons.Join";
    public const string Learning_Lessons_Cancel = "Learning.Lessons.Cancel";

    // Recordings
    public const string Learning_Recordings = "Learning.Recordings";
    public const string Learning_Recordings_View = "Learning.Recordings.View";
    public const string Learning_Recordings_Upload = "Learning.Recordings.Upload";
    public const string Learning_Recordings_Delete = "Learning.Recordings.Delete";

    #endregion

    #region Administration Module

    public const string Administration = "Administration";

    // Tenants (Host only)
    public const string Administration_Tenants = "Administration.Tenants";
    public const string Administration_Tenants_View = "Administration.Tenants.View";
    public const string Administration_Tenants_Create = "Administration.Tenants.Create";
    public const string Administration_Tenants_Edit = "Administration.Tenants.Edit";
    public const string Administration_Tenants_Delete = "Administration.Tenants.Delete";
    public const string Administration_Tenants_Impersonate = "Administration.Tenants.Impersonate";

    // Users
    public const string Administration_Users = "Administration.Users";
    public const string Administration_Users_View = "Administration.Users.View";
    public const string Administration_Users_Create = "Administration.Users.Create";
    public const string Administration_Users_Edit = "Administration.Users.Edit";
    public const string Administration_Users_Delete = "Administration.Users.Delete";
    public const string Administration_Users_ResetPassword = "Administration.Users.ResetPassword";
    public const string Administration_Users_Unlock = "Administration.Users.Unlock";

    // Roles
    public const string Administration_Roles = "Administration.Roles";
    public const string Administration_Roles_View = "Administration.Roles.View";
    public const string Administration_Roles_Manage = "Administration.Roles.Manage";
    public const string Administration_Permissions_Assign = "Administration.Permissions.Assign";

    // Settings
    public const string Administration_Settings = "Administration.Settings";
    public const string Administration_Settings_View = "Administration.Settings.View";
    public const string Administration_Settings_Edit = "Administration.Settings.Edit";

    // Audit Logs
    public const string Administration_AuditLogs = "Administration.AuditLogs";
    public const string Administration_AuditLogs_View = "Administration.AuditLogs.View";
    public const string Administration_AuditLogs_Export = "Administration.AuditLogs.Export";

    // Backups (Host only)
    public const string Administration_Backups = "Administration.Backups";
    public const string Administration_Backups_View = "Administration.Backups.View";
    public const string Administration_Backups_Create = "Administration.Backups.Create";
    public const string Administration_Backups_Restore = "Administration.Backups.Restore";

    #endregion
}
