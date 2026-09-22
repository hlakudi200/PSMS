using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Assessment.Weightings.Dto;
using System.Threading.Tasks;

namespace psms.Assessment.Weightings;

/// <summary>
/// A school's SBA / examination split per grade band — what a year-end subject
/// mark is made of. Defaults come from DBE Circular S8 of 2023 and a school may
/// override them.
/// </summary>
public interface IAssessmentWeightingAppService : IApplicationService
{
    Task<ListResultDto<AssessmentWeightingDto>> GetAllAsync();
    Task<ListResultDto<AssessmentWeightingDto>> UpdateAsync(UpdateAssessmentWeightingsDto input);
    Task<ListResultDto<AssessmentWeightingDto>> ResetToDefaultsAsync();
}
