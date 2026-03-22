using Abp.Application.Services.Dto;
using psms.Domain.Workflow.Enums;
using System;

namespace psms.Workflow.WorkflowInstances.Dto;

public class GetWorkflowInstancesInput : PagedAndSortedResultRequestDto
{
    public WorkflowEntityType? EntityType { get; set; }
    public WorkflowStatus? Status { get; set; }
    public Guid? EntityId { get; set; }
    public string Search { get; set; }
}
