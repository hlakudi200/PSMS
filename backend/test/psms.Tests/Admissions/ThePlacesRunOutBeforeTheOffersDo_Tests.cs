using Abp.UI;
using psms.Admissions.Shared;
using psms.Domain.Academic.Entities;
using psms.Domain.Admissions.Entities;
using psms.Domain.Shared.Enums;
using psms.Domain.Workflow.Enums;
using psms.Workflow.Engine;
using psms.Workflow.Engine.Handlers;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace psms.Tests.Admissions;

/// <summary>
/// A grade with two places does not quietly offer ten.
/// <para>
/// Capacity was only ever checked at enrolment. The decision itself called
/// Approve with no check at all, so ten applicants could be accepted for two
/// places: all ten were told they had a place, the first two enrolled, and the
/// other eight were refused — after being offered one. The screens warned and
/// the arithmetic was right; the decision never read it.
/// </para>
/// <para>
/// Not a hard block. Schools over-offer deliberately, expecting some families
/// to decline, so going past the last place stays available — it just has to be
/// something somebody chose with the numbers in front of them.
/// </para>
/// </summary>
public class ThePlacesRunOutBeforeTheOffersDo_Tests : psmsTestBase
{
    private readonly ApplicationWorkflowHandler _handler;

    private Guid _yearId;
    private Guid _gradeId;

    public ThePlacesRunOutBeforeTheOffersDo_Tests()
    {
        _handler = Resolve<ApplicationWorkflowHandler>();
        LoginAsDefaultTenantAdmin();
    }

    /// <summary>
    /// A grade with <paramref name="capacity"/> places, <paramref name="enrolled"/>
    /// learners already in it, and <paramref name="offered"/> applications
    /// approved and waiting to enrol.
    /// </summary>
    private void Grade(int? capacity, int enrolled, int offered)
    {
        _yearId = Guid.NewGuid();
        _gradeId = Guid.NewGuid();

        UsingDbContext(context =>
        {
            context.AcademicYears.Add(new AcademicYear(_yearId, 1, 2027,
                new DateTime(2027, 1, 1), new DateTime(2027, 12, 31)));
            context.Grades.Add(new Grade(_gradeId, 1, SouthAfricanGradeLevel.Grade8, "Grade 8",
                SouthAfricanSchoolPhase.Senior));

            context.AdmissionSettings.Add(new AdmissionSettings(Guid.NewGuid(), 1, _yearId, 0m)
            {
                GradeId = _gradeId,
                MaxCapacity = capacity,
                CurrentEnrolledCount = enrolled,
            });

            for (var i = 0; i < offered; i++)
            {
                var held = Applicant("APP-HELD-" + i.ToString("D3"));
                held.Submit(feeRequired: false);
                held.MoveToConsideration();
                held.Approve(1, "offered earlier", 14);
                context.Applications.Add(held);
            }
        });
    }

    private Application Applicant(string number) => new Application(
        Guid.NewGuid(), 1, number, "Ann", "Adams", new DateTime(2013, 4, 1),
        Gender.Female, _gradeId, _yearId, number.ToLowerInvariant() + "@example.test");

    /// <summary>One more applicant, sitting where a decision would find them.</summary>
    private Guid AnotherApplicant()
    {
        var application = Applicant("APP-NEW-" + Guid.NewGuid().ToString("N").Substring(0, 8));
        application.Submit(feeRequired: false);
        application.MoveToConsideration();
        UsingDbContext(context => context.Applications.Add(application));
        return application.Id;
    }

    /// <summary>
    /// The handler is resolved directly rather than driven through the workflow
    /// service, so there is no interceptor to open a unit of work for it — its
    /// repositories need an ambient one.
    /// </summary>
    private async Task DecideAsync(Guid applicationId, string decision, bool? offerAnyway = null)
    {
        var values = new Dictionary<string, object> { ["decision"] = decision };
        if (offerAnyway.HasValue) values["offerBeyondCapacity"] = offerAnyway.Value;

        // ADM-020: a rejection needs a reason of at least 50 characters, which
        // is the domain's rule and not this ticket's business.
        if (decision == nameof(AdmissionDecision.Rejected))
            values["reason"] = "The panel considered this application carefully and "
                + "could not offer a place for the coming year.";

        using var uow = Resolve<Abp.Domain.Uow.IUnitOfWorkManager>().Begin();

        await _handler.OnCompletedAsync(new WorkflowEffectContext
        {
            TenantId = 1,
            EntityType = WorkflowEntityType.Application,
            EntityId = applicationId,
            ActorUserId = 1,
            Comment = "decided",
            Decision = new WorkflowDecision(values),
        });

        await uow.CompleteAsync();
    }

    private ApplicationStatus StatusOf(Guid id) =>
        UsingDbContext(context => context.Applications.Single(a => a.Id == id).Status);

    // ---- room left ----

    [Fact]
    public async Task With_a_place_left_the_decision_goes_through_untouched()
    {
        Grade(capacity: 2, enrolled: 0, offered: 1);
        var applicant = AnotherApplicant();

        await DecideAsync(applicant, nameof(AdmissionDecision.Accepted));

        StatusOf(applicant).ShouldBe(ApplicationStatus.Approved);
    }

