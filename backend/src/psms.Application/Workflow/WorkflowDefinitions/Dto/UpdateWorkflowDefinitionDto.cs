using System.ComponentModel.DataAnnotations;

namespace psms.Workflow.WorkflowDefinitions.Dto;

public class UpdateWorkflowDefinitionDto
{
    [StringLength(200)]
    public string Name { get; set; }

    [StringLength(1000)]
    public string Description { get; set; }
}
