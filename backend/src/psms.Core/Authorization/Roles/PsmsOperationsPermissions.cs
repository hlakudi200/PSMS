using System.Collections.Generic;

namespace psms.Authorization.Roles;

/// <summary>
/// Per-role grants for the "operations" feature groups that were added after the
/// original role matrix was written — Parents, Student Transfers, Fee Waivers,
/// Expenses, Discipline, HR (Staff Leave), Activities (Field Trips) and the SA
/// specific programmes (Extramurals, Transport, After Care).
///
/// Every one of these app services is [AbpAuthorize]-gated at class level, so a
/// role that is not granted the group gets a 403 on the corresponding portal page.
/// The seeded approval workflows also route Disciplinary / Transfer / Leave /
/// Field Trip / Expense steps to the Principal, who must be able to open the
/// underlying record.
///
/// Single source of truth — consumed by both <see cref="PsmsRolePermissionSeeder"/>
/// (runtime tenant provisioning + additive backfill) and the EF seed
/// <c>DefaultRolesCreator</c> (tenant 1 / tests), so the two never drift.
/// </summary>
public static class PsmsOperationsPermissions
{
    /// <summary>Principal / Vice Principal: full control of every operations group.</summary>
    public static List<string> ForPrincipal()
    {
        return new List<string>
        {
            // Academic - Parents & Transfers
            PermissionNames.Academic_Parents,
            PermissionNames.Academic_Parents_View,
            PermissionNames.Academic_Parents_ViewAll,
            PermissionNames.Academic_Parents_Create,
            PermissionNames.Academic_Parents_Edit,
            PermissionNames.Academic_Parents_Delete,
            PermissionNames.Academic_Transfers,
            PermissionNames.Academic_Transfers_View,
            PermissionNames.Academic_Transfers_Create,
            PermissionNames.Academic_Transfers_Approve,

            // Financial - Fee Waivers & Expenses (approval authority)
            PermissionNames.Financial_FeeWaivers,
            PermissionNames.Financial_FeeWaivers_View,
            PermissionNames.Financial_FeeWaivers_Create,
            PermissionNames.Financial_FeeWaivers_Edit,
            PermissionNames.Financial_FeeWaivers_Delete,
            PermissionNames.Financial_FeeWaivers_Approve,
            PermissionNames.Financial_Expenses,
            PermissionNames.Financial_Expenses_View,
            PermissionNames.Financial_Expenses_Create,
            PermissionNames.Financial_Expenses_Approve,

            // Discipline
            PermissionNames.Discipline,
            PermissionNames.Discipline_Cases,
            PermissionNames.Discipline_Cases_View,
            PermissionNames.Discipline_Cases_Create,
            PermissionNames.Discipline_Cases_Edit,
            PermissionNames.Discipline_Cases_Delete,
            PermissionNames.Discipline_Cases_Manage,

            // HR - Staff Leave
            PermissionNames.HR,
            PermissionNames.HR_Leave,
            PermissionNames.HR_Leave_View,
            PermissionNames.HR_Leave_ViewAll,
            PermissionNames.HR_Leave_Create,
            PermissionNames.HR_Leave_Approve,

            // Activities - Field Trips
            PermissionNames.Activities,
            PermissionNames.Activities_FieldTrips,
            PermissionNames.Activities_FieldTrips_View,
            PermissionNames.Activities_FieldTrips_Create,
            PermissionNames.Activities_FieldTrips_Edit,
            PermissionNames.Activities_FieldTrips_Approve,

            // SA Specific - Extramurals, Transport, After Care
            PermissionNames.SASpecific,
            PermissionNames.SASpecific_Extramurals,
            PermissionNames.SASpecific_Extramurals_View,
            PermissionNames.SASpecific_Extramurals_Create,
            PermissionNames.SASpecific_Extramurals_Edit,
            PermissionNames.SASpecific_Extramurals_Delete,
            PermissionNames.SASpecific_Extramurals_Manage,
            PermissionNames.SASpecific_Transport,
            PermissionNames.SASpecific_Transport_View,
            PermissionNames.SASpecific_Transport_Create,
            PermissionNames.SASpecific_Transport_Edit,
            PermissionNames.SASpecific_Transport_Delete,
            PermissionNames.SASpecific_Transport_Manage,
            PermissionNames.SASpecific_AfterCare,
            PermissionNames.SASpecific_AfterCare_View,
            PermissionNames.SASpecific_AfterCare_Create,
            PermissionNames.SASpecific_AfterCare_Edit,
            PermissionNames.SASpecific_AfterCare_Delete,
            PermissionNames.SASpecific_AfterCare_Manage,
        };
    }

