using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Activities.FieldTrips.Dto;

/// <summary>
/// DTO for creating a field trip.
/// </summary>
public class CreateFieldTripDto
{
    [Required]
    [StringLength(200)]
    public string TripName { get; set; }

    [Required]
    public Guid AcademicYearId { get; set; }

    [Required]
    public Guid OrganizingTeacherId { get; set; }

    [Required]
    [StringLength(500)]
    public string Destination { get; set; }

    [Required]
    public DateTime TripDate { get; set; }

    [Required]
    public decimal EstimatedCost { get; set; }

    [Required]
    public int NumberOfStudents { get; set; }

    [Required]
    public int NumberOfChaperones { get; set; }

    [StringLength(2000)]
    public string Description { get; set; }

    public Guid? TermId { get; set; }
    public Guid? ClassId { get; set; }
    public Guid? GradeId { get; set; }
    public DateTime? ReturnDate { get; set; }

    [StringLength(500)]
    public string TransportArrangement { get; set; }

    [StringLength(2000)]
    public string RiskAssessmentNotes { get; set; }

    [StringLength(1000)]
    public string EmergencyPlan { get; set; }
}
