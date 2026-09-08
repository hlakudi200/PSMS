---
name: abp-backend-crud
description: The ABP (classic ASP.NET Boilerplate) backend CRUD vertical-slice pattern — Entity (FullAuditedEntity + IMayHaveTenant) → IAppService interface → custom AppService with [AbpAuthorize] → Create/Update/Get/List DTOs → AutoMapper Profile → ExceptionCodes/UserFriendlyException → permission + convention DI. Use when adding a new backend feature/entity or a new endpoint to an existing one.
---

# ABP Backend CRUD Vertical Slice

How a feature is built end-to-end on the backend in this codebase: **classic ABP** (Abp 10.x, NOT Volo), .NET 9, EF Core + Npgsql, multi-tenant. Pair with the **frontend-crud-page** and **react-state-management** skills for the full stack, and **abp-authorization** for the permission layer.

The canonical reference in this repo is the **Subject** feature. Substitute `Foo` for your entity below.

## Folder layout for one feature

```
src/<App>.Core/Domain/<Module>/Entities/Foo.cs            # entity
src/<App>.Core/Authorization/PermissionNames.cs            # permission string constants (shared)
src/<App>.Application/<Module>/Foos/IFooAppService.cs       # interface
src/<App>.Application/<Module>/Foos/FooAppService.cs        # implementation
src/<App>.Application/<Module>/Foos/Dto/CreateFooDto.cs
src/<App>.Application/<Module>/Foos/Dto/UpdateFooDto.cs
src/<App>.Application/<Module>/Foos/Dto/FooDto.cs           # full (detail) DTO
src/<App>.Application/<Module>/Foos/Dto/FooListDto.cs       # lightweight (list) DTO
src/<App>.Application/<Module>/Shared/<Module>Mapper.cs     # one AutoMapper Profile per module
src/<App>.Application/<Module>/Shared/<Module>ExceptionCodes.cs
src/<App>.Application/<Module>/Shared/Get<Module>EntityInput.cs   # shared paged/filter input
```

## 1. Entity — `Foo.cs`

`FullAuditedEntity<Guid>` (creation/modification/deletion audit) + `IMayHaveTenant` for tenant-scoped rows. Define max-length consts, a protected ctor for EF, and a public ctor that sets required fields.

```csharp
[Table("Foos")]
public class Foo : FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
{
    public const int MaxNameLength = 100;

    public int? TenantId { get; set; }                 // IMayHaveTenant

    [Required, StringLength(MaxNameLength)]
    public string Name { get; set; }

    [Required, StringLength(20)]
    public string Code { get; set; }

    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }                // ISoftDelete

    public virtual ICollection<Bar> Bars { get; set; } // navigation

    protected Foo() { }

    public Foo(Guid id, int? tenantId, string name, string code) : this()
    {
        Id = id; TenantId = tenantId; Name = name; Code = code; IsActive = true; IsDeleted = false;
    }
}
```

> Base-class choice: `FullAuditedEntity<Guid>` (full audit + soft delete) is the default. Use `AuditedEntity` / `Entity<Guid>` for lighter rows. Use `IMustHaveTenant` (non-null `TenantId`) for rows that can never be host-level.

## 2. Interface — `IFooAppService.cs`

```csharp
public interface IFooAppService : IApplicationService
{
    Task<FooDto> GetAsync(Guid id);
    Task<PagedResultDto<FooListDto>> GetAllAsync(GetModuleEntityInput input);
    Task<FooDto> CreateAsync(CreateFooDto input);
    Task<FooDto> UpdateAsync(Guid id, UpdateFooDto input);
    Task DeleteAsync(Guid id);
    // + feature methods, e.g. ActivateAsync / DeactivateAsync / GetActiveAsync
}
```

## 3. AppService — `FooAppService.cs`

This project writes a **custom `ApplicationService`** with explicit methods — **not** `AsyncCrudAppService<...>` — for full control over includes, filtering, and validation. `[AbpAuthorize]` at class level (broad permission) and per-method (granular View/Manage). Inherited members: `ObjectMapper`, `CurrentUnitOfWork`, `AbpSession`, `PermissionChecker`.

