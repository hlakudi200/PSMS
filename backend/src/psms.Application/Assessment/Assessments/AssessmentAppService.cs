using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using psms.Assessment.Assessments.Dto;
using psms.Assessment.Shared;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using psms.Domain.Assessment.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using AssessmentEntity = psms.Domain.Assessment.Entities.Assessment;

namespace psms.Assessment.Assessments;

/// <summary>
/// Service for managing assessments (tests, exams, assignments).
/// </summary>
[AbpAuthorize(PermissionNames.Assessment_Marks)]
public class AssessmentAppService : ApplicationService, IAssessmentAppService
{
    private readonly IRepository<AssessmentEntity, Guid> _assessmentRepository;
    private readonly IRepository<ClassSubject, Guid> _classSubjectRepository;
    private readonly IRepository<Term, Guid> _termRepository;
    private readonly IRepository<Mark, Guid> _markRepository;
    private readonly IRepository<AssessmentQuestion, Guid> _questionRepository;

    // QA-001 question-structure bounds. Mirrored client-side in the
    // QuestionBuilder so the teacher gets inline errors; the server is the
    // source of truth and re-validates in CreateWithQuestionsAsync.
    private const int MinQuestions = 5;
    private const int MaxQuestions = 100;
    private const int MinOptions = 2;
    private const int MaxOptions = 6;
    private const int MaxOptionLength = 500;

    public AssessmentAppService(
        IRepository<AssessmentEntity, Guid> assessmentRepository,
        IRepository<ClassSubject, Guid> classSubjectRepository,
        IRepository<Term, Guid> termRepository,
        IRepository<Mark, Guid> markRepository,
        IRepository<AssessmentQuestion, Guid> questionRepository)
    {
        _assessmentRepository = assessmentRepository;
        _classSubjectRepository = classSubjectRepository;
        _termRepository = termRepository;
        _markRepository = markRepository;
        _questionRepository = questionRepository;
    }

    [AbpAuthorize(PermissionNames.Assessment_Marks_View)]
    public async Task<AssessmentDto> GetAsync(Guid id)
    {
        var assessment = await _assessmentRepository
            .GetAll()
            .Include(a => a.ClassSubject).ThenInclude(cs => cs.Class)
            .Include(a => a.ClassSubject).ThenInclude(cs => cs.Subject)
            .Include(a => a.Term)
            .Include(a => a.Marks)
            .Include(a => a.Questions)
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == AbpSession.TenantId);

        if (assessment == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.AssessmentNotFound,
                "Assessment not found.");

