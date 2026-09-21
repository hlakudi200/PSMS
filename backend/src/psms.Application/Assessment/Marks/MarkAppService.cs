using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using psms.Assessment.Marks.Dto;
using psms.Assessment.Shared;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using psms.Domain.Assessment.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AssessmentEntity = psms.Domain.Assessment.Entities.Assessment;

namespace psms.Assessment.Marks;

/// <summary>
/// Service for managing student marks / scores.
/// </summary>
[AbpAuthorize(PermissionNames.Assessment_Marks)]
public class MarkAppService : ApplicationService, IMarkAppService
{
    private readonly IRepository<Mark, Guid> _markRepository;
    private readonly IRepository<AssessmentEntity, Guid> _assessmentRepository;
    private readonly IRepository<Student, Guid> _studentRepository;
    private readonly psms.Academic.Students.ICurrentStudentResolver _currentStudent;
    private readonly psms.Academic.Parents.ICurrentParentResolver _currentParent;

    public MarkAppService(
        IRepository<Mark, Guid> markRepository,
        IRepository<AssessmentEntity, Guid> assessmentRepository,
        IRepository<Student, Guid> studentRepository,
        psms.Academic.Students.ICurrentStudentResolver currentStudent,
        psms.Academic.Parents.ICurrentParentResolver currentParent)
    {
        _markRepository = markRepository;
        _assessmentRepository = assessmentRepository;
        _studentRepository = studentRepository;
        _currentStudent = currentStudent;
        _currentParent = currentParent;
    }

    [AbpAuthorize(PermissionNames.Assessment_Marks_View)]
    public async Task<MarkDto> GetAsync(Guid id)
    {
        var mark = await _markRepository
            .GetAll()
            .Include(m => m.Assessment)
            .Include(m => m.Student)
            .FirstOrDefaultAsync(m => m.Id == id && m.TenantId == AbpSession.TenantId);

        if (mark == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.MarkNotFound, "Mark not found.");

        // LC-10: a student may only read their own marks.
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();
        if (selfId.HasValue && selfId.Value != mark.StudentId)
            throw new UserFriendlyException(AssessmentExceptionCodes.MarkNotFound, "Mark not found.");

        // MOB-BE-04: a parent may only read their own children's marks.
        var childIds = await _currentParent.GetCurrentChildStudentIdsAsync();
        if (childIds != null && !childIds.Contains(mark.StudentId))
            throw new UserFriendlyException(AssessmentExceptionCodes.MarkNotFound, "Mark not found.");

        return ObjectMapper.Map<MarkDto>(mark);
    }

