using psms.Domain.Shared.Enums;
using psms.Workflow.Engine;
using psms.Workflow.Engine.Decisions;
using psms.Workflow.WorkflowInstances.Dto;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace psms.Tests.Workflow;

/// <summary>
/// What the server accepts and what the dialog asks for are one declaration.
/// <para>
/// The rejection reason rule (ADM-020) and the conditions-of-acceptance rule
/// lived inside the schema's <c>Validate</c>, where the dialog could not see
/// them. A principal filled the whole form in, pressed OK, and was refused.
/// They are now conditions on the reason field: the dialog builds its own rule
/// from them, and the server enforces the same ones through
/// <see cref="WorkflowDecisionFieldRules"/>.
/// </para>
/// </summary>
public class TheReasonRulesAreOnTheField_Tests
{
    private readonly ApplicationDecisionSchema _schema = new();

    /// <summary>The composition the engine uses: the schema's own pass, then the fields'.</summary>
    private string Check(params (string Key, object Value)[] values)
    {
        var decision = new WorkflowDecision(values.ToDictionary(v => v.Key, v => v.Value));
        return _schema.Validate(decision)
            ?? WorkflowDecisionFieldRules.Validate(_schema.Fields, decision);
    }

    private static (string, object) Decision(AdmissionDecision d) => ("decision", d.ToString());

    private const string LongEnough =
        "The applicant did not meet the minimum academic requirements for this grade.";

    [Fact]
    public void No_decision_is_still_the_first_thing_said()
    {
        Check(("reason", "anything")).ShouldBe("Select a decision.");
    }

    [Fact]
    public void Rejecting_without_a_reason_is_refused()
    {
        Check(Decision(AdmissionDecision.Rejected))
            .ShouldBe("A rejection reason of at least 50 characters is required.");
    }

    [Fact]
    public void Rejecting_with_a_short_reason_is_refused()
    {
        Check(Decision(AdmissionDecision.Rejected), ("reason", "Not good enough"))
            .ShouldBe("A rejection reason of at least 50 characters is required.");
    }

    [Fact]
    public void Rejecting_with_a_real_reason_is_accepted()
    {
        LongEnough.Length.ShouldBeGreaterThanOrEqualTo(ApplicationDecisionSchema.MinRejectionReasonLength);
        Check(Decision(AdmissionDecision.Rejected), ("reason", LongEnough)).ShouldBeNull();
    }

    [Fact]
    public void Accepting_with_conditions_has_to_state_them()
    {
        Check(Decision(AdmissionDecision.ConditionalAcceptance))
            .ShouldBe("State the conditions of acceptance.");

        Check(Decision(AdmissionDecision.ConditionalAcceptance), ("reason", "Pass the entrance test."))
            .ShouldBeNull();
    }

    [Fact]
    public void A_plain_acceptance_needs_no_reason()
    {
        Check(Decision(AdmissionDecision.Accepted)).ShouldBeNull();
    }

    [Fact]
    public void Waitlisting_needs_no_reason()
    {
        // What the principal was doing when this came up: a waitlist with a
        // one-line note, which nothing should object to.
        Check(Decision(AdmissionDecision.Waitlisted)).ShouldBeNull();
        Check(Decision(AdmissionDecision.Waitlisted), ("reason", "Good student")).ShouldBeNull();
    }

    [Fact]
    public void Pending_is_not_a_decision()
    {
        Check(Decision(AdmissionDecision.Pending)).ShouldBe("Pending is not a decision.");
    }

    [Fact]
    public void The_offer_window_keeps_its_bounds()
    {
        Check(Decision(AdmissionDecision.Accepted), ("offerExpiryDays", 0))
            .ShouldBe("Offer valid for (days) must be between 1 and 60.");
        Check(Decision(AdmissionDecision.Accepted), ("offerExpiryDays", 90))
            .ShouldBe("Offer valid for (days) must be between 1 and 60.");
        Check(Decision(AdmissionDecision.Accepted), ("offerExpiryDays", 30)).ShouldBeNull();
    }

    [Fact]
    public void A_required_field_left_empty_is_named()
    {
        var fields = new[]
        {
            new WorkflowDecisionField { Key = "amount", Label = "Approved amount (R)", Type = "number", Required = true },
        };

        WorkflowDecisionFieldRules.Validate(fields, new WorkflowDecision(new Dictionary<string, object>()))
            .ShouldBe("Approved amount (R) is required.");
    }

    [Fact]
    public void A_condition_that_does_not_apply_says_nothing()
    {
        var fields = new[]
        {
            new WorkflowDecisionField
            {
                Key = "note", Label = "Note", Type = "textarea",
                Conditions = new[]
                {
                    new WorkflowDecisionCondition
                    {
                        WhenField = "mode", WhenValueIn = new[] { "Strict" },
                        Required = true, Message = "A note is required in strict mode.",
                    },
                },
            },
        };

        var relaxed = new WorkflowDecision(new Dictionary<string, object> { ["mode"] = "Relaxed" });
        WorkflowDecisionFieldRules.Validate(fields, relaxed).ShouldBeNull();

        var strict = new WorkflowDecision(new Dictionary<string, object> { ["mode"] = "strict" });
        WorkflowDecisionFieldRules.Validate(fields, strict).ShouldBe("A note is required in strict mode.");
    }

    [Fact]
    public void The_dialog_is_given_the_conditions_to_enforce()
    {
        var dto = WorkflowDecisionSchemaDto.From(_schema);
        var reason = dto.Fields.Single(f => f.Key == "reason");

        reason.Conditions.Count.ShouldBe(2);
        var rejection = reason.Conditions.Single(c => c.WhenValueIn.Contains(nameof(AdmissionDecision.Rejected)));
        rejection.WhenField.ShouldBe("decision");
        rejection.Required.ShouldBeTrue();
        rejection.MinLength.ShouldBe(ApplicationDecisionSchema.MinRejectionReasonLength);
    }
}
