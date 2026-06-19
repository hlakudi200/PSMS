using Abp.Application.Services;
using psms.Communication.Preferences.Dto;
using System.Threading.Tasks;

namespace psms.Communication.Preferences;

/// <summary>
/// COMM-05: lets the signed-in user view and set their notification consents
/// (per external channel, POPIA opt-in/out) and category preferences. Self-service
/// only — each user manages their own.
/// </summary>
public interface IUserNotificationPreferenceAppService : IApplicationService
{
    Task<MyPreferencesDto> GetMyPreferencesAsync();
    Task SetConsentAsync(SetConsentDto input);
    Task SetPreferenceAsync(SetPreferenceDto input);
}
