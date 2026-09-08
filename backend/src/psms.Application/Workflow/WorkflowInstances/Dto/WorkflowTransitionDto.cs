using Abp.Application.Services.Dto;
using psms.Domain.Workflow.Enums;
using System;

namespace psms.Workflow.WorkflowInstances.Dto;

public class WorkflowTransitionDto : EntityDto<Guid>
{
    public Guid WorkflowInstanceId { get; set; }
    public Guid FromStepId { get; set; }
    public string FromStepName { get; set; }
    public Guid? ToStepId { get; set; }
    public string ToStepName { get; set; }
    public WorkflowActionType Action { get; set; }
    public long ActorUserId { get; set; }
    public string ActorUserName { get; set; }
    public string Comment { get; set; }
    public DateTime TransitionDate { get; set; }
    public string AttachmentUrl { get; set; }
    /// <summary>WF-32: the decision payload recorded with this transition, as JSON.</summary>
    public string DecisionJson { get; set; }
    /// <summary>WF-30: the step was waived rather than satisfied.</summary>
    public bool IsWaived { get; set; }
    /// <summary>WF-30: the step's guard was overridden.</summary>
    public bool IsGuardOverridden { get; set; }
}
