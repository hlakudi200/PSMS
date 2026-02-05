using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Academic.AcademicYears.Dto;
using psms.Academic.Shared;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Threading.Tasks;

namespace psms.Academic.AcademicYears;

/// <summary>
/// Service for managing academic years. Implements AR-001, AR-002, AR-003.
/// </summary>
[AbpAuthorize(PermissionNames.Academic_Calendar)]
public class AcademicYearAppService : ApplicationService, IAcademicYearAppService
{
    private readonly IRepository<AcademicYear, Guid> _academicYearRepository;
    private readonly IRepository<Term, Guid> _termRepository;

    public AcademicYearAppService(
        IRepository<AcademicYear, Guid> academicYearRepository,
        IRepository<Term, Guid> termRepository)
    {
        _academicYearRepository = academicYearRepository;
        _termRepository = termRepository;
    }

    [AbpAuthorize(PermissionNames.Academic_Calendar_View)]
    public async Task<AcademicYearDto> GetAsync(Guid id)
    {
        var academicYear = await _academicYearRepository
            .GetAll()
            .Include(ay => ay.Terms.OrderBy(t => t.TermNumber))
            .Include(ay => ay.Classes)
            .FirstOrDefaultAsync(ay => ay.Id == id);

        if (academicYear == null)
            throw new UserFriendlyException(AcademicExceptionCodes.AcademicYearNotFound, "Academic year not found.");

        return MapToDto(academicYear);
    }

    [AbpAuthorize(PermissionNames.Academic_Calendar_View)]
    public async Task<PagedResultDto<AcademicYearListDto>> GetAllAsync(PagedAndSortedResultRequestDto input)
    {
        var query = _academicYearRepository
            .GetAll()
            .Include(ay => ay.Terms)
            .Include(ay => ay.Classes);

        var totalCount = await query.CountAsync();

        var years = await query
            .OrderBy(input.Sorting ?? "Year DESC")
            .PageBy(input)
            .ToListAsync();

        var dtos = years.Select(ay => new AcademicYearListDto
        {
            Id = ay.Id,
            Year = ay.Year,
            YearName = ay.YearName,
            StartDate = ay.StartDate,
            EndDate = ay.EndDate,
            IsCurrent = ay.IsCurrent,
            TermCount = ay.Terms?.Count ?? 0,
            ClassCount = ay.Classes?.Count ?? 0
        }).ToList();

        return new PagedResultDto<AcademicYearListDto>(totalCount, dtos);
    }

    [AbpAuthorize(PermissionNames.Academic_AcademicYears_Manage)]
    public async Task<AcademicYearDto> CreateAsync(CreateAcademicYearDto input)
    {
        // AR-002: Validate SA academic year dates
        ValidateSADates(input.StartDate, input.EndDate);

        // Check for duplicate year
        var existingYear = await _academicYearRepository
            .FirstOrDefaultAsync(ay => ay.Year == input.Year);

        if (existingYear != null)
            throw new UserFriendlyException(AcademicExceptionCodes.DuplicateAcademicYear,
                $"Academic year {input.Year} already exists.");

        var academicYear = new AcademicYear(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.Year,
            input.StartDate,
            input.EndDate);

        await _academicYearRepository.InsertAsync(academicYear);
        await CurrentUnitOfWork.SaveChangesAsync();

        // AR-003: Create default 4 SA terms if requested
        if (input.CreateDefaultTerms)
        {
            await CreateDefaultSATermsAsync(academicYear);
        }

        return await GetAsync(academicYear.Id);
    }

