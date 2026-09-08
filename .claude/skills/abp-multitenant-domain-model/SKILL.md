---
name: abp-multitenant-domain-model
description: Design and implement a multi-tenant domain model for classic ASP.NET Boilerplate (Abp 10.x + EF Core/Npgsql) that supports MULTIPLE TENANT TYPES (e.g. School / District / ServiceProvider) — tenant-type discriminator on AbpTenant, per-type profile entities, the IMayHaveTenant vs IMustHaveTenant filter contract, host-owned shared reference data with tenant overrides, Editions+Features per tenant type, per-type role/permission seeding, tenant hierarchy, provisioning, and the DbContext/migration wiring. Use when adding a new tenant type, modelling a new domain entity's tenancy, or making an existing single-type app multi-type.
---

# Multi-Tenant (Multi-Type) Domain Modelling for ABP

How to model the **Domain layer** (`src/<App>.Core/Domain/**`) and the **EF Core layer**
(`src/<App>.EntityFrameworkCore/**`) so one deployment serves several *kinds* of tenant, not just
several instances of one kind.

Stack assumed (this repo): classic ABP (`Abp` 10.x, **not** Volo), .NET 9, EF Core + Npgsql,
single shared database, `AbpZeroDbContext<Tenant, Role, User, psmsDbContext>`.

Companion skills: **abp-backend-crud** (the per-entity vertical slice), **abp-authorization**
(permission tree + role seeding), **frontend-crud-page** (the UI on top).

---

## 0. Decision gate — do you actually need multiple tenant *types*?

Answer these four before writing code. The answers pick the strategy.

| Question | If **no** | If **yes** |
|---|---|---|
| Do the types own **different data** (different entities / different required attributes)? | — | Strategy **A**: discriminator + profile entities |
| Do the types only **behave** differently (same data, some modules on/off)? | — | Strategy **B**: Editions + Features (may be enough on its own) |
| Does one type **contain** another (a District owns Schools)? | — | Strategy **C**: tenant hierarchy (`ParentTenantId`) |
| Do the types need **different roles / permission sets**? | — | Per-type role seeding (§7) |

**Only B?** Stop — don't add a discriminator. ABP's Edition + Feature system already models
"same app, different capabilities" and costs no schema surface. A discriminator you never branch on
is dead weight.

**A (± B ± C) is the normal answer** for something like School / District / ServiceProvider. Combine
them: A gives the data shape, B gives the behaviour gate, C gives the org chart.

**Never** model a tenant type as a separate `DbContext`, a separate database per type, or a
`TenantType` column duplicated onto every domain entity. One `DbContext`, one discriminator on
`Tenant`, and derive everything else from it.

---

## 1. Name the types — `TenantType` enum

`src/<App>.Core/Domain/Shared/Enums/TenantType.cs`, alongside the other domain enums.

```csharp
namespace psms.Domain.Shared.Enums
{
    /// <summary>
    /// The kind of organisation a tenant represents. Drives edition/feature
    /// assignment, role seeding, and which profile entity the tenant owns.
    /// Values are persisted as int — never renumber an existing member.
    /// </summary>
    public enum TenantType
    {
        /// <summary>An individual school — the operational tenant.</summary>
        School = 1,

        /// <summary>A district/group office that owns many School tenants.</summary>
        District = 2,

        /// <summary>An external provider (transport, after-care) with scoped access.</summary>
        ServiceProvider = 3
    }
}
```

Rules: explicit numeric values, never reuse or renumber, never a `[Flags]` enum (a tenant is exactly
one type). Adding a type later is additive and safe.

---

## 2. Extend `Tenant` — the discriminator lives here and nowhere else

`src/<App>.Core/MultiTenancy/Tenant.cs`. This is the *only* place the type is stored.

