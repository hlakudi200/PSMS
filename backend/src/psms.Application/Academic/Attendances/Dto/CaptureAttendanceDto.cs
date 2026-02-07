using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.Attendances.Dto;

public class CaptureAttendanceDto
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public Guid ClassId { get; set; }

    [Required]
    public Guid TeacherId { get; set; }

    [Required]
    public DateTime AttendanceDate { get; set; }

    [Required]
    public AttendanceStatus Status { get; set; }

    [StringLength(500)]
    public string Notes { get; set; }

    public Guid? SubjectId { get; set; }
}
