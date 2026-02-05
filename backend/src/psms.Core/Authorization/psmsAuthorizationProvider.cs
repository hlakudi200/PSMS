using Abp.Authorization;
using Abp.Localization;
using Abp.MultiTenancy;

namespace psms.Authorization;

/// <summary>
/// Authorization provider for PSMS (Private School Management System).
/// Defines all permissions with hierarchical structure.
/// See PSMS-Permissions-Matrix.md for role-permission mappings.
/// </summary>
public class psmsAuthorizationProvider : AuthorizationProvider
{
    public override void SetPermissions(IPermissionDefinitionContext context)
    {
        // System/Pages Permissions (ABP Default)
        SetSystemPermissions(context);

        // Module Permissions
        SetAdmissionsPermissions(context);
        SetAcademicPermissions(context);
        SetFinancialPermissions(context);
        SetAssessmentPermissions(context);
        SetCommunicationPermissions(context);
        SetLearningPermissions(context);
        SetAdministrationPermissions(context);
    }

    #region System Permissions

    private void SetSystemPermissions(IPermissionDefinitionContext context)
    {
        context.CreatePermission(PermissionNames.Pages_Users, L("Users"));
        context.CreatePermission(PermissionNames.Pages_Users_Activation, L("UsersActivation"));
        context.CreatePermission(PermissionNames.Pages_Roles, L("Roles"));
        context.CreatePermission(PermissionNames.Pages_Tenants, L("Tenants"), multiTenancySides: MultiTenancySides.Host);
    }

    #endregion

    #region Admissions Module

