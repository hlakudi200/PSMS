using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Financial.StudentFees.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Financial.StudentFees;

/// <summary>
/// Application service interface for managing student fees.
/// </summary>
public interface IStudentFeeAppService : IApplicationService
{
    Task<StudentFeeDto> GetAsync(Guid id);
    Task<PagedResultDto<StudentFeeListDto>> GetAllAsync(GetStudentFeesInput input);
    Task<ListResultDto<StudentFeeListDto>> GetByStudentAsync(Guid studentId);
    Task<StudentFeeDto> CreateAsync(CreateStudentFeeDto input);
    Task<ListResultDto<StudentFeeDto>> BulkCreateAsync(BulkCreateStudentFeesDto input);
    Task<StudentFeeDto> UpdateAsync(Guid id, UpdateStudentFeeDto input);
    Task DeleteAsync(Guid id);
    Task<StudentFeeDto> ApplyDiscountAsync(Guid id, decimal discountAmount);
    Task<StudentFeeDto> WaiveAsync(Guid id);
    Task<StudentFeeDto> CancelAsync(Guid id);
    Task CheckOverdueFeesAsync();
}
