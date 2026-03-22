using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Activities.FieldTrips.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Activities.FieldTrips;

/// <summary>
/// Interface for managing field trips.
/// </summary>
public interface IFieldTripAppService : IApplicationService
{
    Task<FieldTripDto> GetAsync(Guid id);
    Task<PagedResultDto<FieldTripListDto>> GetAllAsync(GetFieldTripsInput input);
    Task<FieldTripDto> CreateAsync(CreateFieldTripDto input);
    Task<FieldTripDto> UpdateAsync(Guid id, UpdateFieldTripDto input);
    Task DeleteAsync(Guid id);
    Task<FieldTripDto> SubmitAsync(Guid id);
    Task<FieldTripDto> ApproveAsync(Guid id, decimal approvedBudget);
    Task<FieldTripDto> RejectAsync(Guid id, string reason);
    Task<FieldTripDto> CancelAsync(Guid id, string reason);
    Task<FieldTripDto> CompleteAsync(Guid id);
}
