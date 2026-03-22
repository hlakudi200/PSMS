using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Financial.FeeWaivers.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Financial.FeeWaivers;

public interface IFeeWaiverAppService : IApplicationService
{
    Task<FeeWaiverDto> GetAsync(Guid id);
    Task<PagedResultDto<FeeWaiverListDto>> GetAllAsync(GetFeeWaiversInput input);
    Task<FeeWaiverDto> CreateAsync(CreateFeeWaiverDto input);
    Task<FeeWaiverDto> UpdateAsync(Guid id, UpdateFeeWaiverDto input);
    Task DeleteAsync(Guid id);
    Task<FeeWaiverDto> SubmitAsync(Guid id);
    Task<FeeWaiverDto> ApproveAsync(Guid id, decimal approvedAmount, string notes);
    Task<FeeWaiverDto> RejectAsync(Guid id, string notes);
}