    private void SetAdmissionsPermissions(IPermissionDefinitionContext context)
    {
        var admissions = context.CreatePermission(PermissionNames.Admissions, L("Admissions"));

        // Applications
        var applications = admissions.CreateChildPermission(PermissionNames.Admissions_Applications, L("Applications"));
        applications.CreateChildPermission(PermissionNames.Admissions_Applications_View, L("ViewApplications"));
        applications.CreateChildPermission(PermissionNames.Admissions_Applications_ViewAll, L("ViewAllApplications"));
        applications.CreateChildPermission(PermissionNames.Admissions_Applications_Create, L("CreateApplication"));
        applications.CreateChildPermission(PermissionNames.Admissions_Applications_Edit, L("EditApplication"));
        applications.CreateChildPermission(PermissionNames.Admissions_Applications_Delete, L("DeleteApplication"));
        applications.CreateChildPermission(PermissionNames.Admissions_Applications_Submit, L("SubmitApplication"));
        applications.CreateChildPermission(PermissionNames.Admissions_Applications_Withdraw, L("WithdrawApplication"));

        // Review
        var review = admissions.CreateChildPermission(PermissionNames.Admissions_Review, L("Review"));
        review.CreateChildPermission(PermissionNames.Admissions_Review_StartReview, L("StartReview"));
        review.CreateChildPermission(PermissionNames.Admissions_Review_RequestDocuments, L("RequestDocuments"));
        review.CreateChildPermission(PermissionNames.Admissions_Review_MarkDocumentsComplete, L("MarkDocumentsComplete"));

        // Decision
        var decision = admissions.CreateChildPermission(PermissionNames.Admissions_Decision, L("Decision"));
        decision.CreateChildPermission(PermissionNames.Admissions_Decision_Approve, L("ApproveApplication"));
        decision.CreateChildPermission(PermissionNames.Admissions_Decision_Reject, L("RejectApplication"));
        decision.CreateChildPermission(PermissionNames.Admissions_Decision_Waitlist, L("WaitlistApplication"));
        decision.CreateChildPermission(PermissionNames.Admissions_Decision_ExtendOffer, L("ExtendOffer"));

        // Documents
        var documents = admissions.CreateChildPermission(PermissionNames.Admissions_Documents, L("Documents"));
        documents.CreateChildPermission(PermissionNames.Admissions_Documents_Upload, L("UploadDocuments"));
        documents.CreateChildPermission(PermissionNames.Admissions_Documents_View, L("ViewDocuments"));
        documents.CreateChildPermission(PermissionNames.Admissions_Documents_Download, L("DownloadDocuments"));
        documents.CreateChildPermission(PermissionNames.Admissions_Documents_Verify, L("VerifyDocuments"));
        documents.CreateChildPermission(PermissionNames.Admissions_Documents_Reject, L("RejectDocuments"));
        documents.CreateChildPermission(PermissionNames.Admissions_Documents_Delete, L("DeleteDocuments"));

        // Interviews
        var interviews = admissions.CreateChildPermission(PermissionNames.Admissions_Interviews, L("Interviews"));
        interviews.CreateChildPermission(PermissionNames.Admissions_Interviews_View, L("ViewInterviews"));
        interviews.CreateChildPermission(PermissionNames.Admissions_Interviews_Schedule, L("ScheduleInterview"));
        interviews.CreateChildPermission(PermissionNames.Admissions_Interviews_Reschedule, L("RescheduleInterview"));
        interviews.CreateChildPermission(PermissionNames.Admissions_Interviews_Cancel, L("CancelInterview"));
        interviews.CreateChildPermission(PermissionNames.Admissions_Interviews_Conduct, L("ConductInterview"));
        interviews.CreateChildPermission(PermissionNames.Admissions_Interviews_RecordOutcome, L("RecordInterviewOutcome"));

        // Assessments
        var assessments = admissions.CreateChildPermission(PermissionNames.Admissions_Assessments, L("Assessments"));
        assessments.CreateChildPermission(PermissionNames.Admissions_Assessments_View, L("ViewAssessments"));
        assessments.CreateChildPermission(PermissionNames.Admissions_Assessments_Schedule, L("ScheduleAssessment"));
        assessments.CreateChildPermission(PermissionNames.Admissions_Assessments_Cancel, L("CancelAssessment"));
        assessments.CreateChildPermission(PermissionNames.Admissions_Assessments_Conduct, L("ConductAssessment"));
        assessments.CreateChildPermission(PermissionNames.Admissions_Assessments_RecordResults, L("RecordAssessmentResults"));

        // Waitlist
        var waitlist = admissions.CreateChildPermission(PermissionNames.Admissions_Waitlist, L("Waitlist"));
        waitlist.CreateChildPermission(PermissionNames.Admissions_Waitlist_View, L("ViewWaitlist"));
        waitlist.CreateChildPermission(PermissionNames.Admissions_Waitlist_ViewAll, L("ViewAllWaitlist"));
        waitlist.CreateChildPermission(PermissionNames.Admissions_Waitlist_OfferPosition, L("OfferWaitlistPosition"));
        waitlist.CreateChildPermission(PermissionNames.Admissions_Waitlist_AcceptOffer, L("AcceptWaitlistOffer"));
        waitlist.CreateChildPermission(PermissionNames.Admissions_Waitlist_DeclineOffer, L("DeclineWaitlistOffer"));
        waitlist.CreateChildPermission(PermissionNames.Admissions_Waitlist_Withdraw, L("WithdrawFromWaitlist"));

        // Enrollment
        var enrollment = admissions.CreateChildPermission(PermissionNames.Admissions_Enrollment, L("Enrollment"));
        enrollment.CreateChildPermission(PermissionNames.Admissions_Enrollment_View, L("ViewEnrollment"));
        enrollment.CreateChildPermission(PermissionNames.Admissions_Enrollment_AcceptOffer, L("AcceptEnrollmentOffer"));
        enrollment.CreateChildPermission(PermissionNames.Admissions_Enrollment_SubmitForms, L("SubmitEnrollmentForms"));
        enrollment.CreateChildPermission(PermissionNames.Admissions_Enrollment_AssignClass, L("AssignEnrollmentClass"));
        enrollment.CreateChildPermission(PermissionNames.Admissions_Enrollment_Complete, L("CompleteEnrollment"));

        // Settings
        var settings = admissions.CreateChildPermission(PermissionNames.Admissions_Settings, L("AdmissionSettings"));
        settings.CreateChildPermission(PermissionNames.Admissions_Settings_View, L("ViewAdmissionSettings"));
        settings.CreateChildPermission(PermissionNames.Admissions_Settings_Manage, L("ManageAdmissionSettings"));
        settings.CreateChildPermission(PermissionNames.Admissions_Settings_OpenApplications, L("OpenApplications"));
        settings.CreateChildPermission(PermissionNames.Admissions_Settings_CloseApplications, L("CloseApplications"));
        settings.CreateChildPermission(PermissionNames.Admissions_Settings_UpdateCapacity, L("UpdateCapacity"));
    }

