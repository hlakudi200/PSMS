using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Activities.FieldTrips.Dto;
using psms.Activities.Shared;
using psms.Authorization;
using psms.Domain.Activities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.Activities.FieldTrips;

/// <summary>
/// Service for managing field trips.
/// </summary>
[AbpAuthorize(PermissionNames.Activities_FieldTrips)]
public class FieldTripAppService : ApplicationService, IFieldTripAppService
{
    private readonly IRepository<FieldTrip, Guid> _fieldTripRepository;

    public FieldTripAppService(
        IRepository<FieldTrip, Guid> fieldTripRepository)
    {
        _fieldTripRepository = fieldTripRepository;
    }

    [AbpAuthorize(PermissionNames.Activities_FieldTrips_View)]
    public async Task<FieldTripDto> GetAsync(Guid id)
    {
        var trip = await _fieldTripRepository
            .GetAll()
            .Include(t => t.AcademicYear)
            .Include(t => t.Term)
            .Include(t => t.OrganizingTeacher)
            .Include(t => t.Class)
            .Include(t => t.Grade)
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == AbpSession.TenantId);

        if (trip == null)
            throw new UserFriendlyException(ActivitiesExceptionCodes.TripNotFound,
                "Field trip not found.");

        return ObjectMapper.Map<FieldTripDto>(trip);
    }

    [AbpAuthorize(PermissionNames.Activities_FieldTrips_View)]
    public async Task<PagedResultDto<FieldTripListDto>> GetAllAsync(GetFieldTripsInput input)
    {
        var query = _fieldTripRepository
            .GetAll()
            .Include(t => t.OrganizingTeacher)
            .Where(t => t.TenantId == AbpSession.TenantId)
            .WhereIf(input.AcademicYearId.HasValue, t => t.AcademicYearId == input.AcademicYearId.Value)
            .WhereIf(input.Status.HasValue, t => (int)t.Status == input.Status.Value)
            .WhereIf(input.OrganizingTeacherId.HasValue, t => t.OrganizingTeacherId == input.OrganizingTeacherId.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Search),
                t => t.TripName.ToLower().Contains(input.Search.Trim().ToLower())
                    || t.Destination.ToLower().Contains(input.Search.Trim().ToLower()));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "TripDate DESC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<FieldTripListDto>(
            totalCount,
            ObjectMapper.Map<List<FieldTripListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Activities_FieldTrips_Create)]
    public async Task<FieldTripDto> CreateAsync(CreateFieldTripDto input)
    {
        // Validate trip date is not in the past
        if (input.TripDate.Date < DateTime.UtcNow.Date)
            throw new UserFriendlyException(ActivitiesExceptionCodes.TripDateInPast,
                "Trip date cannot be in the past.");

        var trip = new FieldTrip(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.TripName,
            input.AcademicYearId,
            input.OrganizingTeacherId,
            input.Destination,
            input.TripDate,
            input.EstimatedCost,
            input.NumberOfStudents,
            input.NumberOfChaperones)
        {
            Description = input.Description,
            TermId = input.TermId,
            ClassId = input.ClassId,
            GradeId = input.GradeId,
            ReturnDate = input.ReturnDate,
            TransportArrangement = input.TransportArrangement,
            RiskAssessmentNotes = input.RiskAssessmentNotes,
            EmergencyPlan = input.EmergencyPlan
        };

        await _fieldTripRepository.InsertAsync(trip);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(trip.Id);
    }

    [AbpAuthorize(PermissionNames.Activities_FieldTrips_Edit)]
    public async Task<FieldTripDto> UpdateAsync(Guid id, UpdateFieldTripDto input)
    {
        var trip = await _fieldTripRepository
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == AbpSession.TenantId);

        if (trip == null)
            throw new UserFriendlyException(ActivitiesExceptionCodes.TripNotFound,
                "Field trip not found.");

        if (input.TripName != null) trip.TripName = input.TripName;
        if (input.Description != null) trip.Description = input.Description;
        if (input.OrganizingTeacherId.HasValue) trip.OrganizingTeacherId = input.OrganizingTeacherId.Value;
        if (input.Destination != null) trip.Destination = input.Destination;
        if (input.TripDate.HasValue) trip.TripDate = input.TripDate.Value;
        if (input.ReturnDate.HasValue) trip.ReturnDate = input.ReturnDate.Value;
        if (input.EstimatedCost.HasValue) trip.EstimatedCost = input.EstimatedCost.Value;
        if (input.NumberOfStudents.HasValue) trip.NumberOfStudents = input.NumberOfStudents.Value;
        if (input.NumberOfChaperones.HasValue) trip.NumberOfChaperones = input.NumberOfChaperones.Value;
        if (input.TermId.HasValue) trip.TermId = input.TermId.Value;
        if (input.ClassId.HasValue) trip.ClassId = input.ClassId.Value;
        if (input.GradeId.HasValue) trip.GradeId = input.GradeId.Value;
        if (input.TransportArrangement != null) trip.TransportArrangement = input.TransportArrangement;
        if (input.RiskAssessmentNotes != null) trip.RiskAssessmentNotes = input.RiskAssessmentNotes;
        if (input.EmergencyPlan != null) trip.EmergencyPlan = input.EmergencyPlan;

        await _fieldTripRepository.UpdateAsync(trip);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Activities_FieldTrips_Edit)]
    public async Task DeleteAsync(Guid id)
    {
        var trip = await _fieldTripRepository
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == AbpSession.TenantId);

        if (trip == null)
            throw new UserFriendlyException(ActivitiesExceptionCodes.TripNotFound,
                "Field trip not found.");

        await _fieldTripRepository.DeleteAsync(trip);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    [AbpAuthorize(PermissionNames.Activities_FieldTrips_Create)]
    public async Task<FieldTripDto> SubmitAsync(Guid id)
    {
        var trip = await _fieldTripRepository
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == AbpSession.TenantId);

        if (trip == null)
            throw new UserFriendlyException(ActivitiesExceptionCodes.TripNotFound,
                "Field trip not found.");

        try
        {
            trip.Submit();
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(ActivitiesExceptionCodes.InvalidStatus, ex.Message);
        }

        await _fieldTripRepository.UpdateAsync(trip);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Activities_FieldTrips_Approve)]
    public async Task<FieldTripDto> ApproveAsync(Guid id, decimal approvedBudget)
    {
        var trip = await _fieldTripRepository
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == AbpSession.TenantId);

        if (trip == null)
            throw new UserFriendlyException(ActivitiesExceptionCodes.TripNotFound,
                "Field trip not found.");

        try
        {
            trip.Approve(AbpSession.UserId.Value, approvedBudget);
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(ActivitiesExceptionCodes.InvalidStatus, ex.Message);
        }

        await _fieldTripRepository.UpdateAsync(trip);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Activities_FieldTrips_Approve)]
    public async Task<FieldTripDto> RejectAsync(Guid id, string reason)
    {
        var trip = await _fieldTripRepository
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == AbpSession.TenantId);

        if (trip == null)
            throw new UserFriendlyException(ActivitiesExceptionCodes.TripNotFound,
                "Field trip not found.");

        try
        {
            trip.Reject(AbpSession.UserId.Value, reason);
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(ActivitiesExceptionCodes.InvalidStatus, ex.Message);
        }

        await _fieldTripRepository.UpdateAsync(trip);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Activities_FieldTrips_Edit)]
    public async Task<FieldTripDto> CancelAsync(Guid id, string reason)
    {
        var trip = await _fieldTripRepository
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == AbpSession.TenantId);

        if (trip == null)
            throw new UserFriendlyException(ActivitiesExceptionCodes.TripNotFound,
                "Field trip not found.");

        try
        {
            trip.Cancel(reason);
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(ActivitiesExceptionCodes.InvalidStatus, ex.Message);
        }

        await _fieldTripRepository.UpdateAsync(trip);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Activities_FieldTrips_Edit)]
    public async Task<FieldTripDto> CompleteAsync(Guid id)
    {
        var trip = await _fieldTripRepository
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == AbpSession.TenantId);

        if (trip == null)
            throw new UserFriendlyException(ActivitiesExceptionCodes.TripNotFound,
                "Field trip not found.");

        try
        {
            trip.Complete();
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(ActivitiesExceptionCodes.InvalidStatus, ex.Message);
        }

        await _fieldTripRepository.UpdateAsync(trip);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }
}
