using Microsoft.EntityFrameworkCore;
using psms.Domain.Shared.Enums;
using psms.Domain.Workflow.Entities;
using psms.Domain.Workflow.Enums;
using psms.Workflow.Engine;
using psms.Workflow.Engine.Decisions;
using psms.Workflow.Seed;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace psms.Tests.Workflow;

/// <summary>
/// The principal's step actually asks for a decision.
/// <para>
/// The form was written — accept, accept with conditions, place on waitlist,
/// reject, with the reason rule ADM-020 is about — registered with the engine,
/// and attached to nothing. So the terminal step of the admissions workflow
/// carried no decision, and completing it fell through to
/// <c>?? AdmissionDecision.Accepted</c>.
/// </para>
/// <para>
/// Every application the principal passed was accepted, whatever they meant.
/// Nobody could be placed on the waiting list, which is why it was always
/// empty; nobody could be rejected with a recorded reason; and an offer could
/// not be given a life of its own.
/// </para>
/// </summary>
public class ThePrincipalHasADecisionToMake_Tests : psmsTestBase
{
    private const string DecisionKey = "application.decision";
    private const string StepName = "Principal Decision";

    public ThePrincipalHasADecisionToMake_Tests()
    {
        LoginAsDefaultTenantAdmin();
    }

    private async Task RunTheSeederAsync()
    {
        using var uow = Resolve<Abp.Domain.Uow.IUnitOfWorkManager>().Begin();
        await Resolve<WorkflowSeedService>().SeedDefaultsAsync();
        await uow.CompleteAsync();
    }

    private WorkflowStep DecisionStep() =>
        UsingDbContext(1, context => context.WorkflowSteps
            .Include(s => s.WorkflowDefinition)
            .Where(s => s.WorkflowDefinition.EntityType == WorkflowEntityType.Application)
            .OrderBy(s => s.StepOrder)
            .ToList()
            .LastOrDefault(s => s.Name == StepName));

    [Fact]
    public async Task A_freshly_seeded_school_gets_the_decision_form()
    {
        await RunTheSeederAsync();

        DecisionStep().DecisionSchemaKey.ShouldBe(DecisionKey);
    }

    [Fact]
    public async Task And_so_does_one_seeded_before_it_was_attached()
    {
        // Strip it, as a school set up earlier would have it.
        await RunTheSeederAsync();
        var stepId = DecisionStep().Id;
        UsingDbContext(1, context =>
        {
            var step = context.WorkflowSteps.Single(s => s.Id == stepId);
            step.DecisionSchemaKey = null;
            context.SaveChanges();
        });

        await RunTheSeederAsync();

        DecisionStep().DecisionSchemaKey.ShouldBe(DecisionKey);
    }

    [Fact]
    public async Task A_school_that_configured_its_own_form_keeps_it()
    {
        await RunTheSeederAsync();
        var stepId = DecisionStep().Id;
        UsingDbContext(1, context =>
        {
            var step = context.WorkflowSteps.Single(s => s.Id == stepId);
            step.DecisionSchemaKey = "something.else";
            context.SaveChanges();
        });

        await RunTheSeederAsync();

        DecisionStep().DecisionSchemaKey.ShouldBe("something.else");
    }

    [Fact]
    public void The_form_offers_the_waiting_list_as_a_decision()
    {
        // The route onto the waiting list, and the only one: an application is
        // waitlisted by the principal choosing it here.
        var schema = new ApplicationDecisionSchema();

        var options = schema.Fields.Single(f => f.Key == "decision").Options;

        options.Select(o => o.Value).ShouldContain(nameof(AdmissionDecision.Waitlisted));
        options.Single(o => o.Value == nameof(AdmissionDecision.Waitlisted))
            .Label.ShouldBe("Place on waitlist");
    }

    [Fact]
    public void A_decision_has_to_be_chosen()
    {
        new ApplicationDecisionSchema()
            .Validate(new WorkflowDecision(new Dictionary<string, object>()))
            .ShouldBe("Select a decision.");
    }

    [Fact]
    public void A_rejection_carries_a_reason_worth_reading()
    {
        // ADM-020, unreachable until the form was attached to anything.
        var schema = new ApplicationDecisionSchema();

        var tooShort = schema.Validate(new WorkflowDecision(new Dictionary<string, object>
        {
            ["decision"] = nameof(AdmissionDecision.Rejected),
            ["reason"] = "No.",
        }));

        tooShort.ShouldContain("50 characters");

        var proper = schema.Validate(new WorkflowDecision(new Dictionary<string, object>
        {
            ["decision"] = nameof(AdmissionDecision.Rejected),
            ["reason"] = new string('x', ApplicationDecisionSchema.MinRejectionReasonLength),
        }));

        proper.ShouldBeNull();
    }

    [Fact]
    public void Waitlisting_asks_for_nothing_more_than_the_decision()
    {
        // A school should be able to waitlist somebody without composing an
        // essay about it; the reason is kept as the entry's note when given.
        new ApplicationDecisionSchema()
            .Validate(new WorkflowDecision(new Dictionary<string, object>
            {
                ["decision"] = nameof(AdmissionDecision.Waitlisted),
            }))
            .ShouldBeNull();
    }
}
