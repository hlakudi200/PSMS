using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Assessment.Assessments.Dto;

/// <summary>
/// Input DTO for creating an assessment.
/// </summary>
public class CreateAssessmentDto
{
    [Required]
    public Guid ClassSubjectId { get; set; }

    [Required]
    public Guid TermId { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; }

    [StringLength(2000)]
    public string Description { get; set; }

    [Required]
    public AcademicAssessmentType AssessmentType { get; set; }

    public CapsAssessmentCategory? CapsCategory { get; set; }

    [Required]
    [Range(0.01, 99999)]
    public decimal MaxMarks { get; set; }

    [Range(0, 100)]
    public decimal Weight { get; set; }

    [Range(0, 100)]
    public decimal PassPercentage { get; set; } = 50;

    public DateTime? ScheduledDate { get; set; }
    public DateTime? DueDate { get; set; }

    [Range(1, 600)]
    public int? DurationMinutes { get; set; }

    [StringLength(4000)]
    public string Instructions { get; set; }
}
