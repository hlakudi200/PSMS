using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Assessment.AssessmentQuestions.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Assessment.AssessmentQuestions;

/// <summary>
/// Application service interface for managing assessment questions.
/// </summary>
public interface IAssessmentQuestionAppService : IApplicationService
{
    Task<AssessmentQuestionDto> GetAsync(Guid id);
    Task<ListResultDto<AssessmentQuestionListDto>> GetByAssessmentAsync(Guid assessmentId);
    Task<AssessmentQuestionDto> CreateAsync(CreateAssessmentQuestionDto input);
    Task<AssessmentQuestionDto> UpdateAsync(Guid id, UpdateAssessmentQuestionDto input);
    Task DeleteAsync(Guid id);
    Task ReorderAsync(ReorderQuestionsDto input);
}
