using Abp.Application.Services;
using psms.Workflow.WorkflowSteps.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace psms.Workflow.WorkflowSteps;

public interface IWorkflowStepAppService : IApplicationService
{
    Task<WorkflowStepDto> GetAsync(Guid id);
    Task<List<WorkflowStepDto>> GetByDefinitionAsync(Guid definitionId);
    Task<WorkflowStepDto> CreateAsync(CreateWorkflowStepDto input);
    Task<WorkflowStepDto> UpdateAsync(Guid id, UpdateWorkflowStepDto input);
    Task DeleteAsync(Guid id);
    Task<List<WorkflowStepDto>> ReorderAsync(ReorderWorkflowStepsDto input);
}
