using Abp.Application.Services;
using psms.MultiTenancy.Dto;

namespace psms.MultiTenancy;

public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
{
}

