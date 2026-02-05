using Abp.Authorization.Roles;
using Microsoft.EntityFrameworkCore;
using psms.Authorization;
using psms.Authorization.Roles;
using System.Collections.Generic;
using System.Linq;

namespace psms.EntityFrameworkCore.Seed.Tenants;

/// <summary>
/// Creates default PSMS roles and assigns permissions based on the PSMS-Permissions-Matrix.md
/// </summary>
public class DefaultRolesCreator
{
    private readonly psmsDbContext _context;
    private readonly int _tenantId;

    public DefaultRolesCreator(psmsDbContext context, int tenantId)
    {
        _context = context;
        _tenantId = tenantId;
    }

    public void Create()
    {
        CreateRoleIfNotExists(StaticRoleNames.Tenants.Principal, GetPrincipalPermissions());
        CreateRoleIfNotExists(StaticRoleNames.Tenants.VicePrincipal, GetVicePrincipalPermissions());
        CreateRoleIfNotExists(StaticRoleNames.Tenants.HOD, GetHODPermissions());
        CreateRoleIfNotExists(StaticRoleNames.Tenants.AdmissionsOfficer, GetAdmissionsOfficerPermissions());
        CreateRoleIfNotExists(StaticRoleNames.Tenants.Finance, GetFinancePermissions());
        CreateRoleIfNotExists(StaticRoleNames.Tenants.Teacher, GetTeacherPermissions());
        CreateRoleIfNotExists(StaticRoleNames.Tenants.Parent, GetParentPermissions());
        CreateRoleIfNotExists(StaticRoleNames.Tenants.Student, GetStudentPermissions());
        CreateRoleIfNotExists(StaticRoleNames.Tenants.Applicant, GetApplicantPermissions());
    }

    private void CreateRoleIfNotExists(string roleName, List<string> permissions)
    {
        var role = _context.Roles.IgnoreQueryFilters()
            .FirstOrDefault(r => r.TenantId == _tenantId && r.Name == roleName);

        if (role == null)
        {
            role = _context.Roles.Add(new Role(_tenantId, roleName, roleName) { IsStatic = true }).Entity;
            _context.SaveChanges();
        }

        // Grant permissions to role
        var grantedPermissions = _context.Permissions.IgnoreQueryFilters()
            .OfType<RolePermissionSetting>()
            .Where(p => p.TenantId == _tenantId && p.RoleId == role.Id)
            .Select(p => p.Name)
            .ToList();

        var permissionsToGrant = permissions.Where(p => !grantedPermissions.Contains(p)).ToList();

        if (permissionsToGrant.Any())
        {
            _context.Permissions.AddRange(
                permissionsToGrant.Select(permission => new RolePermissionSetting
                {
                    TenantId = _tenantId,
                    Name = permission,
                    IsGranted = true,
                    RoleId = role.Id
                })
            );
            _context.SaveChanges();
        }
    }

    #region Role Permission Definitions

