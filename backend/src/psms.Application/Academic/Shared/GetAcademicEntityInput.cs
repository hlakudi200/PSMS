using Abp.Application.Services.Dto;

namespace psms.Academic.Shared;

public class GetAcademicEntityInput : PagedAndSortedResultRequestDto
{
    public string Keyword { get; set; }
}
