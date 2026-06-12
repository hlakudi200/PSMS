using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Assessment.Assessments.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Assessment.Assessments;

/// <summary>
/// Application service interface for managing assessments.
/// </summary>
public interface IAssessmentAppService : IApplicationService
{
    Task<AssessmentDto> GetAsync(Guid id);
    Task<PagedResultDto<AssessmentListDto>> GetAllAsync(GetAssessmentsInput input);
    Task<AssessmentDto> CreateAsync(CreateAssessmentDto input);
    Task<AssessmentDto> CreateWithQuestionsAsync(CreateAssessmentWithQuestionsDto input);
    Task<AssessmentDto> UpdateAsync(Guid id, UpdateAssessmentDto input);
    Task DeleteAsync(Guid id);
    Task<AssessmentDto> PublishAsync(Guid id);
    Task<AssessmentDto> UnpublishAsync(Guid id);
    Task<AssessmentDto> ReleaseMarksAsync(Guid id);
}