    [AbpAuthorize(PermissionNames.Assessment_Marks_View)]
    public async Task<PagedResultDto<MarkListDto>> GetAllAsync(GetMarksInput input)
    {
        // LC-10: a student-portal user only ever sees their own marks.
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();
        // MOB-BE-04: a parent only ever sees their own children's marks.
        var childIds = await _currentParent.GetCurrentChildStudentIdsAsync();

        var query = _markRepository
            .GetAll()
            .Include(m => m.Assessment).ThenInclude(a => a.ClassSubject)
            .Include(m => m.Student)
            .Where(m => m.TenantId == AbpSession.TenantId)
            .WhereIf(selfId.HasValue, m => m.StudentId == selfId.Value)
            .WhereIf(childIds != null, m => childIds.Contains(m.StudentId))
            .WhereIf(input.AssessmentId.HasValue, m => m.AssessmentId == input.AssessmentId.Value)
            .WhereIf(input.StudentId.HasValue, m => m.StudentId == input.StudentId.Value)
            .WhereIf(input.ClassId.HasValue, m => m.Assessment.ClassSubject.ClassId == input.ClassId.Value)
            .WhereIf(input.SubjectId.HasValue, m => m.Assessment.ClassSubject.SubjectId == input.SubjectId.Value)
            .WhereIf(input.TermId.HasValue, m => m.Assessment.TermId == input.TermId.Value)
            .WhereIf(input.Status.HasValue, m => m.Status == input.Status.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.StudentName),
                m => (m.Student.FirstName + " " + m.Student.LastName).ToLower()
                    .Contains(input.StudentName.Trim().ToLower()))
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                m => m.Student.FirstName.ToLower().Contains(input.Keyword.ToLower())
                    || m.Student.LastName.ToLower().Contains(input.Keyword.ToLower()));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "Student.LastName ASC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<MarkListDto>(
            totalCount,
            ObjectMapper.Map<List<MarkListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Assessment_Marks_View)]
    public async Task<ListResultDto<MarkListDto>> GetByAssessmentAsync(Guid assessmentId)
    {
        // LC-10: a student sees only their own mark for the assessment, not the
        // whole class score sheet.
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();
        // MOB-BE-04: a parent sees only their own children's marks for the assessment.
        var childIds = await _currentParent.GetCurrentChildStudentIdsAsync();

        var items = await _markRepository
            .GetAll()
            .Include(m => m.Assessment)
            .Include(m => m.Student)
            .Where(m => m.TenantId == AbpSession.TenantId)
            .Where(m => m.AssessmentId == assessmentId)
            .WhereIf(selfId.HasValue, m => m.StudentId == selfId.Value)
            .WhereIf(childIds != null, m => childIds.Contains(m.StudentId))
            .OrderBy(m => m.Student.LastName)
            .ThenBy(m => m.Student.FirstName)
            .ToListAsync();

        return new ListResultDto<MarkListDto>(
            ObjectMapper.Map<List<MarkListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Assessment_Marks_View)]
    public async Task<ListResultDto<MarkListDto>> GetByStudentAsync(Guid studentId, Guid? termId)
    {
        // LC-10: a student may only read their own marks.
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();
        if (selfId.HasValue && selfId.Value != studentId)
            return new ListResultDto<MarkListDto>(new System.Collections.Generic.List<MarkListDto>());

        // MOB-BE-04: a parent may only read their own children's marks.
        var childIds = await _currentParent.GetCurrentChildStudentIdsAsync();
        if (childIds != null && !childIds.Contains(studentId))
            return new ListResultDto<MarkListDto>(new System.Collections.Generic.List<MarkListDto>());

        var items = await _markRepository
            .GetAll()
            .Include(m => m.Assessment)
            .Include(m => m.Student)
            .Where(m => m.TenantId == AbpSession.TenantId)
            .Where(m => m.StudentId == studentId)
            .WhereIf(termId.HasValue, m => m.Assessment.TermId == termId.Value)
            .OrderBy(m => m.Assessment.Name)
            .ToListAsync();

        return new ListResultDto<MarkListDto>(
            ObjectMapper.Map<List<MarkListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Assessment_Marks_Create)]
    public async Task<MarkDto> RecordMarkAsync(RecordMarkDto input)
    {
        var assessment = await ValidateAssessmentForMarking(input.AssessmentId);

        // Validate student exists
        var student = await _studentRepository.FirstOrDefaultAsync(s => s.Id == input.StudentId);
        if (student == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.StudentNotFound, "Student not found.");

        // GA-006: Duplicate prevention
        var existing = await _markRepository
            .GetAll()
            .Where(m => m.TenantId == AbpSession.TenantId)
            .FirstOrDefaultAsync(m => m.AssessmentId == input.AssessmentId && m.StudentId == input.StudentId);

        if (existing != null)
            throw new UserFriendlyException(AssessmentExceptionCodes.DuplicateMark,
                "A mark already exists for this student on this assessment.");

        // GA-003: Validate raw mark range
        if (input.RawMark.HasValue && !input.WasAbsent)
        {
            if (input.RawMark.Value < 0 || input.RawMark.Value > assessment.MaxMarks)
                throw new UserFriendlyException(AssessmentExceptionCodes.MarkExceedsMax,
                    $"Raw mark must be between 0 and {assessment.MaxMarks}.");
        }

        var mark = new Mark(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.AssessmentId,
            input.StudentId)
        {
            IsReassessment = input.IsReassessment,
            Feedback = input.Feedback
        };

        if (input.WasAbsent)
        {
            mark.MarkAsAbsent();
        }
        else if (input.RawMark.HasValue)
        {
            mark.RecordMark(input.RawMark.Value, assessment.MaxMarks, AbpSession.UserId.Value, input.TeacherComment);
        }

        await _markRepository.InsertAsync(mark);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(mark.Id);
    }

    [AbpAuthorize(PermissionNames.Assessment_Marks_Create)]
    public async Task<ListResultDto<MarkDto>> BulkRecordMarksAsync(BulkRecordMarksDto input)
    {
        var assessment = await ValidateAssessmentForMarking(input.AssessmentId);

        // Get existing marks for this assessment to check duplicates
        var existingStudentIds = await _markRepository
            .GetAll()
            .Where(m => m.TenantId == AbpSession.TenantId)
            .Where(m => m.AssessmentId == input.AssessmentId)
            .Select(m => m.StudentId)
            .ToListAsync();

        // Fail-fast: detect duplicates within the batch
        var batchStudentIds = input.StudentMarks.Select(sm => sm.StudentId).ToList();
        var duplicatesInBatch = batchStudentIds.GroupBy(id => id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicatesInBatch.Any())
            throw new UserFriendlyException(AssessmentExceptionCodes.DuplicateStudentInBatch,
                $"Duplicate student IDs in batch: {string.Join(", ", duplicatesInBatch)}.");

        // Fail-fast: validate ALL entries first
        var newMarks = new List<Mark>();

        foreach (var studentMark in input.StudentMarks)
        {
            // Check duplicate against existing marks
            if (existingStudentIds.Contains(studentMark.StudentId))
                throw new UserFriendlyException(AssessmentExceptionCodes.DuplicateMark,
                    $"A mark already exists for student {studentMark.StudentId} on this assessment.");

            // Validate student exists
            var student = await _studentRepository.FirstOrDefaultAsync(s => s.Id == studentMark.StudentId);
            if (student == null)
                throw new UserFriendlyException(AssessmentExceptionCodes.StudentNotFound,
                    $"Student {studentMark.StudentId} not found.");

            // Validate mark range
            if (studentMark.RawMark.HasValue && !studentMark.WasAbsent)
            {
                if (studentMark.RawMark.Value < 0 || studentMark.RawMark.Value > assessment.MaxMarks)
                    throw new UserFriendlyException(AssessmentExceptionCodes.MarkExceedsMax,
                        $"Raw mark for student {student.AdmissionNumber} must be between 0 and {assessment.MaxMarks}.");
            }

            var mark = new Mark(
                Guid.NewGuid(),
                AbpSession.TenantId,
                input.AssessmentId,
                studentMark.StudentId);

            if (studentMark.WasAbsent)
            {
                mark.MarkAsAbsent();
            }
            else if (studentMark.RawMark.HasValue)
            {
                mark.RecordMark(studentMark.RawMark.Value, assessment.MaxMarks, AbpSession.UserId.Value, studentMark.TeacherComment);
            }

            newMarks.Add(mark);
        }

        // Insert all marks
        foreach (var mark in newMarks)
        {
            await _markRepository.InsertAsync(mark);
        }

        await CurrentUnitOfWork.SaveChangesAsync();

        // Return all created marks
        var createdIds = newMarks.Select(m => m.Id).ToList();
        var savedMarks = await _markRepository
            .GetAll()
            .Include(m => m.Assessment)
            .Include(m => m.Student)
            .Where(m => m.TenantId == AbpSession.TenantId)
            .Where(m => createdIds.Contains(m.Id))
            .ToListAsync();

        return new ListResultDto<MarkDto>(
            ObjectMapper.Map<List<MarkDto>>(savedMarks));
    }

    [AbpAuthorize(PermissionNames.Assessment_Marks_Edit)]
    public async Task<MarkDto> UpdateMarkAsync(Guid id, RecordMarkDto input)
    {
        var mark = await _markRepository
            .GetAll()
            .Include(m => m.Assessment)
            .FirstOrDefaultAsync(m => m.Id == id && m.TenantId == AbpSession.TenantId);

        if (mark == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.MarkNotFound, "Mark not found.");

        // GA-002: Cannot edit locked marks (Completed + released)
        if (mark.Status == MarkStatus.Completed && mark.Assessment.MarksReleased)
            throw new UserFriendlyException(AssessmentExceptionCodes.CannotEditLockedMark,
                "Cannot edit a locked mark. Marks have been released. Use unlock first.");

        // Validate mark range
        if (input.RawMark.HasValue && !input.WasAbsent)
        {
            if (input.RawMark.Value < 0 || input.RawMark.Value > mark.Assessment.MaxMarks)
                throw new UserFriendlyException(AssessmentExceptionCodes.MarkExceedsMax,
                    $"Raw mark must be between 0 and {mark.Assessment.MaxMarks}.");
        }

        mark.IsReassessment = input.IsReassessment;
        mark.Feedback = input.Feedback;

        if (input.WasAbsent)
        {
            mark.MarkAsAbsent();
        }
        else if (input.RawMark.HasValue)
        {
            mark.RecordMark(input.RawMark.Value, mark.Assessment.MaxMarks, AbpSession.UserId.Value, input.TeacherComment);
        }

        await _markRepository.UpdateAsync(mark);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    // TF-004: feedback may be edited up to 48 hours after marks are released.
    private const int FeedbackEditWindowHours = 48;

    // TF-002: minimal inappropriate-language denylist, matched on word
    // boundaries (case-insensitive). A real deployment would source this from
    // configuration / a moderation service; the scan is enforced server-side
    // regardless of the client.
    private static readonly string[] InappropriateTerms =
    {
        "stupid", "idiot", "dumb", "useless", "lazy", "pathetic", "moron", "worthless"
    };

    /// <summary>
    /// Edit a mark's feedback (TF-002/004/005): enforces the 48-hour
    /// post-release window, scans for inappropriate language, and appends the
    /// previous text to the mark's edit history.
    /// </summary>
    [AbpAuthorize(PermissionNames.Assessment_Feedback_Edit)]
    public async Task<MarkDto> UpdateFeedbackAsync(Guid id, UpdateFeedbackDto input)
    {
        var mark = await _markRepository
            .GetAll()
            .Include(m => m.Assessment)
            .FirstOrDefaultAsync(m => m.Id == id && m.TenantId == AbpSession.TenantId);

        if (mark == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.MarkNotFound, "Mark not found.");

        var feedback = (input.Feedback ?? string.Empty).Trim();
        if (feedback.Length > Mark.MaxFeedbackLength)
            throw new UserFriendlyException(AssessmentExceptionCodes.FeedbackTooLong,
                $"Feedback must be {Mark.MaxFeedbackLength} characters or fewer.");

        // TF-004: once marks are released, feedback may only be edited within
        // the 48-hour window measured from the release timestamp. Fail CLOSED:
        // a released assessment with no recorded release date (e.g. released
        // before this feature shipped) is treated as past its window rather
        // than editable forever.
        if (mark.Assessment.MarksReleased)
        {
            var releasedAt = mark.Assessment.MarksReleasedDate;
            var withinWindow = releasedAt.HasValue
                && DateTime.UtcNow <= releasedAt.Value.AddHours(FeedbackEditWindowHours);
            if (!withinWindow)
                throw new UserFriendlyException(AssessmentExceptionCodes.FeedbackEditWindowExpired,
                    releasedAt.HasValue
                        ? $"The {FeedbackEditWindowHours}-hour feedback edit window closed on {releasedAt.Value.AddHours(FeedbackEditWindowHours):u}."
                        : "The feedback edit window has closed for this assessment.");
        }

        // TF-002: reject inappropriate language.
        if (ContainsInappropriateLanguage(feedback))
            throw new UserFriendlyException(AssessmentExceptionCodes.FeedbackInappropriateLanguage,
                "Feedback contains language that isn't allowed. Please revise it.");

        // TF-005: record the previous feedback in the edit history (only when
        // it actually changes and there was prior text to preserve).
        var previous = mark.Feedback?.Trim();
        if (!string.IsNullOrEmpty(previous) && previous != feedback)
        {
            var history = string.IsNullOrWhiteSpace(mark.FeedbackHistory)
                ? new List<FeedbackEditEntry>()
                : JsonConvert.DeserializeObject<List<FeedbackEditEntry>>(mark.FeedbackHistory)
                    ?? new List<FeedbackEditEntry>();
            history.Add(new FeedbackEditEntry
            {
                At = DateTime.UtcNow,
                ByUserId = AbpSession.UserId,
                Previous = mark.Feedback
            });
            mark.FeedbackHistory = JsonConvert.SerializeObject(history);
        }

        mark.Feedback = feedback;

        await _markRepository.UpdateAsync(mark);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    private static bool ContainsInappropriateLanguage(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        return InappropriateTerms.Any(term =>
            Regex.IsMatch(text, $@"\b{Regex.Escape(term)}\b", RegexOptions.IgnoreCase));
    }

    [AbpAuthorize(PermissionNames.Assessment_Marks_Edit)]
    public async Task<MarkDto> MarkAsAbsentAsync(Guid id)
    {
        var mark = await _markRepository
            .GetAll()
            .Include(m => m.Assessment)
            .FirstOrDefaultAsync(m => m.Id == id && m.TenantId == AbpSession.TenantId);

        if (mark == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.MarkNotFound, "Mark not found.");

        // GA-002: Cannot edit locked marks (Completed + released)
        if (mark.Status == MarkStatus.Completed && mark.Assessment.MarksReleased)
            throw new UserFriendlyException(AssessmentExceptionCodes.CannotEditLockedMark,
                "Cannot edit a locked mark. Marks have been released. Use unlock first.");

        mark.MarkAsAbsent();
        await _markRepository.UpdateAsync(mark);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Assessment_Marks_Edit)]
    public async Task<MarkDto> ApplyModerationAsync(Guid id, decimal adjustment)
    {
        var mark = await _markRepository
            .GetAll()
            .Include(m => m.Assessment)
            .FirstOrDefaultAsync(m => m.Id == id && m.TenantId == AbpSession.TenantId);

        if (mark == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.MarkNotFound, "Mark not found.");

        // GA-002: Cannot edit locked marks (Completed + released)
        if (mark.Status == MarkStatus.Completed && mark.Assessment.MarksReleased)
            throw new UserFriendlyException(AssessmentExceptionCodes.CannotEditLockedMark,
                "Cannot edit a locked mark. Marks have been released. Use unlock first.");

        if (!mark.RawMark.HasValue)
            throw new UserFriendlyException(AssessmentExceptionCodes.NoRawMarkToModerate,
                "Cannot apply moderation to a mark with no raw mark recorded. Record a mark first.");

        mark.ApplyModeration(adjustment, mark.Assessment.MaxMarks);
        await _markRepository.UpdateAsync(mark);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Assessment_Marks_Unlock)]
    public async Task<MarkDto> UnlockMarkAsync(Guid id)
    {
        var mark = await _markRepository
            .FirstOrDefaultAsync(m => m.Id == id && m.TenantId == AbpSession.TenantId);

        if (mark == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.MarkNotFound, "Mark not found.");

        // Reset status to allow editing
        mark.Status = MarkStatus.Pending;
        await _markRepository.UpdateAsync(mark);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Assessment_Marks_Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var mark = await _markRepository
            .GetAll()
            .Include(m => m.Assessment)
            .FirstOrDefaultAsync(m => m.Id == id && m.TenantId == AbpSession.TenantId);

        if (mark == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.MarkNotFound, "Mark not found.");

        // Cannot delete marks of released assessment
        if (mark.Assessment.MarksReleased)
            throw new UserFriendlyException(AssessmentExceptionCodes.MarksLocked,
                "Cannot delete marks for an assessment with released marks.");

        await _markRepository.DeleteAsync(mark);
    }

    private async Task<AssessmentEntity> ValidateAssessmentForMarking(Guid assessmentId)
    {
        var assessment = await _assessmentRepository
            .GetAll()
            .Include(a => a.Term)
            .FirstOrDefaultAsync(a => a.Id == assessmentId && a.TenantId == AbpSession.TenantId);

        if (assessment == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.AssessmentNotFound,
                "Assessment not found.");

        // Assessment must be published to record marks
        if (!assessment.IsPublished)
            throw new UserFriendlyException(AssessmentExceptionCodes.AssessmentNotPublished,
                "Cannot record marks for an unpublished assessment.");

        // GA-005: Cannot record marks for future term
        if (assessment.Term.StartDate.Date > DateTime.Today)
            throw new UserFriendlyException(AssessmentExceptionCodes.TermInFuture,
                "Cannot record marks for a future term.");

        return assessment;
    }
}
