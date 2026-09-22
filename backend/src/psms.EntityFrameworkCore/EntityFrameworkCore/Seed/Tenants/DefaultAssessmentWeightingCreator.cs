using Microsoft.EntityFrameworkCore;
using psms.Domain.Assessment;
using psms.Domain.Assessment.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Linq;

namespace psms.EntityFrameworkCore.Seed.Tenants;

/// <summary>
/// Seeds a school's SBA / examination split at the national defaults from DBE
/// Circular S8 of 2023, so a fresh tenant can generate year-end marks without
/// anyone configuring anything first. Idempotent, and adds only the bands that
/// are missing — a school that has retuned a band keeps its value.
/// </summary>
public class DefaultAssessmentWeightingCreator
{
    private readonly psmsDbContext _context;
    private readonly int _tenantId;

    public DefaultAssessmentWeightingCreator(psmsDbContext context, int tenantId)
    {
        _context = context;
        _tenantId = tenantId;
    }

    public void Create()
    {
        var existing = _context.AssessmentWeightings
            .IgnoreQueryFilters()
            .Where(w => w.TenantId == _tenantId && !w.IsDeleted)
            .Select(w => w.Band)
            .ToList();

        var missing = Enum.GetValues(typeof(AssessmentWeightingBand))
            .Cast<AssessmentWeightingBand>()
            .Where(band => !existing.Contains(band))
            .ToList();

        if (missing.Count == 0)
        {
            return;
        }

        foreach (var band in missing)
        {
            _context.AssessmentWeightings.Add(
                new AssessmentWeighting(Guid.NewGuid(), _tenantId, band));
        }

        _context.SaveChanges();
    }
}
