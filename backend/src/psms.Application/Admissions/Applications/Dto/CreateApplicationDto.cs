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

    /// <summary>
    /// Where the school writes about this application.
    /// <para>
    /// Optional, and left out by the person it usually belongs to: a parent
    /// applying for their own child is signed in, and their account's address
    /// is the answer. Requiring it meant the apply form had to ask a parent
    /// for an email address it already knew — and gave them a field in which
    /// to put somebody else's.
    /// </para>
    /// <para>
    /// It stays on the DTO for the other case: an admissions officer capturing
    /// an application for a walk-in, where the address is the family's and not
    /// the person typing.
    /// </para>
    /// </summary>
    [EmailAddress]
    [StringLength(256)]
    public string CreatorEmailAddress { get; set; }
}
