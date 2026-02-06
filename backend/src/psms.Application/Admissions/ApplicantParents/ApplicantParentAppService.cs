using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Admissions.ApplicantParents.Dto;
using psms.Admissions.Shared;
using psms.Authorization;
using psms.Domain.Admissions.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Admissions.ApplicantParents;

/// <summary>
/// Service for managing parents/guardians on admission applications.
/// Implements ADM-003.
/// </summary>
[AbpAuthorize(PermissionNames.Admissions_Applications)]
public class ApplicantParentAppService : ApplicationService, IApplicantParentAppService
{
    private readonly IRepository<ApplicantParent, Guid> _parentRepository;
    private readonly IRepository<Application, Guid> _applicationRepository;

    private const int MaxParentsPerApplication = 4;

    public ApplicantParentAppService(
        IRepository<ApplicantParent, Guid> parentRepository,
        IRepository<Application, Guid> applicationRepository)
    {
        _parentRepository = parentRepository;
        _applicationRepository = applicationRepository;
    }

    [AbpAuthorize(PermissionNames.Admissions_Applications_View)]
    public async Task<ApplicantParentDto> GetAsync(Guid id)
    {
        var parent = await _parentRepository.GetAsync(id);
        return ObjectMapper.Map<ApplicantParentDto>(parent);
    }

    [AbpAuthorize(PermissionNames.Admissions_Applications_View)]
    public async Task<ListResultDto<ApplicantParentDto>> GetAllByApplicationAsync(Guid applicationId)
    {
        var parents = await _parentRepository
            .GetAll()
            .Where(p => p.ApplicationId == applicationId)
            .OrderByDescending(p => p.IsPrimaryContact)
            .ThenBy(p => p.FirstName)
            .ToListAsync();

        return new ListResultDto<ApplicantParentDto>(
            ObjectMapper.Map<List<ApplicantParentDto>>(parents));
    }

    [AbpAuthorize(PermissionNames.Admissions_Applications_Edit)]
    public async Task<ApplicantParentDto> CreateAsync(CreateApplicantParentDto input)
    {
        // Validate application exists and is editable
        var application = await _applicationRepository.GetAsync(input.ApplicationId);

        if (application.Status != ApplicationStatus.Draft)
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidStatusTransition,
                "Cannot add parents to a submitted application.");

        // Check parent count limit (ADM-003: maximum 4)
        var currentCount = await _parentRepository.CountAsync(p => p.ApplicationId == input.ApplicationId);
        if (currentCount >= MaxParentsPerApplication)
            throw new UserFriendlyException(AdmissionsExceptionCodes.MaxParentsExceeded,
                $"Maximum of {MaxParentsPerApplication} parents/guardians allowed per application.");

        var parent = ObjectMapper.Map<ApplicantParent>(input);
        parent.Id = Guid.NewGuid();

        // If this is the first parent, make them primary and financially responsible
        if (currentCount == 0)
        {
            parent.IsPrimaryContact = true;
            parent.IsFinanciallyResponsible = true;
        }

