using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Domain.Workflow.Enums;
using psms.Workflow.WorkflowInstances.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace psms.Workflow.WorkflowInstances;

public interface IWorkflowInstanceAppService : IApplicationService
{
    Task<WorkflowInstanceDto> GetAsync(Guid id);
    Task<WorkflowInstanceDto> GetByEntityAsync(WorkflowEntityType entityType, Guid entityId);
    Task<PagedResultDto<WorkflowInstanceListDto>> GetAllAsync(GetWorkflowInstancesInput input);
    Task<WorkflowInstanceDto> StartAsync(StartWorkflowInput input);
    Task<WorkflowInstanceDto> AdvanceAsync(Guid instanceId, AdvanceWorkflowInput input);
    Task<WorkflowInstanceDto> CancelAsync(Guid instanceId, string comment);
    Task<WorkflowInstanceDto> RecallAsync(Guid instanceId, string comment);
    Task<BatchAdvanceResultDto> BatchAdvanceAsync(BatchAdvanceInput input);
    Task<List<WorkflowTransitionDto>> GetHistoryAsync(Guid instanceId);
    Task<PagedResultDto<WorkflowInstanceListDto>> GetPendingForRoleAsync(string roleName, PagedAndSortedResultRequestDto input);
    Task<PagedResultDto<WorkflowInstanceListDto>> GetMyPendingAsync(PagedAndSortedResultRequestDto input);
    Task<PagedResultDto<WorkflowInstanceListDto>> GetOverdueAsync(PagedAndSortedResultRequestDto input);
}
