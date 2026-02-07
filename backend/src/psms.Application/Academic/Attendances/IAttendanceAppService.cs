using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Academic.Attendances.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Academic.Attendances;

public interface IAttendanceAppService : IApplicationService
{
    Task<AttendanceDto> GetAsync(Guid id);
    Task<PagedResultDto<AttendanceListDto>> GetAllAsync(GetAttendanceInput input);
    Task<ListResultDto<AttendanceListDto>> GetByStudentAsync(Guid studentId, DateTime? startDate, DateTime? endDate);
    Task<ListResultDto<AttendanceListDto>> GetByClassAndDateAsync(Guid classId, DateTime date);

    /// <summary>Captures attendance for a single student.</summary>
    Task<AttendanceDto> CaptureAsync(CaptureAttendanceDto input);

    /// <summary>Captures attendance for multiple students in bulk.</summary>
    Task<ListResultDto<AttendanceDto>> BulkCaptureAsync(BulkCaptureAttendanceDto input);

    Task<AttendanceDto> UpdateAsync(Guid id, UpdateAttendanceDto input);
    Task DeleteAsync(Guid id);

    /// <summary>Gets attendance summary for a student within a date range.</summary>
    Task<AttendanceSummaryDto> GetStudentSummaryAsync(Guid studentId, DateTime startDate, DateTime endDate);

    /// <summary>Gets attendance summary for all students in a class within a date range.</summary>
    Task<ListResultDto<AttendanceSummaryDto>> GetClassSummaryAsync(Guid classId, DateTime startDate, DateTime endDate);
}
