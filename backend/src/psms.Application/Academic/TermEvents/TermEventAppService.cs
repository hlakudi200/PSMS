using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Academic.Shared;
using psms.Academic.TermEvents.Dto;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Academic.TermEvents;

/// <summary>
/// Service for managing term calendar events.
/// TENANT SAFETY: TermEvent has NO IMayHaveTenant — filter via Term.TenantId on EVERY query.
/// </summary>
[AbpAuthorize(PermissionNames.Academic_Calendar)]
public class TermEventAppService : ApplicationService, ITermEventAppService
{
    private readonly IRepository<TermEvent, Guid> _termEventRepository;
    private readonly IRepository<Term, Guid> _termRepository;

    public TermEventAppService(
        IRepository<TermEvent, Guid> termEventRepository,
        IRepository<Term, Guid> termRepository)
    {
        _termEventRepository = termEventRepository;
        _termRepository = termRepository;
    }

    [AbpAuthorize(PermissionNames.Academic_Calendar_View)]
    public async Task<TermEventDto> GetAsync(Guid id)
    {
        var termEvent = await _termEventRepository
            .GetAll()
            .Include(te => te.Term)
                .ThenInclude(t => t.AcademicYear)
            .FirstOrDefaultAsync(te => te.Id == id && te.Term.TenantId == AbpSession.TenantId);

        if (termEvent == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TermEventNotFound, "Term event not found.");

        return ObjectMapper.Map<TermEventDto>(termEvent);
    }

    [AbpAuthorize(PermissionNames.Academic_Calendar_View)]
    public async Task<ListResultDto<TermEventListDto>> GetByTermAsync(Guid termId)
    {
        var events = await _termEventRepository
            .GetAll()
            .Include(te => te.Term)
            .Where(te => te.TermId == termId && te.Term.TenantId == AbpSession.TenantId)
            .OrderBy(te => te.EventDate)
            .ToListAsync();

        return new ListResultDto<TermEventListDto>(
            ObjectMapper.Map<List<TermEventListDto>>(events));
    }

    [AbpAuthorize(PermissionNames.Academic_Calendar_View)]
    public async Task<ListResultDto<TermEventListDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var events = await _termEventRepository
            .GetAll()
            .Include(te => te.Term)
            .Where(te => te.Term.TenantId == AbpSession.TenantId
                && te.EventDate.Date >= startDate.Date
                && te.EventDate.Date <= endDate.Date)
            .OrderBy(te => te.EventDate)
            .ToListAsync();

        return new ListResultDto<TermEventListDto>(
            ObjectMapper.Map<List<TermEventListDto>>(events));
    }

    [AbpAuthorize(PermissionNames.Academic_Calendar_Manage)]
    public async Task<TermEventDto> CreateAsync(CreateTermEventDto input)
    {
        // Validate term exists and belongs to tenant
        var term = await _termRepository
            .FirstOrDefaultAsync(t => t.Id == input.TermId && t.TenantId == AbpSession.TenantId);

        if (term == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TermNotFound, "Term not found.");

        // Validate event date is within term date range
        if (input.EventDate.Date < term.StartDate.Date || input.EventDate.Date > term.EndDate.Date)
            throw new UserFriendlyException(AcademicExceptionCodes.EventDateOutsideTerm,
                $"Event date must be within the term dates ({term.StartDate:yyyy-MM-dd} to {term.EndDate:yyyy-MM-dd}).");

        // Check for duplicate (TermId + EventName + EventDate)
        var duplicate = await _termEventRepository
            .GetAll()
            .FirstOrDefaultAsync(te => te.TermId == input.TermId
                && te.EventName == input.EventName
                && te.EventDate.Date == input.EventDate.Date
                && te.Term.TenantId == AbpSession.TenantId);

        if (duplicate != null)
            throw new UserFriendlyException(AcademicExceptionCodes.DuplicateTermEvent,
                "An event with this name already exists on this date for this term.");

        var termEvent = new TermEvent(
            Guid.NewGuid(),
            input.TermId,
            input.EventName,
            input.EventType,
            input.EventDate)
        {
            Description = input.Description
        };

        await _termEventRepository.InsertAsync(termEvent);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(termEvent.Id);
    }

    [AbpAuthorize(PermissionNames.Academic_Calendar_Manage)]
    public async Task<TermEventDto> UpdateAsync(Guid id, UpdateTermEventDto input)
    {
        var termEvent = await _termEventRepository
            .GetAll()
            .Include(te => te.Term)
            .FirstOrDefaultAsync(te => te.Id == id && te.Term.TenantId == AbpSession.TenantId);

        if (termEvent == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TermEventNotFound, "Term event not found.");

        // If event date is being changed, validate it's within term range
        if (input.EventDate.HasValue)
        {
            if (input.EventDate.Value.Date < termEvent.Term.StartDate.Date
                || input.EventDate.Value.Date > termEvent.Term.EndDate.Date)
                throw new UserFriendlyException(AcademicExceptionCodes.EventDateOutsideTerm,
                    $"Event date must be within the term dates ({termEvent.Term.StartDate:yyyy-MM-dd} to {termEvent.Term.EndDate:yyyy-MM-dd}).");
        }

        // If name or date is changing, check for duplicate
        var newName = input.EventName ?? termEvent.EventName;
        var newDate = input.EventDate ?? termEvent.EventDate;

        if (input.EventName != null || input.EventDate.HasValue)
        {
            var duplicate = await _termEventRepository
                .GetAll()
                .FirstOrDefaultAsync(te => te.TermId == termEvent.TermId
                    && te.EventName == newName
                    && te.EventDate.Date == newDate.Date
                    && te.Id != id
                    && te.Term.TenantId == AbpSession.TenantId);

            if (duplicate != null)
                throw new UserFriendlyException(AcademicExceptionCodes.DuplicateTermEvent,
                    "An event with this name already exists on this date for this term.");
        }

        if (input.EventName != null) termEvent.EventName = input.EventName;
        if (input.EventType.HasValue) termEvent.EventType = input.EventType.Value;
        if (input.EventDate.HasValue) termEvent.EventDate = input.EventDate.Value;
        if (input.Description != null) termEvent.Description = input.Description;

        await _termEventRepository.UpdateAsync(termEvent);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Academic_Calendar_Manage)]
    public async Task DeleteAsync(Guid id)
    {
        var termEvent = await _termEventRepository
            .GetAll()
            .Include(te => te.Term)
            .FirstOrDefaultAsync(te => te.Id == id && te.Term.TenantId == AbpSession.TenantId);

        if (termEvent == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TermEventNotFound, "Term event not found.");

        await _termEventRepository.DeleteAsync(termEvent);
    }
}