    [AbpAuthorize(PermissionNames.Academic_AcademicYears_Manage)]
    public async Task<AcademicYearDto> UpdateAsync(Guid id, UpdateAcademicYearDto input)
    {
        var academicYear = await _academicYearRepository.GetAsync(id);

        var startDate = input.StartDate ?? academicYear.StartDate;
        var endDate = input.EndDate ?? academicYear.EndDate;

        // AR-002: Validate dates if changed
        if (input.StartDate.HasValue || input.EndDate.HasValue)
            ValidateSADates(startDate, endDate);

        if (input.YearName != null) academicYear.YearName = input.YearName;
        if (input.StartDate.HasValue) academicYear.StartDate = input.StartDate.Value;
        if (input.EndDate.HasValue) academicYear.EndDate = input.EndDate.Value;

        await _academicYearRepository.UpdateAsync(academicYear);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Academic_AcademicYears_Manage)]
    public async Task DeleteAsync(Guid id)
    {
        var academicYear = await _academicYearRepository
            .GetAll()
            .Include(ay => ay.Classes)
            .FirstOrDefaultAsync(ay => ay.Id == id);

        if (academicYear == null)
            throw new UserFriendlyException(AcademicExceptionCodes.AcademicYearNotFound, "Academic year not found.");

        if (academicYear.IsCurrent)
            throw new UserFriendlyException(AcademicExceptionCodes.CannotDeleteCurrentAcademicYear,
                "Cannot delete the current academic year.");

        if (academicYear.Classes.Any(c => !c.IsDeleted))
            throw new UserFriendlyException(AcademicExceptionCodes.CannotDeleteAcademicYearWithClasses,
                "Cannot delete an academic year that has classes assigned.");

        // Delete associated terms first
        var terms = await _termRepository
            .GetAll()
            .Where(t => t.AcademicYearId == id)
            .ToListAsync();

        foreach (var term in terms)
            await _termRepository.DeleteAsync(term);

        await _academicYearRepository.DeleteAsync(academicYear);
    }

    [AbpAuthorize(PermissionNames.Academic_Calendar_View)]
    public async Task<AcademicYearDto> GetCurrentAsync()
    {
        var academicYear = await _academicYearRepository
            .GetAll()
            .Include(ay => ay.Terms.OrderBy(t => t.TermNumber))
            .Include(ay => ay.Classes)
            .FirstOrDefaultAsync(ay => ay.IsCurrent);

        if (academicYear == null)
            throw new UserFriendlyException(AcademicExceptionCodes.AcademicYearNotFound,
                "No current academic year is set. Please set an academic year as current.");

        return MapToDto(academicYear);
    }

    [AbpAuthorize(PermissionNames.Academic_AcademicYears_Manage)]
    public async Task<AcademicYearDto> SetAsCurrentAsync(Guid id)
    {
        var academicYear = await _academicYearRepository.GetAsync(id);

        // AR-001: Only one current academic year per tenant
        var currentYear = await _academicYearRepository
            .FirstOrDefaultAsync(ay => ay.IsCurrent && ay.Id != id);

        if (currentYear != null)
        {
            currentYear.IsCurrent = false;
            await _academicYearRepository.UpdateAsync(currentYear);
        }

        academicYear.IsCurrent = true;
        await _academicYearRepository.UpdateAsync(academicYear);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    /// <summary>
    /// AR-002: Validates SA academic year dates (January-December, ~12 months).
    /// </summary>
    private static void ValidateSADates(DateTime startDate, DateTime endDate)
    {
        if (startDate.Month < 1 || startDate.Month > 2)
            throw new UserFriendlyException(AcademicExceptionCodes.InvalidAcademicYearStartMonth,
                "South African academic year must start in January or early February.");

        if (endDate.Month != 12)
            throw new UserFriendlyException(AcademicExceptionCodes.InvalidAcademicYearEndMonth,
                "South African academic year must end in December.");

        var duration = (endDate - startDate).Days;
        if (duration < 300 || duration > 400)
            throw new UserFriendlyException(AcademicExceptionCodes.InvalidAcademicYearDuration,
                "Academic year must be approximately 12 months long (300-400 days).");

        if (startDate >= endDate)
            throw new UserFriendlyException(AcademicExceptionCodes.InvalidAcademicYearDates,
                "Start date must be before end date.");
    }

    /// <summary>
    /// AR-003: Creates the default SA 4-term structure.
    /// </summary>
    private async Task CreateDefaultSATermsAsync(AcademicYear academicYear)
    {
        var year = academicYear.Year;
        var tenantId = academicYear.TenantId;

        var terms = new[]
        {
            new Term(Guid.NewGuid(), tenantId, academicYear.Id, SouthAfricanTermNumber.Term1,
                new DateTime(year, 1, 15), new DateTime(year, 3, 22)),
            new Term(Guid.NewGuid(), tenantId, academicYear.Id, SouthAfricanTermNumber.Term2,
                new DateTime(year, 4, 10), new DateTime(year, 6, 23)),
            new Term(Guid.NewGuid(), tenantId, academicYear.Id, SouthAfricanTermNumber.Term3,
                new DateTime(year, 7, 17), new DateTime(year, 9, 22)),
            new Term(Guid.NewGuid(), tenantId, academicYear.Id, SouthAfricanTermNumber.Term4,
                new DateTime(year, 10, 9), new DateTime(year, 12, 7))
        };

        foreach (var term in terms)
            await _termRepository.InsertAsync(term);

        await CurrentUnitOfWork.SaveChangesAsync();
    }

    private AcademicYearDto MapToDto(AcademicYear academicYear)
    {
        var dto = ObjectMapper.Map<AcademicYearDto>(academicYear);
        dto.Terms = academicYear.Terms?
            .OrderBy(t => t.TermNumber)
            .Select(t => new AcademicYearTermDto
            {
                Id = t.Id,
                TermNumber = (int)t.TermNumber,
                TermName = t.TermName,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                IsCurrent = t.IsCurrent
            }).ToList() ?? new List<AcademicYearTermDto>();
        return dto;
    }
}
