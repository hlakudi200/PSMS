using Microsoft.EntityFrameworkCore;
using psms.Authorization;
using psms.Authorization.Roles;
using psms.Domain.Workflow.Entities;
using psms.Domain.Workflow.Enums;
using psms.Workflow.Engine;
using psms.Workflow.Seed;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace psms.Tests.Workflow;

/// <summary>
/// The step that reviews an application waits for its documents to have been
/// looked at, and somebody can look at them.
/// <para>
/// The first workflow step was called Document Verification and there was no
/// way to verify a document. <c>VerifyAsync</c> and <c>RejectAsync</c> existed,
/// the permissions were granted to four roles, and no component called either —
/// so the Documents tab offered only "View", every document sat Pending for
/// ever, and the step was walked past on the deployed system with all three
/// documents unverified.
/// </para>
/// <para>
/// Order matters here: the guard is only attached in the same change that
/// gives somebody a button to satisfy it with. A guard nobody can satisfy is
/// worse than no guard.
/// </para>
/// </summary>
public class SomebodyCanActuallyVerifyADocument_Tests : psmsTestBase
{
    private const string DocumentsGuard = "application.documents-verified";

    public SomebodyCanActuallyVerifyADocument_Tests()
    {
        LoginAsDefaultTenantAdmin();
    }

    private async Task RunTheSeederAsync()
    {
        using var uow = Resolve<Abp.Domain.Uow.IUnitOfWorkManager>().Begin();
        await Resolve<WorkflowSeedService>().SeedDefaultsAsync();
        await uow.CompleteAsync();
    }

    private List<WorkflowStep> Steps() =>
        UsingDbContext(1, context => context.WorkflowSteps
            .Include(s => s.WorkflowDefinition)
            .Where(s => s.WorkflowDefinition.EntityType == WorkflowEntityType.Application && !s.IsDeleted)
            .OrderBy(s => s.StepOrder)
            .ToList());

    private WorkflowStep Step(string name) => Steps().LastOrDefault(s => s.Name == name);

    [Fact]
    public async Task The_review_step_waits_for_the_documents()
    {
        await RunTheSeederAsync();

        Step("Admin Review").GuardKey.ShouldBe(DocumentsGuard);
    }

    [Fact]
    public async Task And_so_does_a_school_seeded_before_it_was_attached()
    {
        await RunTheSeederAsync();

        var id = Step("Admin Review").Id;
        UsingDbContext(1, context =>
        {
            context.WorkflowSteps.Single(s => s.Id == id).GuardKey = null;
            context.SaveChanges();
        });

        await RunTheSeederAsync();

        Step("Admin Review").GuardKey.ShouldBe(DocumentsGuard);
    }

    [Fact]
    public async Task The_guard_is_one_the_engine_can_resolve()
    {
        await RunTheSeederAsync();

        using var uow = Resolve<Abp.Domain.Uow.IUnitOfWorkManager>().Begin();
        var registry = Resolve<WorkflowExtensionRegistry>();

        Should.NotThrow(() => registry.GetGuard(DocumentsGuard, WorkflowEntityType.Application));

        await uow.CompleteAsync();
    }

    [Fact]
    public async Task An_application_with_nothing_uploaded_does_not_pass_it()
    {
        await RunTheSeederAsync();

        using var uow = Resolve<Abp.Domain.Uow.IUnitOfWorkManager>().Begin();
        var guard = Resolve<WorkflowExtensionRegistry>()
            .GetGuard(DocumentsGuard, WorkflowEntityType.Application);

        var result = await guard.EvaluateAsync(Guid.NewGuid());

        result.Satisfied.ShouldBeFalse();

        await uow.CompleteAsync();
    }

    [Fact]
    public void Being_blocked_by_it_is_not_a_dead_end()
    {
        /* Admin Review cannot be waived — it is not optional — so the only way
           past a family who have uploaded nothing is to override, with a reason
           that is recorded. The roles that hold the review step must therefore
           hold the override, or an application with no documents is stuck for
           ever with nobody able to move it. */
        var full = (List<string>)typeof(PsmsRolePermissionSeeder)
            .GetMethod("GetWorkflowFullPermissions", BindingFlags.NonPublic | BindingFlags.Static)
            .Invoke(null, null);

        full.ShouldContain(PermissionNames.Workflow_Instances_OverrideGuard);
    }

    [Fact]
    public void Somebody_can_verify_and_send_back_from_the_application()
    {
        /* The endpoints and the permissions existed all along. What did not was
           anything on a screen that called them. */
        var page = File.ReadAllText(FrontendFile(
            "components", "modules", "admissions", "ApplicationDetailPage.tsx"));

        page.ShouldContain("verifyAsync(");
        page.ShouldContain("rejectAsync(");
        // The two labels, however the JSX happens to be wrapped.
        page.ShouldContain("Verify");
        page.ShouldContain("Send back");
        // And only offered to somebody the school lets verify.
        page.ShouldContain("mayVerify");
    }

    [Fact]
    public void Sending_one_back_asks_what_is_wrong_with_it()
    {
        /* "Rejected" with no reason tells a family to do something again
           without telling them what. A blurred photograph and the wrong
           document need different things done about them. */
        var page = File.ReadAllText(FrontendFile(
            "components", "modules", "admissions", "ApplicationDetailPage.tsx"));

        page.ShouldContain("Say what is wrong with it");
    }

    [Fact]
    public async Task The_step_that_ends_the_workflow_is_marked_as_ending_it()
    {
        /* A live school had Principal Decision with IsTerminal false.
           Completion still worked, so nothing looked wrong — but every pass
           that searches for the terminal step quietly stopped working, and the
           tidy-up refused to remove anything for three releases running. */
        await RunTheSeederAsync();

        var id = Step("Principal Decision").Id;
        UsingDbContext(1, context =>
        {
            context.WorkflowSteps.Single(s => s.Id == id).IsTerminal = false;
            context.SaveChanges();
        });

        await RunTheSeederAsync();

        Step("Principal Decision").IsTerminal.ShouldBeTrue();
    }

    [Fact]
    public async Task A_school_that_added_stages_after_the_principal_is_left_alone()
    {
        /* Only the last step, and only when it is the decision step. A school
           with its own stage at the end is not being corrected. */
        await RunTheSeederAsync();

        var definitionId = Steps().First().WorkflowDefinitionId;
        var last = Steps().Last();

        UsingDbContext(1, context =>
        {
            context.WorkflowSteps.Single(s => s.Id == last.Id).IsTerminal = false;
            context.WorkflowSteps.Add(new WorkflowStep
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = definitionId,
                StepOrder = last.StepOrder + 1,
                Name = "Board sign-off",
                AssignedRole = "Principal",
                ActionType = WorkflowActionType.Approve,
            });
            context.SaveChanges();
        });

        await RunTheSeederAsync();

        Step("Principal Decision").IsTerminal.ShouldBeFalse();
    }

    private static string FrontendFile(params string[] parts)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, "frontend")))
            directory = directory.Parent;

        directory.ShouldNotBeNull("could not find the frontend folder");
        return Path.Combine(new[] { directory.FullName, "frontend", "src" }.Concat(parts).ToArray());
    }
}
