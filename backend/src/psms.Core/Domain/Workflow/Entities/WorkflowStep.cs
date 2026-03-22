using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Workflow.Enums;

namespace psms.Domain.Workflow.Entities;

/// <summary>
/// A single step in a workflow definition's approval chain.
/// Filtered via parent WorkflowDefinition (no IMayHaveTenant).
/// </summary>
[Table("WorkflowSteps")]
public class WorkflowStep : FullAuditedEntity<Guid>
{
    public const int MaxNameLength = 200;
    public const int MaxDescriptionLength = 500;
    public const int MaxRoleLength = 100;

    [Required]
    public Guid WorkflowDefinitionId { get; set; }

    /// <summary>
    /// 1-based sequential order of this step in the chain.
    /// </summary>
    [Required]
    public int StepOrder { get; set; }

    [Required]
    [StringLength(MaxNameLength)]
    public string Name { get; set; }

    [StringLength(MaxDescriptionLength)]
    public string Description { get; set; }

    /// <summary>
    /// ABP role name required to act on this step (e.g. "Admin", "Principal").
    /// </summary>
    [Required]
    [StringLength(MaxRoleLength)]
    public string AssignedRole { get; set; }

    /// <summary>
    /// The primary action expected at this step.
    /// </summary>
    [Required]
    public WorkflowActionType ActionType { get; set; }

    /// <summary>
    /// If true, completing this step completes the workflow.
    /// </summary>
    public bool IsTerminal { get; set; }

    /// <summary>
    /// StepOrder to advance to on approval. Null = next sequential step.
    /// </summary>
    public int? NextStepOnApprove { get; set; }

    /// <summary>
    /// StepOrder to go to on rejection. Null = terminal rejection.
    /// </summary>
    public int? NextStepOnReject { get; set; }

    /// <summary>
    /// If true, comment is mandatory when advancing past this step.
    /// </summary>
    public bool IsCommentRequired { get; set; }

    /// <summary>
    /// Optional: assign to a specific user instead of a role.
    /// When set, only this user can act on this step.
    /// </summary>
    public long? AssignedUserId { get; set; }

    /// <summary>
    /// Optional SLA in hours. Used to calculate due date on the instance when this step becomes current.
    /// </summary>
    public int? SlaHours { get; set; }

    /// <summary>
    /// Optional guard expression. If set, this step is only entered when the condition evaluates to true.
    /// Stored as a simple key=value expression (e.g. "Amount>10000").
    /// </summary>
    [StringLength(500)]
    public string GuardExpression { get; set; }

    [ForeignKey(nameof(WorkflowDefinitionId))]
    public virtual WorkflowDefinition WorkflowDefinition { get; set; }
}
