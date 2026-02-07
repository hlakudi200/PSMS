using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.SASpecific.AfterCares.Dto;

/// <summary>
/// Lightweight DTO for after-care program lists.
/// </summary>
public class AfterCareListDto : EntityDto<Guid>
{
    public Guid AcademicYearId { get; set; }
    public string ProgramName { get; set; }
    public AfterCareType AfterCareType { get; set; }
    public string Location { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int Capacity { get; set; }
    public int CurrentEnrollment { get; set; }
    public string SupervisorName { get; set; }
    public decimal MonthlyFee { get; set; }
    public bool IsActive { get; set; }

    // Computed
    public int EnrollmentCount { get; set; }
}
