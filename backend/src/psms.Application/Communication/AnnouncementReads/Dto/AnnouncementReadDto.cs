using Abp.Application.Services.Dto;
using System;

namespace psms.Communication.AnnouncementReads.Dto;

public class AnnouncementReadDto : EntityDto<Guid>
{
    public Guid AnnouncementId { get; set; }
    public long UserId { get; set; }
    public DateTime ReadDate { get; set; }
}