    [Fact]
    public async Task A_grade_with_no_stated_limit_never_runs_out()
    {
        // An unset limit is not a limit of zero. A school that never filled the
        // field in has not said there are no places.
        Grade(capacity: null, enrolled: 40, offered: 40);
        var applicant = AnotherApplicant();

        await DecideAsync(applicant, nameof(AdmissionDecision.Accepted));

        StatusOf(applicant).ShouldBe(ApplicationStatus.Approved);
    }

    // ---- no room left ----

    [Fact]
    public async Task The_offer_past_the_last_place_is_refused_and_says_the_numbers()
    {
        // Two places: one child enrolled, one offer already out. Nothing left.
        Grade(capacity: 2, enrolled: 1, offered: 1);
        var applicant = AnotherApplicant();

        var refusal = await Should.ThrowAsync<UserFriendlyException>(
            () => DecideAsync(applicant, nameof(AdmissionDecision.Accepted)));

        // The code is the house pattern's Message; what a person reads is the
        // Details.
        refusal.Message.ShouldBe(AdmissionsExceptionCodes.GradeCapacityFull);

        // The numbers, not just a refusal: a principal told "full" without them
        // has to go and count for themselves.
        refusal.Details.ShouldContain("Grade 8");
        refusal.Details.ShouldContain("2 places");
        refusal.Details.ShouldContain("1 enrolled");
        refusal.Details.ShouldContain("1 offered");
        refusal.Details.ShouldContain("waitlist");

        // Refused before the write-back, so the applicant is untouched rather
        // than half-approved.
        StatusOf(applicant).ShouldBe(ApplicationStatus.UnderConsideration);
    }

    [Fact]
    public async Task An_offer_already_out_counts_against_the_places()
    {
        // Neither offer is enrolled yet. Counting only enrolled learners would
        // call this grade empty and hand out the same two places twice.
        Grade(capacity: 2, enrolled: 0, offered: 2);
        var applicant = AnotherApplicant();

        await Should.ThrowAsync<UserFriendlyException>(
            () => DecideAsync(applicant, nameof(AdmissionDecision.Accepted)));
    }

    [Fact]
    public async Task Accepting_with_conditions_is_still_offering_a_place()
    {
        Grade(capacity: 1, enrolled: 1, offered: 0);
        var applicant = AnotherApplicant();

        await Should.ThrowAsync<UserFriendlyException>(
            () => DecideAsync(applicant, nameof(AdmissionDecision.ConditionalAcceptance)));
    }

    // ---- the ways past it ----

    [Fact]
    public async Task The_principal_can_offer_anyway_when_they_mean_to()
    {
        // Schools over-offer on purpose. The point was never to forbid it.
        Grade(capacity: 2, enrolled: 2, offered: 0);
        var applicant = AnotherApplicant();

        await DecideAsync(applicant, nameof(AdmissionDecision.Accepted), offerAnyway: true);

        StatusOf(applicant).ShouldBe(ApplicationStatus.Approved);
    }

    [Fact]
    public async Task The_waitlist_is_never_blocked_by_a_full_grade()
    {
        // The waitlist is what a full grade is for. Refusing it would be absurd.
        Grade(capacity: 2, enrolled: 2, offered: 0);
        var applicant = AnotherApplicant();

        await DecideAsync(applicant, nameof(AdmissionDecision.Waitlisted));

        StatusOf(applicant).ShouldBe(ApplicationStatus.Waitlisted);
    }

    [Fact]
    public async Task A_rejection_is_never_blocked_by_a_full_grade()
    {
        Grade(capacity: 2, enrolled: 2, offered: 0);
        var applicant = AnotherApplicant();

        await DecideAsync(applicant, nameof(AdmissionDecision.Rejected));

        StatusOf(applicant).ShouldBe(ApplicationStatus.Rejected);
    }

    [Fact]
    public async Task The_full_grade_of_another_year_is_not_this_years_problem()
    {
        // Capacity is per grade per year. This year's full Grade 8 says nothing
        // about next year's.
        Grade(capacity: 2, enrolled: 2, offered: 0);

        var otherYearId = Guid.NewGuid();
        UsingDbContext(context =>
        {
            context.AcademicYears.Add(new AcademicYear(otherYearId, 1, 2028,
                new DateTime(2028, 1, 1), new DateTime(2028, 12, 31)));
            context.AdmissionSettings.Add(new AdmissionSettings(Guid.NewGuid(), 1, otherYearId, 0m)
            {
                GradeId = _gradeId,
                MaxCapacity = 30,
                CurrentEnrolledCount = 0,
            });
        });

        var application = new Application(
            Guid.NewGuid(), 1, "APP-NEXT-YEAR", "Ben", "Brown", new DateTime(2013, 4, 1),
            Gender.Male, _gradeId, otherYearId, "next.year@example.test");
        application.Submit(feeRequired: false);
        application.MoveToConsideration();
        UsingDbContext(context => context.Applications.Add(application));

        await DecideAsync(application.Id, nameof(AdmissionDecision.Accepted));

        StatusOf(application.Id).ShouldBe(ApplicationStatus.Approved);
    }
}
