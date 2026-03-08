using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;

namespace psms.Academic.Shared;

public class GetAcademicEntityInput : PagedAndSortedResultRequestDto
{
    public string Keyword { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsCore { get; set; }
    public SouthAfricanSchoolPhase? SchoolPhase { get; set; }
    public int? Gender { get; set; }
}
