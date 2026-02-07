using Abp.Application.Services.Dto;
using System;

namespace psms.Academic.POPIAConsents.Dto;

public class POPIAConsentDto : FullAuditedEntityDto<Guid>
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; }
    public DateTime ConsentDate { get; set; }
    public bool AllowPhotography { get; set; }
    public bool AllowDataSharing { get; set; }
    public bool AllowNameInPublications { get; set; }
    public bool AllowMarketingUse { get; set; }
    public long ParentSignatureUserId { get; set; }
    public string IpAddress { get; set; }
}
