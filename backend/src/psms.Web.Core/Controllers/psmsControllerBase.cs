using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace psms.Controllers
{
    public abstract class psmsControllerBase : AbpController
    {
        protected psmsControllerBase()
        {
            LocalizationSourceName = psmsConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
