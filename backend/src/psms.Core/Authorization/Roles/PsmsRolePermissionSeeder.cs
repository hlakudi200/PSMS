using Abp.Authorization;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.IdentityFramework;
using Abp.Localization;
using Abp.Runtime.Session;
using psms.Authorization.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Authorization;

/// <summary>
/// Seeds permissions for PSMS-specific roles.
/// Called when a new tenant is created to assign appropriate permissions to each role.
/// </summary>
public class PsmsRolePermissionSeeder : ITransientDependency
{
    private readonly RoleManager _roleManager;
    private readonly IPermissionManager _permissionManager;
    private readonly ILocalizationManager _localizationManager;
    private readonly IAbpSession _abpSession;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public PsmsRolePermissionSeeder(
        RoleManager roleManager,
        IPermissionManager permissionManager,
        ILocalizationManager localizationManager,
        IAbpSession abpSession,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _roleManager = roleManager;
        _permissionManager = permissionManager;
        _localizationManager = localizationManager;
        _abpSession = abpSession;
        _unitOfWorkManager = unitOfWorkManager;
    }

    /// <summary>
    /// LC-09: ensure the Student role exists for the CURRENT tenant, creating it
    /// (with its default permissions) if missing. Static roles are seeded at
    /// tenant-creation time, so a tenant provisioned before the Student role
    /// existed (and never re-seeded) would lack it — and student provisioning
    /// (SetRoles "Student") would then fail and roll back the student create.
    ///
    /// When the role already exists this is a no-op, so a tenant's tuned grants
    /// are never disturbed. The create path is a one-time backfill per tenant
    /// (every later provision hits the fast path), and tolerates a concurrent
    /// first-provision racing to create the same role.
    /// </summary>
    public async Task EnsureStudentRoleExistsAsync()
    {
        var roleName = StaticRoleNames.Tenants.Student;

        var role = _roleManager.Roles.FirstOrDefault(r => r.Name == roleName);
        if (role != null)
            return; // already present — leave its (possibly customised) grants alone

        role = new Role(_abpSession.TenantId, roleName, roleName) { IsStatic = true };
        try
        {
            (await _roleManager.CreateAsync(role)).CheckErrors(_localizationManager);
        }
        catch (Exception) when (_roleManager.Roles.Any(r => r.Name == roleName))
        {
            // A concurrent first-provision created the role just before us — reuse
            // it rather than failing the student create. (Once the role exists,
            // this whole method short-circuits at the check above.)
            return;
        }

        // Flush so the new role gets its Id before we attach permission rows to it
        // — mirrors TenantAppService's "save to get static role ids" ordering.
        await _unitOfWorkManager.Current.SaveChangesAsync();

        var permissionsToGrant = _permissionManager.GetAllPermissions()
            .Where(p => GetStudentPermissions().Contains(p.Name))
            .ToList();
        await _roleManager.SetGrantedPermissionsAsync(role, permissionsToGrant);
    }

    /// <summary>
    /// Seeds permissions for all PSMS roles for the given tenant.
    /// Should be called after static roles are created.
    /// </summary>
    public async Task SeedRolePermissionsAsync(int tenantId)
    {
        await GrantPermissionsToRoleAsync(StaticRoleNames.Tenants.Principal, GetPrincipalPermissions());
        await GrantPermissionsToRoleAsync(StaticRoleNames.Tenants.VicePrincipal, GetVicePrincipalPermissions());
        await GrantPermissionsToRoleAsync(StaticRoleNames.Tenants.HOD, GetHODPermissions());
        await GrantPermissionsToRoleAsync(StaticRoleNames.Tenants.AdmissionsOfficer, GetAdmissionsOfficerPermissions());
        await GrantPermissionsToRoleAsync(StaticRoleNames.Tenants.Finance, GetFinancePermissions());
        await GrantPermissionsToRoleAsync(StaticRoleNames.Tenants.Teacher, GetTeacherPermissions());
        await GrantPermissionsToRoleAsync(StaticRoleNames.Tenants.Parent, GetParentPermissions());
        await GrantPermissionsToRoleAsync(StaticRoleNames.Tenants.Student, GetStudentPermissions());
        await GrantPermissionsToRoleAsync(StaticRoleNames.Tenants.Applicant, GetApplicantPermissions());
    }