```csharp
[AbpAuthorize(PermissionNames.Module_Foos)]
public class FooAppService : ApplicationService, IFooAppService
{
    private readonly IRepository<Foo, Guid> _fooRepository;
    public FooAppService(IRepository<Foo, Guid> fooRepository) => _fooRepository = fooRepository;

    [AbpAuthorize(PermissionNames.Module_Foos_View)]
    public async Task<FooDto> GetAsync(Guid id)
    {
        var foo = await _fooRepository.GetAll()
            .Include(f => f.Bars)
            .FirstOrDefaultAsync(f => f.Id == id && f.TenantId == AbpSession.TenantId);
        if (foo == null) throw new UserFriendlyException(ModuleExceptionCodes.FooNotFound, "Foo not found.");
        return ObjectMapper.Map<FooDto>(foo);
    }

    [AbpAuthorize(PermissionNames.Module_Foos_View)]
    public async Task<PagedResultDto<FooListDto>> GetAllAsync(GetModuleEntityInput input)
    {
        var query = _fooRepository.GetAll()
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                f => f.Name.ToLower().Contains(input.Keyword.ToLower())
                  || f.Code.ToLower().Contains(input.Keyword.ToLower()))
            .WhereIf(input.IsActive.HasValue, f => f.IsActive == input.IsActive.Value);

        var totalCount = await query.CountAsync();
        var items = await query.OrderBy(input.Sorting ?? "Name ASC").PageBy(input).ToListAsync();
        return new PagedResultDto<FooListDto>(totalCount, ObjectMapper.Map<List<FooListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Module_Foos_Manage)]
    public async Task<FooDto> CreateAsync(CreateFooDto input)
    {
        if (await _fooRepository.FirstOrDefaultAsync(f => f.Code == input.Code) != null)
            throw new UserFriendlyException(ModuleExceptionCodes.DuplicateFooCode, $"Code '{input.Code}' already exists.");

        var foo = new Foo(Guid.NewGuid(), AbpSession.TenantId, input.Name, input.Code);
        await _fooRepository.InsertAsync(foo);
        await CurrentUnitOfWork.SaveChangesAsync();   // flush so we can re-read with includes
        return await GetAsync(foo.Id);
    }

    [AbpAuthorize(PermissionNames.Module_Foos_Manage)]
    public async Task<FooDto> UpdateAsync(Guid id, UpdateFooDto input)
    {
        var foo = await _fooRepository.GetAsync(id);
        // partial update: only assign provided (non-null) fields
        if (input.Name != null) foo.Name = input.Name;
        if (input.IsActive.HasValue) foo.IsActive = input.IsActive.Value;
        await _fooRepository.UpdateAsync(foo);
        await CurrentUnitOfWork.SaveChangesAsync();
        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Module_Foos_Manage)]
    public async Task DeleteAsync(Guid id)
    {
        var foo = await _fooRepository.GetAll().Include(f => f.Bars)
            .FirstOrDefaultAsync(f => f.Id == id && f.TenantId == AbpSession.TenantId);
        if (foo == null) throw new UserFriendlyException(ModuleExceptionCodes.FooNotFound, "Foo not found.");
        if (foo.Bars.Any())                            // referential-integrity guard
            throw new UserFriendlyException(ModuleExceptionCodes.CannotDeleteFooWithBars, "Cannot delete; remove its Bars first.");
        await _fooRepository.DeleteAsync(foo);         // soft delete (ISoftDelete)
    }
}
```

Conventions that matter:
- **Always scope reads by `AbpSession.TenantId`** (the tenant filter is automatic for `IMayHaveTenant`, but explicit `&& f.TenantId == AbpSession.TenantId` on `FirstOrDefaultAsync` is the house style for the get-by-id case).
- **`WhereIf` + `PageBy(input)` + `OrderBy(input.Sorting ?? "Default ASC")`** for list endpoints — dynamic sorting via `System.Linq.Dynamic.Core`.
- **Re-read through `GetAsync`** after create/update so the returned DTO has computed/navigation data.
- **Guard deletes** against dependent rows with a specific exception code.

## 4. DTOs — `Dto/`

Create = required fields only; Update = all nullable (partial update); detail DTO extends `FullAuditedEntityDto<Guid>`; list DTO extends `EntityDto<Guid>` and stays lightweight. Validate with **DataAnnotations**.

