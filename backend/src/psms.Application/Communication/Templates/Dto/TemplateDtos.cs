using Abp.Application.Services.Dto;
using psms.Domain.Communication.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Communication.Templates.Dto;

public class NotificationTemplateDto : EntityDto<Guid>
{
    public string TemplateKey { get; set; }
    public NotificationChannel Channel { get; set; }
    public string Language { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public string ProviderTemplateName { get; set; }
    public string ProviderParameterKeys { get; set; }
    public TemplateApprovalStatus ApprovalStatus { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreationTime { get; set; }
}

public class NotificationTemplateListDto : EntityDto<Guid>
{
    public string TemplateKey { get; set; }
    public NotificationChannel Channel { get; set; }
    public string Language { get; set; }
    public string Title { get; set; }
    public TemplateApprovalStatus ApprovalStatus { get; set; }
    public bool IsActive { get; set; }
}

public class CreateNotificationTemplateDto
{
    [Required]
    [StringLength(NotificationTemplate.MaxKeyLength)]
    public string TemplateKey { get; set; }

    [Required]
    public NotificationChannel Channel { get; set; }

    [Required]
    [StringLength(NotificationTemplate.MaxLanguageLength)]
    public string Language { get; set; }

    [StringLength(NotificationTemplate.MaxTitleLength)]
    public string Title { get; set; }

    [Required]
    [StringLength(NotificationTemplate.MaxBodyLength)]
    public string Body { get; set; }

    [StringLength(NotificationTemplate.MaxProviderTemplateNameLength)]
    public string ProviderTemplateName { get; set; }

    [StringLength(NotificationTemplate.MaxProviderParameterKeysLength)]
    public string ProviderParameterKeys { get; set; }
}

public class UpdateNotificationTemplateDto
{
    [StringLength(NotificationTemplate.MaxTitleLength)]
    public string Title { get; set; }

    [Required]
    [StringLength(NotificationTemplate.MaxBodyLength)]
    public string Body { get; set; }

    [StringLength(NotificationTemplate.MaxProviderTemplateNameLength)]
    public string ProviderTemplateName { get; set; }

    [StringLength(NotificationTemplate.MaxProviderParameterKeysLength)]
    public string ProviderParameterKeys { get; set; }

    public bool IsActive { get; set; }
}

public class SetTemplateApprovalDto
{
    [Required]
    public TemplateApprovalStatus ApprovalStatus { get; set; }
}

public class GetTemplatesInput : PagedAndSortedResultRequestDto
{
    public string TemplateKey { get; set; }
    public NotificationChannel? Channel { get; set; }
    public string Language { get; set; }
    public string Search { get; set; }
}