    /// <summary>
    /// Principal has full access to most areas except some admin functions
    /// </summary>
    private static List<string> GetPrincipalPermissions()
    {
        return new List<string>
        {
            // Admissions - Full access including decisions
            PermissionNames.Admissions,
            PermissionNames.Admissions_Applications,
            PermissionNames.Admissions_Applications_View,
            PermissionNames.Admissions_Applications_ViewAll,
            PermissionNames.Admissions_Applications_Create,
            PermissionNames.Admissions_Applications_Edit,
            PermissionNames.Admissions_Applications_Submit,
            PermissionNames.Admissions_Applications_Withdraw,
            PermissionNames.Admissions_Review,
            PermissionNames.Admissions_Review_StartReview,
            PermissionNames.Admissions_Review_RequestDocuments,
            PermissionNames.Admissions_Review_MarkDocumentsComplete,
            PermissionNames.Admissions_Decision,
            PermissionNames.Admissions_Decision_Approve,
            PermissionNames.Admissions_Decision_Reject,
            PermissionNames.Admissions_Decision_Waitlist,
            PermissionNames.Admissions_Decision_ExtendOffer,
            PermissionNames.Admissions_Documents,
            PermissionNames.Admissions_Documents_Upload,
            PermissionNames.Admissions_Documents_View,
            PermissionNames.Admissions_Documents_Download,
            PermissionNames.Admissions_Documents_Verify,
            PermissionNames.Admissions_Documents_Reject,
            PermissionNames.Admissions_Documents_Delete,
            PermissionNames.Admissions_Interviews,
            PermissionNames.Admissions_Interviews_View,
            PermissionNames.Admissions_Interviews_Schedule,
            PermissionNames.Admissions_Interviews_Reschedule,
            PermissionNames.Admissions_Interviews_Cancel,
            PermissionNames.Admissions_Interviews_Conduct,
            PermissionNames.Admissions_Interviews_RecordOutcome,
            PermissionNames.Admissions_Assessments,
            PermissionNames.Admissions_Assessments_View,
            PermissionNames.Admissions_Assessments_Schedule,
            PermissionNames.Admissions_Assessments_Cancel,
            PermissionNames.Admissions_Assessments_Conduct,
            PermissionNames.Admissions_Assessments_RecordResults,
            PermissionNames.Admissions_Waitlist,
            PermissionNames.Admissions_Waitlist_View,
            PermissionNames.Admissions_Waitlist_ViewAll,
            PermissionNames.Admissions_Waitlist_OfferPosition,
            PermissionNames.Admissions_Enrollment,
            PermissionNames.Admissions_Enrollment_View,
            PermissionNames.Admissions_Enrollment_SubmitForms,
            PermissionNames.Admissions_Enrollment_AssignClass,
            PermissionNames.Admissions_Enrollment_Complete,
            PermissionNames.Admissions_Settings,
            PermissionNames.Admissions_Settings_View,
            PermissionNames.Admissions_Settings_Manage,
            PermissionNames.Admissions_Settings_OpenApplications,
            PermissionNames.Admissions_Settings_CloseApplications,
            PermissionNames.Admissions_Settings_UpdateCapacity,

            // Academic - Full access
            PermissionNames.Academic,
            PermissionNames.Academic_Students,
            PermissionNames.Academic_Students_View,
            PermissionNames.Academic_Students_ViewAll,
            PermissionNames.Academic_Students_Create,
            PermissionNames.Academic_Students_Edit,
            PermissionNames.Academic_Students_AssignClass,
            PermissionNames.Academic_Teachers,
            PermissionNames.Academic_Teachers_View,
            PermissionNames.Academic_Teachers_ViewAll,
            PermissionNames.Academic_Teachers_Create,
            PermissionNames.Academic_Teachers_Edit,
            PermissionNames.Academic_Teachers_AssignSubject,
            PermissionNames.Academic_Teachers_AssignClass,
            PermissionNames.Academic_Grades,
            PermissionNames.Academic_Grades_View,
            PermissionNames.Academic_Grades_Manage,
            PermissionNames.Academic_Classes,
            PermissionNames.Academic_Classes_View,
            PermissionNames.Academic_Classes_Create,
            PermissionNames.Academic_Classes_Edit,
            PermissionNames.Academic_Timetables,
            PermissionNames.Academic_Timetables_View,
            PermissionNames.Academic_Timetables_Create,
            PermissionNames.Academic_Timetables_Edit,
            PermissionNames.Academic_Timetables_Publish,
            PermissionNames.Academic_Timetables_MakeVariations,
            PermissionNames.Academic_Attendance,
            PermissionNames.Academic_Attendance_View,
            PermissionNames.Academic_Attendance_ViewAll,
            PermissionNames.Academic_Attendance_Capture,
            PermissionNames.Academic_Attendance_Edit,
            PermissionNames.Academic_Attendance_Reports,
            PermissionNames.Academic_Calendar,
            PermissionNames.Academic_Calendar_View,
            PermissionNames.Academic_Calendar_Manage,
            PermissionNames.Academic_AcademicYears_Manage,
            PermissionNames.Academic_Terms_Manage,

            // Financial - View and approve
            PermissionNames.Financial,
            PermissionNames.Financial_FeeStructures,
            PermissionNames.Financial_FeeStructures_View,
            PermissionNames.Financial_FeeStructures_Approve,
            PermissionNames.Financial_Payments,
            PermissionNames.Financial_Payments_View,
            PermissionNames.Financial_Payments_ViewAll,
            PermissionNames.Financial_Receipts,
            PermissionNames.Financial_Receipts_View,
            PermissionNames.Financial_Receipts_Download,
            PermissionNames.Financial_Reports,
            PermissionNames.Financial_Reports_OutstandingFees,
            PermissionNames.Financial_Reports_Income,
            PermissionNames.Financial_Reports_Collections,
            PermissionNames.Financial_Reports_Export,
            PermissionNames.Financial_Statements,
            PermissionNames.Financial_Statements_View,
            PermissionNames.Financial_Statements_Generate,

            // Assessment - Full access
            PermissionNames.Assessment,
            PermissionNames.Assessment_Marks,
            PermissionNames.Assessment_Marks_View,
            PermissionNames.Assessment_Marks_ViewAll,
            PermissionNames.Assessment_Marks_Create,
            PermissionNames.Assessment_Marks_Edit,
            PermissionNames.Assessment_Marks_Delete,
            PermissionNames.Assessment_Marks_Publish,
            PermissionNames.Assessment_Marks_Unlock,
            PermissionNames.Assessment_Marks_Import,
            PermissionNames.Assessment_Quizzes,
            PermissionNames.Assessment_Quizzes_View,
            PermissionNames.Assessment_Quizzes_Create,
            PermissionNames.Assessment_Quizzes_Edit,
            PermissionNames.Assessment_Quizzes_Delete,
            PermissionNames.Assessment_Quizzes_Publish,
            PermissionNames.Assessment_Quizzes_ViewResults,
            PermissionNames.Assessment_ReportCards,
            PermissionNames.Assessment_ReportCards_View,
            PermissionNames.Assessment_ReportCards_Generate,
            PermissionNames.Assessment_ReportCards_Publish,
            PermissionNames.Assessment_ReportCards_Download,
            PermissionNames.Assessment_Feedback,
            PermissionNames.Assessment_Feedback_View,
            PermissionNames.Assessment_Feedback_Create,
            PermissionNames.Assessment_Feedback_Edit,
            PermissionNames.Assessment_Feedback_Delete,

            // Communication - Full access
            PermissionNames.Communication,
            PermissionNames.Communication_Announcements,
            PermissionNames.Communication_Announcements_View,
            PermissionNames.Communication_Announcements_Create,
            PermissionNames.Communication_Announcements_Edit,
            PermissionNames.Communication_Announcements_Delete,
            PermissionNames.Communication_Announcements_SendSchoolWide,
            PermissionNames.Communication_Messages,
            PermissionNames.Communication_Messages_View,
            PermissionNames.Communication_Messages_Send,
            PermissionNames.Communication_Messages_SendToAll,
            PermissionNames.Communication_Messages_Delete,
            PermissionNames.Communication_Notifications,
            PermissionNames.Communication_Notifications_View,
            PermissionNames.Communication_Notifications_Configure,
            PermissionNames.Communication_Documents,
            PermissionNames.Communication_Documents_View,
            PermissionNames.Communication_Documents_Upload,
            PermissionNames.Communication_Documents_Download,
            PermissionNames.Communication_Documents_Delete,
            PermissionNames.Communication_Documents_Approve,

            // Learning - Full access
            PermissionNames.Learning,
            PermissionNames.Learning_Materials,
            PermissionNames.Learning_Materials_View,
            PermissionNames.Learning_Materials_Upload,
            PermissionNames.Learning_Materials_Edit,
            PermissionNames.Learning_Materials_Delete,
            PermissionNames.Learning_Materials_Download,
            PermissionNames.Learning_Materials_ManageVersions,
            PermissionNames.Learning_Lessons,
            PermissionNames.Learning_Lessons_View,
            PermissionNames.Learning_Lessons_Schedule,
            PermissionNames.Learning_Lessons_Host,
            PermissionNames.Learning_Lessons_Join,
            PermissionNames.Learning_Lessons_Cancel,
            PermissionNames.Learning_Recordings,
            PermissionNames.Learning_Recordings_View,
            PermissionNames.Learning_Recordings_Upload,
            PermissionNames.Learning_Recordings_Delete,

            // Administration - User management
            PermissionNames.Administration,
            PermissionNames.Administration_Users,
            PermissionNames.Administration_Users_View,
            PermissionNames.Administration_Users_Create,
            PermissionNames.Administration_Users_Edit,
            PermissionNames.Administration_Users_ResetPassword,
            PermissionNames.Administration_Users_Unlock,
            PermissionNames.Administration_Roles,
            PermissionNames.Administration_Roles_View,
            PermissionNames.Administration_Settings,
            PermissionNames.Administration_Settings_View,
            PermissionNames.Administration_AuditLogs,
            PermissionNames.Administration_AuditLogs_View,

            // System
            PermissionNames.Pages_Users,
            PermissionNames.Pages_Roles,
        };
    }