        return ObjectMapper.Map<AssessmentDto>(assessment);
    }

    [AbpAuthorize(PermissionNames.Assessment_Marks_View)]
    public async Task<PagedResultDto<AssessmentListDto>> GetAllAsync(GetAssessmentsInput input)
    {
        var query = _assessmentRepository
            .GetAll()
            .Include(a => a.ClassSubject).ThenInclude(cs => cs.Class)
            .Include(a => a.ClassSubject).ThenInclude(cs => cs.Subject)
            .Include(a => a.Term)
            .Include(a => a.Marks)
            .Include(a => a.Questions)
            .Where(a => a.TenantId == AbpSession.TenantId)
            .WhereIf(input.ClassSubjectId.HasValue, a => a.ClassSubjectId == input.ClassSubjectId.Value)
            .WhereIf(input.TermId.HasValue, a => a.TermId == input.TermId.Value)
            .WhereIf(input.ClassId.HasValue, a => a.ClassSubject.ClassId == input.ClassId.Value)
            .WhereIf(input.SubjectId.HasValue, a => a.ClassSubject.SubjectId == input.SubjectId.Value)
            .WhereIf(input.AssessmentType.HasValue, a => a.AssessmentType == input.AssessmentType.Value)
            .WhereIf(input.IsPublished.HasValue, a => a.IsPublished == input.IsPublished.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Name),
                a => a.Name.ToLower().Contains(input.Name.Trim().ToLower()))
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                a => a.Name.ToLower().Contains(input.Keyword.ToLower()));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "Name ASC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<AssessmentListDto>(
            totalCount,
            ObjectMapper.Map<List<AssessmentListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Assessment_Marks_Create)]
    public async Task<AssessmentDto> CreateAsync(CreateAssessmentDto input)
    {
        await ValidateClassSubjectAndTermOrThrowAsync(input);

        var assessment = BuildAssessment(input);
        await _assessmentRepository.InsertAsync(assessment);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(assessment.Id);
    }

    /// <summary>
    /// Creates an assessment and all of its questions atomically (QA-001).
    /// The whole method body runs inside one ABP unit of work, so if any
    /// question is rejected the assessment insert is rolled back too —
    /// the caller never ends up with an orphaned assessment.
    /// </summary>
    // Creating an assessment *with its questions* is a quiz-authoring action,
    // so it requires both the marks-create and quiz-create permissions — the
    // same boundary AssessmentQuestionAppService enforces. Stacked attributes
    // are AND-ed, so the caller must hold both. (The Teacher role is seeded
    // with both.)
    [AbpAuthorize(PermissionNames.Assessment_Marks_Create)]
    [AbpAuthorize(PermissionNames.Assessment_Quizzes_Create)]
    public async Task<AssessmentDto> CreateWithQuestionsAsync(CreateAssessmentWithQuestionsDto input)
    {
        var questions = input.Questions ?? new List<CreateAssessmentQuestionInlineDto>();

        // QA-001: question-count bounds.
        if (questions.Count < MinQuestions)
            throw new UserFriendlyException(AssessmentExceptionCodes.InsufficientQuestions,
                $"An assessment must have at least {MinQuestions} questions.");
        if (questions.Count > MaxQuestions)
            throw new UserFriendlyException(AssessmentExceptionCodes.TooManyQuestions,
                $"An assessment cannot have more than {MaxQuestions} questions.");

        // QA-001: per-question structure — 2-6 non-empty options, no
        // duplicates, exactly one valid correct option. Index loop so error
        // messages can name the offending question for the teacher.
        for (var i = 0; i < questions.Count; i++)
        {
            var q = questions[i];
            var options = (q.Options ?? new List<string>()).Select(o => (o ?? string.Empty).Trim()).ToList();

            if (string.IsNullOrWhiteSpace(q.QuestionText))
                throw new UserFriendlyException(AssessmentExceptionCodes.InvalidQuestionOptions,
                    $"Question {i + 1}: question text is required.");

            if (options.Count < MinOptions || options.Count > MaxOptions)
                throw new UserFriendlyException(AssessmentExceptionCodes.InvalidQuestionOptions,
                    $"Question {i + 1} must have between {MinOptions} and {MaxOptions} options.");

            if (options.Any(o => o.Length == 0 || o.Length > MaxOptionLength))
                throw new UserFriendlyException(AssessmentExceptionCodes.InvalidQuestionOptions,
                    $"Question {i + 1}: each option must be 1-{MaxOptionLength} characters and non-empty.");

            // Duplicate option text makes the text-based CorrectAnswer
            // ambiguous at grading time — reject case-insensitively.
            if (options.Select(o => o.ToLowerInvariant()).Distinct().Count() != options.Count)
                throw new UserFriendlyException(AssessmentExceptionCodes.InvalidQuestionOptions,
                    $"Question {i + 1}: options must be distinct.");

            // Guard the serialized payload against the Options column limit
            // (escaping can inflate length beyond MaxOptionLength * MaxOptions),
            // so an over-long set fails with a friendly message instead of an
            // EF truncation error mid-transaction.
            if (JsonConvert.SerializeObject(options).Length > AssessmentQuestion.MaxOptionsLength)
                throw new UserFriendlyException(AssessmentExceptionCodes.InvalidQuestionOptions,
                    $"Question {i + 1}: the options are too long.");

            if (q.CorrectOptionIndex < 0 || q.CorrectOptionIndex >= options.Count)
                throw new UserFriendlyException(AssessmentExceptionCodes.InvalidCorrectOption,
                    $"Question {i + 1} must mark exactly one valid correct option.");
        }

        // Match the invariant the single-question endpoint enforces: the sum
        // of question marks cannot exceed the assessment's MaxMarks.
        var totalQuestionMarks = questions.Sum(q => q.Marks);
        if (totalQuestionMarks > input.MaxMarks)
            throw new UserFriendlyException(AssessmentExceptionCodes.QuestionTotalExceedsMax,
                $"Total question marks ({totalQuestionMarks}) exceed the assessment maximum ({input.MaxMarks}).");

        await ValidateClassSubjectAndTermOrThrowAsync(input);

        var assessment = BuildAssessment(input);
        await _assessmentRepository.InsertAsync(assessment);

        for (var i = 0; i < questions.Count; i++)
        {
            var q = questions[i];
            var trimmedOptions = q.Options.Select(o => o.Trim()).ToList();
            var question = new AssessmentQuestion(
                Guid.NewGuid(),
                assessment.Id,
                i + 1,
                QuestionType.MultipleChoice,
                q.QuestionText.Trim(),
                q.Marks)
            {
                // Store the correct option's text (not the index) so grading
                // and review stay readable even if the option order changes.
                CorrectAnswer = trimmedOptions[q.CorrectOptionIndex],
                Explanation = string.IsNullOrWhiteSpace(q.Explanation) ? null : q.Explanation.Trim(),
                CognitiveLevel = q.CognitiveLevel
            };
            question.SetOptions(JsonConvert.SerializeObject(trimmedOptions));
            await _questionRepository.InsertAsync(question);
        }

        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(assessment.Id);
    }

    /// <summary>
    /// Shared create-time validation: the class-subject must exist, belong
    /// to this tenant, and be active; the term must exist and have started
    /// (GA-005); and MaxMarks must be positive.
    /// </summary>
    private async Task ValidateClassSubjectAndTermOrThrowAsync(CreateAssessmentDto input)
    {
        var classSubject = await _classSubjectRepository
            .GetAll()
            .Include(cs => cs.Class)
            .Include(cs => cs.Subject)
            .FirstOrDefaultAsync(cs => cs.Id == input.ClassSubjectId && cs.TenantId == AbpSession.TenantId);

        if (classSubject == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.ClassSubjectNotFound,
                "Class-subject assignment not found.");

        if (!classSubject.IsActive)
            throw new UserFriendlyException(AssessmentExceptionCodes.ClassSubjectNotFound,
                "Cannot create an assessment for an inactive class-subject assignment.");

        var term = await _termRepository.FirstOrDefaultAsync(t => t.Id == input.TermId);
        if (term == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.TermNotFound, "Term not found.");

        // GA-005: Term must have started (not future).
        if (term.StartDate.Date > DateTime.Today)
            throw new UserFriendlyException(AssessmentExceptionCodes.TermInFuture,
                "Cannot create an assessment for a future term.");

        if (input.MaxMarks <= 0)
            throw new UserFriendlyException(AssessmentExceptionCodes.InvalidMaxMarks,
                "Maximum marks must be greater than zero.");

        // QA-002: a due date cannot fall before the scheduled date. Enforced
        // server-side so it holds for every create path, not just the modal.
        if (input.ScheduledDate.HasValue && input.DueDate.HasValue
            && input.DueDate.Value < input.ScheduledDate.Value)
            throw new UserFriendlyException(AssessmentExceptionCodes.DueDateBeforeScheduled,
                "Due date cannot be before the scheduled date.");
    }

    private AssessmentEntity BuildAssessment(CreateAssessmentDto input)
    {
        return new AssessmentEntity(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.ClassSubjectId,
            input.TermId,
            input.Name,
            input.AssessmentType,
            input.MaxMarks,
            AbpSession.UserId.Value)
        {
            Description = input.Description,
            CapsCategory = input.CapsCategory,
            Weight = input.Weight,
            PassPercentage = input.PassPercentage,
            ScheduledDate = input.ScheduledDate,
            DueDate = input.DueDate,
            DurationMinutes = input.DurationMinutes,
            Instructions = input.Instructions
        };
    }

    [AbpAuthorize(PermissionNames.Assessment_Marks_Edit)]
    public async Task<AssessmentDto> UpdateAsync(Guid id, UpdateAssessmentDto input)
    {
        var assessment = await _assessmentRepository
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == AbpSession.TenantId);

        if (assessment == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.AssessmentNotFound,
                "Assessment not found.");

        // Cannot edit published assessment
        if (assessment.IsPublished)
            throw new UserFriendlyException(AssessmentExceptionCodes.CannotEditPublished,
                "Cannot edit a published assessment. Unpublish it first.");

        if (input.Name != null) assessment.Name = input.Name;
        if (input.Description != null) assessment.Description = input.Description;
        if (input.CapsCategory.HasValue) assessment.CapsCategory = input.CapsCategory.Value;
        if (input.MaxMarks.HasValue)
        {
            if (input.MaxMarks.Value <= 0)
                throw new UserFriendlyException(AssessmentExceptionCodes.InvalidMaxMarks,
                    "Maximum marks must be greater than zero.");

            // Validate MaxMarks reduction doesn't go below existing question totals
            if (input.MaxMarks.Value < assessment.MaxMarks)
            {
                var questionTotalMarks = await _assessmentRepository
                    .GetAll()
                    .Where(a => a.Id == id)
                    .SelectMany(a => a.Questions)
                    .SumAsync(q => q.Marks);

                if (input.MaxMarks.Value < questionTotalMarks)
                    throw new UserFriendlyException(AssessmentExceptionCodes.MaxMarksReductionInvalid,
                        $"Cannot reduce maximum marks to {input.MaxMarks.Value}. Existing questions total {questionTotalMarks} marks.");
            }

            assessment.MaxMarks = input.MaxMarks.Value;
        }
        if (input.Weight.HasValue) assessment.Weight = input.Weight.Value;
        if (input.PassPercentage.HasValue) assessment.PassPercentage = input.PassPercentage.Value;
        if (input.ScheduledDate.HasValue) assessment.ScheduledDate = input.ScheduledDate.Value;
        if (input.DueDate.HasValue) assessment.DueDate = input.DueDate.Value;
        if (input.DurationMinutes.HasValue) assessment.DurationMinutes = input.DurationMinutes.Value;
        if (input.Instructions != null) assessment.Instructions = input.Instructions;

        await _assessmentRepository.UpdateAsync(assessment);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Assessment_Marks_Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var assessment = await _assessmentRepository
            .GetAll()
            .Include(a => a.Marks)
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == AbpSession.TenantId);

        if (assessment == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.AssessmentNotFound,
                "Assessment not found.");

        // Cannot delete assessment that has marks
        if (assessment.Marks.Any())
            throw new UserFriendlyException(AssessmentExceptionCodes.CannotDeleteWithMarks,
                "Cannot delete an assessment that has marks recorded. Remove all marks first.");

        await _assessmentRepository.DeleteAsync(assessment);
    }

    [AbpAuthorize(PermissionNames.Assessment_Marks_Publish)]
    public async Task<AssessmentDto> PublishAsync(Guid id)
    {
        var assessment = await _assessmentRepository
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == AbpSession.TenantId);

        if (assessment == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.AssessmentNotFound,
                "Assessment not found.");

        if (assessment.IsPublished)
            throw new UserFriendlyException(AssessmentExceptionCodes.AssessmentAlreadyPublished,
                "Assessment is already published.");

        assessment.Publish();
        await _assessmentRepository.UpdateAsync(assessment);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Assessment_Marks_Publish)]
    public async Task<AssessmentDto> UnpublishAsync(Guid id)
    {
        var assessment = await _assessmentRepository
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == AbpSession.TenantId);

        if (assessment == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.AssessmentNotFound,
                "Assessment not found.");

        // Entity method throws if marks are released
        try
        {
            assessment.Unpublish();
        }
        catch (InvalidOperationException)
        {
            throw new UserFriendlyException(AssessmentExceptionCodes.CannotUnpublishReleasedMarks,
                "Cannot unpublish an assessment with released marks.");
        }

        await _assessmentRepository.UpdateAsync(assessment);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Assessment_Marks_Publish)]
    public async Task<AssessmentDto> ReleaseMarksAsync(Guid id)
    {
        var assessment = await _assessmentRepository
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == AbpSession.TenantId);

        if (assessment == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.AssessmentNotFound,
                "Assessment not found.");

        if (assessment.MarksReleased)
            throw new UserFriendlyException(AssessmentExceptionCodes.MarksAlreadyReleased,
                "Marks have already been released for this assessment.");

        // Entity method throws if not published
        try
        {
            assessment.ReleaseMarks();
        }
        catch (InvalidOperationException)
        {
            throw new UserFriendlyException(AssessmentExceptionCodes.AssessmentNotPublished,
                "Cannot release marks for an unpublished assessment. Publish the assessment first.");
        }

        await _assessmentRepository.UpdateAsync(assessment);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }
}