    private async Task GrantPermissionsToRoleAsync(string roleName, List<string> permissionNames)
    {
        var role = _roleManager.Roles.FirstOrDefault(r => r.Name == roleName);
        if (role == null) return;

        var allPermissions = _permissionManager.GetAllPermissions();
        var permissionsToGrant = allPermissions
            .Where(p => permissionNames.Contains(p.Name))
            .ToList();

        await _roleManager.SetGrantedPermissionsAsync(role, permissionsToGrant);
    }

    /// <summary>
    /// WF-01: ADDITIVELY grant the workflow permissions each staff role should
    /// have for the CURRENT tenant, leaving every other grant untouched.
    ///
    /// Use this to backfill workflow access on EXISTING tenants — NOT
    /// <see cref="SeedRolePermissionsAsync"/>, which REPLACES a role's entire
    /// permission set (via SetGrantedPermissions) and would strip any
    /// tenant-specific customisations a role has accumulated.
    /// </summary>
    public async Task GrantWorkflowPermissionsToStaffRolesAsync()
    {
        await AddPermissionsToRoleAsync(StaticRoleNames.Tenants.Principal, GetWorkflowFullPermissions());
        await AddPermissionsToRoleAsync(StaticRoleNames.Tenants.VicePrincipal, GetWorkflowFullPermissions());
        await AddPermissionsToRoleAsync(StaticRoleNames.Tenants.HOD, GetWorkflowActPermissions());
        await AddPermissionsToRoleAsync(StaticRoleNames.Tenants.Teacher, GetWorkflowActPermissions());
    }

    /// <summary>
    /// Additively grant the operations feature groups (Parents, Transfers, Fee
    /// Waivers, Expenses, Discipline, Staff Leave, Field Trips, Extramurals,
    /// Transport, After Care — see <see cref="PsmsOperationsPermissions"/>) to the
    /// staff roles of the CURRENT tenant, leaving every other grant untouched.
    ///
    /// Use this to backfill EXISTING tenants — NOT <see cref="SeedRolePermissionsAsync"/>,
    /// which REPLACES a role's entire permission set. Idempotent.
    /// </summary>
    public async Task GrantOperationsPermissionsToStaffRolesAsync()
    {
        await AddPermissionsToRoleAsync(StaticRoleNames.Tenants.Principal, PsmsOperationsPermissions.ForPrincipal());
        await AddPermissionsToRoleAsync(StaticRoleNames.Tenants.VicePrincipal, PsmsOperationsPermissions.ForVicePrincipal());
        await AddPermissionsToRoleAsync(StaticRoleNames.Tenants.HOD, PsmsOperationsPermissions.ForHOD());
        await AddPermissionsToRoleAsync(StaticRoleNames.Tenants.Teacher, PsmsOperationsPermissions.ForTeacher());
        await AddPermissionsToRoleAsync(StaticRoleNames.Tenants.Finance, PsmsOperationsPermissions.ForFinance());
    }

    /// <summary>
    /// Grants the named permissions to the role and NEVER removes an existing
    /// grant (unlike SetGrantedPermissions, which replaces the whole set).
    /// GrantPermissionAsync is itself idempotent — it no-ops when the permission
    /// is already granted — so we simply grant each desired permission and always
    /// materialise an explicit grant row (robust even if a permission's default
    /// grant ever changes).
    /// </summary>
    private async Task AddPermissionsToRoleAsync(string roleName, List<string> permissionNames)
    {
        var role = _roleManager.Roles.FirstOrDefault(r => r.Name == roleName);
        if (role == null) return;

        var toGrant = _permissionManager.GetAllPermissions()
            .Where(p => permissionNames.Contains(p.Name))
            .ToList();

        foreach (var permission in toGrant)
            await _roleManager.GrantPermissionAsync(role, permission);
    }

    // NOTE: the workflow permission sets below are also embedded inline in the
    // four role getters in this file AND mirrored in DefaultRolesCreator (the
    // seed-time creator for tenant 1 / tests). Keep all three in sync if you
    // change a role's workflow grants.

