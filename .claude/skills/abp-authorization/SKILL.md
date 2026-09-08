---
name: abp-authorization
description: Authorization & permissions across the stack — ABP permission-name constants, the AuthorizationProvider permission tree (with multi-tenancy sides + localization), [AbpAuthorize] and runtime PermissionChecker enforcement, role→permission seeding per tenant, and how frontend role-gating (JWT role → ProtectedRoute → portal layouts) maps onto it. Use when adding permissions, gating an endpoint or UI, or wiring a new role.
---

# ABP Authorization & Permissions

The full authorization picture: **backend defines fine-grained permissions and enforces them; roles bundle permissions; the frontend gates UI by role** (coarse) while the backend gates by permission (fine). Classic ABP, multi-tenant. Pairs with **abp-react-auth** (login/JWT/role extraction) and **abp-backend-crud** (where `[AbpAuthorize]` lands).

## The model in one breath
`PermissionNames` (string constants) → declared in `AuthorizationProvider` as a tree → enforced on AppServices via `[AbpAuthorize]` / `PermissionChecker` → granted to **roles** by a per-tenant **seeder** → a user's roles determine granted permissions → the JWT carries the user's **role**, which the frontend uses for portal/route gating.

## 1. Permission constants — `<App>.Core/Authorization/PermissionNames.cs`

One static class, naming convention `Module.Entity.Action`:

```csharp
public static class PermissionNames
{
    // ABP defaults
    public const string Pages_Users = "Pages.Users";
    public const string Pages_Roles = "Pages.Roles";

    // Feature module — base + granular actions
    public const string Module_Foos        = "Module.Foos";
    public const string Module_Foos_View   = "Module.Foos.View";
    public const string Module_Foos_Manage = "Module.Foos.Manage";
    // a "ViewAll" variant is the house pattern for "see beyond your own scope" (e.g. backfill, cross-class)
    public const string Module_Foos_ViewAll = "Module.Foos.ViewAll";
}
```

Two-tier (`View` vs `Manage`) is the default granularity; add `Create/Edit/Delete/Approve/ViewAll` when a role needs finer splits.

## 2. The permission tree — `<App>.Core/Authorization/<App>AuthorizationProvider.cs`

Declare every permission here or it can't be granted. Build a parent→child tree; restrict host-only permissions with `MultiTenancySides.Host`; localize names via `L(...)`.

```csharp
public class AppAuthorizationProvider : AuthorizationProvider
{
    public override void SetPermissions(IPermissionDefinitionContext context)
    {
        SetSystemPermissions(context);
        SetModulePermissions(context);
    }

    private void SetModulePermissions(IPermissionDefinitionContext context)
    {
        var module = context.CreatePermission(PermissionNames.Module_Foos, L("Foos"));      // parent
        module.CreateChildPermission(PermissionNames.Module_Foos_View,    L("ViewFoos"));    // children
        module.CreateChildPermission(PermissionNames.Module_Foos_Manage,  L("ManageFoos"));
        module.CreateChildPermission(PermissionNames.Module_Foos_ViewAll, L("ViewAllFoos"));
    }

    private void SetSystemPermissions(IPermissionDefinitionContext context)
    {
        var admin = context.CreatePermission(PermissionNames.Administration, L("Administration"));
        // Host-only: tenants management is never granted inside a tenant
        var tenants = admin.CreateChildPermission(PermissionNames.Administration_Tenants, L("Tenants"),
            multiTenancySides: MultiTenancySides.Host);
        tenants.CreateChildPermission(PermissionNames.Administration_Tenants_View, L("ViewTenants"),
            multiTenancySides: MultiTenancySides.Host);
    }

    private static ILocalizableString L(string name) => new LocalizableString(name, AppConsts.LocalizationSourceName);
}
```

## 3. Enforcement — on AppServices

**Declarative** (the default — covers most cases): `[AbpAuthorize(permission)]` at class level for the broad gate, per-method for granular:

```csharp
[AbpAuthorize(PermissionNames.Module_Foos)]
public class FooAppService : ApplicationService, IFooAppService
{
    [AbpAuthorize(PermissionNames.Module_Foos_View)]   public Task<FooDto> GetAsync(Guid id) { ... }
    [AbpAuthorize(PermissionNames.Module_Foos_Manage)] public Task<FooDto> CreateAsync(...) { ... }
}
```

**Runtime / conditional** (when authorization depends on data, not just the endpoint) — inject/inherit `PermissionChecker` and call `IsGrantedAsync`:

