using Abp.Application.Services.Dto;
using System;

namespace psms.Communication.Messages.Dto;

public class MessageDto : CreationAuditedEntityDto<Guid>
{
    public long SenderUserId { get; set; }
    public long RecipientUserId { get; set; }
    public string Subject { get; set; }
    public string Content { get; set; }
    public string AttachmentUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadDate { get; set; }
    public Guid? ParentMessageId { get; set; }
    public Guid? ThreadId { get; set; }
}