    #endregion

    #region Academic Module

    private void SetAcademicPermissions(IPermissionDefinitionContext context)
    {
        var academic = context.CreatePermission(PermissionNames.Academic, L("Academic"));

        // Students
        var students = academic.CreateChildPermission(PermissionNames.Academic_Students, L("Students"));
        students.CreateChildPermission(PermissionNames.Academic_Students_View, L("ViewStudents"));
        students.CreateChildPermission(PermissionNames.Academic_Students_ViewAll, L("ViewAllStudents"));
        students.CreateChildPermission(PermissionNames.Academic_Students_Create, L("CreateStudent"));
        students.CreateChildPermission(PermissionNames.Academic_Students_Edit, L("EditStudent"));
        students.CreateChildPermission(PermissionNames.Academic_Students_Delete, L("DeleteStudent"));
        students.CreateChildPermission(PermissionNames.Academic_Students_AssignClass, L("AssignStudentClass"));

        // Teachers
        var teachers = academic.CreateChildPermission(PermissionNames.Academic_Teachers, L("Teachers"));
        teachers.CreateChildPermission(PermissionNames.Academic_Teachers_View, L("ViewTeachers"));
        teachers.CreateChildPermission(PermissionNames.Academic_Teachers_ViewAll, L("ViewAllTeachers"));
        teachers.CreateChildPermission(PermissionNames.Academic_Teachers_Create, L("CreateTeacher"));
        teachers.CreateChildPermission(PermissionNames.Academic_Teachers_Edit, L("EditTeacher"));
        teachers.CreateChildPermission(PermissionNames.Academic_Teachers_Delete, L("DeleteTeacher"));
        teachers.CreateChildPermission(PermissionNames.Academic_Teachers_AssignSubject, L("AssignTeacherSubject"));
        teachers.CreateChildPermission(PermissionNames.Academic_Teachers_AssignClass, L("AssignTeacherClass"));

        // Parents
        var parents = academic.CreateChildPermission(PermissionNames.Academic_Parents, L("Parents"));
        parents.CreateChildPermission(PermissionNames.Academic_Parents_View, L("ViewParents"));
        parents.CreateChildPermission(PermissionNames.Academic_Parents_ViewAll, L("ViewAllParents"));
        parents.CreateChildPermission(PermissionNames.Academic_Parents_Create, L("CreateParent"));
        parents.CreateChildPermission(PermissionNames.Academic_Parents_Edit, L("EditParent"));
        parents.CreateChildPermission(PermissionNames.Academic_Parents_Delete, L("DeleteParent"));

        // Grades
        var grades = academic.CreateChildPermission(PermissionNames.Academic_Grades, L("Grades"));
        grades.CreateChildPermission(PermissionNames.Academic_Grades_View, L("ViewGrades"));
        grades.CreateChildPermission(PermissionNames.Academic_Grades_Manage, L("ManageGrades"));

        // Subjects
        var subjects = academic.CreateChildPermission(PermissionNames.Academic_Subjects, L("Subjects"));
        subjects.CreateChildPermission(PermissionNames.Academic_Subjects_View, L("ViewSubjects"));
        subjects.CreateChildPermission(PermissionNames.Academic_Subjects_Manage, L("ManageSubjects"));

        // Classes
        var classes = academic.CreateChildPermission(PermissionNames.Academic_Classes, L("Classes"));
        classes.CreateChildPermission(PermissionNames.Academic_Classes_View, L("ViewClasses"));
        classes.CreateChildPermission(PermissionNames.Academic_Classes_Create, L("CreateClass"));
        classes.CreateChildPermission(PermissionNames.Academic_Classes_Edit, L("EditClass"));
        classes.CreateChildPermission(PermissionNames.Academic_Classes_Delete, L("DeleteClass"));

        // Timetables
        var timetables = academic.CreateChildPermission(PermissionNames.Academic_Timetables, L("Timetables"));
        timetables.CreateChildPermission(PermissionNames.Academic_Timetables_View, L("ViewTimetables"));
        timetables.CreateChildPermission(PermissionNames.Academic_Timetables_Create, L("CreateTimetable"));
        timetables.CreateChildPermission(PermissionNames.Academic_Timetables_Edit, L("EditTimetable"));
        timetables.CreateChildPermission(PermissionNames.Academic_Timetables_Delete, L("DeleteTimetable"));
        timetables.CreateChildPermission(PermissionNames.Academic_Timetables_Publish, L("PublishTimetable"));
        timetables.CreateChildPermission(PermissionNames.Academic_Timetables_MakeVariations, L("MakeTimetableVariations"));

        // Attendance
        var attendance = academic.CreateChildPermission(PermissionNames.Academic_Attendance, L("Attendance"));
        attendance.CreateChildPermission(PermissionNames.Academic_Attendance_View, L("ViewAttendance"));
        attendance.CreateChildPermission(PermissionNames.Academic_Attendance_ViewAll, L("ViewAllAttendance"));
        attendance.CreateChildPermission(PermissionNames.Academic_Attendance_Capture, L("CaptureAttendance"));
        attendance.CreateChildPermission(PermissionNames.Academic_Attendance_Edit, L("EditAttendance"));
        attendance.CreateChildPermission(PermissionNames.Academic_Attendance_Reports, L("AttendanceReports"));

        // Calendar
        var calendar = academic.CreateChildPermission(PermissionNames.Academic_Calendar, L("Calendar"));
        calendar.CreateChildPermission(PermissionNames.Academic_Calendar_View, L("ViewCalendar"));
        calendar.CreateChildPermission(PermissionNames.Academic_Calendar_Manage, L("ManageCalendar"));
        calendar.CreateChildPermission(PermissionNames.Academic_AcademicYears_Manage, L("ManageAcademicYears"));
        calendar.CreateChildPermission(PermissionNames.Academic_Terms_Manage, L("ManageTerms"));
    }

