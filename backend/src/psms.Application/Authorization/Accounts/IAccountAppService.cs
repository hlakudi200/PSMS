using Abp.Application.Services;
using psms.Authorization.Accounts.Dto;
using System.Threading.Tasks;

namespace psms.Authorization.Accounts;

public interface IAccountAppService : IApplicationService
{
    Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);

    Task<RegisterOutput> Register(RegisterInput input);

    /// <summary>
    /// ADM-APPLY. A prospective parent or guardian signs themselves up so they
    /// can apply to the school.
    /// </summary>
    Task<RegisterOutput> RegisterApplicant(RegisterApplicantInput input);
}
