using psms.Domain.Workflow.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Workflow.WorkflowInstances.Dto;

public class StartWorkflowInput
{
    [Required]
    public WorkflowEntityType EntityType { get; set; }

    [Required]
    public Guid EntityId { get; set; }

    /// <summary>
    /// Optional: specify a particular definition. If null, uses the active
    /// definition for the given EntityType in the current tenant.
    /// </summary>
    public Guid? WorkflowDefinitionId { get; set; }
}
