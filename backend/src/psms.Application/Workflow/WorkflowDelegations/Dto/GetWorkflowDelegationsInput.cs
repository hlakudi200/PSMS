using Abp.Application.Services.Dto;

namespace psms.Workflow.WorkflowDelegations.Dto;

public class GetWorkflowDelegationsInput : PagedAndSortedResultRequestDto
{
    public long? DelegatorUserId { get; set; }
    public long? DelegateUserId { get; set; }
    public bool? IsActive { get; set; }
    public string Search { get; set; }
}
