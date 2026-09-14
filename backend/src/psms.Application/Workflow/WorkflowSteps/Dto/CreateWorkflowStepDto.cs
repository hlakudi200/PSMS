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

    /// <summary>WF-30: exit criterion key (see WorkflowExtension/GetAvailable).</summary>
    [StringLength(100)]
    public string GuardKey { get; set; }
    /// <summary>WF-31: effect applied on entering the step.</summary>
    [StringLength(100)]
    public string EntryEffectKey { get; set; }
    /// <summary>WF-31: effect applied when a forward action leaves the step.</summary>
    [StringLength(100)]
    public string ExitEffectKey { get; set; }
    /// <summary>WF-32: decision fields the actor supplies on this step.</summary>
    [StringLength(100)]
    public string DecisionSchemaKey { get; set; }
    /// <summary>WF-30: the step may be waived with a reason.</summary>
    public bool IsOptional { get; set; }
}