    #endregion

    #region Financial Module

    private void SetFinancialPermissions(IPermissionDefinitionContext context)
    {
        var financial = context.CreatePermission(PermissionNames.Financial, L("Financial"));

        // Fee Structures
        var feeStructures = financial.CreateChildPermission(PermissionNames.Financial_FeeStructures, L("FeeStructures"));
        feeStructures.CreateChildPermission(PermissionNames.Financial_FeeStructures_View, L("ViewFeeStructures"));
        feeStructures.CreateChildPermission(PermissionNames.Financial_FeeStructures_Create, L("CreateFeeStructure"));
        feeStructures.CreateChildPermission(PermissionNames.Financial_FeeStructures_Edit, L("EditFeeStructure"));
        feeStructures.CreateChildPermission(PermissionNames.Financial_FeeStructures_Delete, L("DeleteFeeStructure"));
        feeStructures.CreateChildPermission(PermissionNames.Financial_FeeStructures_Approve, L("ApproveFeeStructure"));

        // Payments
        var payments = financial.CreateChildPermission(PermissionNames.Financial_Payments, L("Payments"));
        payments.CreateChildPermission(PermissionNames.Financial_Payments_View, L("ViewPayments"));
        payments.CreateChildPermission(PermissionNames.Financial_Payments_ViewAll, L("ViewAllPayments"));
        payments.CreateChildPermission(PermissionNames.Financial_Payments_Process, L("ProcessPayment"));
        payments.CreateChildPermission(PermissionNames.Financial_Payments_RecordManual, L("RecordManualPayment"));
        payments.CreateChildPermission(PermissionNames.Financial_Payments_Reconcile, L("ReconcilePayments"));
        payments.CreateChildPermission(PermissionNames.Financial_Payments_Void, L("VoidPayment"));

        // Receipts
        var receipts = financial.CreateChildPermission(PermissionNames.Financial_Receipts, L("Receipts"));
        receipts.CreateChildPermission(PermissionNames.Financial_Receipts_View, L("ViewReceipts"));
        receipts.CreateChildPermission(PermissionNames.Financial_Receipts_Download, L("DownloadReceipts"));
        receipts.CreateChildPermission(PermissionNames.Financial_Receipts_Void, L("VoidReceipt"));
        receipts.CreateChildPermission(PermissionNames.Financial_Receipts_Resend, L("ResendReceipt"));

        // Reports
        var reports = financial.CreateChildPermission(PermissionNames.Financial_Reports, L("FinancialReports"));
        reports.CreateChildPermission(PermissionNames.Financial_Reports_OutstandingFees, L("OutstandingFeesReport"));
        reports.CreateChildPermission(PermissionNames.Financial_Reports_Income, L("IncomeReport"));
        reports.CreateChildPermission(PermissionNames.Financial_Reports_Collections, L("CollectionsReport"));
        reports.CreateChildPermission(PermissionNames.Financial_Reports_Export, L("ExportFinancialReports"));

        // Statements
        var statements = financial.CreateChildPermission(PermissionNames.Financial_Statements, L("Statements"));
        statements.CreateChildPermission(PermissionNames.Financial_Statements_View, L("ViewStatements"));
        statements.CreateChildPermission(PermissionNames.Financial_Statements_Generate, L("GenerateStatement"));
    }

