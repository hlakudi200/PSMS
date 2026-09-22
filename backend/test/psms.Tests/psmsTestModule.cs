using Abp.AutoMapper;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Modules;
using Abp.Net.Mail;
using Abp.TestBase;
using Abp.Zero.Configuration;
using Abp.Zero.EntityFrameworkCore;
using psms.EntityFrameworkCore;
using psms.Tests.DependencyInjection;
using Castle.MicroKernel.Registration;
using NSubstitute;
using System;

namespace psms.Tests;

[DependsOn(
    typeof(psmsApplicationModule),
    typeof(psmsEntityFrameworkModule),
    typeof(AbpTestBaseModule)
    )]
public class psmsTestModule : AbpModule
{
    public psmsTestModule(psmsEntityFrameworkModule abpProjectNameEntityFrameworkModule)
    {
        abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
        abpProjectNameEntityFrameworkModule.SkipDbSeed = true;
    }

    public override void PreInitialize()
    {
        Configuration.UnitOfWork.Timeout = TimeSpan.FromMinutes(30);
        Configuration.UnitOfWork.IsTransactional = false;

        // Disable static mapper usage since it breaks unit tests (see https://github.com/aspnetboilerplate/aspnetboilerplate/issues/2052)
        Configuration.Modules.AbpAutoMapper().UseStaticMapper = false;

        Configuration.BackgroundJobs.IsJobExecutionEnabled = false;

        // Use database for language management
        Configuration.Modules.Zero().LanguageManagement.EnableDbLocalization();

        RegisterFakeService<AbpZeroDbMigrator<psmsDbContext>>();

        // File storage talks to Supabase through IConfiguration, which the test
        // host does not register. Services that merely depend on it — the report
        // service needs it to sign a download link — could not be constructed at
        // all, so every test resolving one failed on a DI error rather than on
        // its own assertion.
        Configuration.ReplaceService<psms.Domain.Shared.Storage.IFileStorageService>(
            () => IocManager.IocContainer.Register(
                Component.For<psms.Domain.Shared.Storage.IFileStorageService>()
                    .UsingFactoryMethod(() => Substitute.For<psms.Domain.Shared.Storage.IFileStorageService>())
                    .LifestyleSingleton()));

        Configuration.ReplaceService<IEmailSender, NullEmailSender>(DependencyLifeStyle.Transient);
    }

    public override void Initialize()
    {
        ServiceCollectionRegistrar.Register(IocManager);
    }

    private void RegisterFakeService<TService>() where TService : class
    {
        IocManager.IocContainer.Register(
            Component.For<TService>()
                .UsingFactoryMethod(() => Substitute.For<TService>())
                .LifestyleSingleton()
        );
    }
}
