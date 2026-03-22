using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.HR.StaffLeaveRequests.Dto;
using System;
using System.Threading.Tasks;

namespace psms.HR.StaffLeaveRequests;

/// <summary>
/// Interface for managing staff leave requests.
/// </summary>
public interface IStaffLeaveRequestAppService : IApplicationService
{
    Task<StaffLeaveRequestDto> GetAsync(Guid id);
    Task<PagedResultDto<StaffLeaveRequestListDto>> GetAllAsync(GetStaffLeaveRequestsInput input);
    Task<StaffLeaveRequestDto> CreateAsync(CreateStaffLeaveRequestDto input);
    Task<StaffLeaveRequestDto> UpdateAsync(Guid id, UpdateStaffLeaveRequestDto input);
    Task DeleteAsync(Guid id);
    Task<StaffLeaveRequestDto> SubmitAsync(Guid id);
    Task<StaffLeaveRequestDto> ApproveAsync(Guid id);
    Task<StaffLeaveRequestDto> RejectAsync(Guid id, string reason);
    Task<StaffLeaveRequestDto> CancelAsync(Guid id);
    Task<PagedResultDto<StaffLeaveRequestListDto>> GetMyLeavesAsync(PagedAndSortedResultRequestDto input);
}