```csharp
// Example: a regular user can only act on "today"; ViewAll lets a privileged user backfill the past.
if (date.Date < DateTime.Today
    && !await PermissionChecker.IsGrantedAsync(PermissionNames.Module_Foos_ViewAll))
    throw new UserFriendlyException(ModuleExceptionCodes.RecordLocked, "Past records are locked.");
```

Use the declarative attribute by default; reach for `PermissionChecker` only for row/condition-dependent checks.

## 4. Roles & per-tenant seeding

**Static role names** live in `Roles/StaticRoleNames.cs`, split by tenancy side:

```csharp
public static class StaticRoleNames
{
    public static class Host    { public const string Admin = "Admin"; }
    public static class Tenants { public const string Admin = "Admin"; public const string Principal = "Principal";
                                  public const string Teacher = "Teacher"; public const string Parent = "Parent"; /* … */ }
}
```

A **seeder** (`Roles/<App>RolePermissionSeeder.cs`, an `ITransientDependency`) grants each role its permission set at tenant creation, and re-runnable for incremental updates:

```csharp
public async Task SeedRolePermissionsAsync(int tenantId)
{
    await GrantPermissionsToRoleAsync(StaticRoleNames.Tenants.Principal, GetPrincipalPermissions());
    await GrantPermissionsToRoleAsync(StaticRoleNames.Tenants.Teacher,   GetTeacherPermissions());
    // …one call per role…
}

private async Task GrantPermissionsToRoleAsync(string roleName, List<string> permissionNames)
{
    var role = _roleManager.Roles.FirstOrDefault(r => r.Name == roleName);
    if (role == null) return;
    var toGrant = _permissionManager.GetAllPermissions().Where(p => permissionNames.Contains(p.Name)).ToList();
    await _roleManager.SetGrantedPermissionsAsync(role, toGrant);   // replaces the role's grants
}

private static List<string> GetTeacherPermissions() => new()
{
    PermissionNames.Module_Foos, PermissionNames.Module_Foos_View, PermissionNames.Module_Foos_Manage,
    // …scope each role to exactly what it should do…
};
```

> `SetGrantedPermissionsAsync` is a **replace**, not a merge — list every permission the role should have. To add a permission to an existing deployment, add it to the role's list and re-run the seeder.

## 5. Frontend gating (coarse, by role)

The backend enforces permissions; the **frontend gates by the single role in the JWT** — enough to route users to the right portal and hide what they can't reach. (See **abp-react-auth** for `getRole` / the auth provider.)

**`ProtectedRoute`** blocks render until auth resolves, redirects unauthenticated → login and wrong-role → `/unauthorized`:

```tsx
const hasAllowedRole = roleRequired
  ? !!currentRole && allowedRoles.some(r => r.toLowerCase() === currentRole.toLowerCase())
  : true;
// while (isPending || (jwtToken && !currentUser)) → <Spin/>;  unauth/denied → null + redirect
```

**Portal layouts** wrap their whole subtree via a shared `LayoutShell` that takes `allowedRoles` and the role-specific menu — so `/principal/**` is Principal-only, `/admin/**` is Admin-only, etc.:

```tsx
<LayoutShell config={{ basePath: '/principal', menuItems, allowedRoles: ['Principal'], /* … */ }}>
  {children}
</LayoutShell>
// LayoutShell internally: <ProtectedRoute allowedRoles={config.allowedRoles}>…</ProtectedRoute>
```

For finer in-page control, gate columns/actions with `currentUserRole` + `requiredPermissions` on `EnterpriseTable` (see **frontend-crud-page**). The frontend gate is **defense-in-depth / UX only** — the backend `[AbpAuthorize]` is the real boundary.

## Checklist for adding a permission
1. Constant in `PermissionNames` (`Module.Foos.Action`).
2. Declare it in `AuthorizationProvider` (right parent; `MultiTenancySides.Host` if host-only; `L(...)` label).
3. Enforce: `[AbpAuthorize(...)]` on the method (or `PermissionChecker.IsGrantedAsync` for conditional).
4. Grant it to the right role lists in the seeder; re-run the seeder.
5. (If UI-visible) gate the route/portal by role and/or pass `requiredPermissions` to the table.

## Gotchas
- **Declare before grant** — a permission not in the `AuthorizationProvider` tree silently can't be granted/checked.
- **Host vs tenant** — host-only permissions (`MultiTenancySides.Host`) must not appear in tenant role lists.
- **Seeder replaces grants** — partial lists drop permissions. Always pass the full intended set.
- **Frontend role ≠ permissions** — one role in the JWT for routing; real authority is the role's granted permission set on the backend. Never trust the frontend gate alone.
- **Role string casing** — frontend comparisons lowercase both sides; backend role names are exact (`StaticRoleNames`).
