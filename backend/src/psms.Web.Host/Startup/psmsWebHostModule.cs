using Abp.Modules;
using Abp.Reflection.Extensions;
using psms.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace psms.Web.Host.Startup
{
    [DependsOn(
       typeof(psmsWebCoreModule))]
    public class psmsWebHostModule : AbpModule
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public psmsWebHostModule(IWebHostEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(psmsWebHostModule).GetAssembly());
        }
    }
}
