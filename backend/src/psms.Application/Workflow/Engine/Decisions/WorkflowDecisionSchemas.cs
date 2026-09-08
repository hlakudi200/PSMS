using Abp.Dependency;
using psms.Domain.Shared.Enums;
using psms.Domain.Workflow.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace psms.Workflow.Engine.Decisions;

/// <summary>Fee waiver terminal approval: the approved amount (may be partial) and review notes.</summary>
public class FeeWaiverApprovalSchema : IWorkflowDecisionSchema, ITransientDependency
{
    public string Key => "feewaiver.approval";
    public WorkflowEntityType EntityType => WorkflowEntityType.FeeWaiver;
    public string DisplayName => "Approved amount and notes";

    public IReadOnlyList<WorkflowDecisionField> Fields { get; } = new[]
    {
        new WorkflowDecisionField { Key = "approvedAmount", Label = "Approved amount (R)", Type = "number", Required = true, Min = 0, Placeholder = "Defaults to the requested amount" },
        new WorkflowDecisionField { Key = "notes", Label = "Review notes", Type = "textarea", Required = false },
    };

    public string Validate(WorkflowDecision d)
    {
        var amount = d.GetDecimal("approvedAmount");
        if (amount == null) return "Approved amount is required.";
        if (amount < 0) return "Approved amount cannot be negative.";
        return null;
    }
}

/// <summary>Field trip terminal approval: the approved budget.</summary>
public class FieldTripApprovalSchema : IWorkflowDecisionSchema, ITransientDependency
{
    public string Key => "fieldtrip.approval";
    public WorkflowEntityType EntityType => WorkflowEntityType.FieldTrip;
    public string DisplayName => "Approved budget";

    public IReadOnlyList<WorkflowDecisionField> Fields { get; } = new[]
    {
        new WorkflowDecisionField { Key = "approvedBudget", Label = "Approved budget (R)", Type = "number", Required = true, Min = 0, Placeholder = "Defaults to the estimated cost" },
    };

    public string Validate(WorkflowDecision d)
    {
        var budget = d.GetDecimal("approvedBudget");
        if (budget == null) return "Approved budget is required.";
        if (budget < 0) return "Approved budget cannot be negative.";
        return null;
    }
}

/// <summary>Expense terminal approval: the approved amount (may be partial).</summary>
public class ExpenseApprovalSchema : IWorkflowDecisionSchema, ITransientDependency
{
    public string Key => "expense.approval";
    public WorkflowEntityType EntityType => WorkflowEntityType.ExpenseRequest;
    public string DisplayName => "Approved amount";

    public IReadOnlyList<WorkflowDecisionField> Fields { get; } = new[]
    {
        new WorkflowDecisionField { Key = "approvedAmount", Label = "Approved amount (R)", Type = "number", Required = true, Min = 0, Placeholder = "Defaults to the requested amount" },
    };

    public string Validate(WorkflowDecision d)
    {
        var amount = d.GetDecimal("approvedAmount");
        if (amount == null) return "Approved amount is required.";
        if (amount < 0) return "Approved amount cannot be negative.";
        return null;
    }
}

/// <summary>Finance / HOD review steps: a recommended amount carried forward to the decision step.</summary>
public class RecommendedAmountSchema : IWorkflowDecisionSchema, ITransientDependency
{
    public string Key => "review.recommended-amount";
    public WorkflowEntityType EntityType => WorkflowEntityType.FeeWaiver;
    public string DisplayName => "Recommended amount";

    public IReadOnlyList<WorkflowDecisionField> Fields { get; } = new[]
    {
        new WorkflowDecisionField { Key = "recommendedAmount", Label = "Recommended amount (R)", Type = "number", Required = false, Min = 0 },
    };

    public string Validate(WorkflowDecision d)
    {
        var amount = d.GetDecimal("recommendedAmount");
        return amount is < 0 ? "Recommended amount cannot be negative." : null;
    }
}

/// <summary>Report terminal approval: the principal's comment printed on the report.</summary>
public class ReportApprovalSchema : IWorkflowDecisionSchema, ITransientDependency
{
    public string Key => "report.approval";
    public WorkflowEntityType EntityType => WorkflowEntityType.Report;
    public string DisplayName => "Principal's comment";

    public IReadOnlyList<WorkflowDecisionField> Fields { get; } = new[]
    {
        new WorkflowDecisionField { Key = "principalComment", Label = "Principal's comment", Type = "textarea", Required = false, Max = 1000 },
    };

    public string Validate(WorkflowDecision d)
    {
        var c = d.GetString("principalComment");
        return c != null && c.Length > 1000 ? "Principal's comment must be 1000 characters or fewer." : null;
    }
}

/// <summary>
/// Admissions terminal decision (ADM-017): accept, accept with conditions, reject
/// (reason of at least 50 characters, ADM-020) or waitlist.
/// </summary>
public class ApplicationDecisionSchema : IWorkflowDecisionSchema, ITransientDependency
{
    public const int MinRejectionReasonLength = 50;

    public string Key => "application.decision";
    public WorkflowEntityType EntityType => WorkflowEntityType.Application;
    public string DisplayName => "Admission decision";

    public IReadOnlyList<WorkflowDecisionField> Fields { get; } = new[]
    {
        new WorkflowDecisionField
        {
            Key = "decision", Label = "Decision", Type = "select", Required = true,
            Options = new[]
            {
                new WorkflowDecisionOption { Value = nameof(AdmissionDecision.Accepted), Label = "Accept" },
                new WorkflowDecisionOption { Value = nameof(AdmissionDecision.ConditionalAcceptance), Label = "Accept with conditions" },
                new WorkflowDecisionOption { Value = nameof(AdmissionDecision.Waitlisted), Label = "Place on waitlist" },
                new WorkflowDecisionOption { Value = nameof(AdmissionDecision.Rejected), Label = "Reject" },
            },
        },
        new WorkflowDecisionField { Key = "reason", Label = "Reason / conditions", Type = "textarea", Required = false, Placeholder = "Required for rejection (min 50 characters) and conditional acceptance" },
        new WorkflowDecisionField { Key = "offerExpiryDays", Label = "Offer valid for (days)", Type = "number", Required = false, Min = 1, Max = 60, Placeholder = "14" },
    };

    public string Validate(WorkflowDecision d)
    {
        var decision = d.GetEnum<AdmissionDecision>("decision");
        if (decision == null) return "Select a decision.";
        var reason = d.GetString("reason");
        switch (decision)
        {
            case AdmissionDecision.Rejected when string.IsNullOrWhiteSpace(reason) || reason.Length < MinRejectionReasonLength:
                return $"A rejection reason of at least {MinRejectionReasonLength} characters is required.";
            case AdmissionDecision.ConditionalAcceptance when string.IsNullOrWhiteSpace(reason):
                return "State the conditions of acceptance.";
            case AdmissionDecision.Pending:
                return "Pending is not a decision.";
        }
        var days = d.GetInt("offerExpiryDays");
        if (days is < 1 or > 60) return "Offer validity must be between 1 and 60 days.";
        return null;
    }
}
