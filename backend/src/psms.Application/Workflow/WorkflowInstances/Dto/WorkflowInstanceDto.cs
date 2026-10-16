using Abp.Application.Services.Dto;
using psms.Domain.Workflow.Enums;
using psms.Workflow.WorkflowSteps.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace psms.Workflow.WorkflowInstances.Dto;

public class WorkflowInstanceDto : FullAuditedEntityDto<Guid>
{
    public Guid WorkflowDefinitionId { get; set; }
    public string WorkflowDefinitionName { get; set; }
    public WorkflowEntityType EntityType { get; set; }
    public Guid EntityId { get; set; }
    public WorkflowStatus Status { get; set; }
    public int CurrentStepOrder { get; set; }
    public Guid? CurrentStepId { get; set; }
    public string CurrentStepName { get; set; }
    public string CurrentStepAssignedRole { get; set; }
    /// <summary>WF-22: the current step's configured action type, so the UI can
    /// offer only the actions valid for this step.</summary>
    public WorkflowActionType? CurrentStepActionType { get; set; }
    /// <summary>WF-22: whether the current step requires a comment, so the detail
    /// page's action modal enforces it client-side (the list DTOs already carry this).</summary>
    public bool CurrentStepIsCommentRequired { get; set; }
    public int WorkflowDefinitionVersion { get; set; }
    public DateTime? CurrentStepDueDate { get; set; }
    public bool IsOverdue { get; set; }
    public DateTime? StartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public long? CompletedByUserId { get; set; }
    public string CompletionComment { get; set; }
    public List<WorkflowTransitionDto> Transitions { get; set; }

    /// <summary>WF-30: the current step may be waived instead of satisfied.</summary>
    public bool CurrentStepIsOptional { get; set; }
    /// <summary>WF-30: the current step's guard key, if any.</summary>
    public string CurrentStepGuardKey { get; set; }
    /// <summary>WF-30: live evaluation of the current step's guard (null when the step has none).</summary>
    public WorkflowGuardStatusDto CurrentStepGuard { get; set; }
    /// <summary>WF-32: the decision fields the actor must supply on this step (null when none).</summary>
    public WorkflowDecisionSchemaDto CurrentStepDecisionSchema { get; set; }

    /// <summary>
    /// The final step does not ask for a decision, although this kind of record
    /// has a decision form registered for it.
    /// <para>
    /// A misconfiguration, and one the person looking at the step cannot fix:
    /// they fill the dialog in, press OK, and are told the step has nothing to
    /// act on. Surfaced here so the screen can say so before they bother.
    /// </para>
    /// </summary>
    public bool CurrentStepIsMissingItsDecisionForm { get; set; }

    /// <summary>
    /// Whether the signed-in user may act on the current step.
    /// <para>
    /// Take Action used to be offered on every step. A principal looking at an
    /// Admin step got an enabled button and a dialog, and found out only on OK
    /// — the same shape as the decision-form bug above, and the same remedy:
    /// say it before they fill anything in.
    /// </para>
    /// <para>
    /// True when the instance is not in progress or has no current step, since
    /// there is then nothing to be barred from.
    /// </para>
    /// </summary>
    public bool CurrentUserCanAct { get; set; } = true;

    /// <summary>Why not, in the words the server would have refused with. Null when they can.</summary>
    public string CurrentUserCannotActReason { get; set; }
}

public class WorkflowGuardStatusDto
{
    public string Key { get; set; }
    public string DisplayName { get; set; }
    public bool Satisfied { get; set; }
    public string Message { get; set; }
}

public class WorkflowDecisionSchemaDto
{
    public string Key { get; set; }
    public string DisplayName { get; set; }
    public List<WorkflowDecisionFieldDto> Fields { get; set; } = new();

    public static WorkflowDecisionSchemaDto From(psms.Workflow.Engine.IWorkflowDecisionSchema schema) => new()
    {
        Key = schema.Key,
        DisplayName = schema.DisplayName,
        Fields = schema.Fields.Select(f => new WorkflowDecisionFieldDto
        {
            Key = f.Key, Label = f.Label, Type = f.Type, Required = f.Required, Placeholder = f.Placeholder,
            Min = f.Min, Max = f.Max,
            Options = f.Options?.Select(o => new WorkflowDecisionOptionDto { Value = o.Value, Label = o.Label }).ToList(),
            Conditions = f.Conditions?.Select(c => new WorkflowDecisionConditionDto
            {
                WhenField = c.WhenField, WhenValueIn = c.WhenValueIn?.ToList(),
                Required = c.Required, MinLength = c.MinLength, Message = c.Message,
            }).ToList(),
        }).ToList(),
    };
}

public class WorkflowDecisionFieldDto
{
    public string Key { get; set; }
    public string Label { get; set; }
    public string Type { get; set; }
    public bool Required { get; set; }
    public string Placeholder { get; set; }
    public decimal? Min { get; set; }
    public decimal? Max { get; set; }
    public List<WorkflowDecisionOptionDto> Options { get; set; }

    /// <summary>
    /// Requirements that only apply for certain values of another field, so the
    /// dialog can enforce them on the field instead of refusing a filled-in form.
    /// </summary>
    public List<WorkflowDecisionConditionDto> Conditions { get; set; }
}

public class WorkflowDecisionConditionDto
{
    public string WhenField { get; set; }
    public List<string> WhenValueIn { get; set; }
    public bool Required { get; set; }
    public int? MinLength { get; set; }
    public string Message { get; set; }
}

public class WorkflowDecisionOptionDto
{
    public string Value { get; set; }
    public string Label { get; set; }
}
