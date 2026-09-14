using Abp.Application.Services.Dto;
using psms.Domain.Workflow.Enums;
using System;

namespace psms.Workflow.WorkflowInstances.Dto;

public class WorkflowInstanceListDto : EntityDto<Guid>
{
    public string WorkflowDefinitionName { get; set; }
    public WorkflowEntityType EntityType { get; set; }
    public Guid EntityId { get; set; }
    /// <summary>
    /// WF-20: a concise human label for the linked record (e.g. the student's name
    /// for a report, the case number + student for a disciplinary case) so an
    /// approver can tell rows apart in a list without opening each one. Null when
    /// the entity is missing/unmapped. Populated post-map, not via AutoMapper.
    /// </summary>
    public string SubjectLabel { get; set; }
    public WorkflowStatus Status { get; set; }
    public int CurrentStepOrder { get; set; }
    public string CurrentStepName { get; set; }
    public string CurrentStepAssignedRole { get; set; }
    /// <summary>WF-22: the current step's configured action type, so the UI can
    /// offer only the actions valid for this step.</summary>
    public WorkflowActionType? CurrentStepActionType { get; set; }
    public bool CurrentStepIsCommentRequired { get; set; }
    public DateTime? CurrentStepDueDate { get; set; }
    public bool IsOverdue { get; set; }
    public DateTime? StartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime CreationTime { get; set; }
    /// <summary>WF-34: only the creator may Recall; the UI hides the action for everyone else.</summary>
    public long? CreatorUserId { get; set; }
}
