using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Authorization;
using psms.Domain.Workflow.Enums;
using psms.Workflow.Shared;
using psms.Discipline.DisciplinaryCases.Dto;
using psms.Discipline.Shared;
using psms.Domain.Discipline.Entities;
using psms.Authorization.Users;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.Discipline.DisciplinaryCases;

/// <summary>
/// Service for managing disciplinary cases.
/// </summary>
[AbpAuthorize(PermissionNames.Discipline_Cases)]
public class DisciplinaryCaseAppService : ApplicationService, IDisciplinaryCaseAppService
{
    private readonly WorkflowStarterService _workflowStarter;
    private readonly WorkflowInstanceGuard _workflowGuard;
    private readonly IRepository<DisciplinaryCase, Guid> _disciplinaryCaseRepository;
    private readonly UserManager _userManager;

    public DisciplinaryCaseAppService(
        IRepository<DisciplinaryCase, Guid> disciplinaryCaseRepository,
        UserManager userManager,
        WorkflowStarterService workflowStarter,
        WorkflowInstanceGuard workflowGuard)
    {
        _workflowStarter = workflowStarter;
        _workflowGuard = workflowGuard;
        _disciplinaryCaseRepository = disciplinaryCaseRepository;
        _userManager = userManager;
    }

    [AbpAuthorize(PermissionNames.Discipline_Cases_View)]
    public async Task<DisciplinaryCaseDto> GetAsync(Guid id)
    {
        var disciplinaryCase = await _disciplinaryCaseRepository
            .GetAll()
            .Include(dc => dc.Student)
            .Include(dc => dc.AcademicYear)
            .FirstOrDefaultAsync(dc => dc.Id == id && dc.TenantId == AbpSession.TenantId);

        if (disciplinaryCase == null)
            throw new UserFriendlyException(DisciplineExceptionCodes.CaseNotFound,
                "Disciplinary case not found.");

        return ObjectMapper.Map<DisciplinaryCaseDto>(disciplinaryCase);
    }