```csharp
using Abp.MultiTenancy;
using psms.Authorization.Users;
using psms.Domain.Shared.Enums;

namespace psms.MultiTenancy;

public class Tenant : AbpTenant<User>
{
    /// <summary>What kind of organisation this tenant is. Immutable after creation.</summary>
    public TenantType TenantType { get; set; }

    /// <summary>
    /// Owning parent tenant (e.g. the District that owns this School). Null for
    /// top-level tenants. See <see cref="CanBeChildOf"/> for the legal pairings.
    /// </summary>
    public int? ParentTenantId { get; set; }

    /// <summary>EF Core ctor.</summary>
    public Tenant()
    {
    }

    public Tenant(string tenancyName, string name, TenantType tenantType, int? parentTenantId = null)
        : base(tenancyName, name)
    {
        TenantType = tenantType;
        ParentTenantId = parentTenantId;
    }

    /// <summary>
    /// Legal parent/child pairings. Keep this table here — it is a domain
    /// invariant, not a provisioning detail.
    /// </summary>
    public static bool CanBeChildOf(TenantType child, TenantType parent)
        => child == TenantType.School && parent == TenantType.District;
}
```

Keep the parameterless ctor — ABP/EF and `AbpTenantManager` both need it.

**Migration note.** `TenantType` is non-nullable on a table that already has rows. Add the column
with a default that matches today's reality, then drop the default:

```csharp
migrationBuilder.AddColumn<int>(
    name: "TenantType", table: "AbpTenants",
    type: "integer", nullable: false, defaultValue: 1);   // 1 = School

migrationBuilder.AlterColumn<int>(
    name: "TenantType", table: "AbpTenants",
    type: "integer", nullable: false, oldDefaultValue: 1);
```

---

## 3. Per-type attributes — profile entities, not nullable columns on `Tenant`

Everything a *School* needs and a *District* doesn't goes in a **profile entity**: a 1:1 row keyed by
`TenantId`. Do **not** pile nullable type-specific columns onto `AbpTenants`.

`src/<App>.Core/Domain/Tenancy/Entities/SchoolProfile.cs`

```csharp
[Table("SchoolProfiles")]
public class SchoolProfile : FullAuditedEntity<Guid>, IMustHaveTenant, ISoftDelete
{
    public const int MaxEmisNumberLength = 20;
    public const int MaxNameLength = 200;

    /// <summary>The tenant this profile describes. Unique — one profile per tenant.</summary>
    public int TenantId { get; set; }

    [Required, StringLength(MaxEmisNumberLength)]
    public string EmisNumber { get; set; }

    [Required]
    public SouthAfricanSchoolPhase Phase { get; set; }

    [Required]
    public SouthAfricanProvince Province { get; set; }

    public Address PhysicalAddress { get; set; }     // owned type, see §6

    public bool IsDeleted { get; set; }

    protected SchoolProfile() { }

    public SchoolProfile(Guid id, int tenantId, string emisNumber,
        SouthAfricanSchoolPhase phase, SouthAfricanProvince province) : this()
    {
        Id = id; TenantId = tenantId; EmisNumber = emisNumber;
        Phase = phase; Province = province; IsDeleted = false;
    }
}
```

`DistrictProfile`, `ServiceProviderProfile` follow the same shape with their own fields.

Why `IMustHaveTenant` here and not `IMayHaveTenant` — see the next section. It is the single most
consequential choice in this whole skill.

---

## 4. The tenancy contract per entity — pick one of three, deliberately

ABP applies a global EF query filter based on which interface the entity implements. The two behave
**differently for the host**, and getting it backwards is the classic multi-tenant bug.

| Entity implements | `TenantId` | Filter predicate | Tenant session sees | **Host session (`TenantId == null`) sees** |
|---|---|---|---|---|
| `IMustHaveTenant` | `int` | `TenantId == CurrentTenantId`, **filter auto-disabled when host** | own rows | **every tenant's rows** |
| `IMayHaveTenant` | `int?` | `TenantId == CurrentTenantId`, always on | own rows | **only host rows (`TenantId == null`)** |
| *(neither)* | — | none | all rows | all rows |

Decision rule:

- **`IMustHaveTenant`** — the row can never exist without a tenant, *and* host admins / cross-tenant
  reporting must be able to read it. Profile entities, and any operational record you want visible in
  a host console, belong here. This is the right default for a multi-type platform.
- **`IMayHaveTenant`** — the entity legitimately has **host-level rows**: a platform-owned catalogue
  that tenants extend or override (§5). Note the consequence: a host admin querying it will *not*
  see tenant rows unless the filter is explicitly disabled.