    /// <summary>Full workflow rights (configure + manage instances) — Principal/VP.</summary>
    private static List<string> GetWorkflowFullPermissions()
    {
        return new List<string>
        {
            PermissionNames.Workflow,
            PermissionNames.Workflow_Definitions,
            PermissionNames.Workflow_Definitions_View,
            PermissionNames.Workflow_Definitions_Create,
            PermissionNames.Workflow_Definitions_Edit,
            PermissionNames.Workflow_Definitions_Delete,
            PermissionNames.Workflow_Definitions_Activate,
            PermissionNames.Workflow_Instances,
            PermissionNames.Workflow_Instances_View,
            PermissionNames.Workflow_Instances_ViewAll,
            PermissionNames.Workflow_Instances_Start,
            PermissionNames.Workflow_Instances_Advance,
            PermissionNames.Workflow_Instances_Cancel,
            PermissionNames.Workflow_Instances_ViewHistory,
            PermissionNames.Workflow_Instances_Recall,
            PermissionNames.Workflow_Instances_BatchAdvance,
            PermissionNames.Workflow_Instances_OverrideGuard,
            PermissionNames.Workflow_Delegations,
            PermissionNames.Workflow_Delegations_View,
            PermissionNames.Workflow_Delegations_Create,
            PermissionNames.Workflow_Delegations_Revoke,
        };
    }

    /// <summary>Act-only workflow rights (view + advance assigned steps) — HOD/Teacher.</summary>
    private static List<string> GetWorkflowActPermissions()
    {
        return new List<string>
        {
            PermissionNames.Workflow,
            PermissionNames.Workflow_Instances,
            PermissionNames.Workflow_Instances_View,
            PermissionNames.Workflow_Instances_Advance,
            PermissionNames.Workflow_Instances_ViewHistory,
        };
    }

    #region Role Permission Definitions

