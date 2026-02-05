using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.StudentParents.Dto;

/// <summary>
/// Input DTO for linking a student to a parent.
/// </summary>
public class LinkStudentParentDto
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public Guid ParentId { get; set; }

    [Required]
    public RelationshipType RelationshipType { get; set; }

    public bool IsPrimaryContact { get; set; }
    public bool IsFinanciallyResponsible { get; set; }
    public bool CanPickupStudent { get; set; } = true;
    public bool LivesWithStudent { get; set; }
}
