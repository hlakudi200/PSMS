using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.IdentityFramework;
using Abp.Linq.Extensions;
using Abp.MultiTenancy;
using Abp.Runtime.Security;
using psms.Authorization;
using psms.Authorization.Roles;
using psms.Authorization.Users;
using psms.Workflow.Seed;
using psms.Domain.Tenancy.Entities;
using psms.Editions;
using psms.MultiTenancy.Dto;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace psms.MultiTenancy;

[AbpAuthorize(PermissionNames.Pages_Tenants)]
public class TenantAppService : AsyncCrudAppService<Tenant, TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>, ITenantAppService
{
    private readonly TenantManager _tenantManager;
    private readonly EditionManager _editionManager;
    private readonly UserManager _userManager;
    private readonly RoleManager _roleManager;
    private readonly IAbpZeroDbMigrator _abpZeroDbMigrator;
    private readonly PsmsRolePermissionSeeder _rolePermissionSeeder;
    private readonly IRepository<SchoolBranding, Guid> _brandingRepository;
    private readonly WorkflowSeedService _workflowSeeder;

    public TenantAppService(
        IRepository<Tenant, int> repository,
        TenantManager tenantManager,
        EditionManager editionManager,
        UserManager userManager,
        RoleManager roleManager,
        IAbpZeroDbMigrator abpZeroDbMigrator,
        PsmsRolePermissionSeeder rolePermissionSeeder,
        IRepository<SchoolBranding, Guid> brandingRepository,
        WorkflowSeedService workflowSeeder)
        : base(repository)
    {
        _tenantManager = tenantManager;
        _editionManager = editionManager;
        _userManager = userManager;
        _roleManager = roleManager;
        _abpZeroDbMigrator = abpZeroDbMigrator;
        _rolePermissionSeeder = rolePermissionSeeder;
        _brandingRepository = brandingRepository;
        _workflowSeeder = workflowSeeder;
    }

