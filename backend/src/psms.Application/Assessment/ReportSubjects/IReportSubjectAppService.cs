using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Assessment.ReportSubjects.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Assessment.ReportSubjects;

/// <summary>
/// Application service interface for managing report subject entries.
/// </summary>
public interface IReportSubjectAppService : IApplicationService
{
    Task<ReportSubjectDto> GetAsync(Guid id);
    Task<ListResultDto<ReportSubjectDto>> GetByReportAsync(Guid reportId);
    Task<ReportSubjectDto> RecordMarksAsync(RecordReportSubjectMarksDto input);
    Task<ListResultDto<ReportSubjectDto>> BulkRecordMarksAsync(BulkRecordReportSubjectMarksDto input);
    Task<ReportSubjectDto> AddTeacherCommentAsync(Guid id, SubjectTeacherCommentDto input);
}
