using Abp.UI;
using psms.Admissions.Applications;
using psms.Admissions.Applications.Dto;
using psms.Admissions.Shared;
using psms.Domain.Academic.Entities;
using psms.Domain.Admissions.Entities;
using psms.Domain.Shared.Enums;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace psms.Tests.Admissions;

/// <summary>
/// A prospective parent reads their own application and nobody else's.
/// <para>
/// An applicant holds <c>Admissions.Applications.View</c> so they can see
/// theirs, and nothing checked that it <i>was</i> theirs. <c>Get</c> took an id;
/// <c>GetByApplicationNumber</c> took a number that runs in sequence —
/// APP-003-2026-00011 — so another family's application was reachable by
/// counting. On it: a child's full name, date of birth, ID number, and both
/// parents' contact details.
/// </para>
/// <para>
/// Staff are told apart by <c>Applications.ViewAll</c>, the permission that
/// means "every application at this school". Anyone without it sees only what
/// they created.
/// </para>
/// </summary>
public class AnApplicantSeesOnlyTheirOwn_Tests : psmsTestBase
{
    private readonly IApplicationAppService _applications;

    private Guid _yearId;
    private Guid _gradeId;

    public AnApplicantSeesOnlyTheirOwn_Tests()
    {
        LoginAsDefaultTenantAdmin();
        _applications = Resolve<IApplicationAppService>();

        _yearId = Guid.NewGuid();
        _gradeId = Guid.NewGuid();

        UsingDbContext(1, context =>
        {
            context.AcademicYears.Add(new AcademicYear(_yearId, 1, 2027,
                new DateTime(2027, 2, 3), new DateTime(2027, 12, 15)) { YearName = "2027" });
            context.Grades.Add(new Grade(_gradeId, 1,
                SouthAfricanGradeLevel.Grade11, "Grade 11", SouthAfricanSchoolPhase.FET));
            context.SaveChanges();
        });
    }

    /// <summary>An application belonging to <paramref name="creatorUserId"/>.</summary>
    private Guid SeedApplication(long creatorUserId, string number, string firstName)
    {
        var id = Guid.NewGuid();

        UsingDbContext(1, context =>
        {
            var application = new Application(id, 1, number, firstName, "Nkosi",
                new DateTime(2010, 5, 1), Gender.Female, _gradeId, _yearId,
                $"{firstName.ToLowerInvariant()}@example.test");

            application.CreatorUserId = creatorUserId;
            context.Applications.Add(application);
            context.SaveChanges();
        });

        return id;
    }

    /// <summary>
    /// Stands in for an applicant: a signed-in user who is not staff. The
    /// resolver tells them apart by ViewAll, which the test host's admin holds
    /// and an applicant never does.
    /// </summary>
    private sealed class OnlyTheirOwn : ICurrentApplicantResolver
    {
        private readonly long _userId;
        public OnlyTheirOwn(long userId) => _userId = userId;
        public Task<long?> GetOwnApplicationsOnlyForAsync() => Task.FromResult<long?>(_userId);
    }

    private IApplicationAppService AsApplicant(long userId)
    {
        // Replace just the resolver, so everything else runs as it really does.
        LocalIocManager.IocContainer.Register(
            Castle.MicroKernel.Registration.Component
                .For<ICurrentApplicantResolver>()
                .Instance(new OnlyTheirOwn(userId))
                .IsDefault()
                .Named($"applicant-{userId}-{Guid.NewGuid()}"));

        return Resolve<IApplicationAppService>();
    }

    [Fact]
    public async Task A_parent_can_read_the_application_they_started()
    {
        var mine = SeedApplication(creatorUserId: 50, "APP-2027-00001", "Ayanda");

        var read = await AsApplicant(50).GetAsync(mine);

        read.Id.ShouldBe(mine);
    }

    [Fact]
    public async Task They_cannot_read_another_familys_by_id()
    {
        var theirs = SeedApplication(creatorUserId: 51, "APP-2027-00002", "Sipho");

        var refusal = await Should.ThrowAsync<UserFriendlyException>(
            () => AsApplicant(50).GetAsync(theirs));

        refusal.Message.ShouldBe(psms.Admissions.Shared.AdmissionsExceptionCodes.ApplicationNotFound);
    }

    [Fact]
    public async Task Nor_by_counting_up_the_application_numbers()
    {
        // The reason numbers are the dangerous route: they are sequential, so
        // guessing one takes no skill at all.
        SeedApplication(creatorUserId: 51, "APP-2027-00003", "Sipho");

        await Should.ThrowAsync<UserFriendlyException>(
            () => AsApplicant(50).GetByApplicationNumberAsync("APP-2027-00003"));
    }

    [Fact]
    public async Task They_cannot_withdraw_another_familys_application()
    {
        // Reading it is bad; cancelling a stranger's application is worse.
        var theirs = SeedApplication(creatorUserId: 51, "APP-2027-00004", "Sipho");

        await Should.ThrowAsync<UserFriendlyException>(
            () => AsApplicant(50).WithdrawAsync(theirs, "not mine"));
    }

    [Fact]
    public async Task The_refusal_is_not_found_so_it_cannot_be_used_to_go_fishing()
    {
        // "Forbidden" would confirm the application exists, which is half of
        // what someone counting through numbers wants to know.
        var theirs = SeedApplication(creatorUserId: 51, "APP-2027-00005", "Sipho");

        var refusal = await Should.ThrowAsync<UserFriendlyException>(
            () => AsApplicant(50).GetAsync(theirs));

        refusal.Details.ShouldBe("Application not found.");
    }

    [Fact]
    public async Task Staff_still_read_every_application()
    {
        // The test host's admin holds ViewAll, so the resolver returns null and
        // nothing is narrowed.
        var one = SeedApplication(creatorUserId: 50, "APP-2027-00006", "Ayanda");
        var another = SeedApplication(creatorUserId: 51, "APP-2027-00007", "Sipho");

        (await _applications.GetAsync(one)).ShouldNotBeNull();
        (await _applications.GetAsync(another)).ShouldNotBeNull();
    }

    [Fact]
    public async Task A_parent_can_find_the_drafts_they_started()
    {
        // There was no way to. Listing needs ViewAll, which an applicant does
        // not hold, so a parent who signed back in could not reach their own
        // half-finished application.
        SeedApplication(creatorUserId: 50, "APP-2027-00008", "Ayanda");
        SeedApplication(creatorUserId: 50, "APP-2027-00009", "Lerato");
        SeedApplication(creatorUserId: 51, "APP-2027-00010", "Sipho");

        var mine = await AsApplicant(50).GetMineAsync();

        mine.Items.Count.ShouldBe(2);
        mine.Items.ShouldAllBe(a => a.FullName.Contains("Ayanda")
                                 || a.FullName.Contains("Lerato"));
    }
}
