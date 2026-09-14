using psms.Domain.Workflow.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Workflow.Engine;

/// <summary>
/// WF-30/31/32 — the contract between the generic workflow engine and the records
/// it approves. Each entity type contributes small registered classes; the engine
/// resolves them by key from the step configuration and never references a
/// domain entity directly.
///
/// Registration: every implementation in this assembly is registered against its
/// base interface in <c>psmsApplicationModule</c> (BasedOn&lt;…&gt;().WithService.Base()),
/// and <see cref="WorkflowExtensionRegistry"/> resolves them all.
/// </summary>
public interface IWorkflowExtension
{
    /// <summary>Stable, lower-case, dotted key stored on the step, e.g. "application.documents-verified".</summary>
    string Key { get; }

    /// <summary>The entity type this extension applies to.</summary>
    WorkflowEntityType EntityType { get; }

    /// <summary>Human label for the definitions editor.</summary>
    string DisplayName { get; }
}

/// <summary>
/// WF-30: a step's exit criterion. Evaluated before a forward action leaves the
/// step; a failing guard blocks the advance with <see cref="WorkflowGuardResult.Message"/>.
/// </summary>
public interface IWorkflowStepGuard : IWorkflowExtension
{
    Task<WorkflowGuardResult> EvaluateAsync(Guid entityId);
}

public sealed class WorkflowGuardResult
{
    public bool Satisfied { get; init; }
    public string Message { get; init; }

    public static WorkflowGuardResult Ok() => new() { Satisfied = true };
    public static WorkflowGuardResult Fail(string message) => new() { Satisfied = false, Message = message };
}

/// <summary>
/// WF-31: a side effect on the linked record when an instance enters or leaves a
/// step. Runs inside the advance transaction; throw <c>UserFriendlyException</c>
/// (or let <c>InvalidOperationException</c> propagate — the engine translates it)
/// to roll the transition back.
/// </summary>
public interface IWorkflowStepEffect : IWorkflowExtension
{
    Task ApplyAsync(WorkflowEffectContext context);
}

/// <summary>
/// WF-32: the fields an actor must supply when taking a forward action on a step
/// (approved amount, admission decision, …). <see cref="Validate"/> enforces the
/// cross-field rules the flat <see cref="Fields"/> list cannot express.
/// </summary>
public interface IWorkflowDecisionSchema : IWorkflowExtension
{
    IReadOnlyList<WorkflowDecisionField> Fields { get; }

    /// <summary>Returns the validation error, or null when the decision is acceptable.</summary>
    string Validate(WorkflowDecision decision);
}

public sealed class WorkflowDecisionField
{
    public string Key { get; init; }
    public string Label { get; init; }
    /// <summary>number | text | textarea | select | date | boolean</summary>
    public string Type { get; init; }
    public bool Required { get; init; }
    public string Placeholder { get; init; }
    public decimal? Min { get; init; }
    public decimal? Max { get; init; }
    public IReadOnlyList<WorkflowDecisionOption> Options { get; init; }
}

public sealed class WorkflowDecisionOption
{
    public string Value { get; init; }
    public string Label { get; init; }
}

/// <summary>
/// WF-31/32: the terminal write-back for one entity type — what happens to the
/// record when its workflow completes, is rejected, cancelled or recalled. Replaces
/// the switch that lived in <c>WorkflowEntityBridgeService</c>.
/// </summary>
public interface IWorkflowEntityHandler
{
    WorkflowEntityType EntityType { get; }
    Task OnCompletedAsync(WorkflowEffectContext context);
    Task OnRejectedAsync(WorkflowEffectContext context);
    Task OnCancelledAsync(WorkflowEffectContext context);
    Task OnRecalledAsync(WorkflowEffectContext context);
}

public sealed class WorkflowEffectContext
{
    public int? TenantId { get; init; }
    public WorkflowEntityType EntityType { get; init; }
    public Guid EntityId { get; init; }
    public long ActorUserId { get; init; }
    /// <summary>The actor's comment on the transition (rejection reason, waive reason …).</summary>
    public string Comment { get; init; }
    /// <summary>The validated decision payload, never null (may be empty).</summary>
    public WorkflowDecision Decision { get; init; }
}

/// <summary>
/// A tolerant reader over the raw decision dictionary. Values arrive from JSON
/// binding as strings, numbers, booleans or serializer wrapper objects depending
/// on the client, so every accessor parses from the value's string form.
/// </summary>
public sealed class WorkflowDecision
{
    private readonly Dictionary<string, object> _values;

    public WorkflowDecision(IDictionary<string, object> values)
    {
        _values = values == null
            ? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, object>(values, StringComparer.OrdinalIgnoreCase);
    }

    public static WorkflowDecision Empty => new(null);

    public bool IsEmpty => _values.Count == 0;
    public IReadOnlyDictionary<string, object> Raw => _values;

    public bool Has(string key) => _values.TryGetValue(key, out var v) && v != null && !string.IsNullOrWhiteSpace(v.ToString());

    public string GetString(string key)
    {
        if (!_values.TryGetValue(key, out var v) || v == null) return null;
        var s = v.ToString();
        return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }

    public decimal? GetDecimal(string key)
    {
        var s = GetString(key);
        if (s == null) return null;
        return decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : null;
    }

    public int? GetInt(string key)
    {
        var d = GetDecimal(key);
        return d.HasValue ? (int)Math.Round(d.Value) : null;
    }

    public bool? GetBool(string key)
    {
        var s = GetString(key);
        if (s == null) return null;
        if (bool.TryParse(s, out var b)) return b;
        if (s == "1") return true;
        if (s == "0") return false;
        return null;
    }

    public DateTime? GetDate(string key)
    {
        var s = GetString(key);
        if (s == null) return null;
        return DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var d)
            ? d
            : null;
    }

    public TEnum? GetEnum<TEnum>(string key) where TEnum : struct, Enum
    {
        var s = GetString(key);
        if (s == null) return null;
        if (Enum.TryParse<TEnum>(s, true, out var e) && Enum.IsDefined(typeof(TEnum), e)) return e;
        return null;
    }

    public IEnumerable<string> Keys => _values.Keys;
}
