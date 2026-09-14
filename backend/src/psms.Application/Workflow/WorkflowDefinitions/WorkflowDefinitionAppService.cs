using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Authorization;
using psms.Domain.Workflow.Entities;
using psms.Domain.Workflow.Enums;
using psms.Workflow.Shared;
using psms.Workflow.WorkflowDefinitions.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.Workflow.WorkflowDefinitions;

[AbpAuthorize(PermissionNames.Workflow_Definitions)]
public class WorkflowDefinitionAppService : ApplicationService, IWorkflowDefinitionAppService
{
    private readonly IRepository<WorkflowDefinition, Guid> _definitionRepository;
        private readonly IRepository<WorkflowInstance, Guid> _instanceRepository;
    private readonly IRepository<WorkflowStep, Guid> _stepRepository;

    public WorkflowDefinitionAppService(
        IRepository<WorkflowDefinition, Guid> definitionRepository,
        IRepository<WorkflowInstance, Guid> instanceRepository,
        IRepository<WorkflowStep, Guid> stepRepository)
    {
        _definitionRepository = definitionRepository;
        _instanceRepository = instanceRepository;
        _stepRepository = stepRepository;
    }

    private async Task<HashSet<Guid>> DefinitionsWithActiveInstancesAsync(IEnumerable<Guid> definitionIds)
    {
        var ids = definitionIds.ToList();
        var active = await _instanceRepository.GetAll()
            .Where(i => i.TenantId == AbpSession.TenantId
                && ids.Contains(i.WorkflowDefinitionId)
                && (i.Status == WorkflowStatus.NotStarted || i.Status == WorkflowStatus.InProgress))
            .Select(i => i.WorkflowDefinitionId)
            .Distinct()
            .ToListAsync();
        return active.ToHashSet();
    }

    [AbpAuthorize(PermissionNames.Workflow_Definitions_View)]
    public async Task<WorkflowDefinitionDto> GetAsync(Guid id)
    {
        var definition = await _definitionRepository
            .GetAll()
            .Include(d => d.Steps.OrderBy(s => s.StepOrder))
            .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == AbpSession.TenantId);

                if (definition == null)
            throw new UserFriendlyException(WorkflowExceptionCodes.DefinitionNotFound,
                "Workflow definition not found.");

