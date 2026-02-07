using Abp.MultiTenancy;
using Abp.Zero.Configuration;

namespace psms.Authorization.Roles;

/// <summary>
/// Configures static roles for the PSMS (Private School Management System).
/// Static roles are created automatically for each tenant and cannot be deleted.
/// See PSMS-Permissions-Matrix.md for role-permission mappings.
/// </summary>
public static class AppRoleConfig
{
    public static void Configure(IRoleManagementConfig roleManagementConfig)
    {
        #region Host Roles

        // Host Admin - Platform administrator with full access to all tenants
        roleManagementConfig.StaticRoles.Add(
            new StaticRoleDefinition(
                StaticRoleNames.Host.Admin,
                MultiTenancySides.Host,
                grantAllPermissionsByDefault: true
            )
        );

        #endregion

        #region Tenant Roles

        // Admin - School IT administrator with full tenant access
        roleManagementConfig.StaticRoles.Add(
            new StaticRoleDefinition(
                StaticRoleNames.Tenants.Admin,
                MultiTenancySides.Tenant,
                grantAllPermissionsByDefault: true
            )
        );

        // Principal - School principal with all academic/admission decision authority
        roleManagementConfig.StaticRoles.Add(
            new StaticRoleDefinition(
                StaticRoleNames.Tenants.Principal,
                MultiTenancySides.Tenant
            )
        );

        // Vice Principal - Assists principal with limited decision authority
        roleManagementConfig.StaticRoles.Add(
            new StaticRoleDefinition(
                StaticRoleNames.Tenants.VicePrincipal,
                MultiTenancySides.Tenant
            )
        );

        // HOD - Head of Department, manages specific subject area/grade
        roleManagementConfig.StaticRoles.Add(
            new StaticRoleDefinition(
                StaticRoleNames.Tenants.HOD,
                MultiTenancySides.Tenant
            )
        );

        // Admissions Officer - Manages admission applications
        roleManagementConfig.StaticRoles.Add(
            new StaticRoleDefinition(
                StaticRoleNames.Tenants.AdmissionsOfficer,
                MultiTenancySides.Tenant
            )
        );

        // Finance - Finance Manager, manages fees, payments, financial reporting
        roleManagementConfig.StaticRoles.Add(
            new StaticRoleDefinition(
                StaticRoleNames.Tenants.Finance,
                MultiTenancySides.Tenant
            )
        );

        // Teacher - Manages assigned classes and subjects
        roleManagementConfig.StaticRoles.Add(
            new StaticRoleDefinition(
                StaticRoleNames.Tenants.Teacher,
                MultiTenancySides.Tenant
            )
        );

        // Parent - Parent/Guardian, views child's information, makes payments
        roleManagementConfig.StaticRoles.Add(
            new StaticRoleDefinition(
                StaticRoleNames.Tenants.Parent,
                MultiTenancySides.Tenant
            )
        );

        // Student - Views own information, submits work
        roleManagementConfig.StaticRoles.Add(
            new StaticRoleDefinition(
                StaticRoleNames.Tenants.Student,
                MultiTenancySides.Tenant
            )
        );

        // Applicant - Prospective Parent, submits and tracks admission applications
        // This role is used for the public application portal
        roleManagementConfig.StaticRoles.Add(
            new StaticRoleDefinition(
                StaticRoleNames.Tenants.Applicant,
                MultiTenancySides.Tenant
            )
        );

        #endregion
    }
}
