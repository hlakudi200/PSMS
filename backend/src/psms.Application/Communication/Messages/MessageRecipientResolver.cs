using Abp.Dependency;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using psms.Academic.Parents;
using psms.Communication.Messages.Dto;
using psms.Domain.Academic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Communication.Messages;

/// <summary>
/// #204. Who a parent may start a conversation with: the class teacher of each
/// of their children, and the teacher of each subject their class takes. It is
/// worked out from the children's classes, not from a staff directory, so a
/// parent cannot write to any user in the school.
/// </summary>
public interface IMessageRecipientResolver : ITransientDependency
{
    /// <summary>Null when the caller is not a parent, so their sending is not limited here.</summary>
    Task<List<MessageRecipientDto>> GetForCurrentParentAsync();
}

public class MessageRecipientResolver : IMessageRecipientResolver
{
    private readonly ICurrentParentResolver _currentParent;
    private readonly IRepository<Student, Guid> _studentRepository;
    private readonly IRepository<Class, Guid> _classRepository;
    private readonly IRepository<ClassSubject, Guid> _classSubjectRepository;

    public MessageRecipientResolver(
        ICurrentParentResolver currentParent,
        IRepository<Student, Guid> studentRepository,
        IRepository<Class, Guid> classRepository,
        IRepository<ClassSubject, Guid> classSubjectRepository)
    {
        _currentParent = currentParent;
        _studentRepository = studentRepository;
        _classRepository = classRepository;
        _classSubjectRepository = classSubjectRepository;
    }

    public async Task<List<MessageRecipientDto>> GetForCurrentParentAsync()
    {
        var childIds = await _currentParent.GetCurrentChildStudentIdsAsync();
        if (childIds == null) return null;
        if (childIds.Count == 0) return new List<MessageRecipientDto>();

        var children = await _studentRepository
            .GetAll()
            .Where(s => childIds.Contains(s.Id))
            .Select(s => new { s.FirstName, s.CurrentClassId })
            .ToListAsync();
        var classIds = children.Select(c => c.CurrentClassId).Distinct().ToList();

        var classTeachers = await _classRepository
            .GetAll()
            .Where(c => classIds.Contains(c.Id) && c.ClassTeacher != null && c.ClassTeacher.IsActive)
            .Select(c => new
            {
                ClassId = c.Id,
                c.ClassName,
                c.ClassTeacher.UserId,
                c.ClassTeacher.FirstName,
                c.ClassTeacher.LastName,
            })
            .ToListAsync();

        var subjectTeachers = await _classSubjectRepository
            .GetAll()
            .Where(cs => classIds.Contains(cs.ClassId) && cs.IsActive && cs.Teacher != null && cs.Teacher.IsActive)
            .Select(cs => new
            {
                cs.ClassId,
                cs.Class.ClassName,
                cs.Subject.SubjectName,
                cs.Teacher.UserId,
                cs.Teacher.FirstName,
                cs.Teacher.LastName,
            })
            .ToListAsync();

        // "3D (Ayanda)", or "3D (Ayanda, Sipho)" for two children in one class.
        string ForClass(Guid classId, string className)
        {
            var names = children.Where(c => c.CurrentClassId == classId).Select(c => c.FirstName);
            return $"{className} ({string.Join(", ", names)})";
        }

        var roles = classTeachers
            .Select(t => (t.UserId, t.FirstName, t.LastName, Role: $"Class teacher, {ForClass(t.ClassId, t.ClassName)}", Order: 0))
            .Concat(subjectTeachers.Select(t =>
                (t.UserId, t.FirstName, t.LastName, Role: $"{t.SubjectName}, {ForClass(t.ClassId, t.ClassName)}", Order: 1)));

        return roles
            .GroupBy(r => r.UserId)
            .Select(g => new MessageRecipientDto
            {
                UserId = g.Key,
                Name = $"{g.First().FirstName} {g.First().LastName}".Trim(),
                Roles = g.OrderBy(r => r.Order).ThenBy(r => r.Role).Select(r => r.Role).Distinct().ToList(),
            })
            .OrderBy(r => r.Name)
            .ToList();
    }
}
