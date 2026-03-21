using psms.Domain.Workflow.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace psms.Workflow.WorkflowInstances.Dto;

public class BatchAdvanceInput
{
    [Required]
    public List<Guid> InstanceIds { get; set; }

    [Required]
    public WorkflowActionType Action { get; set; }

    [StringLength(2000)]
    public string Comment { get; set; }
}

public class BatchAdvanceResultDto
{
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<BatchAdvanceFailureDto> Failures { get; set; } = new List<BatchAdvanceFailureDto>();
}

public class BatchAdvanceFailureDto
{
    public Guid InstanceId { get; set; }
    public string Error { get; set; }
}
