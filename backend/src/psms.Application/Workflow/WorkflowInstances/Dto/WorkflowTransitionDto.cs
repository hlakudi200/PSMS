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
}
