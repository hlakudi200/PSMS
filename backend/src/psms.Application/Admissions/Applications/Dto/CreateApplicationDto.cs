using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Admissions.Applications.Dto;

/// <summary>
/// DTO for creating a new admission application.
/// Aligned with Application entity.
/// Validates business rules ADM-002, ADM-004.
/// </summary>
public class CreateApplicationDto
{
    // Academic Year and Grade
    [Required]
    public Guid AcademicYearId { get; set; }

    [Required]
    public Guid ApplyingForGradeId { get; set; }

    // Prospective Student Information (ADM-002)
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FirstName { get; set; }

    [StringLength(100)]
    public string MiddleName { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string LastName { get; set; }

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    public Gender Gender { get; set; }

    /// <summary>
    /// SA ID Number - Required for SA citizens (validated using Luhn algorithm).
    /// </summary>
    [StringLength(13, MinimumLength = 13)]
    public string IdNumber { get; set; }

    /// <summary>
    /// Passport Number - Required for non-SA citizens.
    /// </summary>
    [StringLength(50)]
    public string PassportNumber { get; set; }

    [Required]
    public bool IsSACitizen { get; set; }

    // Previous School Information
    [StringLength(200)]
    public string PreviousSchool { get; set; }

    // Creator's Email (required for notifications)
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string CreatorEmailAddress { get; set; }
}
