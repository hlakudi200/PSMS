using Abp.Application.Services;
using Abp.Authorization;
using psms.Authorization;
using psms.Authorization.Roles;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace psms.Authorization;

/// <summary>
/// Brings this school's roles up to what the code now says they should hold.
/// <para>
/// A permission added to a role in code only ever reaches tenants created
/// afterwards. The two existing backfills (<c>GrantWorkflowPermissions</c>,
/// <c>GrantOperationsPermissions</c>) each cover one feature group, and both sit
/// on <c>TenantAppService</c> behind <c>Pages.Tenants</c> — a **host-only**
/// permission. A school's own administrator cannot reach either of them, and
/// the host admin signs in to no tenant, so in practice neither gets run and
/// schools quietly fall behind the code.
/// </para>
/// <para>
/// This one is tenant-level and guarded by <c>Pages.Roles</c>, which the
/// school's administrator already holds because they manage roles. They can run
/// it themselves. It is additive and idempotent: it grants what is missing and
/// never removes anything.
/// </para>
/// </summary>
[AbpAuthorize(PermissionNames.Pages_Roles)]
public class RolePermissionCatchUpService : ApplicationService
{
    private readonly PsmsRolePermissionSeeder _seeder;

    public RolePermissionCatchUpService(PsmsRolePermissionSeeder seeder)
    {
        _seeder = seeder;
    }

    /// <summary>
    /// Grants every role of the signed-in school whatever the current code says
    /// it should have and is missing, and reports what that was. Safe to run
    /// repeatedly: a second run reports nothing granted.
    /// </summary>
    public async Task<List<RoleCatchUp>> CatchUpAsync()
    {
        var report = await _seeder.CatchUpEveryRoleAsync();
        await CurrentUnitOfWork.SaveChangesAsync();
        return report;
    }
}