    #endregion

    #region Assessment Module

    private void SetAssessmentPermissions(IPermissionDefinitionContext context)
    {
        var assessment = context.CreatePermission(PermissionNames.Assessment, L("Assessment"));

        // Marks
        var marks = assessment.CreateChildPermission(PermissionNames.Assessment_Marks, L("Marks"));
        marks.CreateChildPermission(PermissionNames.Assessment_Marks_View, L("ViewMarks"));
        marks.CreateChildPermission(PermissionNames.Assessment_Marks_ViewAll, L("ViewAllMarks"));
        marks.CreateChildPermission(PermissionNames.Assessment_Marks_Create, L("CreateMarks"));
        marks.CreateChildPermission(PermissionNames.Assessment_Marks_Edit, L("EditMarks"));
        marks.CreateChildPermission(PermissionNames.Assessment_Marks_Delete, L("DeleteMarks"));
        marks.CreateChildPermission(PermissionNames.Assessment_Marks_Publish, L("PublishMarks"));
        marks.CreateChildPermission(PermissionNames.Assessment_Marks_Unlock, L("UnlockMarks"));
        marks.CreateChildPermission(PermissionNames.Assessment_Marks_Import, L("ImportMarks"));

        // Quizzes
        var quizzes = assessment.CreateChildPermission(PermissionNames.Assessment_Quizzes, L("Quizzes"));
        quizzes.CreateChildPermission(PermissionNames.Assessment_Quizzes_View, L("ViewQuizzes"));
        quizzes.CreateChildPermission(PermissionNames.Assessment_Quizzes_Create, L("CreateQuiz"));
        quizzes.CreateChildPermission(PermissionNames.Assessment_Quizzes_Edit, L("EditQuiz"));
        quizzes.CreateChildPermission(PermissionNames.Assessment_Quizzes_Delete, L("DeleteQuiz"));
        quizzes.CreateChildPermission(PermissionNames.Assessment_Quizzes_Publish, L("PublishQuiz"));
        quizzes.CreateChildPermission(PermissionNames.Assessment_Quizzes_Attempt, L("AttemptQuiz"));
        quizzes.CreateChildPermission(PermissionNames.Assessment_Quizzes_ViewResults, L("ViewQuizResults"));

        // Report Cards
        var reportCards = assessment.CreateChildPermission(PermissionNames.Assessment_ReportCards, L("ReportCards"));
        reportCards.CreateChildPermission(PermissionNames.Assessment_ReportCards_View, L("ViewReportCards"));
        reportCards.CreateChildPermission(PermissionNames.Assessment_ReportCards_Generate, L("GenerateReportCard"));
        reportCards.CreateChildPermission(PermissionNames.Assessment_ReportCards_Publish, L("PublishReportCards"));
        reportCards.CreateChildPermission(PermissionNames.Assessment_ReportCards_Download, L("DownloadReportCard"));

        // Feedback
        var feedback = assessment.CreateChildPermission(PermissionNames.Assessment_Feedback, L("Feedback"));
        feedback.CreateChildPermission(PermissionNames.Assessment_Feedback_View, L("ViewFeedback"));
        feedback.CreateChildPermission(PermissionNames.Assessment_Feedback_Create, L("CreateFeedback"));
        feedback.CreateChildPermission(PermissionNames.Assessment_Feedback_Edit, L("EditFeedback"));
        feedback.CreateChildPermission(PermissionNames.Assessment_Feedback_Delete, L("DeleteFeedback"));
    }

    #endregion

    #region Communication Module

