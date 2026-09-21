using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Runtime.Session;
using Microsoft.EntityFrameworkCore;
using psms.Domain.Academic.Entities;

namespace psms.Academic.Parents;

/// <summary>
/// Resolves the Parent.Id linked to the current user (Parent.UserId ==
/// AbpSession.UserId), and the Student.Id set of their linked children via
/// StudentParent, or null for staff / non-parent users. Parent-facing read
/// endpoints use this to scope results to the caller's own children so a
/// logged-in parent can't read another family's data via an arbitrary id
/// (mirrors <see cref="psms.Academic.Students.ICurrentStudentResolver"/>).
/// Memoised on the ambient unit of work (per request).
/// </summary>
public interface ICurrentParentResolver : ITransientDependency
{
    Task<Guid?> GetCurrentParentIdAsync();

    /// <summary>Null when the caller isn't a parent (don't scope). Otherwise
    /// the caller's linked children's Student.Id set, which may be empty.</summary>
    Task<List<Guid>?> GetCurrentChildStudentIdsAsync();
}

public class CurrentParentResolver : ICurrentParentResolver
{
    private readonly IRepository<Parent, Guid> _parentRepository;
    private readonly IRepository<StudentParent, Guid> _studentParentRepository;
    private readonly IAbpSession _abpSession;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public CurrentParentResolver(
        IRepository<Parent, Guid> parentRepository,
        IRepository<StudentParent, Guid> studentParentRepository,
        IAbpSession abpSession,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _parentRepository = parentRepository;
        _studentParentRepository = studentParentRepository;
        _abpSession = abpSession;
        _unitOfWorkManager = unitOfWorkManager;
    }

    public async Task<Guid?> GetCurrentParentIdAsync()
    {
        if (_abpSession.UserId == null) return null;

        var cacheKey = $"CurrentParentResolver.ParentIdForUser:{_abpSession.UserId.Value}";
        var uow = _unitOfWorkManager.Current;
        if (uow != null && uow.Items.TryGetValue(cacheKey, out var cached))
            return cached as Guid?;

        var parentId = await _parentRepository
            .GetAll()
            .Where(p => p.UserId == _abpSession.UserId.Value && p.TenantId == _abpSession.TenantId)
            .Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync();

        if (uow != null) uow.Items[cacheKey] = parentId;
        return parentId;
    }

    public async Task<List<Guid>?> GetCurrentChildStudentIdsAsync()
    {
        var parentId = await GetCurrentParentIdAsync();
        if (parentId == null) return null;

        var cacheKey = $"CurrentParentResolver.ChildStudentIdsForParent:{parentId.Value}";
        var uow = _unitOfWorkManager.Current;
        if (uow != null && uow.Items.TryGetValue(cacheKey, out var cached))
            return cached as List<Guid>;

        var childIds = await _studentParentRepository
            .GetAll()
            .Where(sp => sp.ParentId == parentId.Value)
            .Select(sp => sp.StudentId)
            .Distinct()
            .ToListAsync();

        if (uow != null) uow.Items[cacheKey] = childIds;
        return childIds;
    }
}
