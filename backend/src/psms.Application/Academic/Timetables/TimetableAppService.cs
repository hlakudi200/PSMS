using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Academic.Shared;
using psms.Academic.Timetables.Dto;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.Academic.Timetables;

[AbpAuthorize(PermissionNames.Academic_Timetables)]
public class TimetableAppService : ApplicationService, ITimetableAppService
{
    private readonly IRepository<Timetable, Guid> _timetableRepository;
    private readonly IRepository<Class, Guid> _classRepository;

    public TimetableAppService(
        IRepository<Timetable, Guid> timetableRepository,
        IRepository<Class, Guid> classRepository)
    {
        _timetableRepository = timetableRepository;
        _classRepository = classRepository;
    }

    [AbpAuthorize(PermissionNames.Academic_Timetables_View)]
    public async Task<TimetableDto> GetAsync(Guid id)
    {
        var timetable = await _timetableRepository
            .GetAll()
            .Include(t => t.Class)
            .Include(t => t.TimetableSlots)
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == AbpSession.TenantId);

        if (timetable == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TimetableNotFound, "Timetable not found.");

        return ObjectMapper.Map<TimetableDto>(timetable);
    }

    [AbpAuthorize(PermissionNames.Academic_Timetables_View)]
    public async Task<PagedResultDto<TimetableListDto>> GetAllAsync(GetAcademicEntityInput input)
    {
        var query = _timetableRepository
            .GetAll()
            .Include(t => t.Class)
            .Include(t => t.TimetableSlots)
            .Where(t => t.TenantId == AbpSession.TenantId)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                t => t.Class.ClassName.ToLower().Contains(input.Keyword.ToLower()))
            .WhereIf(input.IsActive.HasValue,
                t => t.IsActive == input.IsActive.Value);

        var totalCount = await query.CountAsync();

        var timetables = await query
            .OrderBy(input.Sorting ?? "EffectiveDate DESC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<TimetableListDto>(
            totalCount,
            ObjectMapper.Map<List<TimetableListDto>>(timetables));
    }

    [AbpAuthorize(PermissionNames.Academic_Timetables_View)]
    public async Task<TimetableDto> GetByClassAsync(Guid classId)
    {
        var timetable = await _timetableRepository
            .GetAll()
            .Include(t => t.Class)
            .Include(t => t.TimetableSlots)
            .FirstOrDefaultAsync(t => t.ClassId == classId
                && t.IsActive
                && t.TenantId == AbpSession.TenantId);

        if (timetable == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TimetableNotFound, "No active timetable found for this class.");

        return ObjectMapper.Map<TimetableDto>(timetable);
    }

    [AbpAuthorize(PermissionNames.Academic_Timetables_Create)]
    public async Task<TimetableDto> CreateAsync(CreateTimetableDto input)
    {
        // Validate class exists
        var cls = await _classRepository
            .FirstOrDefaultAsync(c => c.Id == input.ClassId && c.TenantId == AbpSession.TenantId);

        if (cls == null)
            throw new UserFriendlyException(AcademicExceptionCodes.ClassNotFound, "Class not found.");

        // Validate dates
        if (input.EndDate.HasValue && input.EndDate.Value.Date <= input.EffectiveDate.Date)
            throw new UserFriendlyException(AcademicExceptionCodes.InvalidTimetableDates,
                "End date must be after the effective date.");

        var timetable = new Timetable(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.ClassId,
            input.EffectiveDate)
        {
            EndDate = input.EndDate
        };

        // Create as draft — require explicit ActivateAsync to go live
        // (constructor defaults IsActive=true, but we don't want to silently
        // deactivate the existing active timetable while slots are being added)
        timetable.IsActive = false;

        await _timetableRepository.InsertAsync(timetable);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(timetable.Id);
    }

    [AbpAuthorize(PermissionNames.Academic_Timetables_Edit)]
    public async Task<TimetableDto> UpdateAsync(Guid id, UpdateTimetableDto input)
    {
        var timetable = await _timetableRepository
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == AbpSession.TenantId);

        if (timetable == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TimetableNotFound, "Timetable not found.");

        if (input.EffectiveDate.HasValue) timetable.EffectiveDate = input.EffectiveDate.Value;
        if (input.EndDate.HasValue) timetable.EndDate = input.EndDate.Value;

        // Validate dates after update
        if (timetable.EndDate.HasValue && timetable.EndDate.Value.Date <= timetable.EffectiveDate.Date)
            throw new UserFriendlyException(AcademicExceptionCodes.InvalidTimetableDates,
                "End date must be after the effective date.");

        await _timetableRepository.UpdateAsync(timetable);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Academic_Timetables_Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var timetable = await _timetableRepository
            .GetAll()
            .Include(t => t.TimetableSlots)
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == AbpSession.TenantId);

        if (timetable == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TimetableNotFound, "Timetable not found.");

        if (timetable.TimetableSlots.Any())
            throw new UserFriendlyException(AcademicExceptionCodes.CannotDeleteTimetableWithSlots,
                "Cannot delete a timetable that has time slots. Remove all slots first.");

        await _timetableRepository.DeleteAsync(timetable);
    }

    [AbpAuthorize(PermissionNames.Academic_Timetables_Publish)]
    public async Task<TimetableDto> ActivateAsync(Guid id)
    {
        var timetable = await _timetableRepository
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == AbpSession.TenantId);

        if (timetable == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TimetableNotFound, "Timetable not found.");

        // Deactivate all other timetables for the same class
        var activeTimetables = await _timetableRepository
            .GetAll()
            .Where(t => t.ClassId == timetable.ClassId
                && t.IsActive
                && t.Id != id
                && t.TenantId == AbpSession.TenantId)
            .ToListAsync();

        foreach (var active in activeTimetables)
        {
            active.IsActive = false;
            await _timetableRepository.UpdateAsync(active);
        }

        timetable.IsActive = true;

        await _timetableRepository.UpdateAsync(timetable);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Academic_Timetables_Edit)]
    public async Task<TimetableDto> DeactivateAsync(Guid id)
    {
        var timetable = await _timetableRepository
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == AbpSession.TenantId);

        if (timetable == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TimetableNotFound, "Timetable not found.");

        if (!timetable.IsActive)
            throw new UserFriendlyException(AcademicExceptionCodes.TimetableNotActive, "Timetable is already inactive.");

        timetable.IsActive = false;

        await _timetableRepository.UpdateAsync(timetable);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }
}
