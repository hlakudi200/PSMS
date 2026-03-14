using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Academic.Shared;
using psms.Academic.Timetables.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Academic.Timetables;

public interface ITimetableAppService : IApplicationService
{
    Task<TimetableDto> GetAsync(Guid id);
    Task<PagedResultDto<TimetableListDto>> GetAllAsync(GetAcademicEntityInput input);

    /// <summary>Gets the active timetable for a class.</summary>
    Task<TimetableDto> GetByClassAsync(Guid classId);

    Task<TimetableDto> CreateAsync(CreateTimetableDto input);
    Task<TimetableDto> UpdateAsync(Guid id, UpdateTimetableDto input);
    Task DeleteAsync(Guid id);

    /// <summary>Activates a timetable (deactivates others for same class).</summary>
    Task<TimetableDto> ActivateAsync(Guid id);
    Task<TimetableDto> DeactivateAsync(Guid id);
}
