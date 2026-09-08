using psms.Domain.Workflow.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace psms.Workflow.WorkflowInstances.Dto;

public class AdvanceWorkflowInput
{
    [Required]
    public WorkflowActionType Action { get; set; }

    [StringLength(2000)]
    public string Comment { get; set; }

    [StringLength(2048)]
    public string AttachmentUrl { get; set; }

    /// <summary>
    /// WF-32: values for the current step's decision schema fields, keyed by field
    /// key (e.g. approvedAmount, decision, reason). Validated server-side.
    /// </summary>
    public Dictionary<string, object> Decision { get; set; }

    /// <summary>
    /// WF-30: advance even though the step's guard fails. Requires
    /// Workflow.Instances.OverrideGuard and a comment; recorded on the transition.
    /// </summary>
    public bool OverrideGuard { get; set; }
}
