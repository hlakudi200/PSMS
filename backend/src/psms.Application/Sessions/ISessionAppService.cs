using Abp.Application.Services;
using psms.Sessions.Dto;
using System.Threading.Tasks;

namespace psms.Sessions;

public interface ISessionAppService : IApplicationService
{
    Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
}
