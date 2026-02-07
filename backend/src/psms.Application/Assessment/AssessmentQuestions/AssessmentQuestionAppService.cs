using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Assessment.AssessmentQuestions.Dto;
using psms.Assessment.Shared;
using psms.Authorization;
using psms.Domain.Assessment.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssessmentEntity = psms.Domain.Assessment.Entities.Assessment;

namespace psms.Assessment.AssessmentQuestions;

/// <summary>
/// Service for managing assessment questions.
/// TENANT SAFETY: AssessmentQuestion has no IMayHaveTenant — filter via Assessment.TenantId.
/// </summary>
[AbpAuthorize(PermissionNames.Assessment_Quizzes)]
public class AssessmentQuestionAppService : ApplicationService, IAssessmentQuestionAppService
{
    private readonly IRepository<AssessmentQuestion, Guid> _questionRepository;
    private readonly IRepository<AssessmentEntity, Guid> _assessmentRepository;

    public AssessmentQuestionAppService(
        IRepository<AssessmentQuestion, Guid> questionRepository,
        IRepository<AssessmentEntity, Guid> assessmentRepository)
    {
        _questionRepository = questionRepository;
        _assessmentRepository = assessmentRepository;
    }

    [AbpAuthorize(PermissionNames.Assessment_Quizzes_View)]
    public async Task<AssessmentQuestionDto> GetAsync(Guid id)
    {
        // Tenant isolation via Assessment parent
        var question = await _questionRepository
            .GetAll()
            .Include(q => q.Assessment)
            .Where(q => q.Assessment.TenantId == AbpSession.TenantId)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (question == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.QuestionNotFound,
                "Assessment question not found.");

        return ObjectMapper.Map<AssessmentQuestionDto>(question);
    }

    [AbpAuthorize(PermissionNames.Assessment_Quizzes_View)]
    public async Task<ListResultDto<AssessmentQuestionListDto>> GetByAssessmentAsync(Guid assessmentId)
    {
        // Validate assessment exists and belongs to tenant
        var assessmentExists = await _assessmentRepository
            .GetAll()
            .AnyAsync(a => a.Id == assessmentId && a.TenantId == AbpSession.TenantId);

        if (!assessmentExists)
            throw new UserFriendlyException(AssessmentExceptionCodes.AssessmentNotFound,
                "Assessment not found.");

        var questions = await _questionRepository
            .GetAll()
            .Include(q => q.Assessment)
            .Where(q => q.Assessment.TenantId == AbpSession.TenantId)
            .Where(q => q.AssessmentId == assessmentId)
            .OrderBy(q => q.QuestionNumber)
            .ToListAsync();

        return new ListResultDto<AssessmentQuestionListDto>(
            ObjectMapper.Map<List<AssessmentQuestionListDto>>(questions));
    }

    [AbpAuthorize(PermissionNames.Assessment_Quizzes_Create)]
    public async Task<AssessmentQuestionDto> CreateAsync(CreateAssessmentQuestionDto input)
    {
        // Validate assessment exists and belongs to tenant
        var assessment = await _assessmentRepository
            .FirstOrDefaultAsync(a => a.Id == input.AssessmentId && a.TenantId == AbpSession.TenantId);

        if (assessment == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.AssessmentNotFound,
                "Assessment not found.");

        // Cannot add questions to published assessment
        if (assessment.IsPublished)
            throw new UserFriendlyException(AssessmentExceptionCodes.CannotEditPublishedQuestion,
                "Cannot add questions to a published assessment.");

        // Check for duplicate question number within assessment
        var duplicateNumber = await _questionRepository
            .GetAll()
            .Where(q => q.Assessment.TenantId == AbpSession.TenantId)
            .AnyAsync(q => q.AssessmentId == input.AssessmentId && q.QuestionNumber == input.QuestionNumber);

        if (duplicateNumber)
            throw new UserFriendlyException(AssessmentExceptionCodes.DuplicateQuestionNumber,
                $"Question number {input.QuestionNumber} already exists in this assessment.");

        // Validate total marks won't exceed assessment max marks
        var existingTotalMarks = await _questionRepository
            .GetAll()
            .Where(q => q.Assessment.TenantId == AbpSession.TenantId)
            .Where(q => q.AssessmentId == input.AssessmentId)
            .SumAsync(q => q.Marks);

        if (existingTotalMarks + input.Marks > assessment.MaxMarks)
            throw new UserFriendlyException(AssessmentExceptionCodes.QuestionTotalExceedsMax,
                $"Adding this question ({input.Marks} marks) would exceed the assessment's maximum marks ({assessment.MaxMarks}). Current total: {existingTotalMarks}.");

        // Validate options for multiple choice / true-false
        if ((input.QuestionType == Domain.Shared.Enums.QuestionType.MultipleChoice ||
             input.QuestionType == Domain.Shared.Enums.QuestionType.TrueFalse) &&
            string.IsNullOrWhiteSpace(input.Options))
        {
            throw new UserFriendlyException(AssessmentExceptionCodes.InvalidOptionsForType,
                "Options are required for multiple choice and true/false questions.");
        }

        var question = new AssessmentQuestion(
            Guid.NewGuid(),
            input.AssessmentId,
            input.QuestionNumber,
            input.QuestionType,
            input.QuestionText,
            input.Marks)
        {
            CorrectAnswer = input.CorrectAnswer,
            Explanation = input.Explanation,
            CognitiveLevel = input.CognitiveLevel
        };

        // Set options via entity method for validation
        if (!string.IsNullOrWhiteSpace(input.Options) &&
            (input.QuestionType == Domain.Shared.Enums.QuestionType.MultipleChoice ||
             input.QuestionType == Domain.Shared.Enums.QuestionType.TrueFalse))
        {
            question.SetOptions(input.Options);
        }
        else if (!string.IsNullOrWhiteSpace(input.Options))
        {
            question.Options = input.Options;
        }

        await _questionRepository.InsertAsync(question);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(question.Id);
    }

