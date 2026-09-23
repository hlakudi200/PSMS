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

namespace psms.Academic.Teachers;

/// <summary>
/// Resolves the Teacher.Id linked to the current user (Teacher.UserId ==
/// AbpSession.UserId) and the set of classes they actually teach, or null for
/// non-teacher users.
/// <para>
/// A teacher holds ReportCards.View and ReportCards.Comment so they can write
/// the comment printed under their own name and sign the Class Teacher line.
/// Those permissions are school-wide, so without this a teacher opening the
/// report card list would read every learner's marks in the school. This is the
/// same shape as <see cref="psms.Academic.Parents.ICurrentParentResolver"/> and
/// <see cref="psms.Academic.Students.ICurrentStudentResolver"/>.
/// </para>
/// <para>
/// Memoised on the ambient unit of work (per request).
/// </para>
/// </summary>
public interface ICurrentTeacherResolver : ITransientDependency
{
    Task<Guid?> GetCurrentTeacherIdAsync();

    /// <summary>
    /// Null when the caller isn't a teacher (don't scope). Otherwise the classes
    /// they are the class teacher of, or teach a subject in, which may be empty.
    /// </summary>
    Task<List<Guid>?> GetTaughtClassIdsAsync();
}

public class CurrentTeacherResolver : ICurrentTeacherResolver
{
    private readonly IRepository<Teacher, Guid> _teacherRepository;
    private readonly IRepository<Class, Guid> _classRepository;
    private readonly IRepository<ClassSubject, Guid> _classSubjectRepository;
    private readonly IAbpSession _abpSession;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public CurrentTeacherResolver(
        IRepository<Teacher, Guid> teacherRepository,
        IRepository<Class, Guid> classRepository,
        IRepository<ClassSubject, Guid> classSubjectRepository,
        IAbpSession abpSession,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _teacherRepository = teacherRepository;
        _classRepository = classRepository;
        _classSubjectRepository = classSubjectRepository;
        _abpSession = abpSession;
        _unitOfWorkManager = unitOfWorkManager;
    }

    public async Task<Guid?> GetCurrentTeacherIdAsync()
    {
        if (_abpSession.UserId == null) return null;

        var cacheKey = $"CurrentTeacherResolver.TeacherIdForUser:{_abpSession.UserId.Value}";
        var uow = _unitOfWorkManager.Current;
        if (uow != null && uow.Items.TryGetValue(cacheKey, out var cached))
            return cached as Guid?;

        var teacherId = await _teacherRepository
            .GetAll()
            .Where(t => t.UserId == _abpSession.UserId.Value && t.TenantId == _abpSession.TenantId)
            .Select(t => (Guid?)t.Id)
            .FirstOrDefaultAsync();

        if (uow != null) uow.Items[cacheKey] = teacherId;
        return teacherId;
    }

    public async Task<List<Guid>?> GetTaughtClassIdsAsync()
    {
        var teacherId = await GetCurrentTeacherIdAsync();
        if (teacherId == null) return null;

        var cacheKey = $"CurrentTeacherResolver.TaughtClassIds:{teacherId.Value}";
        var uow = _unitOfWorkManager.Current;
        if (uow != null && uow.Items.TryGetValue(cacheKey, out var cached))
            return cached as List<Guid>;

        // The class they register, plus every class they teach a subject in — a
        // subject teacher writes that subject's comment even when someone else
        // is the class teacher.
        var registered = await _classRepository
            .GetAll()
            .Where(c => c.TenantId == _abpSession.TenantId && c.ClassTeacherId == teacherId.Value)
            .Select(c => c.Id)
            .ToListAsync();

        var taught = await _classSubjectRepository
            .GetAll()
            .Where(cs => cs.TenantId == _abpSession.TenantId
                && cs.TeacherId == teacherId.Value
                && cs.IsActive)
            .Select(cs => cs.ClassId)
            .ToListAsync();

        var classIds = registered.Union(taught).Distinct().ToList();

        if (uow != null) uow.Items[cacheKey] = classIds;
        return classIds;
    }
}
