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
    Task<ReportDto> PublishAsync(Guid id);
    Task<ReportDto> AddTeacherCommentAsync(Guid id, TeacherCommentDto input);
    Task<ReportDto> AddPrincipalCommentAsync(Guid id, PrincipalCommentDto input);
    Task<ReportDto> AcknowledgeByParentAsync(Guid id, ParentAcknowledgementDto input);
    Task<ReportDto> RecordPromotionAsync(Guid id, PromotionDecision decision, Guid? promotedToGradeId);

    /// <summary>
    /// RC-16. Records the promotion decision on a year-end report card, with the
    /// destination grade validated and a reason kept when the decision departs
    /// from the national requirements.
    /// </summary>
    Task<ReportDto> RecordPromotionDecisionAsync(RecordPromotionDto input);

    /// <summary>
    /// RC-16. What the national promotion requirements make of this learner's
    /// year — whether they are met, and clause by clause which are not.
    /// </summary>
    Task<PromotionAdviceDto> GetPromotionAdviceAsync(Guid id);

    /// <summary>
    /// RC-17. Records the conduct and diligence ratings and the behaviour
    /// comment — RE-002 fields that had no property to write to.
    /// </summary>
    Task<ReportDto> RecordConductAsync(RecordConductDto input);

    /// <summary>RC-17. The class teacher signs the card off (RE-003).</summary>
    Task<ReportDto> SignAsTeacherAsync(Guid id);

    /// <summary>RC-17. The principal signs the card off (RE-003).</summary>
    Task<ReportDto> SignAsPrincipalAsync(Guid id);
    Task DeleteAsync(Guid id);
    Task GenerateReportPdfAsync(Guid id);
    Task<int> BulkGenerateReportPdfsAsync(BulkGenerateReportPdfsInput input);
    Task<string> GetReportPdfUrlAsync(Guid id);
}
