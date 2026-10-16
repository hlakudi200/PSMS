using psms.Admissions.Offers;
using psms.Domain.Admissions.Entities;
using psms.Domain.Shared.Enums;
using Shouldly;
using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace psms.Tests.Admissions;

/// <summary>
/// A place the school offered and nobody took goes back to the grade.
/// <para>
/// <c>Application.Expire()</c> had no callers at all, and
/// <c>ApplicationStatus.Expired</c> was never assigned. Expiry was noticed only
/// when somebody tried to enrol — so until then an ignored offer kept its
/// application at Approved, kept counting against the grade's places, and kept
/// that place out of reach of the waiting list. For ever. A family that never
/// answered quietly held a seat.
/// </para>
/// <para>
/// These pin which offers the sweep is allowed to touch, because the dangerous
/// mistake is not failing to expire one — it is expiring one that was accepted.
/// </para>
/// </summary>
public class AnUnansweredOfferLetsThePlaceGo_Tests
{
    private static Application Offered(int daysUntilExpiry)
    {
        var application = new Application(
            Guid.NewGuid(), 1, "APP-001-2026-00001",
            "Naledi", "Mokoena", new DateTime(2015, 6, 8), Gender.Female,
            Guid.NewGuid(), Guid.NewGuid(), "parent@example.test");

        application.Status = ApplicationStatus.Approved;
        application.Decision = AdmissionDecision.Accepted;
        application.ExpiryDate = DateTime.UtcNow.AddDays(daysUntilExpiry);
        return application;
    }

    [Fact]
    public void An_offer_that_ran_out_unanswered_expires()
    {
        var application = Offered(-1);

        application.Expire();

        application.Status.ShouldBe(ApplicationStatus.Expired);
    }

    [Fact]
    public void An_expired_application_stops_holding_a_place()
    {
        /* The grade's free places are capacity less enrolled less approved-and-
           waiting. Expired is neither, which is the whole point: the seat is
           available again and the waiting list can move. */
        var application = Offered(-1);
        application.Expire();

        application.Status.ShouldNotBe(ApplicationStatus.Approved);
        application.Status.ShouldNotBe(ApplicationStatus.Enrolled);
    }

    [Fact]
    public void An_accepted_offer_is_never_taken_back()
    {
        /* The dangerous mistake. A family said yes and is waiting for the
           school to enrol them; that is the school's backlog, not a lapse. */
        var source = SweepSource();

        source.ShouldContain("OfferAcceptedDate == null");
        source.ShouldContain("OfferDeclinedDate == null");
    }

    [Fact]
    public void Only_an_offer_that_has_actually_run_out_is_touched()
    {
        var source = SweepSource();

        source.ShouldContain("a.Status == ApplicationStatus.Approved");
        source.ShouldContain("a.ExpiryDate.HasValue");
        source.ShouldContain("a.ExpiryDate.Value < now");
    }

    [Fact]
    public void One_application_the_entity_refuses_does_not_stop_the_sweep()
    {
        /* The entity is the authority on what may expire. One it refuses is a
           row the query should not have matched; letting it throw would leave
           every later school's offers unexpired and nobody any the wiser. */
        var source = SweepSource();

        source.ShouldContain("catch (InvalidOperationException");
        source.ShouldContain("Logger.Warn");
    }

    [Theory]
    [InlineData(ApplicationStatus.Enrolled)]
    [InlineData(ApplicationStatus.Withdrawn)]
    [InlineData(ApplicationStatus.Expired)]
    public void The_entity_refuses_to_expire_what_is_already_finished(ApplicationStatus status)
    {
        var application = Offered(-1);
        application.Status = status;

        Should.Throw<InvalidOperationException>(() => application.Expire());
    }

    [Fact]
    public void The_sweep_can_be_forced_for_one_school_and_says_what_it_did()
    {
        /* An hourly timer is unverifiable and unforceable, and the deployment
           host sleeps between requests — so the timer is not a promise. */
        var service = typeof(AdmissionOfferMaintenanceService);

        service.GetCustomAttribute<Abp.Authorization.AbpAuthorizeAttribute>()
            .Permissions.ShouldContain(psms.Authorization.PermissionNames.Admissions_Decision);

        service.GetMethod(nameof(AdmissionOfferMaintenanceService.ExpireLapsedOffersAsync))
            .ReturnType.ShouldBe(typeof(System.Threading.Tasks.Task<ExpiredOfferReport>));

        new ExpiredOfferReport().Count.ShouldBe(0);
    }

    [Fact]
    public void The_worker_is_actually_added_to_the_manager()
    {
        // A worker nobody adds never ticks, and does so silently.
        var module = System.IO.File.ReadAllText(
            SourceFile("psms.Application", "psmsApplicationModule.cs"));

        module.ShouldContain("PostInitialize");
        module.ShouldContain("AdmissionOfferExpiryWorker");
        module.ShouldContain("IBackgroundWorkerManager");
    }

    private static string SweepSource() =>
        System.IO.File.ReadAllText(
            SourceFile("psms.Application", "Admissions", "Offers", "AdmissionOfferExpirer.cs"));

    /// <summary>Walks up from the test binaries to the repository's src folder.</summary>
    private static string SourceFile(params string[] parts)
    {
        var directory = new System.IO.DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !System.IO.Directory.Exists(System.IO.Path.Combine(directory.FullName, "src")))
            directory = directory.Parent;

        directory.ShouldNotBeNull("could not find the repository src folder");

        var all = new[] { directory.FullName, "src" }.Concat(parts).ToArray();
        return System.IO.Path.Combine(all);
    }
}
