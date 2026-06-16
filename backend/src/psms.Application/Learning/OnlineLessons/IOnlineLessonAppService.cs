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

    /// <summary>
    /// Mints a LiveKit join token for the in-app live classroom (LC-01). The
    /// hosting teacher gets publish rights; everyone else joins as a viewer.
    /// </summary>
    Task<LiveClassJoinDto> GetJoinTokenAsync(Guid id);

    /// <summary>
    /// Returns a short-lived signed URL for a lesson's recording (LC-06). The
    /// recordings bucket is private; external (Zoom/Teams) URLs are returned as-is.
    /// </summary>
    Task<string> GetRecordingDownloadUrlAsync(Guid id);
}
