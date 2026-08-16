using Microsoft.EntityFrameworkCore;
using psms.Domain.Tenancy.Entities;
using System;
using System.Linq;

namespace psms.EntityFrameworkCore.Seed.Tenants;

/// <summary>
/// Seeds a tenant's default branding row (issue #56) so a freshly seeded
/// database has editable branding rather than a missing row. Idempotent.
/// </summary>
public class DefaultBrandingCreator
{
    private readonly psmsDbContext _context;
    private readonly int _tenantId;

    public DefaultBrandingCreator(psmsDbContext context, int tenantId)
    {
        _context = context;
        _tenantId = tenantId;
    }

    public void Create()
    {
        var existing = _context.SchoolBrandings
            .IgnoreQueryFilters()
            .FirstOrDefault(b => b.TenantId == _tenantId && !b.IsDeleted);

        if (existing != null)
        {
            return;
        }

        _context.SchoolBrandings.Add(new SchoolBranding(Guid.NewGuid(), _tenantId));
        _context.SaveChanges();
    }
}
