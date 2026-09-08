using Abp.Application.Services.Dto;
using psms.Domain.Workflow.Enums;
using System;

namespace psms.Workflow.WorkflowDefinitions.Dto;

public class WorkflowDefinitionListDto : EntityDto<Guid>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public WorkflowEntityType EntityType { get; set; }
    public bool IsActive { get; set; }
    public int Version { get; set; }
    public int StepCount { get; set; }
    /// <summary>WF-33: steps are locked while this is true; clone to change them.</summary>
    public bool HasActiveInstances { get; set; }
}
