using psms.Authorization;
using psms.Authorization.Roles;
using psms.Authorization.Users;
using psms.Editions;
using psms.MultiTenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace psms.Identity;

public static class IdentityRegistrar
{
    public static IdentityBuilder Register(IServiceCollection services)
    {
        services.AddLogging();

        return services.AddAbpIdentity<Tenant, User, Role>()
            .AddAbpTenantManager<TenantManager>()
            .AddAbpUserManager<UserManager>()
            .AddAbpRoleManager<RoleManager>()
            .AddAbpEditionManager<EditionManager>()
            .AddAbpUserStore<UserStore>()
            .AddAbpRoleStore<RoleStore>()
            .AddAbpLogInManager<LogInManager>()
            .AddAbpSignInManager<SignInManager>()
            .AddAbpSecurityStampValidator<SecurityStampValidator>()
            .AddAbpUserClaimsPrincipalFactory<UserClaimsPrincipalFactory>()
            .AddPermissionChecker<PermissionChecker>()
            .AddDefaultTokenProviders();
    }
}
