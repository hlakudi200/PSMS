using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Financial.ExpenseRequests.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Financial.ExpenseRequests;

/// <summary>
/// Interface for managing expense requests.
/// </summary>
public interface IExpenseRequestAppService : IApplicationService
{
    Task<ExpenseRequestDto> GetAsync(Guid id);
    Task<PagedResultDto<ExpenseRequestListDto>> GetAllAsync(GetExpenseRequestsInput input);
    Task<ExpenseRequestDto> CreateAsync(CreateExpenseRequestDto input);
    Task<ExpenseRequestDto> UpdateAsync(Guid id, UpdateExpenseRequestDto input);
    Task DeleteAsync(Guid id);
    Task<ExpenseRequestDto> SubmitAsync(Guid id);
    Task<ExpenseRequestDto> ApproveAsync(Guid id, decimal approvedAmount);
    Task<ExpenseRequestDto> RejectAsync(Guid id, string reason);
    Task<ExpenseRequestDto> CancelAsync(Guid id);
    Task<ExpenseRequestDto> MarkAsPaidAsync(Guid id, string paymentReference);
}
