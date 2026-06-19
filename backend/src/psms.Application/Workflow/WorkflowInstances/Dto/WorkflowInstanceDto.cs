using Abp.Application.Services.Dto;
using psms.Domain.Workflow.Enums;
using psms.Workflow.WorkflowSteps.Dto;
using System;
using System.Collections.Generic;

namespace psms.Workflow.WorkflowInstances.Dto;

public class WorkflowInstanceDto : FullAuditedEntityDto<Guid>
{
    public Guid WorkflowDefinitionId { get; set; }
    public string WorkflowDefinitionName { get; set; }
    public WorkflowEntityType EntityType { get; set; }
    public Guid EntityId { get; set; }
    public WorkflowStatus Status { get; set; }
    public int CurrentStepOrder { get; set; }
    public Guid? CurrentStepId { get; set; }
    public string CurrentStepName { get; set; }
    public string CurrentStepAssignedRole { get; set; }
    /// <summary>WF-22: the current step's configured action type, so the UI can
    /// offer only the actions valid for this step.</summary>
    public WorkflowActionType? CurrentStepActionType { get; set; }
    /// <summary>WF-22: whether the current step requires a comment, so the detail
    /// page's action modal enforces it client-side (the list DTOs already carry this).</summary>
    public bool CurrentStepIsCommentRequired { get; set; }
    public int WorkflowDefinitionVersion { get; set; }
    public DateTime? CurrentStepDueDate { get; set; }
    public bool IsOverdue { get; set; }
    public DateTime? StartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public long? CompletedByUserId { get; set; }
    public string CompletionComment { get; set; }
    public List<WorkflowTransitionDto> Transitions { get; set; }
}
