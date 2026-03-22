using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Activities.FieldTrips.Dto;

/// <summary>
/// DTO for updating a field trip.
/// All fields optional (null-skip pattern).
/// </summary>
public class UpdateFieldTripDto
{
    [StringLength(200)]
    public string TripName { get; set; }

    [StringLength(2000)]
    public string Description { get; set; }

    public Guid? OrganizingTeacherId { get; set; }

    [StringLength(500)]
    public string Destination { get; set; }

    public DateTime? TripDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public decimal? EstimatedCost { get; set; }
    public int? NumberOfStudents { get; set; }
    public int? NumberOfChaperones { get; set; }
    public Guid? TermId { get; set; }
    public Guid? ClassId { get; set; }
    public Guid? GradeId { get; set; }

    [StringLength(500)]
    public string TransportArrangement { get; set; }

    [StringLength(2000)]
    public string RiskAssessmentNotes { get; set; }

    [StringLength(1000)]
    public string EmergencyPlan { get; set; }
}
