using Abp.Application.Services.Dto;
using psms.Domain.Workflow.Enums;
using System;

namespace psms.Workflow.WorkflowDelegations.Dto;

public class WorkflowDelegationDto : FullAuditedEntityDto<Guid>
{
    public long DelegatorUserId { get; set; }
    public long DelegateUserId { get; set; }
    public string DelegatorUserName { get; set; }
    public string DelegateUserName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; }
    public bool IsActive { get; set; }
    public WorkflowEntityType? EntityType { get; set; }
    public string AssignedRole { get; set; }
    public bool IsEffective { get; set; }
}