    /// <summary>
    /// Vice Principal - Similar to Principal but limited decision authority (needs delegation)
    /// </summary>
    private static List<string> GetVicePrincipalPermissions()
    {
        return new List<string>
        {
            // Admissions - Review and limited decisions
            PermissionNames.Admissions,
            PermissionNames.Admissions_Applications,
            PermissionNames.Admissions_Applications_View,
            PermissionNames.Admissions_Applications_ViewAll,
            PermissionNames.Admissions_Applications_Edit,
            PermissionNames.Admissions_Applications_Withdraw,
            PermissionNames.Admissions_Review,
            PermissionNames.Admissions_Review_StartReview,
            PermissionNames.Admissions_Review_RequestDocuments,
            PermissionNames.Admissions_Review_MarkDocumentsComplete,
            PermissionNames.Admissions_Decision,
            PermissionNames.Admissions_Decision_ExtendOffer,
            PermissionNames.Admissions_Documents,
            PermissionNames.Admissions_Documents_Upload,
            PermissionNames.Admissions_Documents_View,
            PermissionNames.Admissions_Documents_Download,
            PermissionNames.Admissions_Documents_Verify,
            PermissionNames.Admissions_Documents_Reject,
            PermissionNames.Admissions_Interviews,
            PermissionNames.Admissions_Interviews_View,
            PermissionNames.Admissions_Interviews_Schedule,
            PermissionNames.Admissions_Interviews_Reschedule,
            PermissionNames.Admissions_Interviews_Cancel,
            PermissionNames.Admissions_Interviews_Conduct,
            PermissionNames.Admissions_Interviews_RecordOutcome,
            PermissionNames.Admissions_Assessments,
            PermissionNames.Admissions_Assessments_View,
            PermissionNames.Admissions_Assessments_Schedule,
            PermissionNames.Admissions_Assessments_Cancel,
            PermissionNames.Admissions_Assessments_Conduct,
            PermissionNames.Admissions_Assessments_RecordResults,
            PermissionNames.Admissions_Waitlist,
            PermissionNames.Admissions_Waitlist_View,
            PermissionNames.Admissions_Waitlist_ViewAll,
            PermissionNames.Admissions_Waitlist_OfferPosition,
            PermissionNames.Admissions_Enrollment,
            PermissionNames.Admissions_Enrollment_View,
            PermissionNames.Admissions_Enrollment_SubmitForms,
            PermissionNames.Admissions_Enrollment_AssignClass,
            PermissionNames.Admissions_Enrollment_Complete,
            PermissionNames.Admissions_Settings,
            PermissionNames.Admissions_Settings_View,

            // Academic - Most access
            PermissionNames.Academic,
            PermissionNames.Academic_Students,
            PermissionNames.Academic_Students_View,
            PermissionNames.Academic_Students_ViewAll,
            PermissionNames.Academic_Students_Create,
            PermissionNames.Academic_Students_Edit,
            PermissionNames.Academic_Students_AssignClass,
            PermissionNames.Academic_Teachers,
            PermissionNames.Academic_Teachers_View,
            PermissionNames.Academic_Teachers_ViewAll,
            PermissionNames.Academic_Teachers_AssignSubject,
            PermissionNames.Academic_Teachers_AssignClass,
            PermissionNames.Academic_Grades,
            PermissionNames.Academic_Grades_View,
            PermissionNames.Academic_Classes,
            PermissionNames.Academic_Classes_View,
            PermissionNames.Academic_Classes_Create,
            PermissionNames.Academic_Classes_Edit,
            PermissionNames.Academic_Timetables,
            PermissionNames.Academic_Timetables_View,
            PermissionNames.Academic_Timetables_Create,
            PermissionNames.Academic_Timetables_Edit,
            PermissionNames.Academic_Timetables_Publish,
            PermissionNames.Academic_Timetables_MakeVariations,
            PermissionNames.Academic_Attendance,
            PermissionNames.Academic_Attendance_View,
            PermissionNames.Academic_Attendance_ViewAll,
            PermissionNames.Academic_Attendance_Capture,
            PermissionNames.Academic_Attendance_Edit,
            PermissionNames.Academic_Attendance_Reports,
            PermissionNames.Academic_Calendar,
            PermissionNames.Academic_Calendar_View,
            PermissionNames.Academic_Calendar_Manage,
            PermissionNames.Academic_Terms_Manage,

            // Financial - View only
            PermissionNames.Financial,
            PermissionNames.Financial_FeeStructures,
            PermissionNames.Financial_FeeStructures_View,
            PermissionNames.Financial_Payments,
            PermissionNames.Financial_Payments_View,
            PermissionNames.Financial_Payments_ViewAll,
            PermissionNames.Financial_Receipts,
            PermissionNames.Financial_Receipts_View,
            PermissionNames.Financial_Receipts_Download,
            PermissionNames.Financial_Reports,
            PermissionNames.Financial_Reports_OutstandingFees,
            PermissionNames.Financial_Reports_Income,
            PermissionNames.Financial_Reports_Collections,
            PermissionNames.Financial_Reports_Export,
            PermissionNames.Financial_Statements,
            PermissionNames.Financial_Statements_View,
            PermissionNames.Financial_Statements_Generate,

            // Assessment - Full access
            PermissionNames.Assessment,
            PermissionNames.Assessment_Marks,
            PermissionNames.Assessment_Marks_View,
            PermissionNames.Assessment_Marks_ViewAll,
            PermissionNames.Assessment_Marks_Create,
            PermissionNames.Assessment_Marks_Edit,
            PermissionNames.Assessment_Marks_Publish,
            PermissionNames.Assessment_Marks_Import,
            PermissionNames.Assessment_Quizzes,
            PermissionNames.Assessment_Quizzes_View,
            PermissionNames.Assessment_Quizzes_Create,
            PermissionNames.Assessment_Quizzes_Edit,
            PermissionNames.Assessment_Quizzes_Publish,
            PermissionNames.Assessment_Quizzes_ViewResults,
            PermissionNames.Assessment_ReportCards,
            PermissionNames.Assessment_ReportCards_View,
            PermissionNames.Assessment_ReportCards_Generate,
            PermissionNames.Assessment_ReportCards_Publish,
            PermissionNames.Assessment_ReportCards_Download,
            PermissionNames.Assessment_Feedback,
            PermissionNames.Assessment_Feedback_View,
            PermissionNames.Assessment_Feedback_Create,
            PermissionNames.Assessment_Feedback_Edit,

            // Communication
            PermissionNames.Communication,
            PermissionNames.Communication_Announcements,
            PermissionNames.Communication_Announcements_View,
            PermissionNames.Communication_Announcements_Create,
            PermissionNames.Communication_Announcements_Edit,
            PermissionNames.Communication_Announcements_Delete,
            PermissionNames.Communication_Announcements_SendSchoolWide,
            PermissionNames.Communication_Messages,
            PermissionNames.Communication_Messages_View,
            PermissionNames.Communication_Messages_Send,
            PermissionNames.Communication_Messages_SendToAll,
            PermissionNames.Communication_Notifications,
            PermissionNames.Communication_Notifications_View,
            PermissionNames.Communication_Documents,
            PermissionNames.Communication_Documents_View,
            PermissionNames.Communication_Documents_Upload,
            PermissionNames.Communication_Documents_Download,
            PermissionNames.Communication_Documents_Approve,

            // Learning
            PermissionNames.Learning,
            PermissionNames.Learning_Materials,
            PermissionNames.Learning_Materials_View,
            PermissionNames.Learning_Materials_Upload,
            PermissionNames.Learning_Materials_Edit,
            PermissionNames.Learning_Materials_Delete,
            PermissionNames.Learning_Materials_Download,
            PermissionNames.Learning_Materials_ManageVersions,
            PermissionNames.Learning_Lessons,
            PermissionNames.Learning_Lessons_View,
            PermissionNames.Learning_Lessons_Schedule,
            PermissionNames.Learning_Lessons_Host,
            PermissionNames.Learning_Lessons_Join,
            PermissionNames.Learning_Lessons_Cancel,
            PermissionNames.Learning_Recordings,
            PermissionNames.Learning_Recordings_View,
            PermissionNames.Learning_Recordings_Upload,

            // Administration - View users
            PermissionNames.Administration,
            PermissionNames.Administration_Users,
            PermissionNames.Administration_Users_View,
            PermissionNames.Administration_Settings,
            PermissionNames.Administration_Settings_View,
            PermissionNames.Administration_AuditLogs,
            PermissionNames.Administration_AuditLogs_View,

            PermissionNames.Pages_Users,
        };
    }

