using Abp.Application.Services.Dto;
using psms.Domain.Workflow.Enums;
using System;

namespace psms.Workflow.WorkflowInstances.Dto;

public class WorkflowInstanceListDto : EntityDto<Guid>
{
    public string WorkflowDefinitionName { get; set; }
    public WorkflowEntityType EntityType { get; set; }
    public Guid EntityId { get; set; }
    public WorkflowStatus Status { get; set; }
    public int CurrentStepOrder { get; set; }
    public string CurrentStepName { get; set; }
    public string CurrentStepAssignedRole { get; set; }
    public bool CurrentStepIsCommentRequired { get; set; }
    public DateTime? CurrentStepDueDate { get; set; }
    public bool IsOverdue { get; set; }
    public DateTime? StartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime CreationTime { get; set; }
}
