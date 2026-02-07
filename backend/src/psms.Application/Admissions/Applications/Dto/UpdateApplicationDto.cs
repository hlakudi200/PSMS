using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Admissions.Applications.Dto;

/// <summary>
/// DTO for updating an existing admission application.
/// Aligned with Application entity.
/// Only allowed when status is Draft.
/// </summary>
public class UpdateApplicationDto
{
    // Grade can be changed before submission
    public Guid? ApplyingForGradeId { get; set; }

    // Prospective Student Information
    [StringLength(100, MinimumLength = 2)]
    public string FirstName { get; set; }

    [StringLength(100)]
    public string MiddleName { get; set; }

    [StringLength(100, MinimumLength = 2)]
    public string LastName { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public Gender? Gender { get; set; }

    [StringLength(13, MinimumLength = 13)]
    public string IdNumber { get; set; }

    [StringLength(50)]
    public string PassportNumber { get; set; }

    public bool? IsSACitizen { get; set; }

    // Previous School Information
    [StringLength(200)]
    public string PreviousSchool { get; set; }
}