        await _parentRepository.InsertAsync(parent);
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ApplicantParentDto>(parent);
    }

    [AbpAuthorize(PermissionNames.Admissions_Applications_Edit)]
    public async Task<ApplicantParentDto> UpdateAsync(Guid id, UpdateApplicantParentDto input)
    {
        var parent = await _parentRepository.GetAsync(id);

        // Validate application is editable
        var application = await _applicationRepository.GetAsync(parent.ApplicationId);
        if (application.Status != ApplicationStatus.Draft)
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidStatusTransition,
                "Cannot modify parents on a submitted application.");

        // Update fields
        if (input.Relationship.HasValue)
            parent.Relationship = input.Relationship.Value;

        if (!string.IsNullOrWhiteSpace(input.FirstName))
            parent.FirstName = input.FirstName;

        if (!string.IsNullOrWhiteSpace(input.LastName))
            parent.LastName = input.LastName;

        if (input.IdNumber != null)
            parent.IdNumber = input.IdNumber;

        if (!string.IsNullOrWhiteSpace(input.Email))
            parent.Email = input.Email;

        if (!string.IsNullOrWhiteSpace(input.PhoneNumber))
            parent.PhoneNumber = input.PhoneNumber;

        if (input.AlternatePhone != null)
            parent.AlternatePhone = input.AlternatePhone;

        if (input.StreetAddress != null)
            parent.StreetAddress = input.StreetAddress;

        if (input.Suburb != null)
            parent.Suburb = input.Suburb;

        if (input.City != null)
            parent.City = input.City;

        if (input.Province != null)
            parent.Province = input.Province;

        if (input.PostalCode != null)
            parent.PostalCode = input.PostalCode;

        if (input.Occupation != null)
            parent.Occupation = input.Occupation;

        if (input.Employer != null)
            parent.Employer = input.Employer;

        if (input.IsPrimaryContact.HasValue)
        {
            if (input.IsPrimaryContact.Value)
                await SetAsPrimaryContactInternalAsync(parent);
            else
                parent.IsPrimaryContact = false;
        }

        if (input.IsFinanciallyResponsible.HasValue)
        {
            if (input.IsFinanciallyResponsible.Value)
                await SetAsFinanciallyResponsibleInternalAsync(parent);
            else
                parent.IsFinanciallyResponsible = false;
        }

        await _parentRepository.UpdateAsync(parent);
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ApplicantParentDto>(parent);
    }

    [AbpAuthorize(PermissionNames.Admissions_Applications_Edit)]
    public async Task DeleteAsync(Guid id)
    {
        var parent = await _parentRepository.GetAsync(id);

        // Validate application is editable
        var application = await _applicationRepository.GetAsync(parent.ApplicationId);
        if (application.Status != ApplicationStatus.Draft)
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidStatusTransition,
                "Cannot remove parents from a submitted application.");

        // Check if this is the only parent
        var parentCount = await _parentRepository.CountAsync(p => p.ApplicationId == parent.ApplicationId);
        if (parentCount == 1)
            throw new UserFriendlyException(AdmissionsExceptionCodes.ParentInformationRequired,
                "Cannot remove the only parent. At least one parent is required.");

        var wasPrimary = parent.IsPrimaryContact;
        var wasFinanciallyResponsible = parent.IsFinanciallyResponsible;

        await _parentRepository.DeleteAsync(parent);
        await CurrentUnitOfWork.SaveChangesAsync();

        // Re-assign primary contact and financially responsible if needed
        if (wasPrimary || wasFinanciallyResponsible)
        {
            var remainingParents = await _parentRepository
                .GetAll()
                .Where(p => p.ApplicationId == parent.ApplicationId)
                .OrderBy(p => p.Id)
                .ToListAsync();

            if (remainingParents.Any())
            {
                if (wasPrimary && !remainingParents.Any(p => p.IsPrimaryContact))
                    remainingParents.First().IsPrimaryContact = true;

                if (wasFinanciallyResponsible && !remainingParents.Any(p => p.IsFinanciallyResponsible))
                    remainingParents.First().IsFinanciallyResponsible = true;

                await CurrentUnitOfWork.SaveChangesAsync();
            }
        }
    }

    [AbpAuthorize(PermissionNames.Admissions_Applications_Edit)]
    public async Task SetAsPrimaryContactAsync(Guid id)
    {
        var parent = await _parentRepository.GetAsync(id);
        await SetAsPrimaryContactInternalAsync(parent);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    [AbpAuthorize(PermissionNames.Admissions_Applications_Edit)]
    public async Task SetAsFinanciallyResponsibleAsync(Guid id)
    {
        var parent = await _parentRepository.GetAsync(id);
        await SetAsFinanciallyResponsibleInternalAsync(parent);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    #region Private Methods

    private async Task SetAsPrimaryContactInternalAsync(ApplicantParent parent)
    {
        // Remove primary flag from other parents
        var otherParents = await _parentRepository
            .GetAll()
            .Where(p => p.ApplicationId == parent.ApplicationId && p.Id != parent.Id && p.IsPrimaryContact)
            .ToListAsync();

        foreach (var other in otherParents)
        {
            other.IsPrimaryContact = false;
        }

        parent.IsPrimaryContact = true;
    }

    private async Task SetAsFinanciallyResponsibleInternalAsync(ApplicantParent parent)
    {
        // Note: Multiple parents can be financially responsible, so we don't clear others
        parent.IsFinanciallyResponsible = true;
    }

    #endregion
}