    private static List<string> GetPrincipalPermissions()
    {
        var permissions = new List<string>
        {
            // Workflow - full configuration + instance management (WF-01)
            PermissionNames.Workflow,
            PermissionNames.Workflow_Definitions,
            PermissionNames.Workflow_Definitions_View,
            PermissionNames.Workflow_Definitions_Create,
            PermissionNames.Workflow_Definitions_Edit,
            PermissionNames.Workflow_Definitions_Delete,
            PermissionNames.Workflow_Definitions_Activate,
            PermissionNames.Workflow_Instances,
            PermissionNames.Workflow_Instances_View,
            PermissionNames.Workflow_Instances_ViewAll,
            PermissionNames.Workflow_Instances_Start,
            PermissionNames.Workflow_Instances_Advance,
            PermissionNames.Workflow_Instances_Cancel,
            PermissionNames.Workflow_Instances_ViewHistory,
            PermissionNames.Workflow_Instances_Recall,
            PermissionNames.Workflow_Instances_BatchAdvance,
            PermissionNames.Workflow_Delegations,
            PermissionNames.Workflow_Delegations_View,
            PermissionNames.Workflow_Delegations_Create,
            PermissionNames.Workflow_Delegations_Revoke,

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
            PermissionNames.Academic_Subjects,
            PermissionNames.Academic_Subjects_View,
            PermissionNames.Academic_Subjects_Manage,
            PermissionNames.Academic_Classes,
            PermissionNames.Academic_Classes_View,
            PermissionNames.Academic_Classes_Create,
            PermissionNames.Academic_Classes_Edit,
            PermissionNames.Academic_ClassSubjects,
            PermissionNames.Academic_ClassSubjects_View,
            PermissionNames.Academic_ClassSubjects_Manage,
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
            PermissionNames.Assessment_Weightings,
            PermissionNames.Assessment_Weightings_View,
            PermissionNames.Assessment_Weightings_Manage,
            PermissionNames.Assessment_Weightings,
            PermissionNames.Assessment_Weightings_View,
            PermissionNames.Assessment_Weightings_Manage,
            PermissionNames.Assessment_Weightings,
            PermissionNames.Assessment_Weightings_View,
            PermissionNames.Assessment_Weightings,
            PermissionNames.Assessment_Weightings_View,
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

        permissions.AddRange(PsmsOperationsPermissions.ForPrincipal());
        return permissions;
    }

    private static List<string> GetVicePrincipalPermissions()
    {
        var permissions = new List<string>
        {
            // Workflow - full configuration + instance management (WF-01)
            PermissionNames.Workflow,
            PermissionNames.Workflow_Definitions,
            PermissionNames.Workflow_Definitions_View,
            PermissionNames.Workflow_Definitions_Create,
            PermissionNames.Workflow_Definitions_Edit,
            PermissionNames.Workflow_Definitions_Delete,
            PermissionNames.Workflow_Definitions_Activate,
            PermissionNames.Workflow_Instances,
            PermissionNames.Workflow_Instances_View,
            PermissionNames.Workflow_Instances_ViewAll,
            PermissionNames.Workflow_Instances_Start,
            PermissionNames.Workflow_Instances_Advance,
            PermissionNames.Workflow_Instances_Cancel,
            PermissionNames.Workflow_Instances_ViewHistory,
            PermissionNames.Workflow_Instances_Recall,
            PermissionNames.Workflow_Instances_BatchAdvance,
            PermissionNames.Workflow_Delegations,
            PermissionNames.Workflow_Delegations_View,
            PermissionNames.Workflow_Delegations_Create,
            PermissionNames.Workflow_Delegations_Revoke,

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
            PermissionNames.Academic_Subjects,
            PermissionNames.Academic_Subjects_View,
            PermissionNames.Academic_Subjects_Manage,
            PermissionNames.Academic_Classes,
            PermissionNames.Academic_Classes_View,
            PermissionNames.Academic_Classes_Create,
            PermissionNames.Academic_Classes_Edit,
            PermissionNames.Academic_ClassSubjects,
            PermissionNames.Academic_ClassSubjects_View,
            PermissionNames.Academic_ClassSubjects_Manage,
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

        permissions.AddRange(PsmsOperationsPermissions.ForVicePrincipal());
        return permissions;
    }

    private static List<string> GetHODPermissions()
    {
        var permissions = new List<string>
        {
            // Workflow - act on assigned approval steps (WF-01)
            PermissionNames.Workflow,
            PermissionNames.Workflow_Instances,
            PermissionNames.Workflow_Instances_View,
            PermissionNames.Workflow_Instances_Advance,
            PermissionNames.Workflow_Instances_ViewHistory,

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
            PermissionNames.Academic_Subjects,
            PermissionNames.Academic_Subjects_View,
            PermissionNames.Academic_Classes,
            PermissionNames.Academic_Classes_View,
            PermissionNames.Academic_ClassSubjects,
            PermissionNames.Academic_ClassSubjects_View,
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

        permissions.AddRange(PsmsOperationsPermissions.ForHOD());
        return permissions;
    }

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

    private static List<string> GetFinancePermissions()
    {
        var permissions = new List<string>
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

        permissions.AddRange(PsmsOperationsPermissions.ForFinance());
        return permissions;
    }

    private static List<string> GetTeacherPermissions()
    {
        var permissions = new List<string>
        {
            // Workflow - act on assigned approval steps (WF-01)
            PermissionNames.Workflow,
            PermissionNames.Workflow_Instances,
            PermissionNames.Workflow_Instances_View,
            PermissionNames.Workflow_Instances_Advance,
            PermissionNames.Workflow_Instances_ViewHistory,

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
            PermissionNames.Academic_Subjects,
            PermissionNames.Academic_Subjects_View,
            PermissionNames.Academic_Classes,
            PermissionNames.Academic_Classes_View,
            PermissionNames.Academic_ClassSubjects,
            PermissionNames.Academic_ClassSubjects_View,
            PermissionNames.Academic_Timetables,
            PermissionNames.Academic_Timetables_View,
            PermissionNames.Academic_Attendance,
            PermissionNames.Academic_Attendance_View,
            PermissionNames.Academic_Attendance_Capture,
            PermissionNames.Academic_Attendance_Edit,
            PermissionNames.Academic_Attendance_Reports,
            PermissionNames.Academic_Calendar,
            PermissionNames.Academic_Calendar_View,

            // Assessment - Class scoped
            PermissionNames.Assessment,
            PermissionNames.Assessment_Marks,
            PermissionNames.Assessment_Marks_View,
            PermissionNames.Assessment_Marks_Create,
            PermissionNames.Assessment_Marks_Edit,
            PermissionNames.Assessment_Marks_Delete,
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

        permissions.AddRange(PsmsOperationsPermissions.ForTeacher());
        return permissions;
    }

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
