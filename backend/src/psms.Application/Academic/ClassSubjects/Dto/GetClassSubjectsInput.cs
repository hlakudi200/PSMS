using Abp.Application.Services.Dto;
using System;

namespace psms.Academic.ClassSubjects.Dto;

/// <summary>
/// Input DTO for querying class-subject assignments with optional filters.
/// </summary>
public class GetClassSubjectsInput : PagedAndSortedResultRequestDto
{
    public Guid? ClassId { get; set; }
    public Guid? SubjectId { get; set; }
    public Guid? TeacherId { get; set; }
    public bool? IsActive { get; set; }
}
