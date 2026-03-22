using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Workflow.Enums;

namespace psms.Domain.Workflow.Entities;

/// <summary>
/// Immutable audit record of a workflow state transition.
/// Filtered via parent WorkflowInstance (has IMayHaveTenant for direct filtering).
/// </summary>
[Table("WorkflowTransitions")]
public class WorkflowTransition : CreationAuditedEntity<Guid>, IMayHaveTenant
{
    public const int MaxCommentLength = 2000;
    public const int MaxUserNameLength = 256;
    public const int MaxAttachmentUrlLength = 2048;

    public int? TenantId { get; set; }

    [Required]
    public Guid WorkflowInstanceId { get; set; }

    [Required]
    public Guid FromStepId { get; set; }

    public Guid? ToStepId { get; set; }

    [Required]
    public WorkflowActionType Action { get; set; }

    [Required]
    public long ActorUserId { get; set; }

    [StringLength(MaxUserNameLength)]
    public string ActorUserName { get; set; }

    [StringLength(MaxCommentLength)]
    public string Comment { get; set; }

    [Required]
    public DateTime TransitionDate { get; set; }

    /// <summary>
    /// Optional attachment URL (e.g. supporting document uploaded with this transition).
    /// </summary>
    [StringLength(MaxAttachmentUrlLength)]
    public string AttachmentUrl { get; set; }

    [ForeignKey(nameof(WorkflowInstanceId))]
    public virtual WorkflowInstance WorkflowInstance { get; set; }

    [ForeignKey(nameof(FromStepId))]
    public virtual WorkflowStep FromStep { get; set; }

    [ForeignKey(nameof(ToStepId))]
    public virtual WorkflowStep ToStep { get; set; }
}
