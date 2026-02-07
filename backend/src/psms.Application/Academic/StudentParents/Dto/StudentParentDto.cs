using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Academic.StudentParents.Dto;

/// <summary>
/// DTO for student-parent link with flattened navigation properties.
/// </summary>
public class StudentParentDto : EntityDto<Guid>
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; }
    public string StudentAdmissionNumber { get; set; }
    public Guid ParentId { get; set; }
    public string ParentName { get; set; }
    public string ParentEmail { get; set; }
    public string ParentPhone { get; set; }
    public RelationshipType RelationshipType { get; set; }
    public bool IsPrimaryContact { get; set; }
    public bool IsFinanciallyResponsible { get; set; }
    public bool CanPickupStudent { get; set; }
    public bool LivesWithStudent { get; set; }
}
