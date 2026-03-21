using psms.Domain.Workflow.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Workflow.WorkflowSteps.Dto;

public class CreateWorkflowStepDto
{
    [Required]
    public Guid WorkflowDefinitionId { get; set; }

    [Required]
    public int StepOrder { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; }

    [StringLength(500)]
    public string Description { get; set; }

    [Required]
    [StringLength(100)]
    public string AssignedRole { get; set; }

    [Required]
    public WorkflowActionType ActionType { get; set; }

    public bool IsTerminal { get; set; }
    public int? NextStepOnApprove { get; set; }
    public int? NextStepOnReject { get; set; }
    public bool IsCommentRequired { get; set; }
    public long? AssignedUserId { get; set; }
    public int? SlaHours { get; set; }

    [StringLength(500)]
    public string GuardExpression { get; set; }
}
