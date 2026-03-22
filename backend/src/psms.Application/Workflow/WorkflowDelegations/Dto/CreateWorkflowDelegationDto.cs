using psms.Domain.Workflow.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Workflow.WorkflowDelegations.Dto;

public class CreateWorkflowDelegationDto
{
    [Required]
    public long DelegateUserId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [StringLength(500)]
    public string Reason { get; set; }

    public WorkflowEntityType? EntityType { get; set; }

    [StringLength(100)]
    public string AssignedRole { get; set; }
}
