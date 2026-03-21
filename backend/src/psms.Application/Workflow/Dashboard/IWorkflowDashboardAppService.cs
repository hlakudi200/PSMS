using Abp.Application.Services;
using psms.Workflow.Dashboard.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace psms.Workflow.Dashboard;

public interface IWorkflowDashboardAppService : IApplicationService
{
    Task<WorkflowDashboardDto> GetDashboardAsync();
    Task<List<WorkflowActivityDto>> GetRecentActivityAsync(int count = 20);
}
