using Abp.Authorization;
using Abp.Runtime.Session;
using psms.Configuration.Dto;
using System.Threading.Tasks;

namespace psms.Configuration;

[AbpAuthorize]
public class ConfigurationAppService : psmsAppServiceBase, IConfigurationAppService
{
    public async Task ChangeUiTheme(ChangeUiThemeInput input)
    {
        await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettingNames.UiTheme, input.Theme);
    }
}