    /// <summary>
    /// HOD - Department scoped access
    /// </summary>
    private static List<string> GetHODPermissions()
    {
        return new List<string>
        {
            // Admissions - Limited to interviews/assessments
            PermissionNames.Admissions_Interviews,
            PermissionNames.Admissions_Interviews_View,
            PermissionNames.Admissions_Interviews_Conduct,
            PermissionNames.Admissions_Interviews_RecordOutcome,
            PermissionNames.Admissions_Assessments,
            PermissionNames.Admissions_Assessments_View,
            PermissionNames.Admissions_Assessments_Conduct,
            PermissionNames.Admissions_Assessments_RecordResults,

            // Academic - Department scoped
            PermissionNames.Academic,
            PermissionNames.Academic_Students,
            PermissionNames.Academic_Students_View,
            PermissionNames.Academic_Students_ViewAll,
            PermissionNames.Academic_Students_AssignClass,
            PermissionNames.Academic_Teachers,
            PermissionNames.Academic_Teachers_View,
            PermissionNames.Academic_Teachers_ViewAll,
            PermissionNames.Academic_Teachers_AssignClass,
            PermissionNames.Academic_Grades,
            PermissionNames.Academic_Grades_View,
            PermissionNames.Academic_Classes,
            PermissionNames.Academic_Classes_View,
            PermissionNames.Academic_Timetables,
            PermissionNames.Academic_Timetables_View,
            PermissionNames.Academic_Attendance,
            PermissionNames.Academic_Attendance_View,
            PermissionNames.Academic_Attendance_ViewAll,
            PermissionNames.Academic_Attendance_Capture,
            PermissionNames.Academic_Attendance_Reports,
            PermissionNames.Academic_Calendar,
            PermissionNames.Academic_Calendar_View,

            // Assessment - Full for department
            PermissionNames.Assessment,
            PermissionNames.Assessment_Marks,
            PermissionNames.Assessment_Marks_View,
            PermissionNames.Assessment_Marks_ViewAll,
            PermissionNames.Assessment_Marks_Create,
            PermissionNames.Assessment_Marks_Edit,
            PermissionNames.Assessment_Marks_Publish,
            PermissionNames.Assessment_Marks_Import,
            PermissionNames.Assessment_Quizzes,
            PermissionNames.Assessment_Quizzes_View,
            PermissionNames.Assessment_Quizzes_Create,
            PermissionNames.Assessment_Quizzes_Edit,
            PermissionNames.Assessment_Quizzes_Delete,
            PermissionNames.Assessment_Quizzes_Publish,
            PermissionNames.Assessment_Quizzes_ViewResults,
            PermissionNames.Assessment_ReportCards,
            PermissionNames.Assessment_ReportCards_View,
            PermissionNames.Assessment_ReportCards_Generate,
            PermissionNames.Assessment_ReportCards_Download,
            PermissionNames.Assessment_Feedback,
            PermissionNames.Assessment_Feedback_View,
            PermissionNames.Assessment_Feedback_Create,
            PermissionNames.Assessment_Feedback_Edit,

            // Communication
            PermissionNames.Communication,
            PermissionNames.Communication_Announcements,
            PermissionNames.Communication_Announcements_View,
            PermissionNames.Communication_Announcements_Create,
            PermissionNames.Communication_Announcements_Edit,
            PermissionNames.Communication_Messages,
            PermissionNames.Communication_Messages_View,
            PermissionNames.Communication_Messages_Send,
            PermissionNames.Communication_Notifications,
            PermissionNames.Communication_Notifications_View,
            PermissionNames.Communication_Documents,
            PermissionNames.Communication_Documents_View,
            PermissionNames.Communication_Documents_Upload,
            PermissionNames.Communication_Documents_Download,

            // Learning
            PermissionNames.Learning,
            PermissionNames.Learning_Materials,
            PermissionNames.Learning_Materials_View,
            PermissionNames.Learning_Materials_Upload,
            PermissionNames.Learning_Materials_Edit,
            PermissionNames.Learning_Materials_Delete,
            PermissionNames.Learning_Materials_Download,
            PermissionNames.Learning_Materials_ManageVersions,
            PermissionNames.Learning_Lessons,
            PermissionNames.Learning_Lessons_View,
            PermissionNames.Learning_Lessons_Schedule,
            PermissionNames.Learning_Lessons_Host,
            PermissionNames.Learning_Lessons_Join,
            PermissionNames.Learning_Lessons_Cancel,
            PermissionNames.Learning_Recordings,
            PermissionNames.Learning_Recordings_View,
            PermissionNames.Learning_Recordings_Upload,
        };
    }

