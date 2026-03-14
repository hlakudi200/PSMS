using Abp.Application.Services.Dto;

namespace psms.Admissions.AdmissionInterviews.Dto;

public class GetAdmissionInterviewsInput : PagedAndSortedResultRequestDto
{
    public string Keyword { get; set; }
}
