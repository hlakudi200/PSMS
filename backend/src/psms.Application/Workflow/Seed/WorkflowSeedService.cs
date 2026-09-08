using Abp.Application.Services;
using Abp.Authorization;
using psms.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace psms.Workflow.Seed;

/// <summary>
/// Tenant-side endpoint to (re)apply the eight default approval workflows to the
/// CURRENT tenant. Creates missing definitions and refreshes untouched
/// version-1 defaults; customised or in-use definitions are left alone.
/// New tenants get the defaults at creation (TenantAppService); the host can
/// backfill any tenant with Tenant/SeedWorkflowDefinitions.
/// </summary>
[AbpAuthorize(PermissionNames.Workflow_Definitions_Create)]
public class WorkflowSeedService : ApplicationService
{
    private readonly WorkflowDefinitionSeeder _seeder;

    public WorkflowSeedService(WorkflowDefinitionSeeder seeder)
    {
        _seeder = seeder;
    }

    public async Task<List<WorkflowSeedOutcome>> SeedDefaultsAsync()
    {
        var outcomes = await _seeder.SeedDefaultsAsync(AbpSession.TenantId);
        await CurrentUnitOfWork.SaveChangesAsync();
        return outcomes;
    }
}
