using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Academic.Attendances.Dto;

public class AttendanceDto : CreationAuditedEntityDto<Guid>
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; }
    public Guid ClassId { get; set; }
    public string ClassName { get; set; }
    public Guid TeacherId { get; set; }
    public string TeacherName { get; set; }
    public Guid? SubjectId { get; set; }
    public string SubjectName { get; set; }
    public DateTime AttendanceDate { get; set; }
    public AttendanceStatus Status { get; set; }
    public string Notes { get; set; }
}
