using Abp.Events.Bus;
using Abp.Modules;
using Abp.Reflection.Extensions;
using psms.Configuration;
using psms.EntityFrameworkCore;
using psms.Migrator.DependencyInjection;
using Castle.MicroKernel.Registration;
using Microsoft.Extensions.Configuration;

namespace psms.Migrator;

[DependsOn(typeof(psmsEntityFrameworkModule))]
public class psmsMigratorModule : AbpModule
{
    private readonly IConfigurationRoot _appConfiguration;

    public psmsMigratorModule(psmsEntityFrameworkModule abpProjectNameEntityFrameworkModule)
    {
        abpProjectNameEntityFrameworkModule.SkipDbSeed = true;

        _appConfiguration = AppConfigurations.Get(
            typeof(psmsMigratorModule).GetAssembly().GetDirectoryPathOrNull()
        );
    }

    public override void PreInitialize()
    {
        Configuration.DefaultNameOrConnectionString = _appConfiguration.GetConnectionString(
            psmsConsts.ConnectionStringName
        );

        Configuration.BackgroundJobs.IsJobExecutionEnabled = false;
        Configuration.ReplaceService(
            typeof(IEventBus),
            () => IocManager.IocContainer.Register(
                Component.For<IEventBus>().Instance(NullEventBus.Instance)
            )
        );
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(psmsMigratorModule).GetAssembly());
        ServiceCollectionRegistrar.Register(IocManager);
    }
}
