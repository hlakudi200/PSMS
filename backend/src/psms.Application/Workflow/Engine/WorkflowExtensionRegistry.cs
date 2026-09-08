using Abp.Dependency;
using Abp.UI;
using psms.Domain.Workflow.Enums;
using psms.Workflow.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace psms.Workflow.Engine;

/// <summary>
/// Resolves the registered guards, effects, decision schemas and entity handlers
/// (see <see cref="IWorkflowExtension"/>) and looks them up by key. Keys are
/// compared case-insensitively; a key stored on a step that no longer resolves is
/// a configuration error and is reported as such rather than silently ignored.
/// </summary>
public class WorkflowExtensionRegistry : ITransientDependency
{
    private readonly IIocResolver _iocResolver;

    public WorkflowExtensionRegistry(IIocResolver iocResolver)
    {
        _iocResolver = iocResolver;
    }

    public IReadOnlyList<IWorkflowStepGuard> Guards => _iocResolver.ResolveAll<IWorkflowStepGuard>();
    public IReadOnlyList<IWorkflowStepEffect> Effects => _iocResolver.ResolveAll<IWorkflowStepEffect>();
    public IReadOnlyList<IWorkflowDecisionSchema> DecisionSchemas => _iocResolver.ResolveAll<IWorkflowDecisionSchema>();
    public IReadOnlyList<IWorkflowEntityHandler> EntityHandlers => _iocResolver.ResolveAll<IWorkflowEntityHandler>();

    public IWorkflowStepGuard GetGuard(string key, WorkflowEntityType entityType) =>
        Find(Guards, key, entityType, "guard");

    public IWorkflowStepEffect GetEffect(string key, WorkflowEntityType entityType) =>
        Find(Effects, key, entityType, "effect");

    public IWorkflowDecisionSchema GetDecisionSchema(string key, WorkflowEntityType entityType) =>
        Find(DecisionSchemas, key, entityType, "decision schema");

    public IWorkflowEntityHandler GetEntityHandler(WorkflowEntityType entityType) =>
        EntityHandlers.FirstOrDefault(h => h.EntityType == entityType);

    /// <summary>
    /// Returns null for a null/blank key; throws for a key that does not resolve
    /// for the entity type (a misconfigured step must not pass silently).
    /// </summary>
    private static T Find<T>(IEnumerable<T> all, string key, WorkflowEntityType entityType, string kind)
        where T : IWorkflowExtension
    {
        if (string.IsNullOrWhiteSpace(key)) return default;
        var match = all.FirstOrDefault(x =>
            x.EntityType == entityType && string.Equals(x.Key, key.Trim(), StringComparison.OrdinalIgnoreCase));
        if (match == null)
            throw new UserFriendlyException(WorkflowExceptionCodes.ExtensionNotFound,
                $"No {kind} '{key}' is registered for {entityType}. Fix the step configuration.");
        return match;
    }

    public bool GuardExists(string key, WorkflowEntityType entityType) =>
        Exists(Guards, key, entityType);

    public bool EffectExists(string key, WorkflowEntityType entityType) =>
        Exists(Effects, key, entityType);

    public bool DecisionSchemaExists(string key, WorkflowEntityType entityType) =>
        Exists(DecisionSchemas, key, entityType);

    private static bool Exists<T>(IEnumerable<T> all, string key, WorkflowEntityType entityType)
        where T : IWorkflowExtension =>
        string.IsNullOrWhiteSpace(key)
        || all.Any(x => x.EntityType == entityType && string.Equals(x.Key, key.Trim(), StringComparison.OrdinalIgnoreCase));
}