- **Neither** — truly global, tenant-agnostic lookups (country list, currency codes). Rare; prefer a
  host-level `IMayHaveTenant` row so a tenant can override later.

> This repo's existing entities (`Student`, `Teacher`, …) use `IMayHaveTenant` because they predate
> the multi-type model. **Do not "fix" them wholesale** — changing the interface changes the filter
> and silently changes what every existing query returns. Use `IMustHaveTenant` for *new* entities,
> and migrate an existing one only as a deliberate, tested ticket.

### Never branch on tenant type inside a domain entity

An entity does **not** carry `TenantType`. If `Student` only exists for `School` tenants, that is
enforced at the application/provisioning boundary (§8), not by a column. A `TenantType` copied onto
domain rows will drift out of sync with `AbpTenants` the first time a tenant is reclassified.

---

## 5. Shared reference data — host catalogue + tenant override

The pattern for "the platform ships a default list; each tenant may customise it": one
`IMayHaveTenant` entity where `TenantId == null` rows are the platform catalogue.

```csharp
[Table("SubjectCatalogEntries")]
public class SubjectCatalogEntry : FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
{
    /// <summary>Null = platform-owned default; set = this tenant's override.</summary>
    public int? TenantId { get; set; }

    [Required, StringLength(20)]
    public string Code { get; set; }          // the identity across host + tenant rows

    [Required, StringLength(200)]
    public string Name { get; set; }

    public bool IsDeleted { get; set; }
}
```

Reading it requires disabling the filter and resolving the override yourself — the filter cannot
express "mine, else the host's":

```csharp
using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
{
    var tenantId = AbpSession.TenantId;

    var rows = await _catalogRepository.GetAll()
        .Where(e => e.TenantId == null || e.TenantId == tenantId)
        .ToListAsync();

    // Tenant row wins over the host row with the same Code.
    var effective = rows
        .GroupBy(e => e.Code)
        .Select(g => g.FirstOrDefault(e => e.TenantId != null) ?? g.First())
        .ToList();
}
```

Two guardrails: (1) `DisableFilter` must wrap the *smallest* possible scope — never a whole app
service method; (2) the `Where` clause re-imposing `TenantId == null || TenantId == tenantId` is
mandatory, it is the only thing standing between this query and a cross-tenant leak.

Unique index must tolerate both sides: `(TenantId, Code)` unique, soft-delete-filtered (§6).

---

## 6. EF Core wiring — `psmsDbContext`

All configuration is central; there are no per-entity `IEntityTypeConfiguration` classes in this repo.

```csharp
/* DbSets — group under a Tenancy module header */
public DbSet<SchoolProfile> SchoolProfiles { get; set; }
public DbSet<DistrictProfile> DistrictProfiles { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    ConfigureDateTimeUtcConversion(modelBuilder);   // existing — PostgreSQL needs UTC
    ConfigureTenancyModule(modelBuilder);           // add this
    ConfigureAcademicModule(modelBuilder);
    // …
}

private void ConfigureTenancyModule(ModelBuilder modelBuilder)
{
    // Tenant type is stored as int (the enum's declared values).
    modelBuilder.Entity<Tenant>()
        .Property(t => t.TenantType)
        .HasConversion<int>();

    // Hierarchy self-reference. RESTRICT: never cascade-delete a district's schools.
    modelBuilder.Entity<Tenant>()
        .HasOne<Tenant>()
        .WithMany()
        .HasForeignKey(t => t.ParentTenantId)
        .OnDelete(DeleteBehavior.Restrict);

    // Query index for "all tenants of type X" / "children of district Y".
    modelBuilder.Entity<Tenant>()
        .HasIndex(t => new { t.TenantType, t.ParentTenantId })
        .HasDatabaseName("IX_AbpTenants_TenantType_ParentTenantId");

    // One profile per tenant, soft-delete aware.
    modelBuilder.Entity<SchoolProfile>()
        .HasIndex(p => p.TenantId)
        .IsUnique()
        .HasFilter("\"IsDeleted\" = false")
        .HasDatabaseName("IX_SchoolProfiles_TenantId");

    modelBuilder.Entity<SchoolProfile>().OwnsOne(p => p.PhysicalAddress);

    // Catalogue: one row per code per tenant, and one host row per code.
    modelBuilder.Entity<SubjectCatalogEntry>()
        .HasIndex(e => new { e.TenantId, e.Code })
        .IsUnique()
        .HasFilter("\"IsDeleted\" = false")
        .HasDatabaseName("IX_SubjectCatalogEntries_TenantId_Code");
}
```

