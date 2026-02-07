using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Financial.Payments.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Financial.Payments;

/// <summary>
/// Application service interface for managing payments.
/// </summary>
public interface IPaymentAppService : IApplicationService
{
    Task<PaymentDto> GetAsync(Guid id);
    Task<PagedResultDto<PaymentListDto>> GetAllAsync(GetPaymentsInput input);
    Task<ListResultDto<PaymentListDto>> GetByStudentAsync(Guid studentId);
    Task<ListResultDto<PaymentListDto>> GetByParentAsync(Guid parentId);
    Task<PaymentDto> CreateAsync(CreatePaymentDto input);
    Task<PaymentDto> UpdateAsync(Guid id, UpdatePaymentDto input);
    Task<PaymentDto> CompleteAsync(Guid id);
    Task<PaymentDto> FailAsync(Guid id);
    Task<PaymentDto> RefundAsync(Guid id);
    Task<PaymentDto> CancelAsync(Guid id);
    Task<PaymentDto> GetByReceiptNumberAsync(string receiptNumber);
}
