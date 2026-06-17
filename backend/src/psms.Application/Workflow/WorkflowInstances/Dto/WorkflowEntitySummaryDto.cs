using System.Collections.Generic;

namespace psms.Workflow.WorkflowInstances.Dto;

/// <summary>
/// WF-09: a human-readable snapshot of the entity a workflow instance is about
/// (the report card, leave request, etc.), so an approver can see WHAT they're
/// approving instead of a bare entity GUID.
/// </summary>
public class WorkflowEntitySummaryDto
{
    public string Title { get; set; }
    public string Subtitle { get; set; }
    public List<WorkflowEntitySummaryFieldDto> Fields { get; set; } = new List<WorkflowEntitySummaryFieldDto>();
}

public class WorkflowEntitySummaryFieldDto
{
    public string Label { get; set; }
    public string Value { get; set; }

    public WorkflowEntitySummaryFieldDto() { }

    public WorkflowEntitySummaryFieldDto(string label, string value)
    {
        Label = label;
        Value = value;
    }
}
