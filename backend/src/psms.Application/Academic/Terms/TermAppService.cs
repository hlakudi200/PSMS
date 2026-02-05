using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Academic.Shared;
using psms.Academic.Terms.Dto;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Academic.Terms;

/// <summary>
/// Service for managing terms. Implements AR-003, AR-004.
/// </summary>
[AbpAuthorize(PermissionNames.Academic_Calendar)]
public class TermAppService : ApplicationService, ITermAppService
{
    private readonly IRepository<Term, Guid> _termRepository;
    private readonly IRepository<AcademicYear, Guid> _academicYearRepository;

    public TermAppService(
        IRepository<Term, Guid> termRepository,
        IRepository<AcademicYear, Guid> academicYearRepository)
    {
        _termRepository = termRepository;
        _academicYearRepository = academicYearRepository;
    }

    [AbpAuthorize(PermissionNames.Academic_Calendar_View)]
    public async Task<TermDto> GetAsync(Guid id)
    {
        var term = await _termRepository
            .GetAll()
            .Include(t => t.AcademicYear)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (term == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TermNotFound, "Term not found.");

        return ObjectMapper.Map<TermDto>(term);
    }

    [AbpAuthorize(PermissionNames.Academic_Calendar_View)]
    public async Task<ListResultDto<TermListDto>> GetByAcademicYearAsync(Guid academicYearId)
    {
        var terms = await _termRepository
            .GetAll()
            .Include(t => t.AcademicYear)
            .Where(t => t.AcademicYearId == academicYearId)
            .OrderBy(t => t.TermNumber)
            .ToListAsync();

        return new ListResultDto<TermListDto>(
            ObjectMapper.Map<List<TermListDto>>(terms));
    }

    [AbpAuthorize(PermissionNames.Academic_Terms_Manage)]
    public async Task<TermDto> CreateAsync(CreateTermDto input)
    {
        // Validate academic year exists
        var academicYear = await _academicYearRepository.GetAsync(input.AcademicYearId);

        // Validate dates
        if (input.StartDate >= input.EndDate)
            throw new UserFriendlyException(AcademicExceptionCodes.TermOutsideAcademicYear,
                "Term start date must be before end date.");

        if (input.StartDate < academicYear.StartDate || input.EndDate > academicYear.EndDate)
            throw new UserFriendlyException(AcademicExceptionCodes.TermOutsideAcademicYear,
                "Term dates must fall within the academic year period.");

        // Check for duplicate term number in this academic year
        var existingTerm = await _termRepository
            .FirstOrDefaultAsync(t => t.AcademicYearId == input.AcademicYearId
                && t.TermNumber == input.TermNumber);

        if (existingTerm != null)
            throw new UserFriendlyException(AcademicExceptionCodes.DuplicateTermNumber,
                $"Term {(int)input.TermNumber} already exists for this academic year.");

        // AR-003: Check for overlapping terms
        await ValidateNoOverlappingTermsAsync(input.AcademicYearId, input.StartDate, input.EndDate, null);

        // AR-003: Check max 4 terms
        var termCount = await _termRepository
            .CountAsync(t => t.AcademicYearId == input.AcademicYearId);

        if (termCount >= 4)
            throw new UserFriendlyException(AcademicExceptionCodes.InvalidTermCount,
                "South African academic year must have exactly 4 terms. Maximum reached.");

        var term = new Term(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.AcademicYearId,
            input.TermNumber,
            input.StartDate,
            input.EndDate);

        if (input.TermName != null)
            term.TermName = input.TermName;

        await _termRepository.InsertAsync(term);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(term.Id);
    }

    [AbpAuthorize(PermissionNames.Academic_Terms_Manage)]
    public async Task<TermDto> UpdateAsync(Guid id, UpdateTermDto input)
    {
        var term = await _termRepository
            .GetAll()
            .Include(t => t.AcademicYear)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (term == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TermNotFound, "Term not found.");

        var startDate = input.StartDate ?? term.StartDate;
        var endDate = input.EndDate ?? term.EndDate;

        if (input.StartDate.HasValue || input.EndDate.HasValue)
        {
            if (startDate >= endDate)
                throw new UserFriendlyException(AcademicExceptionCodes.TermOutsideAcademicYear,
                    "Term start date must be before end date.");

            if (startDate < term.AcademicYear.StartDate || endDate > term.AcademicYear.EndDate)
                throw new UserFriendlyException(AcademicExceptionCodes.TermOutsideAcademicYear,
                    "Term dates must fall within the academic year period.");

            await ValidateNoOverlappingTermsAsync(term.AcademicYearId, startDate, endDate, id);
        }

        if (input.TermName != null) term.TermName = input.TermName;
        if (input.StartDate.HasValue) term.StartDate = input.StartDate.Value;
        if (input.EndDate.HasValue) term.EndDate = input.EndDate.Value;

        await _termRepository.UpdateAsync(term);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Academic_Terms_Manage)]
    public async Task DeleteAsync(Guid id)
    {
        var term = await _termRepository.GetAsync(id);

        if (term.IsCurrent)
            throw new UserFriendlyException(AcademicExceptionCodes.CannotDeleteCurrentTerm,
                "Cannot delete the current term.");

        await _termRepository.DeleteAsync(term);
    }

    [AbpAuthorize(PermissionNames.Academic_Calendar_View)]
    public async Task<TermDto> GetCurrentAsync()
    {
        var term = await _termRepository
            .GetAll()
            .Include(t => t.AcademicYear)
            .FirstOrDefaultAsync(t => t.IsCurrent && t.AcademicYear.IsCurrent);

        if (term == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TermNotFound,
                "No current term is set. Please set a term as current.");

        return ObjectMapper.Map<TermDto>(term);
    }

    [AbpAuthorize(PermissionNames.Academic_Terms_Manage)]
    public async Task<TermDto> SetAsCurrentAsync(Guid id)
    {
        var term = await _termRepository
            .GetAll()
            .Include(t => t.AcademicYear)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (term == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TermNotFound, "Term not found.");

        // AR-004: Can only set term as current if academic year is current
        if (!term.AcademicYear.IsCurrent)
            throw new UserFriendlyException(AcademicExceptionCodes.AcademicYearNotCurrent,
                "Cannot set term as current when its academic year is not current.");

        // AR-004: Unset the current term first
        var currentTerm = await _termRepository
            .FirstOrDefaultAsync(t => t.AcademicYearId == term.AcademicYearId && t.IsCurrent && t.Id != id);

        if (currentTerm != null)
        {
            currentTerm.IsCurrent = false;
            await _termRepository.UpdateAsync(currentTerm);
        }

        term.IsCurrent = true;
        await _termRepository.UpdateAsync(term);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    /// <summary>
    /// AR-003: Validates that terms don't overlap within an academic year.
    /// </summary>
    private async Task ValidateNoOverlappingTermsAsync(
        Guid academicYearId, DateTime startDate, DateTime endDate, Guid? excludeTermId)
    {
        var overlapping = await _termRepository
            .GetAll()
            .Where(t => t.AcademicYearId == academicYearId)
            .WhereIf(excludeTermId.HasValue, t => t.Id != excludeTermId.Value)
            .AnyAsync(t => t.StartDate < endDate && t.EndDate > startDate);

        if (overlapping)
            throw new UserFriendlyException(AcademicExceptionCodes.OverlappingTerms,
                "Term dates overlap with an existing term.");
    }
}