    /// <summary>Vice Principal mirrors the Principal (delegated decision authority).</summary>
    public static List<string> ForVicePrincipal() => ForPrincipal();

    /// <summary>
    /// HOD: department-level operations — raise and manage disciplinary cases,
    /// submit leave, propose field trips, view parents/transfers and the SA
    /// programmes. No financial or final-approval authority.
    /// </summary>
    public static List<string> ForHOD()
    {
        return new List<string>
        {
            PermissionNames.Academic_Parents,
            PermissionNames.Academic_Parents_View,
            PermissionNames.Academic_Parents_ViewAll,
            PermissionNames.Academic_Transfers,
            PermissionNames.Academic_Transfers_View,

            PermissionNames.Discipline,
            PermissionNames.Discipline_Cases,
            PermissionNames.Discipline_Cases_View,
            PermissionNames.Discipline_Cases_Create,
            PermissionNames.Discipline_Cases_Edit,
            PermissionNames.Discipline_Cases_Manage,

            PermissionNames.HR,
            PermissionNames.HR_Leave,
            PermissionNames.HR_Leave_View,
            PermissionNames.HR_Leave_ViewAll,
            PermissionNames.HR_Leave_Create,

            PermissionNames.Activities,
            PermissionNames.Activities_FieldTrips,
            PermissionNames.Activities_FieldTrips_View,
            PermissionNames.Activities_FieldTrips_Create,
            PermissionNames.Activities_FieldTrips_Edit,

            PermissionNames.SASpecific,
            PermissionNames.SASpecific_Extramurals,
            PermissionNames.SASpecific_Extramurals_View,
            PermissionNames.SASpecific_Transport,
            PermissionNames.SASpecific_Transport_View,
            PermissionNames.SASpecific_AfterCare,
            PermissionNames.SASpecific_AfterCare_View,
        };
    }

    /// <summary>
    /// Teacher: report disciplinary incidents, request own leave, propose field
    /// trips and look up parents / SA programme rosters for their learners.
    /// </summary>
    public static List<string> ForTeacher()
    {
        return new List<string>
        {
            PermissionNames.Academic_Parents,
            PermissionNames.Academic_Parents_View,

            PermissionNames.Discipline,
            PermissionNames.Discipline_Cases,
            PermissionNames.Discipline_Cases_View,
            PermissionNames.Discipline_Cases_Create,
            PermissionNames.Discipline_Cases_Edit,

            PermissionNames.HR,
            PermissionNames.HR_Leave,
            PermissionNames.HR_Leave_View,
            PermissionNames.HR_Leave_Create,

            PermissionNames.Activities,
            PermissionNames.Activities_FieldTrips,
            PermissionNames.Activities_FieldTrips_View,
            PermissionNames.Activities_FieldTrips_Create,

            PermissionNames.SASpecific,
            PermissionNames.SASpecific_Extramurals,
            PermissionNames.SASpecific_Extramurals_View,
            PermissionNames.SASpecific_Transport,
            PermissionNames.SASpecific_Transport_View,
            PermissionNames.SASpecific_AfterCare,
            PermissionNames.SASpecific_AfterCare_View,
        };
    }

    /// <summary>
    /// Finance: owns fee waivers end to end and raises / tracks expenses. Final
    /// approval of both stays with the Principal (see seeded workflows).
    /// </summary>
    public static List<string> ForFinance()
    {
        return new List<string>
        {
            PermissionNames.Financial_FeeWaivers,
            PermissionNames.Financial_FeeWaivers_View,
            PermissionNames.Financial_FeeWaivers_Create,
            PermissionNames.Financial_FeeWaivers_Edit,
            PermissionNames.Financial_FeeWaivers_Delete,
            PermissionNames.Financial_Expenses,
            PermissionNames.Financial_Expenses_View,
            PermissionNames.Financial_Expenses_Create,
        };
    }
}
