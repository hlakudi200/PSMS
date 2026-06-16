using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Runtime.Session;
using Microsoft.EntityFrameworkCore;
using psms.Domain.Academic.Entities;

namespace psms.Academic.Students;

/// <summary>
/// Resolves the Student.Id linked to the current user (Student.UserId ==
/// AbpSession.UserId), or null for staff / non-student users (LC-08). Student
/// PII read endpoints use this to scope results to the caller's own record so
/// a logged-in student can't read other students' data via an arbitrary id.
/// Memoised on the ambient unit of work (per request).
/// </summary>
public interface ICurrentStudentResolver : ITransientDependency
{
    Task<Guid?> GetCurrentStudentIdAsync();
}

public class CurrentStudentResolver : ICurrentStudentResolver
{
    private readonly IRepository<Student, Guid> _studentRepository;
    private readonly IAbpSession _abpSession;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public CurrentStudentResolver(
        IRepository<Student, Guid> studentRepository,
        IAbpSession abpSession,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _studentRepository = studentRepository;
        _abpSession = abpSession;
        _unitOfWorkManager = unitOfWorkManager;
    }

    public async Task<Guid?> GetCurrentStudentIdAsync()
    {
        if (_abpSession.UserId == null) return null;

        var cacheKey = $"CurrentStudentResolver.StudentIdForUser:{_abpSession.UserId.Value}";
        var uow = _unitOfWorkManager.Current;
        if (uow != null && uow.Items.TryGetValue(cacheKey, out var cached))
            return cached as Guid?;

        var studentId = await _studentRepository
            .GetAll()
            .Where(s => s.UserId == _abpSession.UserId.Value && s.TenantId == _abpSession.TenantId)
            .Select(s => (Guid?)s.Id)
            .FirstOrDefaultAsync();

        if (uow != null) uow.Items[cacheKey] = studentId;
        return studentId;
    }
}