Non-negotiables in this layer:

- **Every uniqueness index on a tenant-scoped entity leads with `TenantId`.** A bare
  `HasIndex(x => x.Code).IsUnique()` makes one tenant's code collide with another's. Audit for this
  first when reviewing a multi-tenant model.
- **Soft-delete-aware filters.** Any `FullAuditedEntity` unique index needs
  `.HasFilter("\"IsDeleted\" = false")`, or a deleted row blocks re-creating the same key.
- **Never call `HasQueryFilter` for tenancy.** ABP's `AbpDbContext` already applies the
  MayHaveTenant / MustHaveTenant / SoftDelete filters. A hand-written filter stacks on top and cannot
  be lifted by `DisableFilter`, producing queries you cannot debug.
- **Value objects use `OwnsOne`** (see `Address`, `Money` in `Domain/Shared/ValueObjects`) — they
  inherit the owner's tenancy, so they need no `TenantId` of their own.
- **`DateTime` is handled globally** by `ConfigureDateTimeUtcConversion`; don't add per-property
  converters.

Migration (from `backend/`):

```bash
dotnet ef migrations add Add_TenantTypes_And_Profiles \
  --project src/psms.EntityFrameworkCore \
  --startup-project src/psms.Web.Host
```

---

## 7. Behaviour per type — Editions + Features, then roles

### 7a. One Edition per tenant type

ABP's Edition is the native "plan/type" carrier, and `EditionId` already exists on `AbpTenant`. Seed
one edition per tenant type in `Seed/Host/DefaultEditionCreator.cs` and switch the feature values.

```csharp
public static class AppEditionNames
{
    public const string School          = "School";
    public const string District        = "District";
    public const string ServiceProvider = "ServiceProvider";
}
```

Map type → edition in one place so provisioning and reporting agree:

```csharp
public static string EditionNameFor(TenantType type) => type switch
{
    TenantType.School          => AppEditionNames.School,
    TenantType.District        => AppEditionNames.District,
    TenantType.ServiceProvider => AppEditionNames.ServiceProvider,
    _ => throw new ArgumentOutOfRangeException(nameof(type))
};
```

### 7b. Features gate the modules

`src/<App>.Core/Features/AppFeatureProvider.cs`:

```csharp
public class AppFeatureProvider : FeatureProvider
{
    public override void SetFeatures(IFeatureDefinitionContext context)
    {
        var admissions = context.Create(
            AppFeatures.Admissions, defaultValue: "false",
            displayName: L("Admissions"), inputType: new CheckboxInputType());

        admissions.CreateChildFeature(
            AppFeatures.Admissions_OnlineApplications, defaultValue: "false",
            displayName: L("OnlineApplications"), inputType: new CheckboxInputType());

        context.Create(AppFeatures.DistrictReporting, defaultValue: "false",
            displayName: L("DistrictReporting"), inputType: new CheckboxInputType());
    }

    private static ILocalizableString L(string name)
        => new LocalizableString(name, psmsConsts.LocalizationSourceName);
}
```

Register in `psmsCoreModule.PreInitialize()`:

```csharp
Configuration.Features.Providers.Add<AppFeatureProvider>();
```

Enforce on the app service (this is the ABP-native gate — prefer it over hand-rolled type checks):

```csharp
[AbpAuthorize(PermissionNames.Admissions_Applications_Create)]
[RequiresFeature(AppFeatures.Admissions)]
public async Task<ApplicationDto> CreateAsync(CreateApplicationDto input) { … }
```

For conditional logic rather than a hard gate, inject `IFeatureChecker`:

```csharp
if (await FeatureChecker.IsEnabledAsync(AppFeatures.DistrictReporting)) { … }
```

Features are per-*tenant* with an edition default, so a single tenant can be granted an exception
without inventing a new type. That is why B usually beats adding a fourth `TenantType`.

### 7c. Roles and permissions per type

Extend `StaticRoleNames` with a nested class per tenant type, then make the role seeder
type-aware. See **abp-authorization** for the permission-tree side.

