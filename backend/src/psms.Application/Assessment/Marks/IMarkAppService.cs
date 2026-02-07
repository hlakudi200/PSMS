using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Assessment.Marks.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Assessment.Marks;

/// <summary>
/// Application service interface for managing student marks.
/// </summary>
public interface IMarkAppService : IApplicationService
{
    Task<MarkDto> GetAsync(Guid id);
    Task<PagedResultDto<MarkListDto>> GetAllAsync(GetMarksInput input);
    Task<ListResultDto<MarkListDto>> GetByAssessmentAsync(Guid assessmentId);
    Task<ListResultDto<MarkListDto>> GetByStudentAsync(Guid studentId, Guid? termId);
    Task<MarkDto> RecordMarkAsync(RecordMarkDto input);
    Task<ListResultDto<MarkDto>> BulkRecordMarksAsync(BulkRecordMarksDto input);
    Task<MarkDto> UpdateMarkAsync(Guid id, RecordMarkDto input);
    Task<MarkDto> MarkAsAbsentAsync(Guid id);
    Task<MarkDto> ApplyModerationAsync(Guid id, decimal adjustment);
    Task<MarkDto> UnlockMarkAsync(Guid id);
    Task DeleteAsync(Guid id);
}
