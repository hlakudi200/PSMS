using Abp.Dependency;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using psms.Academic.Parents;
using psms.Academic.Students;
using psms.Domain.Academic.Entities;
using psms.Domain.Communication.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Communication.Announcements;

/// <summary>
/// #203: who a parent or learner is, as far as an announcement's audience goes.
/// </summary>
public class FamilyAudience
{
    public bool IsParent { get; init; }
    public bool IsStudent { get; init; }

    /// <summary>The grades and classes of the caller's children, or their own as a learner.</summary>
    public List<Guid> GradeIds { get; init; } = new();
    public List<Guid> ClassIds { get; init; } = new();
}

/// <summary>
/// #203. The Parent and Student roles both hold Announcements.View, and the
/// reads returned every announcement in the school to them: drafts, staff and
/// teacher notices, other classes' notices, and ones not yet due or already
/// expired. This works out which apply to the caller.
/// </summary>
public interface IFamilyAnnouncementScope : ITransientDependency
{
    /// <summary>Null for staff, whose reads are not scoped.</summary>
    Task<FamilyAudience> ResolveAsync();
}

public class FamilyAnnouncementScope : IFamilyAnnouncementScope
{
    private readonly ICurrentParentResolver _currentParent;
    private readonly ICurrentStudentResolver _currentStudent;
    private readonly IRepository<Student, Guid> _studentRepository;

    public FamilyAnnouncementScope(
        ICurrentParentResolver currentParent,
        ICurrentStudentResolver currentStudent,
        IRepository<Student, Guid> studentRepository)
    {
        _currentParent = currentParent;
        _currentStudent = currentStudent;
        _studentRepository = studentRepository;
    }

    public async Task<FamilyAudience> ResolveAsync()
    {
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();
        var childIds = await _currentParent.GetCurrentChildStudentIdsAsync();
        if (!selfId.HasValue && childIds == null) return null;

        var learnerIds = new List<Guid>(childIds ?? new List<Guid>());
        if (selfId.HasValue) learnerIds.Add(selfId.Value);

        var placements = await _studentRepository
            .GetAll()
            .Where(s => learnerIds.Contains(s.Id))
            .Select(s => new { s.CurrentGradeId, s.CurrentClassId })
            .ToListAsync();

        return new FamilyAudience
        {
            IsParent = childIds != null,
            IsStudent = selfId.HasValue,
            GradeIds = placements.Select(p => p.CurrentGradeId).Distinct().ToList(),
            ClassIds = placements.Select(p => p.CurrentClassId).Distinct().ToList(),
        };
    }
}

public static class FamilyAnnouncementQueryExtensions
{
    /// <summary>
    /// What a family may read: published, due and not expired (the same test as
    /// <see cref="Announcement.IsActive"/>), and addressed to everyone, to them
    /// as a parent or learner, or to a grade or class one of their learners is
    /// in. Never Staff or Teachers. A null audience is staff, and is unchanged.
    /// </summary>
    public static IQueryable<Announcement> VisibleTo(this IQueryable<Announcement> query, FamilyAudience audience)
    {
        if (audience == null) return query;

        var now = DateTime.UtcNow;
        var isParent = audience.IsParent;
        var isStudent = audience.IsStudent;
        var gradeIds = audience.GradeIds;
        var classIds = audience.ClassIds;

        return query.Where(a =>
            a.IsPublished
            && a.PublishDate <= now
            && (a.ExpiryDate == null || a.ExpiryDate > now)
            && (a.TargetAudience == AnnouncementAudience.All
                || (isParent && a.TargetAudience == AnnouncementAudience.Parents)
                || (isStudent && a.TargetAudience == AnnouncementAudience.Students)
                || (a.TargetAudience == AnnouncementAudience.Grade
                    && a.TargetGradeId != null && gradeIds.Contains(a.TargetGradeId.Value))
                || (a.TargetAudience == AnnouncementAudience.Class
                    && a.TargetClassId != null && classIds.Contains(a.TargetClassId.Value))));
    }
}
