using Abp.Application.Services.Dto;
using System;

namespace psms.Academic.POPIAConsents.Dto;

public class GetPOPIAConsentsInput : PagedAndSortedResultRequestDto
{
    public Guid? StudentId { get; set; }
    public bool? AllowPhotography { get; set; }
    public bool? AllowDataSharing { get; set; }
    public bool? AllowNameInPublications { get; set; }
    public bool? AllowMarketingUse { get; set; }
}
