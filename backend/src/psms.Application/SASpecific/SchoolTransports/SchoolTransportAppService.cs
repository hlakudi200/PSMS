using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Authorization;
using psms.Domain.SASpecific.Entities;
using psms.Domain.Shared.Enums;
using psms.SASpecific.SchoolTransports.Dto;
using psms.SASpecific.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.SASpecific.SchoolTransports;

/// <summary>
/// Service for managing school transport routes.
/// </summary>
[AbpAuthorize(PermissionNames.SASpecific_Transport)]
public class SchoolTransportAppService : ApplicationService, ISchoolTransportAppService
{
    private readonly IRepository<SchoolTransport, Guid> _schoolTransportRepository;

    public SchoolTransportAppService(
        IRepository<SchoolTransport, Guid> schoolTransportRepository)
    {
        _schoolTransportRepository = schoolTransportRepository;
    }

    [AbpAuthorize(PermissionNames.SASpecific_Transport_View)]
    public async Task<SchoolTransportDto> GetAsync(Guid id)
    {
        var transport = await _schoolTransportRepository
            .GetAll()
            .Include(st => st.StudentEnrollments)
            .FirstOrDefaultAsync(st => st.Id == id && st.TenantId == AbpSession.TenantId);

        if (transport == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.SchoolTransportNotFound,
                "School transport route not found.");

        return ObjectMapper.Map<SchoolTransportDto>(transport);
    }

    [AbpAuthorize(PermissionNames.SASpecific_Transport_View)]
    public async Task<PagedResultDto<SchoolTransportListDto>> GetAllAsync(GetSchoolTransportsInput input)
    {
        var query = _schoolTransportRepository
            .GetAll()
            .Include(st => st.StudentEnrollments)
            .Where(st => st.TenantId == AbpSession.TenantId)
            .WhereIf(input.TransportType.HasValue, st => st.TransportType == input.TransportType.Value)
            .WhereIf(input.IsActive.HasValue, st => st.IsActive == input.IsActive.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.RouteName),
                st => st.RouteName.ToLower().Contains(input.RouteName.Trim().ToLower()))
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                st => st.RouteName.ToLower().Contains(input.Keyword.ToLower()));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "RouteName ASC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<SchoolTransportListDto>(
            totalCount,
            ObjectMapper.Map<List<SchoolTransportListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.SASpecific_Transport_Create)]
    public async Task<SchoolTransportDto> CreateAsync(CreateSchoolTransportDto input)
    {
        // Duplicate check: (RouteName) per tenant
        var duplicateExists = await _schoolTransportRepository
            .GetAll()
            .AnyAsync(st => st.TenantId == AbpSession.TenantId
                && st.RouteName.ToLower() == input.RouteName.Trim().ToLower());

        if (duplicateExists)
            throw new UserFriendlyException(SASpecificExceptionCodes.DuplicateSchoolTransport,
                "A transport route with this name already exists.");

        var transport = new SchoolTransport(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.RouteName.Trim(),
            input.TransportType,
            input.Capacity,
            input.MonthlyFee)
        {
            Description = input.Description?.Trim(),
            VehicleNumber = input.VehicleNumber?.Trim(),
            DriverName = input.DriverName?.Trim(),
            DriverPhone = input.DriverPhone?.Trim(),
            AreasCovered = input.AreasCovered,
            MorningPickupTime = input.MorningPickupTime,
            AfternoonDepartureTime = input.AfternoonDepartureTime
        };

        await _schoolTransportRepository.InsertAsync(transport);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(transport.Id);
    }

    [AbpAuthorize(PermissionNames.SASpecific_Transport_Edit)]
    public async Task<SchoolTransportDto> UpdateAsync(Guid id, UpdateSchoolTransportDto input)
    {
        var transport = await _schoolTransportRepository
            .FirstOrDefaultAsync(st => st.Id == id && st.TenantId == AbpSession.TenantId);

        if (transport == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.SchoolTransportNotFound,
                "School transport route not found.");

        // Duplicate check if RouteName is changing
        if (input.RouteName != null && input.RouteName.Trim().ToLower() != transport.RouteName.ToLower())
        {
            var duplicateExists = await _schoolTransportRepository
                .GetAll()
                .AnyAsync(st => st.TenantId == AbpSession.TenantId
                    && st.RouteName.ToLower() == input.RouteName.Trim().ToLower()
                    && st.Id != id);

            if (duplicateExists)
                throw new UserFriendlyException(SASpecificExceptionCodes.DuplicateSchoolTransport,
                    "A transport route with this name already exists.");
        }

        if (input.RouteName != null) transport.RouteName = input.RouteName.Trim();
        if (input.Description != null) transport.Description = input.Description.Trim();
        if (input.VehicleNumber != null) transport.VehicleNumber = input.VehicleNumber.Trim();
        if (input.TransportType.HasValue) transport.TransportType = input.TransportType.Value;
        if (input.Capacity.HasValue) transport.Capacity = input.Capacity.Value;
        if (input.DriverName != null) transport.DriverName = input.DriverName.Trim();
        if (input.DriverPhone != null) transport.DriverPhone = input.DriverPhone.Trim();
        if (input.AreasCovered != null) transport.AreasCovered = input.AreasCovered;
        if (input.MorningPickupTime.HasValue) transport.MorningPickupTime = input.MorningPickupTime.Value;
        if (input.AfternoonDepartureTime.HasValue) transport.AfternoonDepartureTime = input.AfternoonDepartureTime.Value;
        if (input.MonthlyFee.HasValue) transport.MonthlyFee = input.MonthlyFee.Value;

        await _schoolTransportRepository.UpdateAsync(transport);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.SASpecific_Transport_Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var transport = await _schoolTransportRepository
            .GetAll()
            .Include(st => st.StudentEnrollments)
            .FirstOrDefaultAsync(st => st.Id == id && st.TenantId == AbpSession.TenantId);

        if (transport == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.SchoolTransportNotFound,
                "School transport route not found.");

        if (transport.StudentEnrollments != null && transport.StudentEnrollments.Any(se => se.Status == EnrollmentStatus.Active))
            throw new UserFriendlyException(SASpecificExceptionCodes.CannotDeleteTransportWithEnrollments,
                "Cannot delete a transport route that has active enrollments.");

        await _schoolTransportRepository.DeleteAsync(transport);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    [AbpAuthorize(PermissionNames.SASpecific_Transport_Manage)]
    public async Task<SchoolTransportDto> ActivateAsync(Guid id)
    {
        var transport = await _schoolTransportRepository
            .FirstOrDefaultAsync(st => st.Id == id && st.TenantId == AbpSession.TenantId);

        if (transport == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.SchoolTransportNotFound,
                "School transport route not found.");

        transport.Activate();
        await _schoolTransportRepository.UpdateAsync(transport);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.SASpecific_Transport_Manage)]
    public async Task<SchoolTransportDto> DeactivateAsync(Guid id)
    {
        var transport = await _schoolTransportRepository
            .FirstOrDefaultAsync(st => st.Id == id && st.TenantId == AbpSession.TenantId);

        if (transport == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.SchoolTransportNotFound,
                "School transport route not found.");

        transport.Deactivate();
        await _schoolTransportRepository.UpdateAsync(transport);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }
}
