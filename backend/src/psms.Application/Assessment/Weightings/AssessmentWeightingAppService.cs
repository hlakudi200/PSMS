using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Assessment.Shared;
using psms.Assessment.Weightings.Dto;
using psms.Authorization;
using psms.Domain.Assessment;
using psms.Domain.Assessment.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Assessment.Weightings;

/// <summary>
/// A school's School-Based Assessment to examination split, per grade band.
/// <para>
/// Seeded from DBE Circular S8 of 2023 and editable, because an independent
/// school may run an approved variation. Reading is open to any authenticated
/// user — report screens need it to explain a mark — while editing sits behind
/// the same settings permission as branding.
/// </para>
/// </summary>
[AbpAuthorize]
public class AssessmentWeightingAppService : ApplicationService, IAssessmentWeightingAppService
{
    private static readonly IReadOnlyDictionary<AssessmentWeightingBand, string> BandNames =
        new Dictionary<AssessmentWeightingBand, string>
        {
            [AssessmentWeightingBand.Foundation] = "Foundation Phase (Grade R-3)",
            [AssessmentWeightingBand.Intermediate] = "Intermediate Phase (Grade 4-6)",
            [AssessmentWeightingBand.Senior] = "Senior Phase (Grade 7-9)",
            [AssessmentWeightingBand.Grade10And11] = "Grade 10 and 11",
            [AssessmentWeightingBand.Grade12] = "Grade 12"
        };

    private readonly IRepository<AssessmentWeighting, Guid> _repository;

    public AssessmentWeightingAppService(IRepository<AssessmentWeighting, Guid> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// The school's table, in band order. Missing bands are created at the
    /// national default on the way out, so a tenant seeded before this feature
    /// existed still gets a complete table without anyone running a seeder.
    /// </summary>
    public async Task<ListResultDto<AssessmentWeightingDto>> GetAllAsync()
    {
        var rows = await EnsureAllBandsAsync();
        return Map(rows);
    }

    /// <summary>
    /// Saves the table. Each band's two halves must total 100 — a split that
    /// does not silently under- or over-states every learner's final mark, so
    /// it is refused rather than normalised.
    /// </summary>
    [AbpAuthorize(PermissionNames.Administration_Settings_Edit)]
    public async Task<ListResultDto<AssessmentWeightingDto>> UpdateAsync(UpdateAssessmentWeightingsDto input)
    {
        if (input?.Weightings == null || input.Weightings.Count == 0)
            throw new UserFriendlyException(AssessmentExceptionCodes.InvalidAssessmentWeighting,
                "No weightings were supplied.");

        var duplicate = input.Weightings
            .GroupBy(w => w.Band)
            .FirstOrDefault(g => g.Count() > 1);

        if (duplicate != null)
            throw new UserFriendlyException(AssessmentExceptionCodes.InvalidAssessmentWeighting,
                $"{BandNameFor(duplicate.Key)} was supplied more than once.");

        foreach (var item in input.Weightings)
        {
            if (!AssessmentWeighting.IsValidSplit(item.SbaPercentage, item.ExamPercentage))
            {
                throw new UserFriendlyException(AssessmentExceptionCodes.InvalidAssessmentWeighting,
                    $"{BandNameFor(item.Band)}: school-based assessment and examination must total 100%, "
                    + $"but {item.SbaPercentage}% + {item.ExamPercentage}% is {item.SbaPercentage + item.ExamPercentage}%.");
            }
        }

        var rows = await EnsureAllBandsAsync();

        foreach (var item in input.Weightings)
        {
            var row = rows.FirstOrDefault(r => r.Band == item.Band);
            if (row == null) continue;

            row.SetSplit(item.SbaPercentage, item.ExamPercentage);
            await _repository.UpdateAsync(row);
        }

        await CurrentUnitOfWork.SaveChangesAsync();
        return Map(rows);
    }

    /// <summary>Returns every band to the DBE Circular S8 of 2023 value.</summary>
    [AbpAuthorize(PermissionNames.Administration_Settings_Edit)]
    public async Task<ListResultDto<AssessmentWeightingDto>> ResetToDefaultsAsync()
    {
        var rows = await EnsureAllBandsAsync();

        foreach (var row in rows)
        {
            row.ResetToPolicyDefault();
            await _repository.UpdateAsync(row);
        }

        await CurrentUnitOfWork.SaveChangesAsync();
        return Map(rows);
    }

    private async Task<List<AssessmentWeighting>> EnsureAllBandsAsync()
    {
        var rows = await _repository.GetAll().ToListAsync();

        var missing = Enum.GetValues<AssessmentWeightingBand>()
            .Where(band => rows.All(r => r.Band != band))
            .ToList();

        if (missing.Count > 0)
        {
            if (!AbpSession.TenantId.HasValue)
                throw new UserFriendlyException(AssessmentExceptionCodes.InvalidAssessmentWeighting,
                    "Assessment weightings belong to a school. Sign in to a school to view them.");

            foreach (var band in missing)
            {
                var row = new AssessmentWeighting(Guid.NewGuid(), AbpSession.TenantId.Value, band);
                await _repository.InsertAsync(row);
                rows.Add(row);
            }

            await CurrentUnitOfWork.SaveChangesAsync();
        }

        return rows.OrderBy(r => r.Band).ToList();
    }

    private static string BandNameFor(AssessmentWeightingBand band) =>
        BandNames.TryGetValue(band, out var name) ? name : band.ToString();

    private static ListResultDto<AssessmentWeightingDto> Map(IEnumerable<AssessmentWeighting> rows) =>
        new ListResultDto<AssessmentWeightingDto>(rows.Select(r => new AssessmentWeightingDto
        {
            Band = r.Band,
            BandName = BandNameFor(r.Band),
            SbaPercentage = r.SbaPercentage,
            ExamPercentage = r.ExamPercentage,
            PolicySbaPercentage = AssessmentWeightingDefaults.SbaFor(r.Band),
            PolicyExamPercentage = AssessmentWeightingDefaults.ExamFor(r.Band),
            MatchesPolicyDefault = r.MatchesPolicyDefault()
        }).ToList());
}