```csharp
public class CreateFooDto { [Required, StringLength(100, MinimumLength = 2)] public string Name { get; set; }
                            [Required, StringLength(20, MinimumLength = 2)] public string Code { get; set; } }

public class UpdateFooDto { [StringLength(100, MinimumLength = 2)] public string Name { get; set; }
                            public bool? IsActive { get; set; } }

public class FooDto : FullAuditedEntityDto<Guid> { public string Name { get; set; } public string Code { get; set; }
                                                    public bool IsActive { get; set; } public int BarCount { get; set; } }

public class FooListDto : EntityDto<Guid> { public string Name { get; set; } public string Code { get; set; } public bool IsActive { get; set; } }
```

Shared paged input (one per module, reused by every list endpoint):

```csharp
public class GetModuleEntityInput : PagedAndSortedResultRequestDto   // gives SkipCount, MaxResultCount, Sorting
{ public string Keyword { get; set; } public bool? IsActive { get; set; } }
```

## 5. AutoMapper — `Shared/<Module>Mapper.cs`

One `AutoMapper.Profile` per module, with a private `Create<Entity>Mappings()` per entity. Use `MapFrom` for computed fields. The profile is auto-discovered by the module's `AddMaps(thisAssembly)`.

```csharp
public class ModuleMapper : Profile
{
    public ModuleMapper() { CreateFooMappings(); /* …other entities… */ }

    private void CreateFooMappings()
    {
        CreateMap<Foo, FooDto>()
            .ForMember(d => d.BarCount, o => o.MapFrom(s => s.Bars != null ? s.Bars.Count : 0));
        CreateMap<Foo, FooListDto>();
    }
}
```

## 6. Exception codes — `Shared/<Module>ExceptionCodes.cs`

Static class of stable string codes (prefix per module), thrown via `UserFriendlyException(code, message)`. The frontend axios interceptor surfaces `message`; the code is for logs/i18n.

```csharp
public static class ModuleExceptionCodes
{
    public const string FooNotFound          = "MOD_FOO_NOT_FOUND";
    public const string DuplicateFooCode     = "MOD_DUPLICATE_FOO_CODE";
    public const string CannotDeleteFooWithBars = "MOD_CANNOT_DELETE_FOO_WITH_BARS";
}
```

## 7. Permissions & DI

- Add permission constants to `PermissionNames` and declare them in the `AuthorizationProvider` tree — see the **abp-authorization** skill. Convention: `Module.Foos`, `Module.Foos.View`, `Module.Foos.Manage`.
- **DI is convention-based**: `IocManager.RegisterAssemblyByConvention(thisAssembly)` in `<App>ApplicationModule` auto-wires `FooAppService` → `IFooAppService` (matching names). **No manual registration** for standard services.
- **Exception:** a class whose interface name does NOT match its class name (e.g. `IWhatsAppGateway` → `MetaWhatsAppGateway`, or a plugin collection resolved via `IIocResolver.ResolveAll<>()`) is **not** auto-registered — register it explicitly in the module's `PreInitialize`/`Initialize` (`IocManager.Register<IFoo, FooImpl>(DependencyLifeStyle.Transient)`), and do **not** also put `ITransientDependency` on it (double registration).

## 8. Migration

Adding/changing an entity needs an EF Core migration. This repo applies migrations through a dedicated **Migrator** project (manual, not auto-on-startup), and dev == prod on a shared DB — so **never** run an unreviewed migration against it. (Out of scope here; mentioned so the slice isn't considered "done" until the migration exists and is applied deliberately.)

## The endpoint URL it produces
ABP's dynamic Web API exposes the service as `POST/GET/PUT/DELETE /api/services/app/Foo/{Method}` — e.g. `GET /api/services/app/Foo/GetAll`, `POST /api/services/app/Foo/Create`. That's exactly what the frontend provider calls.

## Checklist for a new slice
1. Entity (base class + tenancy + ctor) → 2. EF migration → 3. DTOs (create/update/detail/list) → 4. interface → 5. AppService (authorize + tenant scope + WhereIf/PageBy + UserFriendlyException guards) → 6. mapper mappings → 7. exception codes → 8. permission constants + AuthorizationProvider + role seeding. DI is automatic if names match.
