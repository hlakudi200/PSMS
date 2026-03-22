using psms.Domain.Workflow.Enums;
using System.ComponentModel.DataAnnotations;

namespace psms.Workflow.WorkflowDefinitions.Dto;

public class CreateWorkflowDefinitionDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; }

    [StringLength(1000)]
    public string Description { get; set; }

    [Required]
    public WorkflowEntityType EntityType { get; set; }

    public bool IsActive { get; set; }
}
