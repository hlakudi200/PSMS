using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Assessment.Reports.Dto;
using psms.Domain.Shared.Enums;
using System;
using System.Threading.Tasks;

namespace psms.Assessment.Reports;

/// <summary>
/// Application service interface for managing student reports.
/// </summary>
public interface IReportAppService : IApplicationService
{
    Task<ReportDto> GetAsync(Guid id);
    Task<PagedResultDto<ReportListDto>> GetAllAsync(GetReportsInput input);
    Task<ReportDto> GetByStudentTermAsync(Guid studentId, Guid termId, ReportType reportType);
    Task<ReportDto> GenerateAsync(GenerateReportDto input);
    Task<BulkGenerateReportsResultDto> PreviewBulkGenerateAsync(BulkGenerateReportsInput input);
    Task<BulkGenerateReportsResultDto> BulkGenerateAsync(BulkGenerateReportsInput input);
    Task<ReportDto> SubmitForApprovalAsync(Guid id);
    Task<ReportDto> ApproveAsync(Guid id);
    Task<ReportDto> PublishAsync(Guid id);
    Task<ReportDto> AddTeacherCommentAsync(Guid id, ReportCommentDto input);
    Task<ReportDto> AddPrincipalCommentAsync(Guid id, ReportCommentDto input);
    Task<ReportDto> AcknowledgeByParentAsync(Guid id, ReportCommentDto input);
    Task<ReportDto> RecordPromotionAsync(Guid id, PromotionDecision decision, Guid? promotedToGradeId);
    Task DeleteAsync(Guid id);
    Task GenerateReportPdfAsync(Guid id);
    Task<int> BulkGenerateReportPdfsAsync(BulkGenerateReportPdfsInput input);
    Task<string> GetReportPdfUrlAsync(Guid id);
}