    /// <summary>
    /// Admissions Officer - Full admissions access except final decisions
    /// </summary>
    private static List<string> GetAdmissionsOfficerPermissions()
    {
        return new List<string>
        {
            // Admissions - Full operational access
            PermissionNames.Admissions,
            PermissionNames.Admissions_Applications,
            PermissionNames.Admissions_Applications_View,
            PermissionNames.Admissions_Applications_ViewAll,
            PermissionNames.Admissions_Applications_Create,
            PermissionNames.Admissions_Applications_Edit,
            PermissionNames.Admissions_Applications_Submit,
            PermissionNames.Admissions_Applications_Withdraw,
            PermissionNames.Admissions_Review,
            PermissionNames.Admissions_Review_StartReview,
            PermissionNames.Admissions_Review_RequestDocuments,
            PermissionNames.Admissions_Review_MarkDocumentsComplete,
            PermissionNames.Admissions_Documents,
            PermissionNames.Admissions_Documents_Upload,
            PermissionNames.Admissions_Documents_View,
            PermissionNames.Admissions_Documents_Download,
            PermissionNames.Admissions_Documents_Verify,
            PermissionNames.Admissions_Documents_Reject,
            PermissionNames.Admissions_Documents_Delete,
            PermissionNames.Admissions_Interviews,
            PermissionNames.Admissions_Interviews_View,
            PermissionNames.Admissions_Interviews_Schedule,
            PermissionNames.Admissions_Interviews_Reschedule,
            PermissionNames.Admissions_Interviews_Cancel,
            PermissionNames.Admissions_Interviews_Conduct,
            PermissionNames.Admissions_Interviews_RecordOutcome,
            PermissionNames.Admissions_Assessments,
            PermissionNames.Admissions_Assessments_View,
            PermissionNames.Admissions_Assessments_Schedule,
            PermissionNames.Admissions_Assessments_Cancel,
            PermissionNames.Admissions_Waitlist,
            PermissionNames.Admissions_Waitlist_View,
            PermissionNames.Admissions_Waitlist_ViewAll,
            PermissionNames.Admissions_Waitlist_OfferPosition,
            PermissionNames.Admissions_Enrollment,
            PermissionNames.Admissions_Enrollment_View,
            PermissionNames.Admissions_Enrollment_SubmitForms,
            PermissionNames.Admissions_Enrollment_AssignClass,
            PermissionNames.Admissions_Enrollment_Complete,
            PermissionNames.Admissions_Settings,
            PermissionNames.Admissions_Settings_View,

            // Financial - View payments for admissions
            PermissionNames.Financial_Payments,
            PermissionNames.Financial_Payments_View,
            PermissionNames.Financial_Payments_Process,
            PermissionNames.Financial_Payments_RecordManual,
            PermissionNames.Financial_Receipts,
            PermissionNames.Financial_Receipts_View,

            // Communication
            PermissionNames.Communication,
            PermissionNames.Communication_Announcements,
            PermissionNames.Communication_Announcements_View,
            PermissionNames.Communication_Messages,
            PermissionNames.Communication_Messages_View,
            PermissionNames.Communication_Messages_Send,
            PermissionNames.Communication_Notifications,
            PermissionNames.Communication_Notifications_View,
        };
    }

