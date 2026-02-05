using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Admissions.Enrollments.Dto;

/// <summary>
/// DTO for assigning a class to an enrolling student.
/// Implements ADM-029.
/// </summary>
public class AssignClassDto
{
    /// <summary>
    /// Application ID.
    /// </summary>
    [Required]
    public Guid ApplicationId { get; set; }

    /// <summary>
    /// Class ID to assign the student to.
    /// </summary>
    [Required]
    public Guid ClassId { get; set; }
}
