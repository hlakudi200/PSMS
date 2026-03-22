using Abp.Application.Services.Dto;
using psms.Domain.Workflow.Enums;
using System;

namespace psms.Workflow.WorkflowSteps.Dto;

public class WorkflowStepDto : EntityDto<Guid>
{
    public Guid WorkflowDefinitionId { get; set; }
    public int StepOrder { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string AssignedRole { get; set; }
    public WorkflowActionType ActionType { get; set; }
    public bool IsTerminal { get; set; }
    public int? NextStepOnApprove { get; set; }
    public int? NextStepOnReject { get; set; }
    public bool IsCommentRequired { get; set; }
    public long? AssignedUserId { get; set; }
    public int? SlaHours { get; set; }
    public string GuardExpression { get; set; }
}