    /// <summary>
    /// Finance Manager - Full financial access
    /// </summary>
    private static List<string> GetFinancePermissions()
    {
        return new List<string>
        {
            // Financial - Full access
            PermissionNames.Financial,
            PermissionNames.Financial_FeeStructures,
            PermissionNames.Financial_FeeStructures_View,
            PermissionNames.Financial_FeeStructures_Create,
            PermissionNames.Financial_FeeStructures_Edit,
            PermissionNames.Financial_Payments,
            PermissionNames.Financial_Payments_View,
            PermissionNames.Financial_Payments_ViewAll,
            PermissionNames.Financial_Payments_Process,
            PermissionNames.Financial_Payments_RecordManual,
            PermissionNames.Financial_Payments_Reconcile,
            PermissionNames.Financial_Receipts,
            PermissionNames.Financial_Receipts_View,
            PermissionNames.Financial_Receipts_Download,
            PermissionNames.Financial_Receipts_Resend,
            PermissionNames.Financial_Reports,
            PermissionNames.Financial_Reports_OutstandingFees,
            PermissionNames.Financial_Reports_Income,
            PermissionNames.Financial_Reports_Collections,
            PermissionNames.Financial_Reports_Export,
            PermissionNames.Financial_Statements,
            PermissionNames.Financial_Statements_View,
            PermissionNames.Financial_Statements_Generate,

            // Academic - View students for billing
            PermissionNames.Academic_Students,
            PermissionNames.Academic_Students_View,
            PermissionNames.Academic_Students_ViewAll,

            // Communication
            PermissionNames.Communication,
            PermissionNames.Communication_Announcements,
            PermissionNames.Communication_Announcements_View,
            PermissionNames.Communication_Messages,
            PermissionNames.Communication_Messages_View,
            PermissionNames.Communication_Messages_Send,
            PermissionNames.Communication_Notifications,
            PermissionNames.Communication_Notifications_View,
        };
    }

