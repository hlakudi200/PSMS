using Abp.Application.Services.Dto;

namespace psms.Workflow.WorkflowInstances.Dto;

/// <summary>
/// WF-21: input for the "My Approvals" list. Adds a free-text Keyword on top of
/// the standard paging/sorting. The keyword matches the computed SubjectLabel
/// (e.g. the student's name), the workflow name, and the current step — so a
/// user can find an approval by who/what it's about, not just by workflow name.
/// </summary>
public class GetMyPendingInput : PagedAndSortedResultRequestDto
{
    public string Keyword { get; set; }
}
