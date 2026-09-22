using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.Attendances.Dto;

public class BulkCaptureAttendanceDto
{
    [Required]
    public Guid ClassId { get; set; }

    [Required]
    public Guid TeacherId { get; set; }

    [Required]
    public DateTime AttendanceDate { get; set; }

    public Guid? SubjectId { get; set; }

    [Required]
    public List<StudentAttendanceEntry> Entries { get; set; }
}

public class StudentAttendanceEntry
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    [EnumDataType(typeof(AttendanceStatus))]
    public AttendanceStatus Status { get; set; }

    [StringLength(500)]
    public string Notes { get; set; }
}
