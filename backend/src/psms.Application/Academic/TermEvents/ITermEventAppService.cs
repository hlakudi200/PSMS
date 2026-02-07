using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Academic.TermEvents.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Academic.TermEvents;

public interface ITermEventAppService : IApplicationService
{
    Task<TermEventDto> GetAsync(Guid id);
    Task<ListResultDto<TermEventListDto>> GetByTermAsync(Guid termId);
    Task<ListResultDto<TermEventListDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<TermEventDto> CreateAsync(CreateTermEventDto input);
    Task<TermEventDto> UpdateAsync(Guid id, UpdateTermEventDto input);
    Task DeleteAsync(Guid id);
}
