using psms.Configuration.Dto;
using System.Threading.Tasks;

namespace psms.Configuration;

public interface IConfigurationAppService
{
    Task ChangeUiTheme(ChangeUiThemeInput input);
}
