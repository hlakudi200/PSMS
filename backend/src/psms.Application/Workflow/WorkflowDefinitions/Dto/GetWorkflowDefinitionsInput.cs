using Abp.Application.Services.Dto;
using psms.Domain.Workflow.Enums;

namespace psms.Workflow.WorkflowDefinitions.Dto;

public class GetWorkflowDefinitionsInput : PagedAndSortedResultRequestDto
{
    public WorkflowEntityType? EntityType { get; set; }
    public bool? IsActive { get; set; }
    public string Search { get; set; }
}
