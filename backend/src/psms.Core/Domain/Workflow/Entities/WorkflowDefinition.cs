using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Workflow.Enums;

namespace psms.Domain.Workflow.Entities;

/// <summary>
/// A tenant-specific workflow template describing an approval chain.
/// </summary>
[Table("WorkflowDefinitions")]
public class WorkflowDefinition : FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
{
    public const int MaxNameLength = 200;
    public const int MaxDescriptionLength = 1000;

    public int? TenantId { get; set; }

    [Required]
    [StringLength(MaxNameLength)]
    public string Name { get; set; }

    [StringLength(MaxDescriptionLength)]
    public string Description { get; set; }

    [Required]
    public WorkflowEntityType EntityType { get; set; }

    /// <summary>
    /// Only one definition per EntityType per tenant can be active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Auto-incremented version. Bumped when steps are modified.
    /// </summary>
    public int Version { get; set; } = 1;

    public virtual ICollection<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();

    public void BumpVersion()
    {
        Version++;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
