using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Workflow.WorkflowDelegations.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Workflow.WorkflowDelegations;

public interface IWorkflowDelegationAppService : IApplicationService
{
    Task<WorkflowDelegationDto> GetAsync(Guid id);
    Task<PagedResultDto<WorkflowDelegationDto>> GetAllAsync(GetWorkflowDelegationsInput input);
    Task<PagedResultDto<WorkflowDelegationDto>> GetMyDelegationsAsync(PagedAndSortedResultRequestDto input);
    Task<WorkflowDelegationDto> CreateAsync(CreateWorkflowDelegationDto input);
    Task RevokeAsync(Guid id);
}
