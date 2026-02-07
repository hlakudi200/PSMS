using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.SASpecific.StudentTransports.Dto;

/// <summary>
/// Input DTO for creating a student transport enrollment.
/// </summary>
public class CreateStudentTransportDto
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public Guid SchoolTransportId { get; set; }

    [Required]
    public Guid AcademicYearId { get; set; }

    [Required]
    public TransportDirection Direction { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [StringLength(500)]
    public string PickupAddress { get; set; }

    [StringLength(500)]
    public string DropoffAddress { get; set; }

    public TimeSpan? PickupTime { get; set; }

    [StringLength(100)]
    public string EmergencyContactName { get; set; }

    [StringLength(20)]
    public string EmergencyContactPhone { get; set; }

    [StringLength(1000)]
    public string Notes { get; set; }
}
