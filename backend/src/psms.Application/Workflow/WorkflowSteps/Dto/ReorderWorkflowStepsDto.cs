using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace psms.Workflow.WorkflowSteps.Dto;

public class ReorderWorkflowStepsDto
{
    [Required]
    public Guid WorkflowDefinitionId { get; set; }

    /// <summary>
    /// Ordered list of step IDs in their new order.
    /// </summary>
    [Required]
    public List<Guid> StepIds { get; set; }
}
