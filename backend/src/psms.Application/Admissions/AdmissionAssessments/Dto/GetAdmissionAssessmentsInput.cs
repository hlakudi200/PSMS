using Abp.Application.Services.Dto;

namespace psms.Admissions.AdmissionAssessments.Dto;

public class GetAdmissionAssessmentsInput : PagedAndSortedResultRequestDto
{
    public string Keyword { get; set; }
}