```csharp
public static class StaticRoleNames
{
    public static class Host { public const string Admin = "Admin"; }

    public static class Schools          // existing Tenants.* roles move/alias here
    {
        public const string Admin     = "Admin";
        public const string Principal = "Principal";
        public const string Teacher   = "Teacher";
        // …
    }

    public static class Districts
    {
        public const string Admin              = "Admin";
        public const string DistrictDirector   = "DistrictDirector";
        public const string DistrictAnalyst    = "DistrictAnalyst";
    }
}
```

```csharp
public class DefaultRolesCreator
{
    private readonly psmsDbContext _context;
    private readonly int _tenantId;
    private readonly TenantType _tenantType;

    public DefaultRolesCreator(psmsDbContext context, int tenantId, TenantType tenantType)
    { _context = context; _tenantId = tenantId; _tenantType = tenantType; }

    public void Create()
    {
        switch (_tenantType)
        {
            case TenantType.School:          CreateSchoolRoles();          break;
            case TenantType.District:        CreateDistrictRoles();        break;
            case TenantType.ServiceProvider: CreateServiceProviderRoles(); break;
        }
    }
    // CreateRoleIfNotExists(roleName, permissions) unchanged
}
```

Permissions that only a host may hold get `multiTenancySides: MultiTenancySides.Host` in the
authorization provider; the rest default to both sides. There is no per-tenant-*type* side in ABP —
type scoping is done by which roles you seed and by `[RequiresFeature]`.

---

## 8. Provisioning — one place that knows how to build each type

`src/<App>.Core/MultiTenancy/TenantProvisioner.cs`, a domain service. Everything type-specific
converges here: validation, edition, profile, roles, admin user.

```csharp
public class TenantProvisioner : DomainService
{
    private readonly TenantManager _tenantManager;
    private readonly EditionManager _editionManager;
    private readonly IRepository<SchoolProfile, Guid> _schoolProfiles;

    public async Task<Tenant> CreateAsync(
        string tenancyName, string name, TenantType type, int? parentTenantId)
    {
        // 1. Domain invariants on the hierarchy.
        if (parentTenantId.HasValue)
        {
            var parent = await _tenantManager.GetByIdAsync(parentTenantId.Value);
            if (!Tenant.CanBeChildOf(type, parent.TenantType))
                throw new UserFriendlyException(
                    TenancyExceptionCodes.InvalidParentTenantType,
                    L("InvalidParentTenantType"));
        }

        // 2. Edition follows from the type.
        var edition = await _editionManager.FindByNameAsync(EditionNameFor(type));

        var tenant = new Tenant(tenancyName, name, type, parentTenantId)
        {
            EditionId = edition?.Id,
            IsActive = true
        };

        await _tenantManager.CreateAsync(tenant);
        await CurrentUnitOfWork.SaveChangesAsync();      // materialise tenant.Id

        // 3. Everything below is written AS the new tenant.
        using (CurrentUnitOfWork.SetTenantId(tenant.Id))
        {
            await CreateProfileAsync(tenant);
            // roles + admin user seeded here (see DefaultRolesCreator, §7c)
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        return tenant;
    }

    private Task CreateProfileAsync(Tenant tenant) => tenant.TenantType switch
    {
        TenantType.School   => _schoolProfiles.InsertAsync(
                                   new SchoolProfile(Guid.NewGuid(), tenant.Id, …)),
        TenantType.District => …,
        _ => Task.CompletedTask
    };
}
```

Key mechanics:

- `CurrentUnitOfWork.SetTenantId(id)` is how you write rows *into* another tenant. Without it,
  `IMayHaveTenant` rows get the ambient (host = null) tenant id and vanish from the tenant's view.
- `SaveChangesAsync()` **before** `SetTenantId` — you need the generated `tenant.Id`.
- In DB seeders (`Seed/**`, running outside a normal session) set
  `context.SuppressAutoSetTenantId = true` and assign `TenantId` explicitly, as `SeedHelper` already
  does.

### Runtime type checks in app services

When a gate can't be expressed as a feature, resolve the current tenant's type — don't re-query
`AbpTenants` inline everywhere.

