using System;
using System.Collections.Generic;

namespace psms.Admissions.Enrollment.Dto;

/// <summary>
/// DTO for class assignment information (ADM-029).
/// </summary>
public class ClassAssignmentDto
{
    public Guid ApplicationId { get; set; }
    public string ApplicationNumber { get; set; }
    public string ApplicantName { get; set; }

    // Assigned Class
    public Guid? ClassId { get; set; }
    public string ClassName { get; set; }
    public string ClassTeacherName { get; set; }
    public int ClassCurrentSize { get; set; }
    public int ClassCapacity { get; set; }

    // Assignment Method
    public string AssignmentMethod { get; set; }
    public DateTime? AssignedDate { get; set; }
    public long? AssignedByUserId { get; set; }
    public string AssignedByUserName { get; set; }

    // Factors Considered (for auto-assignment)
    public bool HasSiblingsInClass { get; set; }
    public string SiblingClassName { get; set; }
    public bool GenderBalanceConsidered { get; set; }
    public bool CapacityBalanceConsidered { get; set; }
}

/// <summary>
/// DTO for available class options during assignment.
/// </summary>
public class AvailableClassDto
{
    public Guid ClassId { get; set; }
    public string ClassName { get; set; }
    public string ClassTeacherName { get; set; }
    public int CurrentSize { get; set; }
    public int Capacity { get; set; }
    public int AvailableSpots => Capacity - CurrentSize;
    public decimal UtilizationPercentage => Capacity > 0 ? (decimal)CurrentSize / Capacity * 100 : 0;
    public int MaleCount { get; set; }
    public int FemaleCount { get; set; }
    public bool HasSiblings { get; set; }
    public List<string> SiblingNames { get; set; } = new();
    public bool IsRecommended { get; set; }
    public string RecommendationReason { get; set; }
}