    /// <summary>
    /// Teacher - Class/Subject scoped access
    /// </summary>
    private static List<string> GetTeacherPermissions()
    {
        return new List<string>
        {
            // Admissions - Assessments only
            PermissionNames.Admissions_Assessments,
            PermissionNames.Admissions_Assessments_View,
            PermissionNames.Admissions_Assessments_Conduct,
            PermissionNames.Admissions_Assessments_RecordResults,

            // Academic - Class scoped
            PermissionNames.Academic,
            PermissionNames.Academic_Students,
            PermissionNames.Academic_Students_View,
            PermissionNames.Academic_Teachers,
            PermissionNames.Academic_Teachers_View,
            PermissionNames.Academic_Grades,
            PermissionNames.Academic_Grades_View,
            PermissionNames.Academic_Classes,
            PermissionNames.Academic_Classes_View,
            PermissionNames.Academic_Timetables,
            PermissionNames.Academic_Timetables_View,
            PermissionNames.Academic_Attendance,
            PermissionNames.Academic_Attendance_View,
            PermissionNames.Academic_Attendance_Capture,
            PermissionNames.Academic_Attendance_Reports,
            PermissionNames.Academic_Calendar,
            PermissionNames.Academic_Calendar_View,

            // Assessment - Class scoped
            PermissionNames.Assessment,
            PermissionNames.Assessment_Marks,
            PermissionNames.Assessment_Marks_View,
            PermissionNames.Assessment_Marks_Create,
            PermissionNames.Assessment_Marks_Edit,
            PermissionNames.Assessment_Marks_Publish,
            PermissionNames.Assessment_Marks_Import,
            PermissionNames.Assessment_Quizzes,
            PermissionNames.Assessment_Quizzes_View,
            PermissionNames.Assessment_Quizzes_Create,
            PermissionNames.Assessment_Quizzes_Edit,
            PermissionNames.Assessment_Quizzes_Delete,
            PermissionNames.Assessment_Quizzes_Publish,
            PermissionNames.Assessment_Quizzes_ViewResults,
            PermissionNames.Assessment_ReportCards,
            PermissionNames.Assessment_ReportCards_View,
            PermissionNames.Assessment_ReportCards_Download,
            PermissionNames.Assessment_Feedback,
            PermissionNames.Assessment_Feedback_View,
            PermissionNames.Assessment_Feedback_Create,
            PermissionNames.Assessment_Feedback_Edit,

            // Communication
            PermissionNames.Communication,
            PermissionNames.Communication_Announcements,
            PermissionNames.Communication_Announcements_View,
            PermissionNames.Communication_Announcements_Create,
            PermissionNames.Communication_Announcements_Edit,
            PermissionNames.Communication_Messages,
            PermissionNames.Communication_Messages_View,
            PermissionNames.Communication_Messages_Send,
            PermissionNames.Communication_Notifications,
            PermissionNames.Communication_Notifications_View,
            PermissionNames.Communication_Notifications_ManagePreferences,
            PermissionNames.Communication_Documents,
            PermissionNames.Communication_Documents_View,
            PermissionNames.Communication_Documents_Download,

            // Learning - Class scoped
            PermissionNames.Learning,
            PermissionNames.Learning_Materials,
            PermissionNames.Learning_Materials_View,
            PermissionNames.Learning_Materials_Upload,
            PermissionNames.Learning_Materials_Edit,
            PermissionNames.Learning_Materials_Delete,
            PermissionNames.Learning_Materials_Download,
            PermissionNames.Learning_Materials_ManageVersions,
            PermissionNames.Learning_Lessons,
            PermissionNames.Learning_Lessons_View,
            PermissionNames.Learning_Lessons_Schedule,
            PermissionNames.Learning_Lessons_Host,
            PermissionNames.Learning_Lessons_Join,
            PermissionNames.Learning_Lessons_Cancel,
            PermissionNames.Learning_Recordings,
            PermissionNames.Learning_Recordings_View,
            PermissionNames.Learning_Recordings_Upload,
        };
    }