    [AbpAuthorize(PermissionNames.Discipline_Cases_View)]
    public async Task<PagedResultDto<DisciplinaryCaseListDto>> GetAllAsync(GetDisciplinaryCasesInput input)
    {
        var query = _disciplinaryCaseRepository
            .GetAll()
            .Include(dc => dc.Student)
            .Where(dc => dc.TenantId == AbpSession.TenantId)
            .WhereIf(input.StudentId.HasValue, dc => dc.StudentId == input.StudentId.Value)
            .WhereIf(input.AcademicYearId.HasValue, dc => dc.AcademicYearId == input.AcademicYearId.Value)
            .WhereIf(input.IncidentCategory.HasValue, dc => (int)dc.IncidentCategory == input.IncidentCategory.Value)
            .WhereIf(input.Severity.HasValue, dc => (int)dc.Severity == input.Severity.Value)
            .WhereIf(input.Status.HasValue, dc => (int)dc.Status == input.Status.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Search),
                dc => dc.CaseNumber.ToLower().Contains(input.Search.Trim().ToLower())
                    || dc.IncidentDescription.ToLower().Contains(input.Search.Trim().ToLower())
                    || (dc.Student != null && (dc.Student.FirstName + " " + dc.Student.LastName)
                        .ToLower().Contains(input.Search.Trim().ToLower())));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "CreationTime DESC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<DisciplinaryCaseListDto>(
            totalCount,
            ObjectMapper.Map<List<DisciplinaryCaseListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Discipline_Cases_Create)]
    public async Task<DisciplinaryCaseDto> CreateAsync(CreateDisciplinaryCaseDto input)
    {
        var caseNumber = await GenerateCaseNumberAsync();

        var disciplinaryCase = new DisciplinaryCase(
            Guid.NewGuid(),
            AbpSession.TenantId,
            caseNumber,
            input.StudentId,
            input.AcademicYearId,
            input.IncidentDate,
            input.IncidentDescription.Trim(),
            (DisciplinaryCategory)input.IncidentCategory,
            (DisciplinarySeverity)input.Severity,
            AbpSession.UserId.Value,
            await GetCurrentUserNameAsync());

        if (!string.IsNullOrWhiteSpace(input.Location))
            disciplinaryCase.Location = input.Location.Trim();

        if (!string.IsNullOrWhiteSpace(input.WitnessNames))
            disciplinaryCase.WitnessNames = input.WitnessNames.Trim();

        await _disciplinaryCaseRepository.InsertAsync(disciplinaryCase);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(disciplinaryCase.Id);
    }

    [AbpAuthorize(PermissionNames.Discipline_Cases_Edit)]
    public async Task<DisciplinaryCaseDto> UpdateAsync(Guid id, UpdateDisciplinaryCaseDto input)
    {
        var disciplinaryCase = await _disciplinaryCaseRepository
            .FirstOrDefaultAsync(dc => dc.Id == id && dc.TenantId == AbpSession.TenantId);

        if (disciplinaryCase == null)
            throw new UserFriendlyException(DisciplineExceptionCodes.CaseNotFound,
                "Disciplinary case not found.");

        if (disciplinaryCase.Status != DisciplinaryStatus.Draft)
            throw new UserFriendlyException(DisciplineExceptionCodes.InvalidStatusTransition,
                "Only draft cases can be updated.");

        if (input.IncidentDescription != null) disciplinaryCase.IncidentDescription = input.IncidentDescription.Trim();
        if (input.IncidentCategory.HasValue) disciplinaryCase.IncidentCategory = (DisciplinaryCategory)input.IncidentCategory.Value;
        if (input.Severity.HasValue) disciplinaryCase.Severity = (DisciplinarySeverity)input.Severity.Value;
        if (input.Location != null) disciplinaryCase.Location = input.Location.Trim();
        if (input.WitnessNames != null) disciplinaryCase.WitnessNames = input.WitnessNames.Trim();
        if (input.InvestigationNotes != null) disciplinaryCase.InvestigationNotes = input.InvestigationNotes.Trim();
        if (input.HearingNotes != null) disciplinaryCase.HearingNotes = input.HearingNotes.Trim();

        await _disciplinaryCaseRepository.UpdateAsync(disciplinaryCase);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Discipline_Cases_Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var disciplinaryCase = await _disciplinaryCaseRepository
            .FirstOrDefaultAsync(dc => dc.Id == id && dc.TenantId == AbpSession.TenantId);

        if (disciplinaryCase == null)
            throw new UserFriendlyException(DisciplineExceptionCodes.CaseNotFound,
                "Disciplinary case not found.");

        if (disciplinaryCase.Status != DisciplinaryStatus.Draft)
            throw new UserFriendlyException(DisciplineExceptionCodes.InvalidStatusTransition,
                "Only draft cases can be deleted.");

        await _disciplinaryCaseRepository.DeleteAsync(disciplinaryCase);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    [AbpAuthorize(PermissionNames.Discipline_Cases_Edit)]
    public async Task<DisciplinaryCaseDto> SubmitAsync(Guid id)
    {
        var disciplinaryCase = await _disciplinaryCaseRepository
            .FirstOrDefaultAsync(dc => dc.Id == id && dc.TenantId == AbpSession.TenantId);

        if (disciplinaryCase == null)
            throw new UserFriendlyException(DisciplineExceptionCodes.CaseNotFound,
                "Disciplinary case not found.");

        try
        {
            disciplinaryCase.Submit();
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(DisciplineExceptionCodes.InvalidStatusTransition, ex.Message);
        }

        await _disciplinaryCaseRepository.UpdateAsync(disciplinaryCase);
        await CurrentUnitOfWork.SaveChangesAsync();

        // WF-36: submitting starts the tenant's active approval workflow (no-op when
        // none is configured — the direct approve endpoints then remain available).
        await _workflowStarter.TryStartWorkflowAsync(
            AbpSession.TenantId, WorkflowEntityType.Disciplinary, id,
            AbpSession.UserId.Value, await GetCurrentUserNameAsync());
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Discipline_Cases_Manage)]
    public async Task<DisciplinaryCaseDto> StartInvestigationAsync(Guid id)
    {
        var disciplinaryCase = await _disciplinaryCaseRepository
            .FirstOrDefaultAsync(dc => dc.Id == id && dc.TenantId == AbpSession.TenantId);

        if (disciplinaryCase == null)
            throw new UserFriendlyException(DisciplineExceptionCodes.CaseNotFound,
                "Disciplinary case not found.");

        try
        {
            disciplinaryCase.StartInvestigation();
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(DisciplineExceptionCodes.InvalidStatusTransition, ex.Message);
        }

        await _disciplinaryCaseRepository.UpdateAsync(disciplinaryCase);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Discipline_Cases_Manage)]
    public async Task<DisciplinaryCaseDto> ScheduleHearingAsync(Guid id, DateTime hearingDate)
    {
        var disciplinaryCase = await _disciplinaryCaseRepository
            .FirstOrDefaultAsync(dc => dc.Id == id && dc.TenantId == AbpSession.TenantId);

        if (disciplinaryCase == null)
            throw new UserFriendlyException(DisciplineExceptionCodes.CaseNotFound,
                "Disciplinary case not found.");

        try
        {
            disciplinaryCase.ScheduleHearing(hearingDate);
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(DisciplineExceptionCodes.InvalidStatusTransition, ex.Message);
        }

        await _disciplinaryCaseRepository.UpdateAsync(disciplinaryCase);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Discipline_Cases_Manage)]
    public async Task<DisciplinaryCaseDto> RecordOutcomeAsync(Guid id, RecordOutcomeDto input)
    {
        var disciplinaryCase = await _disciplinaryCaseRepository
            .FirstOrDefaultAsync(dc => dc.Id == id && dc.TenantId == AbpSession.TenantId);

        if (disciplinaryCase == null)
            throw new UserFriendlyException(DisciplineExceptionCodes.CaseNotFound,
                "Disciplinary case not found.");

        try
        {
            disciplinaryCase.RecordOutcome(
                (DisciplinaryOutcome)input.Outcome,
                input.OutcomeDescription.Trim(),
                AbpSession.UserId.Value);
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(DisciplineExceptionCodes.InvalidStatusTransition, ex.Message);
        }

        if (input.SanctionStartDate.HasValue)
            disciplinaryCase.SanctionStartDate = input.SanctionStartDate.Value;

        if (input.SanctionEndDate.HasValue)
            disciplinaryCase.SanctionEndDate = input.SanctionEndDate.Value;

        await _disciplinaryCaseRepository.UpdateAsync(disciplinaryCase);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Discipline_Cases_Manage)]
    public async Task<DisciplinaryCaseDto> NotifyParentAsync(Guid id)
    {
        var disciplinaryCase = await _disciplinaryCaseRepository
            .FirstOrDefaultAsync(dc => dc.Id == id && dc.TenantId == AbpSession.TenantId);

        if (disciplinaryCase == null)
            throw new UserFriendlyException(DisciplineExceptionCodes.CaseNotFound,
                "Disciplinary case not found.");

        disciplinaryCase.NotifyParent();

        await _disciplinaryCaseRepository.UpdateAsync(disciplinaryCase);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Discipline_Cases_Manage)]
    public async Task<DisciplinaryCaseDto> ResolveAsync(Guid id)
    {
        await _workflowGuard.EnsureNoActiveInstanceAsync(WorkflowEntityType.Disciplinary, id);

        var disciplinaryCase = await _disciplinaryCaseRepository
            .FirstOrDefaultAsync(dc => dc.Id == id && dc.TenantId == AbpSession.TenantId);

        if (disciplinaryCase == null)
            throw new UserFriendlyException(DisciplineExceptionCodes.CaseNotFound,
                "Disciplinary case not found.");

        try
        {
            disciplinaryCase.Resolve(AbpSession.UserId.Value);
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(DisciplineExceptionCodes.InvalidStatusTransition, ex.Message);
        }

        await _disciplinaryCaseRepository.UpdateAsync(disciplinaryCase);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Discipline_Cases_Delete)]
    public async Task<DisciplinaryCaseDto> CancelAsync(Guid id)
    {
        var disciplinaryCase = await _disciplinaryCaseRepository
            .FirstOrDefaultAsync(dc => dc.Id == id && dc.TenantId == AbpSession.TenantId);

        if (disciplinaryCase == null)
            throw new UserFriendlyException(DisciplineExceptionCodes.CaseNotFound,
                "Disciplinary case not found.");

        try
        {
            disciplinaryCase.Cancel();
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(DisciplineExceptionCodes.InvalidStatusTransition, ex.Message);
        }

        await _disciplinaryCaseRepository.UpdateAsync(disciplinaryCase);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    private async Task<string> GenerateCaseNumberAsync()
    {
        var tenantId = AbpSession.TenantId ?? 0;
        var year = DateTime.UtcNow.Year;
        var prefix = $"DC-{tenantId:D3}-{year}-";

        var maxNumber = await _disciplinaryCaseRepository
            .GetAll()
            .Where(dc => dc.TenantId == AbpSession.TenantId && dc.CaseNumber.StartsWith(prefix))
            .Select(dc => dc.CaseNumber)
            .MaxAsync(cn => (string)null == cn ? null : cn);

        var nextSequence = 1;
        if (maxNumber != null)
        {
            var lastPart = maxNumber.Substring(prefix.Length);
            if (int.TryParse(lastPart, out var lastSequence))
                nextSequence = lastSequence + 1;
        }

        return $"{prefix}{nextSequence:D4}";
    }

    private async Task<string> GetCurrentUserNameAsync()
    {
        var userId = AbpSession.UserId;
        if (!userId.HasValue) return "System";

        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        return user != null ? $"{user.Name} {user.Surname}" : "Unknown";
    }
}
