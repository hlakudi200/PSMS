using psms.Domain.Workflow.Enums;
using System.ComponentModel.DataAnnotations;

namespace psms.Workflow.WorkflowSteps.Dto;

public class UpdateWorkflowStepDto
{
    [StringLength(200)]
    public string Name { get; set; }

    [StringLength(500)]
    public string Description { get; set; }

    [StringLength(100)]
    public string AssignedRole { get; set; }

    public WorkflowActionType? ActionType { get; set; }
    public bool? IsTerminal { get; set; }
    public int? NextStepOnApprove { get; set; }
    public int? NextStepOnReject { get; set; }
    public bool ClearNextStepOnApprove { get; set; }
    public bool ClearNextStepOnReject { get; set; }
    public bool? IsCommentRequired { get; set; }
    public long? AssignedUserId { get; set; }
    public bool ClearAssignedUserId { get; set; }
    public int? SlaHours { get; set; }
    public bool ClearSlaHours { get; set; }

    [StringLength(100)]
    public string GuardKey { get; set; }
    public bool ClearGuardKey { get; set; }
    [StringLength(100)]
    public string EntryEffectKey { get; set; }
    public bool ClearEntryEffectKey { get; set; }
    [StringLength(100)]
    public string ExitEffectKey { get; set; }
    public bool ClearExitEffectKey { get; set; }
    [StringLength(100)]
    public string DecisionSchemaKey { get; set; }
    public bool ClearDecisionSchemaKey { get; set; }
    public bool? IsOptional { get; set; }
}