    private void SetCommunicationPermissions(IPermissionDefinitionContext context)
    {
        var communication = context.CreatePermission(PermissionNames.Communication, L("Communication"));

        // Announcements
        var announcements = communication.CreateChildPermission(PermissionNames.Communication_Announcements, L("Announcements"));
        announcements.CreateChildPermission(PermissionNames.Communication_Announcements_View, L("ViewAnnouncements"));
        announcements.CreateChildPermission(PermissionNames.Communication_Announcements_Create, L("CreateAnnouncement"));
        announcements.CreateChildPermission(PermissionNames.Communication_Announcements_Edit, L("EditAnnouncement"));
        announcements.CreateChildPermission(PermissionNames.Communication_Announcements_Delete, L("DeleteAnnouncement"));
        announcements.CreateChildPermission(PermissionNames.Communication_Announcements_SendSchoolWide, L("SendSchoolWideAnnouncement"));

        // Messages
        var messages = communication.CreateChildPermission(PermissionNames.Communication_Messages, L("Messages"));
        messages.CreateChildPermission(PermissionNames.Communication_Messages_View, L("ViewMessages"));
        messages.CreateChildPermission(PermissionNames.Communication_Messages_Send, L("SendMessage"));
        messages.CreateChildPermission(PermissionNames.Communication_Messages_SendToAll, L("SendMessageToAll"));
        messages.CreateChildPermission(PermissionNames.Communication_Messages_Delete, L("DeleteMessage"));

        // Notifications
        var notifications = communication.CreateChildPermission(PermissionNames.Communication_Notifications, L("Notifications"));
        notifications.CreateChildPermission(PermissionNames.Communication_Notifications_View, L("ViewNotifications"));
        notifications.CreateChildPermission(PermissionNames.Communication_Notifications_ManagePreferences, L("ManageNotificationPreferences"));
        notifications.CreateChildPermission(PermissionNames.Communication_Notifications_Configure, L("ConfigureNotifications"));

        // Documents
        var documents = communication.CreateChildPermission(PermissionNames.Communication_Documents, L("SharedDocuments"));
        documents.CreateChildPermission(PermissionNames.Communication_Documents_View, L("ViewSharedDocuments"));
        documents.CreateChildPermission(PermissionNames.Communication_Documents_Upload, L("UploadSharedDocuments"));
        documents.CreateChildPermission(PermissionNames.Communication_Documents_Download, L("DownloadSharedDocuments"));
        documents.CreateChildPermission(PermissionNames.Communication_Documents_Delete, L("DeleteSharedDocuments"));
        documents.CreateChildPermission(PermissionNames.Communication_Documents_Approve, L("ApproveSharedDocuments"));
    }

    #endregion

    #region Learning Module

    private void SetLearningPermissions(IPermissionDefinitionContext context)
    {
        var learning = context.CreatePermission(PermissionNames.Learning, L("Learning"));

        // Materials
        var materials = learning.CreateChildPermission(PermissionNames.Learning_Materials, L("LearningMaterials"));
        materials.CreateChildPermission(PermissionNames.Learning_Materials_View, L("ViewMaterials"));
        materials.CreateChildPermission(PermissionNames.Learning_Materials_Upload, L("UploadMaterials"));
        materials.CreateChildPermission(PermissionNames.Learning_Materials_Edit, L("EditMaterials"));
        materials.CreateChildPermission(PermissionNames.Learning_Materials_Delete, L("DeleteMaterials"));
        materials.CreateChildPermission(PermissionNames.Learning_Materials_Download, L("DownloadMaterials"));
        materials.CreateChildPermission(PermissionNames.Learning_Materials_ManageVersions, L("ManageMaterialVersions"));

        // Lessons
        var lessons = learning.CreateChildPermission(PermissionNames.Learning_Lessons, L("OnlineLessons"));
        lessons.CreateChildPermission(PermissionNames.Learning_Lessons_View, L("ViewLessons"));
        lessons.CreateChildPermission(PermissionNames.Learning_Lessons_Schedule, L("ScheduleLesson"));
        lessons.CreateChildPermission(PermissionNames.Learning_Lessons_Host, L("HostLesson"));
        lessons.CreateChildPermission(PermissionNames.Learning_Lessons_Join, L("JoinLesson"));
        lessons.CreateChildPermission(PermissionNames.Learning_Lessons_Cancel, L("CancelLesson"));

        // Recordings
        var recordings = learning.CreateChildPermission(PermissionNames.Learning_Recordings, L("LessonRecordings"));
        recordings.CreateChildPermission(PermissionNames.Learning_Recordings_View, L("ViewRecordings"));
        recordings.CreateChildPermission(PermissionNames.Learning_Recordings_Upload, L("UploadRecordings"));
        recordings.CreateChildPermission(PermissionNames.Learning_Recordings_Delete, L("DeleteRecordings"));
    }

