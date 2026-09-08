using Abp.Application.Services;
using Abp.Authorization;
using psms.Authorization;
using psms.Domain.Workflow.Enums;
using psms.Workflow.WorkflowInstances.Dto;
using System.Collections.Generic;
using System.Linq;

namespace psms.Workflow.Engine;

/// <summary>
/// WF-30/31/32: the catalogue of guards, effects and decision schemas the
/// definitions editor can attach to a step, per entity type.
/// </summary>
public interface IWorkflowExtensionAppService : IApplicationService
{
    WorkflowExtensionCatalogDto GetAvailable(WorkflowEntityType entityType);
}

public class WorkflowExtensionCatalogDto
{
    public WorkflowEntityType EntityType { get; set; }
    public List<WorkflowExtensionItemDto> Guards { get; set; } = new();
    public List<WorkflowExtensionItemDto> Effects { get; set; } = new();
    public List<WorkflowDecisionSchemaDto> DecisionSchemas { get; set; } = new();
    /// <summary>Whether the entity type has a terminal write-back handler at all.</summary>
    public bool HasEntityHandler { get; set; }
}

public class WorkflowExtensionItemDto
{
    public string Key { get; set; }
    public string DisplayName { get; set; }
}

[AbpAuthorize(PermissionNames.Workflow_Definitions_View)]
public class WorkflowExtensionAppService : ApplicationService, IWorkflowExtensionAppService
{
    private readonly WorkflowExtensionRegistry _registry;

    public WorkflowExtensionAppService(WorkflowExtensionRegistry registry)
    {
        _registry = registry;
    }

    public WorkflowExtensionCatalogDto GetAvailable(WorkflowEntityType entityType)
    {
        return new WorkflowExtensionCatalogDto
        {
            EntityType = entityType,
            Guards = _registry.Guards.Where(g => g.EntityType == entityType)
                .OrderBy(g => g.DisplayName)
                .Select(g => new WorkflowExtensionItemDto { Key = g.Key, DisplayName = g.DisplayName }).ToList(),
            Effects = _registry.Effects.Where(e => e.EntityType == entityType)
                .OrderBy(e => e.DisplayName)
                .Select(e => new WorkflowExtensionItemDto { Key = e.Key, DisplayName = e.DisplayName }).ToList(),
            DecisionSchemas = _registry.DecisionSchemas.Where(s => s.EntityType == entityType)
                .OrderBy(s => s.DisplayName)
                .Select(WorkflowDecisionSchemaDto.From).ToList(),
            HasEntityHandler = _registry.GetEntityHandler(entityType) != null,
        };
    }
}
