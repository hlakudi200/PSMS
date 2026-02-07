using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Academic.Parents.Dto;
using psms.Academic.Shared;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using psms.Domain.Shared.Validators;
using psms.Domain.Shared.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.Academic.Parents;

/// <summary>
/// Service for managing parents/guardians.
/// </summary>
[AbpAuthorize(PermissionNames.Academic_Parents)]
public class ParentAppService : ApplicationService, IParentAppService
{
    private readonly IRepository<Parent, Guid> _parentRepository;

    public ParentAppService(IRepository<Parent, Guid> parentRepository)
    {
        _parentRepository = parentRepository;
    }

    [AbpAuthorize(PermissionNames.Academic_Parents_View)]
    public async Task<ParentDto> GetAsync(Guid id)
    {
        var parent = await _parentRepository
            .GetAll()
            .Include(p => p.StudentLinks)
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == AbpSession.TenantId);

        if (parent == null)
            throw new UserFriendlyException(AcademicExceptionCodes.ParentNotFound, "Parent not found.");

        return ObjectMapper.Map<ParentDto>(parent);
    }

    [AbpAuthorize(PermissionNames.Academic_Parents_View)]
    public async Task<PagedResultDto<ParentListDto>> GetAllAsync(PagedAndSortedResultRequestDto input)
    {
        var query = _parentRepository
            .GetAll()
            .Include(p => p.StudentLinks);

        var totalCount = await query.CountAsync();

        var parents = await query
            .OrderBy(input.Sorting ?? "LastName ASC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<ParentListDto>(
            totalCount,
            ObjectMapper.Map<List<ParentListDto>>(parents));
    }

    [AbpAuthorize(PermissionNames.Academic_Parents_Create)]
    public async Task<ParentDto> CreateAsync(CreateParentDto input)
    {
        // Validate unique email (case-insensitive)
        var normalizedEmail = input.Email.Trim().ToLowerInvariant();
        var existingByEmail = await _parentRepository
            .FirstOrDefaultAsync(p => p.Email.ToLower() == normalizedEmail);

        if (existingByEmail != null)
            throw new UserFriendlyException(AcademicExceptionCodes.DuplicateParentEmail,
                $"A parent with email '{input.Email}' already exists.");

        // ER-002: Validate SA ID number if provided
        if (!string.IsNullOrWhiteSpace(input.IdNumber) && !SAIdNumberValidator.IsValid(input.IdNumber))
            throw new UserFriendlyException(AcademicExceptionCodes.InvalidSAIdNumber,
                "Invalid South African ID number. Must be 13 digits and pass Luhn validation.");

        var parent = new Parent(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.UserId,
            input.FirstName,
            input.LastName,
            normalizedEmail,
            input.Phone)
        {
            IdNumber = input.IdNumber,
            Occupation = input.Occupation,
            Employer = input.Employer,
            WorkPhone = input.WorkPhone
        };

        if (input.Address != null)
        {
            parent.Address = new Address(
                input.Address.StreetAddress,
                input.Address.Suburb,
                input.Address.City,
                input.Address.Province,
                input.Address.PostalCode,
                input.Address.Country);
        }

        await _parentRepository.InsertAsync(parent);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(parent.Id);
    }

    [AbpAuthorize(PermissionNames.Academic_Parents_Edit)]
    public async Task<ParentDto> UpdateAsync(Guid id, UpdateParentDto input)
    {
        var parent = await _parentRepository.GetAsync(id);

        // Validate unique email if changing (case-insensitive)
        if (input.Email != null)
        {
            var normalizedEmail = input.Email.Trim().ToLowerInvariant();
            if (normalizedEmail != parent.Email.ToLowerInvariant())
            {
                var existingByEmail = await _parentRepository
                    .FirstOrDefaultAsync(p => p.Email.ToLower() == normalizedEmail && p.Id != id);

                if (existingByEmail != null)
                    throw new UserFriendlyException(AcademicExceptionCodes.DuplicateParentEmail,
                        $"A parent with email '{input.Email}' already exists.");
            }

            parent.Email = normalizedEmail;
        }

        // ER-002: Validate SA ID number if changing
        if (input.IdNumber != null && !string.IsNullOrWhiteSpace(input.IdNumber)
            && !SAIdNumberValidator.IsValid(input.IdNumber))
            throw new UserFriendlyException(AcademicExceptionCodes.InvalidSAIdNumber,
                "Invalid South African ID number. Must be 13 digits and pass Luhn validation.");

        if (input.FirstName != null) parent.FirstName = input.FirstName;
        if (input.LastName != null) parent.LastName = input.LastName;
        if (input.Phone != null) parent.Phone = input.Phone;
        if (input.IdNumber != null) parent.IdNumber = input.IdNumber;
        if (input.Occupation != null) parent.Occupation = input.Occupation;
        if (input.Employer != null) parent.Employer = input.Employer;
        if (input.WorkPhone != null) parent.WorkPhone = input.WorkPhone;

        if (input.Address != null)
        {
            parent.Address = new Address(
                input.Address.StreetAddress,
                input.Address.Suburb,
                input.Address.City,
                input.Address.Province,
                input.Address.PostalCode,
                input.Address.Country);
        }

        await _parentRepository.UpdateAsync(parent);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Academic_Parents_Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var parent = await _parentRepository
            .GetAll()
            .Include(p => p.StudentLinks)
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == AbpSession.TenantId);

        if (parent == null)
            throw new UserFriendlyException(AcademicExceptionCodes.ParentNotFound, "Parent not found.");

        if (parent.StudentLinks.Any())
            throw new UserFriendlyException(AcademicExceptionCodes.CannotDeleteParentWithStudents,
                "Cannot delete a parent that has linked students. Remove student links first.");

        await _parentRepository.DeleteAsync(parent);
    }

    [AbpAuthorize(PermissionNames.Academic_Parents_View)]
    public async Task<ListResultDto<ParentListDto>> SearchAsync(string searchTerm)
    {
        IQueryable<Parent> query = _parentRepository
            .GetAll()
            .Include(p => p.StudentLinks);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(p =>
                p.FirstName.ToLower().Contains(term) ||
                p.LastName.ToLower().Contains(term) ||
                p.Email.ToLower().Contains(term));
        }

        var parents = await query
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .Take(50)
            .ToListAsync();

        return new ListResultDto<ParentListDto>(
            ObjectMapper.Map<List<ParentListDto>>(parents));
    }
}
