using psms.Domain.Workflow.Enums;
using System.ComponentModel.DataAnnotations;

namespace psms.Workflow.WorkflowInstances.Dto;

public class AdvanceWorkflowInput
{
    [Required]
    public WorkflowActionType Action { get; set; }

    [StringLength(2000)]
    public string Comment { get; set; }

    [StringLength(2048)]
    public string AttachmentUrl { get; set; }
}
