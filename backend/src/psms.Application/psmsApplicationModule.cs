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

        // COMM-07/08/09/10: each external channel gateway defaults to a graceful no-op.
        // A real provider replaces the single registration here once configured — see
        // backend/docs/communication-channel-config.md.
        IocManager.Register<psms.Communication.Channels.WhatsApp.IWhatsAppGateway,
            psms.Communication.Channels.WhatsApp.NotConfiguredWhatsAppGateway>(Abp.Dependency.DependencyLifeStyle.Transient);
        IocManager.Register<psms.Communication.Channels.Sms.ISmsGateway,
            psms.Communication.Channels.Sms.NotConfiguredSmsGateway>(Abp.Dependency.DependencyLifeStyle.Transient);
        IocManager.Register<psms.Communication.Channels.Email.IEmailGateway,
            psms.Communication.Channels.Email.NotConfiguredEmailGateway>(Abp.Dependency.DependencyLifeStyle.Transient);
        IocManager.Register<psms.Communication.Channels.Push.IPushGateway,
            psms.Communication.Channels.Push.NotConfiguredPushGateway>(Abp.Dependency.DependencyLifeStyle.Transient);

        // COMM-11: the channel routing policy (default classifies/orders the request's
        // opted-in channels into always-send + cascade).
        IocManager.Register<psms.Communication.Dispatch.Routing.INotificationRoutingPolicy,
            psms.Communication.Dispatch.Routing.DefaultNotificationRoutingPolicy>(Abp.Dependency.DependencyLifeStyle.Transient);

        // COMM-01: register every notification channel provider against the shared
        // INotificationChannelProvider interface so the dispatcher can discover them
        // all (IIocResolver.ResolveAll). Providers are plain classes (no marker
        // interface), so this is their only registration — no double-registration.
        IocManager.IocContainer.Register(
            Classes.FromAssembly(thisAssembly)
                .BasedOn<INotificationChannelProvider>()
                .WithService.Base()
                .LifestyleTransient());

        // WF-30/31/32: workflow guards, effects, decision schemas and entity handlers
        // are discovered by base interface (WorkflowExtensionRegistry.ResolveAll) —
        // convention registration only binds an interface that matches the class
        // name, so they need this explicit registration.
        //
        // The distinct component name is what makes it take effect. Every one of
        // these classes also carries ITransientDependency, so
        // RegisterAssemblyByConvention above has already registered it under its
        // implementation type's full name, and Windsor's Classes.FromAssembly
        // SILENTLY SKIPS a type that is already registered. Without a name of its
        // own this whole block was a no-op: ResolveAll<IWorkflowStepGuard>()
        // returned nothing, WorkflowExtension/GetAvailable reported no guards,
        // effects, schemas or entity handler for any entity type, and any step
        // carrying a GuardKey failed with WF_EXTENSION_NOT_FOUND — which is what
        // happened to report card approval the moment the seeder attached
        // report.subjects-complete to the HOD Review step.
        IocManager.IocContainer.Register(
            Classes.FromAssembly(thisAssembly).BasedOn<psms.Workflow.Engine.IWorkflowStepGuard>()
                .WithService.Base().Configure(c => c.Named("wfext:" + c.Implementation.FullName)).LifestyleTransient(),
            Classes.FromAssembly(thisAssembly).BasedOn<psms.Workflow.Engine.IWorkflowStepEffect>()
                .WithService.Base().Configure(c => c.Named("wfext:" + c.Implementation.FullName)).LifestyleTransient(),
            Classes.FromAssembly(thisAssembly).BasedOn<psms.Workflow.Engine.IWorkflowDecisionSchema>()
                .WithService.Base().Configure(c => c.Named("wfext:" + c.Implementation.FullName)).LifestyleTransient(),
            Classes.FromAssembly(thisAssembly).BasedOn<psms.Workflow.Engine.IWorkflowEntityHandler>()
                .WithService.Base().Configure(c => c.Named("wfext:" + c.Implementation.FullName)).LifestyleTransient());

        Configuration.Modules.AbpAutoMapper().Configurators.Add(
            // Scan the assembly for classes which inherit from AutoMapper.Profile
            cfg => cfg.AddMaps(thisAssembly)
        );
    }
}
