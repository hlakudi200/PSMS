using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Domain.Shared.Storage;
using psms.Learning.OnlineLessons.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Learning.OnlineLessons;

/// <summary>
/// Application service interface for managing online lessons.
/// </summary>
public interface IOnlineLessonAppService : IApplicationService
{
    Task<OnlineLessonDto> GetAsync(Guid id);
    Task<PagedResultDto<OnlineLessonListDto>> GetAllAsync(GetOnlineLessonsInput input);
    Task<ListResultDto<OnlineLessonListDto>> GetByClassSubjectAsync(Guid classSubjectId);
    Task<ListResultDto<OnlineLessonListDto>> GetUpcomingAsync();
    Task<OnlineLessonDto> CreateAsync(CreateOnlineLessonDto input);
    Task<OnlineLessonDto> UpdateAsync(Guid id, UpdateOnlineLessonDto input);
    Task DeleteAsync(Guid id);
    Task<OnlineLessonDto> StartAsync(Guid id);
    Task<OnlineLessonDto> EndAsync(Guid id, int attendeeCount);
    Task<OnlineLessonDto> CancelAsync(Guid id);
    Task<OnlineLessonDto> RescheduleAsync(Guid id, RescheduleOnlineLessonDto input);
    Task<OnlineLessonDto> AddRecordingAsync(Guid id, AddRecordingDto input);
    Task<FileUploadTicket> RequestRecordingUploadUrlAsync(RequestRecordingUploadUrlDto input);
    Task<OnlineLessonDto> UploadRecordingAsync(UploadRecordingDto input);
}
