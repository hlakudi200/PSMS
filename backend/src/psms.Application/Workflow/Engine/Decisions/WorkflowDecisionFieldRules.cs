using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace psms.Workflow.Engine.Decisions;

/// <summary>
/// Enforces what the decision fields themselves declare: required, numeric
/// bounds, and the conditional requirements of <see cref="WorkflowDecisionCondition"/>.
/// <para>
/// The dialog builds its field rules from the same declarations, so the two
/// cannot drift: whatever this refuses, the form could have said on the field
/// before the actor pressed OK. A schema's own <see cref="IWorkflowDecisionSchema.Validate"/>
/// runs first and keeps the cross-field rules too awkward to declare.
/// </para>
/// </summary>
public static class WorkflowDecisionFieldRules
{
    /// <summary>Returns the first thing wrong with the decision, or null.</summary>
    public static string Validate(IReadOnlyList<WorkflowDecisionField> fields, WorkflowDecision decision)
    {
        if (fields == null || decision == null) return null;

        foreach (var field in fields)
        {
            var value = decision.GetString(field.Key);
            var missing = string.IsNullOrWhiteSpace(value);

            if (field.Required && missing)
                return $"{field.Label} is required.";

            if (!missing && field.Type == "number")
            {
                var error = CheckNumber(field, decision);
                if (error != null) return error;
            }

            foreach (var condition in field.Conditions ?? Array.Empty<WorkflowDecisionCondition>())
            {
                if (!Applies(condition, decision)) continue;
                if (condition.Required && missing) return condition.Message;
                if (condition.MinLength.HasValue && (value?.Length ?? 0) < condition.MinLength.Value)
                    return condition.Message;
            }
        }

        return null;
    }

    private static string CheckNumber(WorkflowDecisionField field, WorkflowDecision decision)
    {
        var number = decision.GetDecimal(field.Key);
        if (number == null) return $"{field.Label} must be a number.";

        var tooSmall = field.Min.HasValue && number < field.Min.Value;
        var tooLarge = field.Max.HasValue && number > field.Max.Value;
        if (!tooSmall && !tooLarge) return null;

        if (field.Min.HasValue && field.Max.HasValue)
            return $"{field.Label} must be between {Plain(field.Min.Value)} and {Plain(field.Max.Value)}.";
        return tooSmall
            ? $"{field.Label} cannot be less than {Plain(field.Min.Value)}."
            : $"{field.Label} cannot be more than {Plain(field.Max.Value)}.";
    }

    /// <summary>Whole numbers read as whole numbers: "60", not "60.00".</summary>
    private static string Plain(decimal value) =>
        (value == Math.Truncate(value) ? value.ToString("0", CultureInfo.InvariantCulture)
                                       : value.ToString(CultureInfo.InvariantCulture));

    private static bool Applies(WorkflowDecisionCondition condition, WorkflowDecision decision)
    {
        if (condition == null
            || string.IsNullOrWhiteSpace(condition.WhenField)
            || condition.WhenValueIn == null) return false;

        var actual = decision.GetString(condition.WhenField);
        return actual != null
            && condition.WhenValueIn.Any(v => string.Equals(v, actual, StringComparison.OrdinalIgnoreCase));
    }
}
