using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.POPIAConsents.Dto;

public class CreatePOPIAConsentDto
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public DateTime ConsentDate { get; set; }

    public bool AllowPhotography { get; set; }

    public bool AllowDataSharing { get; set; }

    public bool AllowNameInPublications { get; set; }

    public bool AllowMarketingUse { get; set; }

    [Required]
    public long ParentSignatureUserId { get; set; }
}
