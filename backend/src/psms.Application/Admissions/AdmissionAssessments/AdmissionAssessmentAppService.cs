using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Admissions.AdmissionAssessments.Dto;
using psms.Admissions.Shared;
using psms.Authorization;
using psms.Authorization.Roles;
using psms.Authorization.Users;
using psms.Domain.Admissions.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.Admissions.AdmissionAssessments;

/// <summary>
/// Service for managing admission assessments.
/// Implements ADM-014 to ADM-016.
/// </summary>
[AbpAuthorize(PermissionNames.Admissions_Assessments)]
public class AdmissionAssessmentAppService : ApplicationService, IAdmissionAssessmentAppService
{
    private readonly IRepository<AdmissionAssessment, Guid> _assessmentRepository;
    private readonly IRepository<Application, Guid> _applicationRepository;
    private readonly UserManager _userManager;
    private readonly RoleManager _roleManager;

    private const int MinNoticeDaysRequired = 7; // ADM-015: 7 days minimum notice

    public AdmissionAssessmentAppService(
        IRepository<AdmissionAssessment, Guid> assessmentRepository,
        IRepository<Application, Guid> applicationRepository,
        UserManager userManager,
        RoleManager roleManager)
    {
        _assessmentRepository = assessmentRepository;
        _applicationRepository = applicationRepository;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    /// <summary>
    /// The assessments this person is down to set and mark.
    /// <para>
    /// The workflow's Placement Assessment step belongs to the teacher who
    /// writes the paper, and they had no way of finding out they were down for
    /// one: an assessment could only be read through the application it belongs
    /// to, and a teacher cannot read applications. Only ever the caller's own —
    /// being named the assessor is the authority.
    /// </para>
    /// </summary>
    [AbpAuthorize(PermissionNames.Admissions_Assessments_View)]
    public async Task<ListResultDto<AdmissionAssessmentDto>> GetMineAsync(bool includePast = false)
    {
        var userId = AbpSession.UserId;
        if (!userId.HasValue)
            return new ListResultDto<AdmissionAssessmentDto>(new List<AdmissionAssessmentDto>());

        var today = DateTime.UtcNow.Date;

        var mine = await _assessmentRepository
            .GetAll()
            .Include(a => a.Application)
            .Include(a => a.AssessedGrade)
            .Where(a => a.AssessorUserId == userId.Value)
            /* One whose day has passed with no result is not "past" — it is the
               thing most needing attention, so it stays on the list. */
            .WhereIf(!includePast, a => a.ScheduledDate >= today || a.CompletedDate == null)
            .OrderBy(a => a.ScheduledDate)
            .ToListAsync();

        return await MapManyAsync(mine);
    }

    /// <summary>
    /// Who may be put down to set and mark one: everybody holding a role the
    /// school has granted <c>Admissions.Assessments.Conduct</c>, with how many
    /// they are already down for.
    /// </summary>
    [AbpAuthorize(PermissionNames.Admissions_Assessments_Schedule)]
    public async Task<ListResultDto<AssessorDto>> GetAssessorsAsync()
    {
        var found = new Dictionary<long, AssessorDto>();

        foreach (var role in await _roleManager.Roles.ToListAsync())
        {
            if (!await _roleManager.IsGrantedAsync(role.Id, PermissionNames.Admissions_Assessments_Conduct))
                continue;

            foreach (var user in await _userManager.GetUsersInRoleAsync(role.NormalizedName))
            {
                if (!user.IsActive)
                    continue;

                if (found.TryGetValue(user.Id, out var already))
                {
                    already.Roles.Add(role.DisplayName ?? role.Name);
                    continue;
                }

                found[user.Id] = new AssessorDto
                {
                    UserId = user.Id,
                    Name = user.FullName,
                    EmailAddress = user.EmailAddress,
                    Roles = new List<string> { role.DisplayName ?? role.Name },
                };
            }
        }

        var ids = found.Keys.ToList();
        var loads = await _assessmentRepository
            .GetAll()
            .Where(a => ids.Contains(a.AssessorUserId) && a.CompletedDate == null)
            .GroupBy(a => a.AssessorUserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToListAsync();

        foreach (var load in loads)
            found[load.UserId].OutstandingAssessments = load.Count;

        return new ListResultDto<AssessorDto>(found.Values.OrderBy(a => a.Name).ToList());
    }

    /// <summary>
    /// Resolves the person an assessment is being set for, and refuses anyone
    /// who could not mark it. The id used to be taken from the request with
    /// nothing checking it belonged to somebody who could open the result.
    /// </summary>
    private async Task<User> ResolveAssessorAsync(long assessorUserId)
    {
        var user = await _userManager.FindByIdAsync(assessorUserId.ToString());

        if (user == null || !user.IsActive)
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidAssessmentSchedule,
                "That assessor is not someone at this school.");

        if (!await _userManager.IsGrantedAsync(user.Id, PermissionNames.Admissions_Assessments_Conduct))
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidAssessmentSchedule,
                $"{user.FullName} is not able to set admission assessments. Choose somebody else, or ask an administrator to give them the role.");

        return user;
    }

