using Abp.Application.Services.Dto;

namespace psms.Admissions.Waitlists.Dto;

public class GetWaitlistsInput : PagedAndSortedResultRequestDto
{
    public string Keyword { get; set; }
}
