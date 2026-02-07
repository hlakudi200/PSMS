using Abp.Application.Services.Dto;
using System;

namespace psms.Communication.Messages.Dto;

public class MessageListDto : EntityDto<Guid>
{
    public long SenderUserId { get; set; }
    public long RecipientUserId { get; set; }
    public string Subject { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadDate { get; set; }
    public Guid? ThreadId { get; set; }
    public DateTime CreationTime { get; set; }
}
