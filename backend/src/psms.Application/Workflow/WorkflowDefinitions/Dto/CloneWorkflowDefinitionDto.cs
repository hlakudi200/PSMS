using System.ComponentModel.DataAnnotations;

namespace psms.Workflow.WorkflowDefinitions.Dto;

/// <summary>WF-33: options for cloning a definition as a new, inactive version.</summary>
public class CloneWorkflowDefinitionDto
{
    /// <summary>Name for the clone. Defaults to "&lt;source name&gt; v&lt;n&gt;" when omitted.</summary>
    [StringLength(200)]
    public string Name { get; set; }
}
