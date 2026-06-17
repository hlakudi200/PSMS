using System.Collections.Generic;

namespace psms.Workflow.WorkflowInstances.Dto;

/// <summary>
/// WF-09/WF-18: the full record a workflow instance is about, rendered inline on
/// the approval so an approver has enough evidence to decide without leaving the
/// task. Grouped into sections (scalar fields) plus optional child-record tables
/// (e.g. a report's subject marks, an application's documents/guardians).
/// </summary>
public class WorkflowEntitySummaryDto
{
    public string Title { get; set; }
    public string Subtitle { get; set; }
    public List<WorkflowEntitySummarySectionDto> Sections { get; set; } = new List<WorkflowEntitySummarySectionDto>();
    public List<WorkflowEntitySummaryTableDto> Tables { get; set; } = new List<WorkflowEntitySummaryTableDto>();
}

public class WorkflowEntitySummarySectionDto
{
    public string Heading { get; set; }
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

public class WorkflowEntitySummaryTableDto
{
    public string Heading { get; set; }
    public List<string> Columns { get; set; } = new List<string>();
    public List<List<string>> Rows { get; set; } = new List<List<string>>();
}
