using psms.Domain.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.Attendances.Dto;

public class UpdateAttendanceDto
{
    [EnumDataType(typeof(AttendanceStatus))]
    public AttendanceStatus? Status { get; set; }

    [StringLength(500)]
    public string Notes { get; set; }
}
