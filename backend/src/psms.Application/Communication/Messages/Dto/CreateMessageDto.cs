using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Communication.Messages.Dto;

public class CreateMessageDto
{
    [Required]
    public long RecipientUserId { get; set; }

    [StringLength(200)]
    public string Subject { get; set; }

    [Required]
    [StringLength(10000)]
    public string Content { get; set; }

    [StringLength(500)]
    public string AttachmentUrl { get; set; }

    public Guid? ParentMessageId { get; set; }
}
