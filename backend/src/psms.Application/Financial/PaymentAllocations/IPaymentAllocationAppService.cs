using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Financial.PaymentAllocations.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Financial.PaymentAllocations;

/// <summary>
/// Application service interface for managing payment allocations.
/// </summary>
public interface IPaymentAllocationAppService : IApplicationService
{
    Task<PaymentAllocationDto> GetAsync(Guid id);
    Task<ListResultDto<PaymentAllocationListDto>> GetByPaymentAsync(Guid paymentId);
    Task<ListResultDto<PaymentAllocationListDto>> GetByStudentFeeAsync(Guid studentFeeId);
    Task<PaymentAllocationDto> CreateAsync(CreatePaymentAllocationDto input);
    Task<ListResultDto<PaymentAllocationDto>> BulkAllocateAsync(BulkAllocatePaymentDto input);
    Task<PaymentAllocationDto> UpdateAsync(Guid id, UpdatePaymentAllocationDto input);
    Task DeleteAsync(Guid id);
}
