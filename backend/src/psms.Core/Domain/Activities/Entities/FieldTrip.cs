using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Academic.Entities;
using psms.Domain.Shared.Enums;

namespace psms.Domain.Activities.Entities;

[Table("FieldTrips")]
public class FieldTrip : FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
{
    public int? TenantId { get; set; }

    [Required]
    [StringLength(200)]
    public string TripName { get; set; }

    [StringLength(2000)]
    public string Description { get; set; }

    [Required]
    public Guid AcademicYearId { get; set; }

    public Guid? TermId { get; set; }

    [Required]
    public Guid OrganizingTeacherId { get; set; }

    public Guid? ClassId { get; set; }
    public Guid? GradeId { get; set; }

    [Required]
    [StringLength(500)]
    public string Destination { get; set; }

    [Required]
    public DateTime TripDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal EstimatedCost { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? ApprovedBudget { get; set; }

    [Required]
    public FieldTripStatus Status { get; set; }

    public int NumberOfStudents { get; set; }
    public int NumberOfChaperones { get; set; }

    [StringLength(500)]
    public string TransportArrangement { get; set; }

    [StringLength(2000)]
    public string RiskAssessmentNotes { get; set; }

    [StringLength(1000)]
    public string EmergencyPlan { get; set; }

    public long? ApprovedByUserId { get; set; }
    public DateTime? ApprovedDate { get; set; }

    [StringLength(500)]
    public string RejectionReason { get; set; }

    [StringLength(500)]
    public string CancellationReason { get; set; }

    [ForeignKey(nameof(AcademicYearId))]
    public virtual AcademicYear AcademicYear { get; set; }

    [ForeignKey(nameof(TermId))]
    public virtual Term Term { get; set; }

    [ForeignKey(nameof(OrganizingTeacherId))]
    public virtual Teacher OrganizingTeacher { get; set; }

    [ForeignKey(nameof(ClassId))]
    public virtual Class Class { get; set; }

    [ForeignKey(nameof(GradeId))]
    public virtual Grade Grade { get; set; }

    protected FieldTrip() { }

    public FieldTrip(Guid id, int? tenantId, string tripName, Guid academicYearId,
        Guid organizingTeacherId, string destination, DateTime tripDate,
        decimal estimatedCost, int numberOfStudents, int numberOfChaperones)
    {
        Id = id;
        TenantId = tenantId;
        TripName = tripName;
        AcademicYearId = academicYearId;
        OrganizingTeacherId = organizingTeacherId;
        Destination = destination;
        TripDate = tripDate;
        EstimatedCost = estimatedCost;
        NumberOfStudents = numberOfStudents;
        NumberOfChaperones = numberOfChaperones;
        Status = FieldTripStatus.Draft;
    }

    public void Submit()
    {
        if (Status != FieldTripStatus.Draft)
            throw new InvalidOperationException("Only draft trips can be submitted.");
        Status = FieldTripStatus.Submitted;
    }

    /// <summary>
    /// WF-31: the approval workflow entered review — the record is now with a
    /// reviewer and can no longer be edited by the requester.
    /// </summary>
    public void StartReview()
    {
        if (Status != FieldTripStatus.Submitted)
            throw new InvalidOperationException("Only submitted trips can move to review.");
        Status = FieldTripStatus.UnderReview;
    }

    /// <summary>
    /// WF-31: the approval workflow was cancelled or recalled before a decision —
    /// return the trip to the requester as a draft.
    /// </summary>
    public void ReopenAsDraft()
    {
        if (Status != FieldTripStatus.Submitted && Status != FieldTripStatus.UnderReview)
            throw new InvalidOperationException("Only submitted or in-review trips can be reopened.");
        Status = FieldTripStatus.Draft;
    }

    public void Approve(long userId, decimal budget)
    {
        if (Status != FieldTripStatus.Submitted && Status != FieldTripStatus.UnderReview)
            throw new InvalidOperationException("Trip must be submitted or under review to approve.");
        ApprovedByUserId = userId;
        ApprovedDate = DateTime.UtcNow;
        ApprovedBudget = budget;
        Status = FieldTripStatus.Approved;
    }

    public void Reject(long userId, string reason)
    {
        if (Status != FieldTripStatus.Submitted && Status != FieldTripStatus.UnderReview)
            throw new InvalidOperationException("Trip must be submitted or under review to reject.");
        ApprovedByUserId = userId;
        ApprovedDate = DateTime.UtcNow;
        RejectionReason = reason;
        Status = FieldTripStatus.Rejected;
    }

    public void Cancel(string reason)
    {
        if (Status == FieldTripStatus.Completed || Status == FieldTripStatus.Cancelled)
            throw new InvalidOperationException("Cannot cancel a completed or already cancelled trip.");
        CancellationReason = reason;
        Status = FieldTripStatus.Cancelled;
    }

    public void Complete()
    {
        if (Status != FieldTripStatus.Approved)
            throw new InvalidOperationException("Trip must be approved to mark as completed.");
        Status = FieldTripStatus.Completed;
    }
}
