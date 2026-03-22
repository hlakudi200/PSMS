using Abp.Application.Services.Dto;
using System;

namespace psms.Activities.FieldTrips.Dto;

/// <summary>
/// Lightweight list DTO for field trips.
/// </summary>
public class FieldTripListDto : EntityDto<Guid>
{
    public string TripName { get; set; }
    public string Destination { get; set; }
    public DateTime TripDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public int Status { get; set; }
    public decimal EstimatedCost { get; set; }
    public decimal? ApprovedBudget { get; set; }
    public int NumberOfStudents { get; set; }
    public string OrganizingTeacherName { get; set; }
    public DateTime CreationTime { get; set; }
}