    /// <summary>
    /// Parent - Child's data only
    /// </summary>
    private static List<string> GetParentPermissions()
    {
        return new List<string>
        {
            // Academic - Child's data
            PermissionNames.Academic_Students,
            PermissionNames.Academic_Students_View,
            PermissionNames.Academic_Timetables,
            PermissionNames.Academic_Timetables_View,
            PermissionNames.Academic_Attendance,
            PermissionNames.Academic_Attendance_View,
            PermissionNames.Academic_Attendance_Reports,
            PermissionNames.Academic_Calendar,
            PermissionNames.Academic_Calendar_View,

            // Financial - Own statements and payments
            PermissionNames.Financial_Payments,
            PermissionNames.Financial_Payments_View,
            PermissionNames.Financial_Payments_Process,
            PermissionNames.Financial_Receipts,
            PermissionNames.Financial_Receipts_View,
            PermissionNames.Financial_Receipts_Download,
            PermissionNames.Financial_Statements,
            PermissionNames.Financial_Statements_View,
            PermissionNames.Financial_Statements_Generate,

            // Assessment - Child's results
            PermissionNames.Assessment_Marks,
            PermissionNames.Assessment_Marks_View,
            PermissionNames.Assessment_Quizzes,
            PermissionNames.Assessment_Quizzes_ViewResults,
            PermissionNames.Assessment_ReportCards,
            PermissionNames.Assessment_ReportCards_View,
            PermissionNames.Assessment_ReportCards_Download,
            PermissionNames.Assessment_Feedback,
            PermissionNames.Assessment_Feedback_View,

            // Communication
            PermissionNames.Communication,
            PermissionNames.Communication_Announcements,
            PermissionNames.Communication_Announcements_View,
            PermissionNames.Communication_Messages,
            PermissionNames.Communication_Messages_View,
            PermissionNames.Communication_Messages_Send,
            PermissionNames.Communication_Notifications,
            PermissionNames.Communication_Notifications_View,
            PermissionNames.Communication_Notifications_ManagePreferences,
            PermissionNames.Communication_Documents,
            PermissionNames.Communication_Documents_View,
            PermissionNames.Communication_Documents_Download,

            // Learning - Child's materials
            PermissionNames.Learning_Materials,
            PermissionNames.Learning_Materials_View,
            PermissionNames.Learning_Materials_Download,
            PermissionNames.Learning_Lessons,
            PermissionNames.Learning_Lessons_View,
            PermissionNames.Learning_Recordings,
            PermissionNames.Learning_Recordings_View,
        };
    }

    /// <summary>
    /// Student - Own data only
    /// </summary>
    private static List<string> GetStudentPermissions()
    {
        return new List<string>
        {
            // Academic - Own data
            PermissionNames.Academic_Students,
            PermissionNames.Academic_Students_View,
            PermissionNames.Academic_Timetables,
            PermissionNames.Academic_Timetables_View,
            PermissionNames.Academic_Attendance,
            PermissionNames.Academic_Attendance_View,
            PermissionNames.Academic_Calendar,
            PermissionNames.Academic_Calendar_View,

            // Assessment - Own results and quizzes
            PermissionNames.Assessment_Marks,
            PermissionNames.Assessment_Marks_View,
            PermissionNames.Assessment_Quizzes,
            PermissionNames.Assessment_Quizzes_View,
            PermissionNames.Assessment_Quizzes_Attempt,
            PermissionNames.Assessment_Quizzes_ViewResults,
            PermissionNames.Assessment_ReportCards,
            PermissionNames.Assessment_ReportCards_View,
            PermissionNames.Assessment_ReportCards_Download,
            PermissionNames.Assessment_Feedback,
            PermissionNames.Assessment_Feedback_View,

            // Communication
            PermissionNames.Communication,
            PermissionNames.Communication_Announcements,
            PermissionNames.Communication_Announcements_View,
            PermissionNames.Communication_Messages,
            PermissionNames.Communication_Messages_View,
            PermissionNames.Communication_Notifications,
            PermissionNames.Communication_Notifications_View,
            PermissionNames.Communication_Notifications_ManagePreferences,
            PermissionNames.Communication_Documents,
            PermissionNames.Communication_Documents_View,
            PermissionNames.Communication_Documents_Download,

            // Learning - Own materials and lessons
            PermissionNames.Learning_Materials,
            PermissionNames.Learning_Materials_View,
            PermissionNames.Learning_Materials_Download,
            PermissionNames.Learning_Lessons,
            PermissionNames.Learning_Lessons_View,
            PermissionNames.Learning_Lessons_Join,
            PermissionNames.Learning_Recordings,
            PermissionNames.Learning_Recordings_View,
        };
    }

    /// <summary>
    /// Applicant (Prospective Parent) - Application portal only
    /// </summary>
    private static List<string> GetApplicantPermissions()
    {
        return new List<string>
        {
            // Admissions - Own application only
            PermissionNames.Admissions,
            PermissionNames.Admissions_Applications,
            PermissionNames.Admissions_Applications_View,
            PermissionNames.Admissions_Applications_Create,
            PermissionNames.Admissions_Applications_Edit,
            PermissionNames.Admissions_Applications_Submit,
            PermissionNames.Admissions_Applications_Withdraw,
            PermissionNames.Admissions_Documents,
            PermissionNames.Admissions_Documents_Upload,
            PermissionNames.Admissions_Documents_View,
            PermissionNames.Admissions_Documents_Download,
            PermissionNames.Admissions_Waitlist,
            PermissionNames.Admissions_Waitlist_View,
            PermissionNames.Admissions_Waitlist_AcceptOffer,
            PermissionNames.Admissions_Waitlist_DeclineOffer,
            PermissionNames.Admissions_Waitlist_Withdraw,
            PermissionNames.Admissions_Enrollment,
            PermissionNames.Admissions_Enrollment_View,
            PermissionNames.Admissions_Enrollment_AcceptOffer,
            PermissionNames.Admissions_Enrollment_SubmitForms,

            // Financial - Pay application fee
            PermissionNames.Financial_Payments,
            PermissionNames.Financial_Payments_View,
            PermissionNames.Financial_Payments_Process,
            PermissionNames.Financial_Receipts,
            PermissionNames.Financial_Receipts_View,
            PermissionNames.Financial_Receipts_Download,

            // Communication - Notifications
            PermissionNames.Communication_Notifications,
            PermissionNames.Communication_Notifications_View,
        };
    }

    #endregion
}
