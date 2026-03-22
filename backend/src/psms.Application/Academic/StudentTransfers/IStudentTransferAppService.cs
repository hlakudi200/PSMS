using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Academic.StudentTransfers.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Academic.StudentTransfers;

/// <summary>
/// Interface for managing student transfer requests.
/// </summary>
public interface IStudentTransferAppService : IApplicationService
{
    Task<StudentTransferDto> GetAsync(Guid id);
    Task<PagedResultDto<StudentTransferListDto>> GetAllAsync(GetStudentTransfersInput input);
    Task<StudentTransferDto> CreateAsync(CreateStudentTransferDto input);
    Task<StudentTransferDto> UpdateAsync(Guid id, UpdateStudentTransferDto input);
    Task DeleteAsync(Guid id);
    Task<StudentTransferDto> SubmitAsync(Guid id);
    Task<StudentTransferDto> ApproveAsync(Guid id);
    Task<StudentTransferDto> RejectAsync(Guid id, string reason);
    Task<StudentTransferDto> CompleteAsync(Guid id, string certificateUrl = null);
    Task<StudentTransferDto> CancelAsync(Guid id);
}
