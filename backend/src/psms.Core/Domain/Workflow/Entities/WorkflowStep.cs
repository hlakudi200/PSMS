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

    public const int MaxExtensionKeyLength = 100;

    /// <summary>
    /// WF-30: key of the registered exit criterion (IWorkflowStepGuard) that must
    /// be satisfied before a forward action leaves this step, e.g.
    /// "application.documents-verified". Null = no criterion.
    /// </summary>
    [StringLength(MaxExtensionKeyLength)]
    public string GuardKey { get; set; }

    /// <summary>
    /// WF-31: key of the registered effect (IWorkflowStepEffect) applied to the
    /// linked record when an instance ENTERS this step, e.g.
    /// "discipline.start-investigation". Null = none.
    /// </summary>
    [StringLength(MaxExtensionKeyLength)]
    public string EntryEffectKey { get; set; }

    /// <summary>
    /// WF-31: key of the registered effect applied to the linked record when a
    /// forward action LEAVES this step, e.g. "leave.hod-approved". Null = none.
    /// </summary>
    [StringLength(MaxExtensionKeyLength)]
    public string ExitEffectKey { get; set; }

    /// <summary>
    /// WF-32: key of the registered decision schema (IWorkflowDecisionSchema)
    /// whose fields the actor must supply when taking a forward action on this
    /// step, e.g. "feewaiver.approval" (approved amount + notes). Null = none.
    /// </summary>
    [StringLength(MaxExtensionKeyLength)]
    public string DecisionSchemaKey { get; set; }

    /// <summary>
    /// WF-30: an optional step may be WAIVED (skipped with a mandatory reason)
    /// instead of satisfied — e.g. an interview a grade does not require.
    /// </summary>
    public bool IsOptional { get; set; }

    [ForeignKey(nameof(WorkflowDefinitionId))]
    public virtual WorkflowDefinition WorkflowDefinition { get; set; }
}