    #endregion

    #region Administration Module

    private void SetAdministrationPermissions(IPermissionDefinitionContext context)
    {
        var administration = context.CreatePermission(PermissionNames.Administration, L("Administration"));

        // Tenants (Host only)
        var tenants = administration.CreateChildPermission(PermissionNames.Administration_Tenants, L("Tenants"), multiTenancySides: MultiTenancySides.Host);
        tenants.CreateChildPermission(PermissionNames.Administration_Tenants_View, L("ViewTenants"), multiTenancySides: MultiTenancySides.Host);
        tenants.CreateChildPermission(PermissionNames.Administration_Tenants_Create, L("CreateTenant"), multiTenancySides: MultiTenancySides.Host);
        tenants.CreateChildPermission(PermissionNames.Administration_Tenants_Edit, L("EditTenant"), multiTenancySides: MultiTenancySides.Host);
        tenants.CreateChildPermission(PermissionNames.Administration_Tenants_Delete, L("DeleteTenant"), multiTenancySides: MultiTenancySides.Host);
        tenants.CreateChildPermission(PermissionNames.Administration_Tenants_Impersonate, L("ImpersonateTenant"), multiTenancySides: MultiTenancySides.Host);

        // Users
        var users = administration.CreateChildPermission(PermissionNames.Administration_Users, L("Users"));
        users.CreateChildPermission(PermissionNames.Administration_Users_View, L("ViewUsers"));
        users.CreateChildPermission(PermissionNames.Administration_Users_Create, L("CreateUser"));
        users.CreateChildPermission(PermissionNames.Administration_Users_Edit, L("EditUser"));
        users.CreateChildPermission(PermissionNames.Administration_Users_Delete, L("DeleteUser"));
        users.CreateChildPermission(PermissionNames.Administration_Users_ResetPassword, L("ResetUserPassword"));
        users.CreateChildPermission(PermissionNames.Administration_Users_Unlock, L("UnlockUser"));

        // Roles
        var roles = administration.CreateChildPermission(PermissionNames.Administration_Roles, L("Roles"));
        roles.CreateChildPermission(PermissionNames.Administration_Roles_View, L("ViewRoles"));
        roles.CreateChildPermission(PermissionNames.Administration_Roles_Manage, L("ManageRoles"));
        roles.CreateChildPermission(PermissionNames.Administration_Permissions_Assign, L("AssignPermissions"));

        // Settings
        var settings = administration.CreateChildPermission(PermissionNames.Administration_Settings, L("Settings"));
        settings.CreateChildPermission(PermissionNames.Administration_Settings_View, L("ViewSettings"));
        settings.CreateChildPermission(PermissionNames.Administration_Settings_Edit, L("EditSettings"));

        // Audit Logs
        var auditLogs = administration.CreateChildPermission(PermissionNames.Administration_AuditLogs, L("AuditLogs"));
        auditLogs.CreateChildPermission(PermissionNames.Administration_AuditLogs_View, L("ViewAuditLogs"));
        auditLogs.CreateChildPermission(PermissionNames.Administration_AuditLogs_Export, L("ExportAuditLogs"));

        // Backups (Host only)
        var backups = administration.CreateChildPermission(PermissionNames.Administration_Backups, L("Backups"), multiTenancySides: MultiTenancySides.Host);
        backups.CreateChildPermission(PermissionNames.Administration_Backups_View, L("ViewBackups"), multiTenancySides: MultiTenancySides.Host);
        backups.CreateChildPermission(PermissionNames.Administration_Backups_Create, L("CreateBackup"), multiTenancySides: MultiTenancySides.Host);
        backups.CreateChildPermission(PermissionNames.Administration_Backups_Restore, L("RestoreBackup"), multiTenancySides: MultiTenancySides.Host);
    }

    #endregion

    private static ILocalizableString L(string name)
    {
        return new LocalizableString(name, psmsConsts.LocalizationSourceName);
    }
}
