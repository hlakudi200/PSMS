using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using psms.MultiTenancy;

namespace psms.Sessions.Dto;

[AutoMapFrom(typeof(Tenant))]
public class TenantLoginInfoDto : EntityDto
{
    public string TenancyName { get; set; }

    public string Name { get; set; }
}