```csharp
public interface ITenantTypeResolver : ITransientDependency
{
    /// <summary>Type of the current session's tenant; null when running as host.</summary>
    Task<TenantType?> GetCurrentAsync();

    /// <summary>Throws unless the current tenant is one of <paramref name="allowed"/>.</summary>
    Task EnsureAsync(params TenantType[] allowed);
}
```

Implement it over `IRepository<Tenant>` + `ICacheManager` (tenant type changes ~never; cache it), and
read the tenant id from `AbpSession.TenantId`. Call `EnsureAsync` at the top of the app-service
method, next to `[AbpAuthorize]`.

---

## 9. Cross-tenant reads (hierarchy roll-up)

A District reading its Schools' data is a *deliberate* filter bypass. It needs all four of these, and
a code reviewer should reject it if any is missing:

```csharp
[AbpAuthorize(PermissionNames.District_Reporting_ViewChildTenants)]   // 1. permission-gated
public async Task<List<EnrolmentSummaryDto>> GetChildEnrolmentAsync()
{
    await _tenantTypeResolver.EnsureAsync(TenantType.District);       // 2. type-gated

    var districtId = AbpSession.GetTenantId();
    var childIds = await _tenantRepository.GetAll()
        .Where(t => t.ParentTenantId == districtId && t.IsActive)
        .Select(t => t.Id)
        .ToListAsync();

    using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant))  // 3. narrow scope
    {
        return await _studentRepository.GetAll()
            .Where(s => childIds.Contains(s.TenantId))                // 4. explicit re-scoping
            .GroupBy(s => s.TenantId)
            .Select(g => new EnrolmentSummaryDto { TenantId = g.Key, Count = g.Count() })
            .ToListAsync();
    }
}
```

The `childIds` list is derived from the hierarchy, never from client input. Accepting a
`tenantId` parameter from the caller here is a cross-tenant IDOR.

---

## 10. Making an existing single-type app multi-type

Order matters; each step ships independently.

1. Add `TenantType` + `ParentTenantId` to `Tenant` with a default matching today's tenants (§2).
   Everything keeps working — every existing tenant is the incumbent type.
2. Add the profile entity for the incumbent type and backfill one row per existing tenant in the
   migration (or a seeder), so profiles are never optional afterwards.
3. Add the feature provider and per-type editions; assign the incumbent edition to existing tenants.
4. Make the role seeder type-aware — existing tenants re-seed to the same roles as before.
5. Only now introduce the second type: enum member, edition, profile entity, role set, provisioner
   branch.

Do not renumber the enum, do not repurpose an existing edition, and do not change any existing
entity's tenancy interface as part of this work.

---

## 11. Review checklist

Entity-level, for each new/changed domain entity:

- [ ] Tenancy interface chosen deliberately — `IMustHaveTenant` (host sees all) vs `IMayHaveTenant`
      (host sees host rows) vs neither — and the choice matches how the host console queries it.
- [ ] `FullAuditedEntity<Guid>` + `ISoftDelete` unless there's a reason not to.
- [ ] Max-length consts, `[Required]`/`[StringLength]`, protected EF ctor + public ctor setting
      required fields.
- [ ] No `TenantType` column on the entity; no per-type nullable column blocks.
- [ ] Value objects via `OwnsOne`, not flattened strings.

Model-level:

- [ ] Every unique index leads with `TenantId` and is soft-delete-filtered.
- [ ] No hand-written `HasQueryFilter` for tenancy.
- [ ] `ParentTenantId` FK is `DeleteBehavior.Restrict`.
- [ ] DbSet registered and configured in the module's `Configure*Module` method.
- [ ] Migration generated against `psms.EntityFrameworkCore` with `psms.Web.Host` as startup, and
      reviewed — non-nullable adds carry an explicit default.

Behaviour-level:

- [ ] Type-specific capability expressed as a **Feature** (`[RequiresFeature]`), not an `if` on the
      discriminator, wherever it can be.
- [ ] Every `DisableFilter` is permission-gated, type-gated, narrowly scoped, and followed by an
      explicit tenant-id `Where`.
- [ ] Roles seeded per type; host-only permissions carry `MultiTenancySides.Host`.
- [ ] Cross-tenant queries derive tenant ids from the hierarchy, never from request input.
