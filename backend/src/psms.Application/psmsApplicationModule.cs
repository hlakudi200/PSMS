using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using psms.Authorization;

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

        Configuration.Modules.AbpAutoMapper().Configurators.Add(
            // Scan the assembly for classes which inherit from AutoMapper.Profile
            cfg => cfg.AddMaps(thisAssembly)
        );
    }
}