    public override async Task<TenantDto> CreateAsync(CreateTenantDto input)
    {
        CheckCreatePermission();

        // Create tenant
        var tenant = ObjectMapper.Map<Tenant>(input);
        tenant.ConnectionString = input.ConnectionString.IsNullOrEmpty()
            ? null
            : SimpleStringCipher.Instance.Encrypt(input.ConnectionString);

        var defaultEdition = await _editionManager.FindByNameAsync(EditionManager.DefaultEditionName);
        if (defaultEdition != null)
        {
            tenant.EditionId = defaultEdition.Id;
        }

        await _tenantManager.CreateAsync(tenant);
        await CurrentUnitOfWork.SaveChangesAsync(); // To get new tenant's id.

        // Create tenant database
        _abpZeroDbMigrator.CreateOrMigrateForTenant(tenant);

        // We are working entities of new tenant, so changing tenant filter
        using (CurrentUnitOfWork.SetTenantId(tenant.Id))
        {
            // Create static roles for new tenant
            CheckErrors(await _roleManager.CreateStaticRoles(tenant.Id));

            await CurrentUnitOfWork.SaveChangesAsync(); // To get static role ids

            // Grant all permissions to admin role
            var adminRole = _roleManager.Roles.Single(r => r.Name == StaticRoleNames.Tenants.Admin);
            await _roleManager.GrantAllPermissionsAsync(adminRole);

            // Grant PSMS-specific permissions to other roles (Principal, Teacher, etc.)
            await _rolePermissionSeeder.SeedRolePermissionsAsync(tenant.Id);

            await CurrentUnitOfWork.SaveChangesAsync();

            // Create admin user for the tenant
            var adminUser = User.CreateTenantAdminUser(tenant.Id, input.AdminEmailAddress);
            await _userManager.InitializeOptionsAsync(tenant.Id);
            CheckErrors(await _userManager.CreateAsync(adminUser, User.DefaultPassword));
            await CurrentUnitOfWork.SaveChangesAsync(); // To get admin user's id

            // Assign admin user to role!
            CheckErrors(await _userManager.AddToRoleAsync(adminUser, adminRole.Name));
            await CurrentUnitOfWork.SaveChangesAsync();

            // Issue #56: seed default branding so the new school has an editable
            // row from day one. Tenants created before this feature have no row;
            // SchoolBrandingAppService creates one on first write, so both paths
            // converge without a data backfill.
            await _brandingRepository.InsertAsync(new SchoolBranding(Guid.NewGuid(), tenant.Id));
            await CurrentUnitOfWork.SaveChangesAsync();

            /* The workflow definitions a school runs on — admissions, report
               approval, leave, the rest. WorkflowSeedService wrote them and
               nothing ever called it, so a new school started with no
               definitions at all: TryStartWorkflowAsync would log "no active
               workflow definition" and return, and every submitted application
               sat under review with no process behind it and nobody's inbox to
               appear in. */
            await _workflowSeeder.SeedDefaultsAsync();
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        return MapToEntityDto(tenant);
    }

    protected override IQueryable<Tenant> CreateFilteredQuery(PagedTenantResultRequestDto input)
    {
        return Repository.GetAll()
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.TenancyName.Contains(input.Keyword) || x.Name.Contains(input.Keyword))
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive);
    }

    protected override IQueryable<Tenant> ApplySorting(IQueryable<Tenant> query, PagedTenantResultRequestDto input)
    {
        return query.OrderBy(input.Sorting);
    }

    protected override void MapToEntity(TenantDto updateInput, Tenant entity)
    {
        // Manually mapped since TenantDto contains non-editable properties too.
        entity.Name = updateInput.Name;
        entity.TenancyName = updateInput.TenancyName;
        entity.IsActive = updateInput.IsActive;
    }

    public override async Task DeleteAsync(EntityDto<int> input)
    {
        CheckDeletePermission();

        var tenant = await _tenantManager.GetByIdAsync(input.Id);
        await _tenantManager.DeleteAsync(tenant);
    }

    /// <summary>
    /// Seeds PSMS role permissions for an existing tenant.
    /// Use this to fix tenants that were created before the permission seeder was added.
    /// WARNING: this REPLACES each standard role's full permission set with the
    /// canonical list — it will strip any tenant-specific customisations a role
    /// has accumulated. For purely additive backfills (e.g. workflow access)
    /// prefer <see cref="GrantWorkflowPermissionsAsync"/>.
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_Tenants)]
    public async Task SeedRolePermissionsAsync(EntityDto<int> input)
    {
        var tenant = await _tenantManager.GetByIdAsync(input.Id);

        using (CurrentUnitOfWork.SetTenantId(tenant.Id))
        {
            await _rolePermissionSeeder.SeedRolePermissionsAsync(tenant.Id);
            await CurrentUnitOfWork.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Writes the default workflow definitions for an existing tenant, and
    /// brings ones written earlier onto the current shape.
    /// <para>
    /// <c>WorkflowSeedService</c> had no caller anywhere in the application. It
    /// was reachable only because ABP generates a controller for every
    /// application service, so a school's definitions existed if somebody had
    /// once posted to that endpoint by hand — and every upgrade written since
    /// then had never run on anybody.
    /// </para>
    /// <para>
    /// New tenants are seeded on creation now. This is the same work for the
    /// ones that came before, and it is safe to run on a live school: each pass
    /// leaves alone any step that has been renamed, reassigned or repurposed.
    /// Idempotent.
    /// </para>
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_Tenants)]
    public async Task SeedWorkflowDefinitionsAsync(EntityDto<int> input)
    {
        var tenant = await _tenantManager.GetByIdAsync(input.Id);

        using (CurrentUnitOfWork.SetTenantId(tenant.Id))
        {
            await _workflowSeeder.SeedDefaultsAsync();
            await CurrentUnitOfWork.SaveChangesAsync();
        }
    }

    /// <summary>
    /// WF-01: additively grant workflow permissions to an existing tenant's staff
    /// roles (Principal/VP get full config; HOD/Teacher get view+advance) without
    /// disturbing their other grants. Safe to run on live, customised tenants —
    /// unlike <see cref="SeedRolePermissionsAsync"/> which replaces the full set.
    /// Idempotent.
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_Tenants)]
    public async Task GrantWorkflowPermissionsAsync(EntityDto<int> input)
    {
        var tenant = await _tenantManager.GetByIdAsync(input.Id);

        using (CurrentUnitOfWork.SetTenantId(tenant.Id))
        {
            await _rolePermissionSeeder.GrantWorkflowPermissionsToStaffRolesAsync();
            await CurrentUnitOfWork.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Brings one school's roles up to what the code now says they should hold,
    /// additively. The host counterpart of
    /// <see cref="psms.Authorization.RolePermissionCatchUpService"/>.
    /// <para>
    /// Both exist because each is the only usable one for somebody. A school's
    /// administrator can run the tenant-level service and cannot reach this,
    /// which is host-only; an operator bringing every school up to date after a
    /// release has no sign-in for most of them and needs this.
    /// </para>
    /// <para>
    /// Additive and idempotent — unlike <c>SeedRolePermissionsAsync</c>, which
    /// replaces a role's whole set and would wipe a school's customisations.
    /// </para>
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_Tenants)]
    public async Task<List<psms.Authorization.RoleCatchUp>> CatchUpRolePermissionsAsync(EntityDto<int> input)
    {
        var tenant = await _tenantManager.GetByIdAsync(input.Id);

        using (CurrentUnitOfWork.SetTenantId(tenant.Id))
        {
            /* Reported back rather than answered with a bare 200: an operator
               running this across every school needs to see which were behind,
               and a host token cannot read a tenant's roles afterwards to
               check for itself. */
            var report = await _rolePermissionSeeder.CatchUpEveryRoleAsync();
            await CurrentUnitOfWork.SaveChangesAsync();
            return report;
        }
    }

    /// <summary>
    /// Additively grant the operations feature groups (Parents, Transfers, Fee
    /// Waivers, Expenses, Discipline, Staff Leave, Field Trips, Extramurals,
    /// Transport, After Care) to an existing tenant's staff roles without
    /// disturbing their other grants. Safe to run on live, customised tenants —
    /// unlike <see cref="SeedRolePermissionsAsync"/> which replaces the full set.
    /// Idempotent.
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_Tenants)]
    public async Task GrantOperationsPermissionsAsync(EntityDto<int> input)
    {
        var tenant = await _tenantManager.GetByIdAsync(input.Id);

        using (CurrentUnitOfWork.SetTenantId(tenant.Id))
        {
            await _rolePermissionSeeder.GrantOperationsPermissionsToStaffRolesAsync();
            await CurrentUnitOfWork.SaveChangesAsync();
        }
    }

    private void CheckErrors(IdentityResult identityResult)
    {
        identityResult.CheckErrors(LocalizationManager);
    }
}

