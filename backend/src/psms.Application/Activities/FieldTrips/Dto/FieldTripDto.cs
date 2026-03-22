using Abp.Application.Services.Dto;
using System;

namespace psms.Activities.FieldTrips.Dto;

/// <summary>
/// Full DTO for a field trip.
/// </summary>
public class FieldTripDto : FullAuditedEntityDto<Guid>
{
    public string TripName { get; set; }
    public string Description { get; set; }
    public Guid AcademicYearId { get; set; }
    public string AcademicYearName { get; set; }
    public Guid? TermId { get; set; }
    public string TermName { get; set; }
    public Guid OrganizingTeacherId { get; set; }
    public string OrganizingTeacherName { get; set; }
    public Guid? ClassId { get; set; }
    public string ClassName { get; set; }
    public Guid? GradeId { get; set; }
    public string GradeName { get; set; }
    public string Destination { get; set; }
    public DateTime TripDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public decimal EstimatedCost { get; set; }
    public decimal? ApprovedBudget { get; set; }
    public int Status { get; set; }
    public int NumberOfStudents { get; set; }
    public int NumberOfChaperones { get; set; }
    public string TransportArrangement { get; set; }
    public string RiskAssessmentNotes { get; set; }
    public string EmergencyPlan { get; set; }
    public long? ApprovedByUserId { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string RejectionReason { get; set; }
    public string CancellationReason { get; set; }
}