    /// <summary>
    /// Recording a result belongs to the person who marked the paper, or to the
    /// admissions staff who arrange assessments. Anyone else holding the
    /// permission would be entering somebody else's marking under their name.
    /// </summary>
    private async Task AssertMayRecordAsync(AdmissionAssessment assessment)
    {
        if (AbpSession.UserId.HasValue && assessment.AssessorUserId == AbpSession.UserId.Value)
            return;

        if (await PermissionChecker.IsGrantedAsync(PermissionNames.Admissions_Assessments_Schedule))
            return;

        throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidAssessmentSchedule,
            "This assessment is the assessor's to record.");
    }

    /// <summary>
    /// The assessor's name, which the entity does not store — only their user
    /// id. The mapper ignored it with a note saying the service would fill it
    /// in, and no service ever did, so every assessment came back with no
    /// assessor at all.
    /// </summary>
    private async Task<ListResultDto<AdmissionAssessmentDto>> MapManyAsync(List<AdmissionAssessment> assessments)
    {
        var dtos = assessments.Select(MapToDto).ToList();
        await NameTheAssessorsAsync(dtos);
        return new ListResultDto<AdmissionAssessmentDto>(dtos);
    }

    private async Task<AdmissionAssessmentDto> MapOneAsync(AdmissionAssessment assessment)
    {
        var dto = MapToDto(assessment);
        await NameTheAssessorsAsync(new List<AdmissionAssessmentDto> { dto });
        return dto;
    }

    private async Task NameTheAssessorsAsync(List<AdmissionAssessmentDto> dtos)
    {
        var ids = dtos.Select(d => d.AssessorUserId).Distinct().ToList();
        if (ids.Count == 0) return;

        var names = await _userManager.Users
            .Where(u => ids.Contains(u.Id))
            .Select(u => new { u.Id, u.Name, u.Surname })
            .ToListAsync();

        foreach (var dto in dtos)
        {
            var who = names.FirstOrDefault(n => n.Id == dto.AssessorUserId);
            dto.AssessorName = who == null ? null : $"{who.Name} {who.Surname}".Trim();
        }
    }

    [AbpAuthorize(PermissionNames.Admissions_Assessments_View)]
    public async Task<AdmissionAssessmentDto> GetAsync(Guid id)
    {
        var assessment = await _assessmentRepository
            .GetAll()
            .Include(a => a.Application)
                .ThenInclude(app => app.AppliedGrade)
            .Include(a => a.AssessedGrade)
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == AbpSession.TenantId);

        if (assessment == null)
            throw new UserFriendlyException(AdmissionsExceptionCodes.AssessmentNotFound, "Assessment not found.");

        return await MapOneAsync(assessment);
    }

    [AbpAuthorize(PermissionNames.Admissions_Assessments_View)]
    public async Task<AdmissionAssessmentDto> GetByApplicationAsync(Guid applicationId)
    {
        var assessment = await _assessmentRepository
            .GetAll()
            .Include(a => a.Application)
                .ThenInclude(app => app.AppliedGrade)
            .Include(a => a.AssessedGrade)
            .FirstOrDefaultAsync(a => a.ApplicationId == applicationId && a.TenantId == AbpSession.TenantId);

        if (assessment == null)
            return null;

        return await MapOneAsync(assessment);
    }

    [AbpAuthorize(PermissionNames.Admissions_Assessments_View)]
    public async Task<PagedResultDto<AdmissionAssessmentDto>> GetAllAsync(GetAdmissionAssessmentsInput input)
    {
        var query = _assessmentRepository
            .GetAll()
            .Include(a => a.Application)
                .ThenInclude(app => app.AppliedGrade)
            .Include(a => a.AssessedGrade)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                a => a.Application.ProspectiveStudentFirstName.ToLower().Contains(input.Keyword.ToLower())
                    || a.Application.ProspectiveStudentLastName.ToLower().Contains(input.Keyword.ToLower()));

        var totalCount = await query.CountAsync();

        var assessments = await query
            .OrderBy(input.Sorting ?? "ScheduledDate DESC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<AdmissionAssessmentDto>(
            totalCount,
            (await MapManyAsync(assessments)).Items.ToList());
    }

    [AbpAuthorize(PermissionNames.Admissions_Assessments_Schedule)]
    public async Task<AdmissionAssessmentDto> ScheduleAsync(ScheduleAssessmentDto input)
    {
        var application = await _applicationRepository.GetAsync(input.ApplicationId);

        if (application.Status != ApplicationStatus.UnderReview)
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidStatusTransition,
                "Application must be under review to schedule an assessment.");

        // Check if assessment already exists
        var existingAssessment = await _assessmentRepository
            .FirstOrDefaultAsync(a => a.ApplicationId == input.ApplicationId);

        if (existingAssessment != null)
            throw new UserFriendlyException(AdmissionsExceptionCodes.AssessmentAlreadyScheduled,
                "An assessment has already been scheduled for this application.");

        // Validate minimum notice (ADM-015: 7 days)
        if (input.ScheduledDate < DateTime.UtcNow.AddDays(MinNoticeDaysRequired))
            throw new UserFriendlyException(AdmissionsExceptionCodes.InsufficientAssessmentNotice,
                $"Assessment must be scheduled at least {MinNoticeDaysRequired} days in advance.");

        // Checked rather than trusted: an assessment set for somebody who
        // cannot mark it fails on the day, not at the point of the mistake.
        var assessor = await ResolveAssessorAsync(input.AssessorUserId);

        var assessment = new AdmissionAssessment(
            Guid.NewGuid(),
            input.ApplicationId,
            input.Type,
            input.ScheduledDate,
            input.AssessedGradeId,
            assessor.Id)
        {
            TenantId = AbpSession.TenantId,
            Subjects = input.Subjects,
            MaxScore = input.MaxScore
        };

        await _assessmentRepository.InsertAsync(assessment);

        // Update application status
        application.ScheduleAssessment();
        await _applicationRepository.UpdateAsync(application);

        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(assessment.Id);
    }

    [AbpAuthorize(PermissionNames.Admissions_Assessments_RecordResults)]
    public async Task<AdmissionAssessmentDto> RecordResultsAsync(Guid id, RecordAssessmentResultsDto input)
    {
        var assessment = await _assessmentRepository.GetAsync(id);

        await AssertMayRecordAsync(assessment);

        if (assessment.CompletedDate.HasValue)
            throw new UserFriendlyException(AdmissionsExceptionCodes.AssessmentAlreadyCompleted,
                "Assessment results have already been recorded.");

        // Record results using entity method
        assessment.RecordResults(input.TotalScore, input.MaxScore, input.PassPercentage, input.Feedback);

        await _assessmentRepository.UpdateAsync(assessment);

        // Update application status
        var application = await _applicationRepository.GetAsync(assessment.ApplicationId);
        application.CompleteAssessment();
        await _applicationRepository.UpdateAsync(application);

        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Admissions_Assessments_Cancel)]
    public async Task CancelAsync(Guid id, string reason)
    {
        var assessment = await _assessmentRepository.GetAsync(id);

        if (assessment.CompletedDate.HasValue)
            throw new UserFriendlyException(AdmissionsExceptionCodes.CannotCancelAssessment,
                "Cannot cancel a completed assessment.");

        // Revert application status back to UnderReview
        var application = await _applicationRepository.GetAsync(assessment.ApplicationId);
        if (application.Status == ApplicationStatus.AssessmentScheduled)
        {
            application.RevertToUnderReview();
            await _applicationRepository.UpdateAsync(application);
        }

        // Delete the assessment
        await _assessmentRepository.DeleteAsync(id);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    #region Private Methods

    private AdmissionAssessmentDto MapToDto(AdmissionAssessment assessment)
    {
        var dto = ObjectMapper.Map<AdmissionAssessmentDto>(assessment);

        // Map additional properties
        dto.ApplicationNumber = assessment.Application?.ApplicationNumber;
        dto.ApplicantName = assessment.Application?.GetProspectiveStudentFullName();
        dto.GradeName = assessment.AssessedGrade?.GradeName;

        return dto;
    }

    #endregion
}
