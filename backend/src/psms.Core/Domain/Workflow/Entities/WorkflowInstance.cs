using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Workflow.Enums;

namespace psms.Domain.Workflow.Entities;

/// <summary>
/// A running workflow instance linked to a specific entity (Application, Report, etc.).
/// </summary>
[Table("WorkflowInstances")]
public class WorkflowInstance : FullAuditedEntity<Guid>, IMayHaveTenant
{
    public const int MaxCommentLength = 2000;

    public int? TenantId { get; set; }

    [Required]
    public Guid WorkflowDefinitionId { get; set; }

    [Required]
    public WorkflowEntityType EntityType { get; set; }

    /// <summary>
    /// The ID of the linked entity (Application.Id, Report.Id, etc.).
    /// </summary>
    [Required]
    public Guid EntityId { get; set; }

    [Required]
    public WorkflowStatus Status { get; set; } = WorkflowStatus.NotStarted;

    /// <summary>
    /// 1-based order of the current step.
    /// </summary>
    [Required]
    public int CurrentStepOrder { get; set; } = 1;

    public Guid? CurrentStepId { get; set; }

    /// <summary>
    /// Snapshot of the workflow definition version when this instance was started.
    /// </summary>
    public int WorkflowDefinitionVersion { get; set; }

    /// <summary>
    /// Due date for the current step based on SlaHours. Null if no SLA configured.
    /// </summary>
    public DateTime? CurrentStepDueDate { get; set; }

    /// <summary>
    /// True when the current step has exceeded its SLA deadline.
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool IsOverdue => CurrentStepDueDate.HasValue && DateTime.UtcNow > CurrentStepDueDate.Value;

    public DateTime? StartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public long? CompletedByUserId { get; set; }

    [StringLength(MaxCommentLength)]
    public string CompletionComment { get; set; }

    [ForeignKey(nameof(WorkflowDefinitionId))]
    public virtual WorkflowDefinition WorkflowDefinition { get; set; }

    [ForeignKey(nameof(CurrentStepId))]
    public virtual WorkflowStep CurrentStep { get; set; }

    public virtual ICollection<WorkflowTransition> Transitions { get; set; } = new List<WorkflowTransition>();

    public void Start(Guid firstStepId)
    {
        if (Status != WorkflowStatus.NotStarted)
            throw new InvalidOperationException("Workflow has already been started.");

        Status = WorkflowStatus.InProgress;
        CurrentStepOrder = 1;
        CurrentStepId = firstStepId;
        StartedDate = DateTime.UtcNow;
    }

    public void AdvanceTo(int stepOrder, Guid stepId, int? slaHours = null)
    {
        if (Status != WorkflowStatus.InProgress)
            throw new InvalidOperationException("Workflow is not in progress.");

        CurrentStepOrder = stepOrder;
        CurrentStepId = stepId;
        CurrentStepDueDate = slaHours.HasValue ? DateTime.UtcNow.AddHours(slaHours.Value) : null;
    }

    public void Complete(long userId, string comment)
    {
        if (Status != WorkflowStatus.InProgress)
            throw new InvalidOperationException("Workflow is not in progress.");

        Status = WorkflowStatus.Completed;
        CompletedDate = DateTime.UtcNow;
        CompletedByUserId = userId;
        CompletionComment = comment;
    }

    public void Reject(long userId, string comment)
    {
        if (Status != WorkflowStatus.InProgress)
            throw new InvalidOperationException("Workflow is not in progress.");

        Status = WorkflowStatus.Rejected;
        CompletedDate = DateTime.UtcNow;
        CompletedByUserId = userId;
        CompletionComment = comment;
    }

    public void Cancel(long userId, string comment)
    {
        if (Status != WorkflowStatus.InProgress && Status != WorkflowStatus.NotStarted)
            throw new InvalidOperationException("Workflow cannot be cancelled in its current state.");

        Status = WorkflowStatus.Cancelled;
        CompletedDate = DateTime.UtcNow;
        CompletedByUserId = userId;
        CompletionComment = comment;
        CurrentStepDueDate = null;
    }

    /// <summary>
    /// Recall/withdraw by the original submitter. Only allowed if still in progress.
    /// </summary>
    public void Recall(long userId, string comment)
    {
        if (Status != WorkflowStatus.InProgress)
            throw new InvalidOperationException("Only in-progress workflows can be recalled.");

        if (CreatorUserId != userId)
            throw new InvalidOperationException("Only the original submitter can recall a workflow.");

        Status = WorkflowStatus.Recalled;
        CompletedDate = DateTime.UtcNow;
        CompletedByUserId = userId;
        CompletionComment = comment;
        CurrentStepDueDate = null;
    }
}
