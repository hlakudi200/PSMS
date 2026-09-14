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
}

public class WorkflowDecisionOptionDto
{
    public string Value { get; set; }
    public string Label { get; set; }
}