    [AbpAuthorize(PermissionNames.Assessment_Quizzes_Edit)]
    public async Task<AssessmentQuestionDto> UpdateAsync(Guid id, UpdateAssessmentQuestionDto input)
    {
        var question = await _questionRepository
            .GetAll()
            .Include(q => q.Assessment)
            .Where(q => q.Assessment.TenantId == AbpSession.TenantId)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (question == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.QuestionNotFound,
                "Assessment question not found.");

        // Cannot edit questions of published assessment
        if (question.Assessment.IsPublished)
            throw new UserFriendlyException(AssessmentExceptionCodes.CannotEditPublishedQuestion,
                "Cannot edit questions of a published assessment.");

        // Check duplicate question number if changing
        if (input.QuestionNumber.HasValue && input.QuestionNumber.Value != question.QuestionNumber)
        {
            var duplicateNumber = await _questionRepository
                .GetAll()
                .Where(q => q.Assessment.TenantId == AbpSession.TenantId)
                .AnyAsync(q => q.AssessmentId == question.AssessmentId
                    && q.QuestionNumber == input.QuestionNumber.Value
                    && q.Id != id);

            if (duplicateNumber)
                throw new UserFriendlyException(AssessmentExceptionCodes.DuplicateQuestionNumber,
                    $"Question number {input.QuestionNumber.Value} already exists in this assessment.");

            question.QuestionNumber = input.QuestionNumber.Value;
        }

        // Validate marks total if changing
        if (input.Marks.HasValue && input.Marks.Value != question.Marks)
        {
            var otherTotalMarks = await _questionRepository
                .GetAll()
                .Where(q => q.Assessment.TenantId == AbpSession.TenantId)
                .Where(q => q.AssessmentId == question.AssessmentId && q.Id != id)
                .SumAsync(q => q.Marks);

            if (otherTotalMarks + input.Marks.Value > question.Assessment.MaxMarks)
                throw new UserFriendlyException(AssessmentExceptionCodes.QuestionTotalExceedsMax,
                    $"Updating this question to {input.Marks.Value} marks would exceed the assessment's maximum marks ({question.Assessment.MaxMarks}).");

            question.Marks = input.Marks.Value;
        }

        if (input.QuestionType.HasValue) question.QuestionType = input.QuestionType.Value;
        if (input.QuestionText != null) question.QuestionText = input.QuestionText;
        if (input.Options != null) question.Options = input.Options;
        if (input.CorrectAnswer != null) question.CorrectAnswer = input.CorrectAnswer;
        if (input.Explanation != null) question.Explanation = input.Explanation;
        if (input.CognitiveLevel.HasValue) question.CognitiveLevel = input.CognitiveLevel.Value;
        if (input.IsActive.HasValue) question.IsActive = input.IsActive.Value;

        await _questionRepository.UpdateAsync(question);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Assessment_Quizzes_Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var question = await _questionRepository
            .GetAll()
            .Include(q => q.Assessment)
            .Where(q => q.Assessment.TenantId == AbpSession.TenantId)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (question == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.QuestionNotFound,
                "Assessment question not found.");

        // Cannot delete questions of published assessment
        if (question.Assessment.IsPublished)
            throw new UserFriendlyException(AssessmentExceptionCodes.CannotEditPublishedQuestion,
                "Cannot delete questions of a published assessment.");

        await _questionRepository.DeleteAsync(question);
    }

    [AbpAuthorize(PermissionNames.Assessment_Quizzes_Edit)]
    public async Task ReorderAsync(ReorderQuestionsDto input)
    {
        // Validate assessment exists and belongs to tenant
        var assessment = await _assessmentRepository
            .FirstOrDefaultAsync(a => a.Id == input.AssessmentId && a.TenantId == AbpSession.TenantId);

        if (assessment == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.AssessmentNotFound,
                "Assessment not found.");

        // Cannot reorder questions of published assessment
        if (assessment.IsPublished)
            throw new UserFriendlyException(AssessmentExceptionCodes.CannotEditPublishedQuestion,
                "Cannot reorder questions of a published assessment.");

        var questions = await _questionRepository
            .GetAll()
            .Where(q => q.Assessment.TenantId == AbpSession.TenantId)
            .Where(q => q.AssessmentId == input.AssessmentId)
            .ToListAsync();

        for (int i = 0; i < input.QuestionIdsInOrder.Count; i++)
        {
            var question = questions.FirstOrDefault(q => q.Id == input.QuestionIdsInOrder[i]);
            if (question != null)
            {
                question.QuestionNumber = i + 1;
            }
        }

        await CurrentUnitOfWork.SaveChangesAsync();
    }
}
