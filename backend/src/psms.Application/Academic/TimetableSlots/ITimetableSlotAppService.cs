using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Academic.TimetableSlots.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace psms.Academic.TimetableSlots;

public interface ITimetableSlotAppService : IApplicationService
{
    Task<TimetableSlotDto> GetAsync(Guid id);
    Task<ListResultDto<TimetableSlotListDto>> GetByTimetableAsync(Guid timetableId);
    Task<ListResultDto<TimetableSlotListDto>> GetByDayAsync(Guid timetableId, DayOfWeek day);
    Task<ListResultDto<TimetableSlotListDto>> GetByTeacherAsync(Guid teacherId);
    Task<TimetableSlotDto> CreateAsync(CreateTimetableSlotDto input);
    Task<TimetableSlotDto> UpdateAsync(Guid id, UpdateTimetableSlotDto input);
    Task DeleteAsync(Guid id);
    Task<ListResultDto<TimetableSlotDto>> BulkCreateAsync(Guid timetableId, List<CreateTimetableSlotDto> input);
}
