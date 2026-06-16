using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using psms.Authorization;
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

        Configuration.Modules.AbpAutoMapper().Configurators.Add(
            // Scan the assembly for classes which inherit from AutoMapper.Profile
            cfg => cfg.AddMaps(thisAssembly)
        );
    }
}
