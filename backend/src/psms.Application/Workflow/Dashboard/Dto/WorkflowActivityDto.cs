using psms.Domain.Workflow.Enums;
using System;

namespace psms.Workflow.Dashboard.Dto;

/// <summary>
/// Recent workflow activity entry for the activity feed.
/// </summary>
public class WorkflowActivityDto
{
    public Guid WorkflowInstanceId { get; set; }
    public WorkflowEntityType EntityType { get; set; }
    public Guid EntityId { get; set; }
    public string WorkflowDefinitionName { get; set; }
    public WorkflowActionType Action { get; set; }
    public string ActorUserName { get; set; }
    public string FromStepName { get; set; }
    public string ToStepName { get; set; }
    public string Comment { get; set; }
    public DateTime TransitionDate { get; set; }
}
