using Abp.Application.Services.Dto;

namespace psms.Admissions.Enrollments.Dto;

public class GetPendingEnrollmentsInput : PagedAndSortedResultRequestDto
{
    public string Keyword { get; set; }
}
