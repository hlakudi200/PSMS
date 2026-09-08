using Abp.Application.Services.Dto;
using psms.Domain.Workflow.Enums;
using psms.Workflow.WorkflowSteps.Dto;
using System;
using System.Collections.Generic;

namespace psms.Workflow.WorkflowDefinitions.Dto;

public class WorkflowDefinitionDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public WorkflowEntityType EntityType { get; set; }
    public bool IsActive { get; set; }
    public int Version { get; set; }
    public List<WorkflowStepDto> Steps { get; set; }
    /// <summary>WF-33: steps are locked while this is true; clone to change them.</summary>
    public bool HasActiveInstances { get; set; }
}
