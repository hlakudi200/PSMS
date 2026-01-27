using Abp.AspNetCore;
using Abp.AspNetCore.TestBase;
using Abp.Modules;
using Abp.Reflection.Extensions;
using psms.EntityFrameworkCore;
using psms.Web.Startup;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace psms.Web.Tests;

[DependsOn(
    typeof(psmsWebMvcModule),
    typeof(AbpAspNetCoreTestBaseModule)
)]
public class psmsWebTestModule : AbpModule
{
    public psmsWebTestModule(psmsEntityFrameworkModule abpProjectNameEntityFrameworkModule)
    {
        abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
    }

    public override void PreInitialize()
    {
        Configuration.UnitOfWork.IsTransactional = false; //EF Core InMemory DB does not support transactions.
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(psmsWebTestModule).GetAssembly());
    }

    public override void PostInitialize()
    {
        IocManager.Resolve<ApplicationPartManager>()
            .AddApplicationPartsIfNotAddedBefore(typeof(psmsWebMvcModule).Assembly);
    }
}