        var dto = ObjectMapper.Map<WorkflowDefinitionDto>(definition);
        dto.HasActiveInstances = (await DefinitionsWithActiveInstancesAsync(new[] { id })).Contains(id);
        return dto;
    }

    [AbpAuthorize(PermissionNames.Workflow_Definitions_View)]
    public async Task<PagedResultDto<WorkflowDefinitionListDto>> GetAllAsync(GetWorkflowDefinitionsInput input)
    {
        var query = _definitionRepository
            .GetAll()
            .Include(d => d.Steps)
            .Where(d => d.TenantId == AbpSession.TenantId)
            .WhereIf(input.EntityType.HasValue, d => d.EntityType == input.EntityType.Value)
            .WhereIf(input.IsActive.HasValue, d => d.IsActive == input.IsActive.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Search),
                d => d.Name.ToLower().Contains(input.Search.Trim().ToLower()));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "Name")
            .PageBy(input)
            .ToListAsync();

        var dtos = ObjectMapper.Map<List<WorkflowDefinitionListDto>>(items);
        var locked = await DefinitionsWithActiveInstancesAsync(items.Select(d => d.Id));
        foreach (var dto in dtos) dto.HasActiveInstances = locked.Contains(dto.Id);
        return new PagedResultDto<WorkflowDefinitionListDto>(totalCount, dtos);
    }

    [AbpAuthorize(PermissionNames.Workflow_Definitions_Create)]
    public async Task<WorkflowDefinitionDto> CreateAsync(CreateWorkflowDefinitionDto input)
    {
        // Check duplicate name per tenant
        var nameExists = await _definitionRepository
            .GetAll()
            .AnyAsync(d => d.TenantId == AbpSession.TenantId
                && d.Name.ToLower() == input.Name.Trim().ToLower());

        if (nameExists)
            throw new UserFriendlyException(WorkflowExceptionCodes.DefinitionNameDuplicate,
                "A workflow definition with this name already exists.");

        // If activating, deactivate others of same entity type
        if (input.IsActive)
        {
            await DeactivateOthersForEntityType(input.EntityType);
        }

        var definition = new WorkflowDefinition
        {
            Id = Guid.NewGuid(),
            TenantId = AbpSession.TenantId,
            Name = input.Name.Trim(),
            Description = input.Description?.Trim(),
            EntityType = input.EntityType,
            IsActive = input.IsActive
        };

        await _definitionRepository.InsertAsync(definition);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(definition.Id);
    }

    [AbpAuthorize(PermissionNames.Workflow_Definitions_Edit)]
    public async Task<WorkflowDefinitionDto> UpdateAsync(Guid id, UpdateWorkflowDefinitionDto input)
    {
        var definition = await _definitionRepository
            .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == AbpSession.TenantId);

        if (definition == null)
            throw new UserFriendlyException(WorkflowExceptionCodes.DefinitionNotFound,
                "Workflow definition not found.");

        if (input.Name != null)
        {
            var trimmedName = input.Name.Trim();
            var nameExists = await _definitionRepository
                .GetAll()
                .AnyAsync(d => d.TenantId == AbpSession.TenantId
                    && d.Id != id
                    && d.Name.ToLower() == trimmedName.ToLower());

            if (nameExists)
                throw new UserFriendlyException(WorkflowExceptionCodes.DefinitionNameDuplicate,
                    "A workflow definition with this name already exists.");

            definition.Name = trimmedName;
        }

        if (input.Description != null) definition.Description = input.Description.Trim();

        await _definitionRepository.UpdateAsync(definition);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Workflow_Definitions_Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var definition = await _definitionRepository
            .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == AbpSession.TenantId);

        if (definition == null)
            throw new UserFriendlyException(WorkflowExceptionCodes.DefinitionNotFound,
                "Workflow definition not found.");

        // Check for active instances
        var hasActiveInstances = await _instanceRepository
            .GetAll()
            .AnyAsync(i => i.WorkflowDefinitionId == id
                && i.TenantId == AbpSession.TenantId
                && (i.Status == WorkflowStatus.NotStarted || i.Status == WorkflowStatus.InProgress));

        if (hasActiveInstances)
            throw new UserFriendlyException(WorkflowExceptionCodes.DefinitionHasActiveInstances,
                "Cannot delete a definition with active workflow instances.");

        await _definitionRepository.DeleteAsync(definition);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    [AbpAuthorize(PermissionNames.Workflow_Definitions_Activate)]
    public async Task<WorkflowDefinitionDto> ActivateAsync(Guid id)
    {
        var definition = await _definitionRepository
            .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == AbpSession.TenantId);

        if (definition == null)
            throw new UserFriendlyException(WorkflowExceptionCodes.DefinitionNotFound,
                "Workflow definition not found.");

        // Deactivate others of same entity type
        await DeactivateOthersForEntityType(definition.EntityType);

        definition.Activate();
        await _definitionRepository.UpdateAsync(definition);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Workflow_Definitions_Activate)]
    public async Task<WorkflowDefinitionDto> DeactivateAsync(Guid id)
    {
        var definition = await _definitionRepository
            .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == AbpSession.TenantId);

        if (definition == null)
            throw new UserFriendlyException(WorkflowExceptionCodes.DefinitionNotFound,
                "Workflow definition not found.");

        definition.Deactivate();
        await _definitionRepository.UpdateAsync(definition);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    /// <summary>
    /// WF-33: copy a definition (and every step, with its routing, guards, effects
    /// and decision schema) as a new INACTIVE definition. Running instances keep
    /// following the original; edit the clone, then Activate it so new instances
    /// start on the new version.
    /// </summary>
    [AbpAuthorize(PermissionNames.Workflow_Definitions_Create)]
    public async Task<WorkflowDefinitionDto> CloneAsync(Guid id, CloneWorkflowDefinitionDto input)
    {
        var source = await _definitionRepository
            .GetAll()
            .Include(d => d.Steps)
            .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == AbpSession.TenantId);
        if (source == null)
            throw new UserFriendlyException(WorkflowExceptionCodes.DefinitionNotFound,
                "Workflow definition not found.");

        var name = string.IsNullOrWhiteSpace(input?.Name) ? null : input.Name.Trim();
        if (name == null)
        {
            // Strip a trailing " v<n>" so cloning "Admissions Approval v2" yields
            // "Admissions Approval v3", not "Admissions Approval v2 v2".
            var baseName = System.Text.RegularExpressions.Regex.Replace(source.Name, @"\s+v\d+$", "");
            var n = 2;
            do { name = $"{baseName} v{n++}"; }
            while (await _definitionRepository.GetAll().AnyAsync(d => d.TenantId == AbpSession.TenantId && d.Name.ToLower() == name.ToLower()));
        }
        else if (await _definitionRepository.GetAll().AnyAsync(d => d.TenantId == AbpSession.TenantId && d.Name.ToLower() == name.ToLower()))
        {
            throw new UserFriendlyException(WorkflowExceptionCodes.DefinitionNameDuplicate,
                "A workflow definition with this name already exists.");
        }

        var clone = new WorkflowDefinition
        {
            Id = Guid.NewGuid(),
            TenantId = AbpSession.TenantId,
            Name = name,
            Description = source.Description,
            EntityType = source.EntityType,
            IsActive = false,
        };
        await _definitionRepository.InsertAsync(clone);

        foreach (var s in source.Steps)
        {
            await _stepRepository.InsertAsync(new WorkflowStep
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = clone.Id,
                StepOrder = s.StepOrder,
                Name = s.Name,
                Description = s.Description,
                AssignedRole = s.AssignedRole,
                ActionType = s.ActionType,
                IsTerminal = s.IsTerminal,
                NextStepOnApprove = s.NextStepOnApprove,
                NextStepOnReject = s.NextStepOnReject,
                IsCommentRequired = s.IsCommentRequired,
                AssignedUserId = s.AssignedUserId,
                SlaHours = s.SlaHours,
                GuardKey = s.GuardKey,
                EntryEffectKey = s.EntryEffectKey,
                ExitEffectKey = s.ExitEffectKey,
                DecisionSchemaKey = s.DecisionSchemaKey,
                IsOptional = s.IsOptional,
            });
        }

        await CurrentUnitOfWork.SaveChangesAsync();
        return await GetAsync(clone.Id);
    }

    private async Task DeactivateOthersForEntityType(WorkflowEntityType entityType)
    {
        var activeDefinitions = await _definitionRepository
            .GetAll()
            .Where(d => d.TenantId == AbpSession.TenantId
                && d.EntityType == entityType
                && d.IsActive)
            .ToListAsync();

        foreach (var def in activeDefinitions)
        {
            def.Deactivate();
        }
    }
}
