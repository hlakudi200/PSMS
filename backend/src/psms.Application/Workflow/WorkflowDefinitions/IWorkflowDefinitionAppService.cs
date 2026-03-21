using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Workflow.WorkflowDefinitions.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Workflow.WorkflowDefinitions;

public interface IWorkflowDefinitionAppService : IApplicationService
{
    Task<WorkflowDefinitionDto> GetAsync(Guid id);
    Task<PagedResultDto<WorkflowDefinitionListDto>> GetAllAsync(GetWorkflowDefinitionsInput input);
    Task<WorkflowDefinitionDto> CreateAsync(CreateWorkflowDefinitionDto input);
    Task<WorkflowDefinitionDto> UpdateAsync(Guid id, UpdateWorkflowDefinitionDto input);
    Task DeleteAsync(Guid id);
    Task<WorkflowDefinitionDto> ActivateAsync(Guid id);
    Task<WorkflowDefinitionDto> DeactivateAsync(Guid id);
}
