using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Discipline.DisciplinaryCases.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Discipline.DisciplinaryCases;

public interface IDisciplinaryCaseAppService : IApplicationService
{
    Task<DisciplinaryCaseDto> GetAsync(Guid id);
    Task<PagedResultDto<DisciplinaryCaseListDto>> GetAllAsync(GetDisciplinaryCasesInput input);
    Task<DisciplinaryCaseDto> CreateAsync(CreateDisciplinaryCaseDto input);
    Task<DisciplinaryCaseDto> UpdateAsync(Guid id, UpdateDisciplinaryCaseDto input);
    Task DeleteAsync(Guid id);
    Task<DisciplinaryCaseDto> SubmitAsync(Guid id);
    Task<DisciplinaryCaseDto> StartInvestigationAsync(Guid id);
    Task<DisciplinaryCaseDto> ScheduleHearingAsync(Guid id, DateTime hearingDate);
    Task<DisciplinaryCaseDto> RecordOutcomeAsync(Guid id, RecordOutcomeDto input);
    Task<DisciplinaryCaseDto> NotifyParentAsync(Guid id);
    Task<DisciplinaryCaseDto> ResolveAsync(Guid id);
    Task<DisciplinaryCaseDto> CancelAsync(Guid id);
}
