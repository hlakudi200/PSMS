using Abp.Application.Services;
using psms.Communication.Contacts.Dto;
using System.Threading.Tasks;

namespace psms.Communication.Contacts;

/// <summary>
/// COMM-04: lets the signed-in user manage the contact details and devices used to
/// reach them on notification channels. Each user manages only their own.
/// </summary>
public interface IUserContactAppService : IApplicationService
{
    Task<MyContactDto> GetMyContactAsync();
    Task<MyContactDto> UpdateMyContactAsync(UpdateMyContactDto input);
    Task RegisterDeviceAsync(RegisterDeviceDto input);
    Task UnregisterDeviceAsync(string token);
}
