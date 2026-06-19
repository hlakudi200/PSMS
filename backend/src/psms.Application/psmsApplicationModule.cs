using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Castle.MicroKernel.Registration;
using psms.Authorization;
using psms.Communication.Dispatch;
using psms.Domain.Shared.LiveStreaming;
using psms.Domain.Shared.Storage;
using psms.Infrastructure.LiveStreaming;
using psms.Infrastructure.Storage;

namespace psms;

[DependsOn(
    typeof(psmsCoreModule),
    typeof(AbpAutoMapperModule))]
public class psmsApplicationModule : AbpModule
{
    public override void PreInitialize()
    {
        Configuration.Authorization.Providers.Add<psmsAuthorizationProvider>();
    }

    public override void Initialize()
    {
        var thisAssembly = typeof(psmsApplicationModule).GetAssembly();

        IocManager.RegisterAssemblyByConvention(thisAssembly);

        IocManager.Register<IFileStorageService, SupabaseStorageService>(Abp.Dependency.DependencyLifeStyle.Transient);
        IocManager.Register<ILiveKitTokenService, LiveKitTokenService>(Abp.Dependency.DependencyLifeStyle.Transient);

        // COMM-07: the WhatsApp gateway defaults to a graceful no-op. A real provider
        // (Meta/CM.com/Clickatell) replaces this single registration once configured.
        IocManager.Register<psms.Communication.Channels.WhatsApp.IWhatsAppGateway,
            psms.Communication.Channels.WhatsApp.NotConfiguredWhatsAppGateway>(Abp.Dependency.DependencyLifeStyle.Transient);

        // COMM-01: register every notification channel provider against the shared
        // INotificationChannelProvider interface so the dispatcher can discover them
        // all (IIocResolver.ResolveAll). Providers are plain classes (no marker
        // interface), so this is their only registration — no double-registration.
        IocManager.IocContainer.Register(
            Classes.FromAssembly(thisAssembly)
                .BasedOn<INotificationChannelProvider>()
                .WithService.Base()
                .LifestyleTransient());

        Configuration.Modules.AbpAutoMapper().Configurators.Add(
            // Scan the assembly for classes which inherit from AutoMapper.Profile
            cfg => cfg.AddMaps(thisAssembly)
        );
    }
}
