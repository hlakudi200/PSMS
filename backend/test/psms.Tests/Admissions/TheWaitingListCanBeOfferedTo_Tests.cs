using Shouldly;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace psms.Tests.Admissions;

/// <summary>
/// A waiting list the school can offer a place from, and a family who can
/// answer.
/// <para>
/// <c>OfferPositionAsync</c>, <c>AcceptOfferAsync</c>, <c>DeclineOfferAsync</c>
/// and <c>GetPositionAsync</c> all existed and <c>Admissions.Waitlist.OfferPosition</c>
/// was granted to four roles. No component called any of them. Walked on the
/// deployed system: the waitlist tab offered Columns, Export and Filters, and
/// the waitlisted family's own screen read
/// </para>
/// <code>
/// Where it stands: On the waiting list.
/// There is no application fee for this grade
/// The school is reviewing your application.      &lt;- no longer true
/// </code>
/// <para>
/// — no position, no explanation, one button, and that button was Withdraw.
/// </para>
/// </summary>
public class TheWaitingListCanBeOfferedTo_Tests
{
    private static string Frontend(params string[] parts)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, "frontend")))
            directory = directory.Parent;

        directory.ShouldNotBeNull("could not find the frontend folder");
        return File.ReadAllText(
            Path.Combine(new[] { directory.FullName, "frontend", "src" }.Concat(parts).ToArray()));
    }

    private static string AdmissionsPage() =>
        Frontend("components", "modules", "admissions", "AdmissionsPageContent.tsx");

    private static string WaitlistPanel() =>
        Frontend("components", "modules", "admissions", "apply", "WaitlistPanel.tsx");

    [Fact]
    public void The_school_can_offer_a_place_from_the_list()
    {
        var page = AdmissionsPage();

        page.ShouldContain("offerPositionAsync(");
        page.ShouldContain("Offer a place");
        // Previously: "row actions are intentionally empty".
        page.ShouldNotContain("waitlistRowActions: RowAction<IWaitlist>[] = [];");
    }

    [Fact]
    public void Only_somebody_still_waiting_is_offered_one()
    {
        /* An entry already offered, accepted, declined or withdrawn is not
           somebody to offer a place to, and offering twice resets the clock on
           a family who already answered. */
        AdmissionsPage().ShouldContain("record.statusDisplayName === 'Active'");
    }

    [Fact]
    public void The_school_chooses_how_long_the_family_has()
    {
        /* Seven days by default, because that is what the service does, but a
           school holding a place over a holiday needs to say so. */
        var page = AdmissionsPage();

        page.ShouldContain("offerDays");
        page.ShouldContain("How long should they have to answer?");
    }

    [Fact]
    public void The_family_is_told_where_they_stand()
    {
        /* "On the waiting list" with no number is not an answer to the only
           question a waitlisted family has. */
        WaitlistPanel().ShouldContain("on the waiting list for");
        WaitlistPanel().ShouldContain("entry.position");
    }

    [Fact]
    public void The_family_can_take_the_place_or_turn_it_down()
    {
        var panel = WaitlistPanel();

        panel.ShouldContain("acceptOfferAsync(");
        panel.ShouldContain("declineOfferAsync(");
        panel.ShouldContain("Take the place");
        panel.ShouldContain("Turn it down");
    }

    [Fact]
    public void An_offer_that_ran_out_is_its_own_state()
    {
        /* Showing "on the waiting list" would be a lie, and leaving the buttons
           up would be a crueller one. */
        var panel = WaitlistPanel();

        panel.ShouldContain("isOfferExpired");
        panel.ShouldContain("The offer ran out");
    }

    [Fact]
    public void Turning_it_down_says_what_that_costs()
    {
        /* It is final: the place goes to the next family and the child comes
           off the list. */
        WaitlistPanel().ShouldContain("goes to the next family");
    }

    [Fact]
    public void The_fee_notice_stops_claiming_the_application_is_under_review()
    {
        /* It said so under every status, including waitlisted and decided. */
        var review = Frontend("components", "modules", "admissions", "apply", "ReviewStep.tsx");

        review.ShouldContain("ApplicationStatus.UnderReview");
        review.ShouldContain("WaitlistPanel");
    }

    [Fact]
    public void The_screen_stops_contradicting_itself_a_line_apart()
    {
        /* "Where it stands: Offered a place." was printed directly above "You
           accepted the place in Grade 11". Both came from the same screen, one
           from the status and one from what the family had actually done. */
        var status = Frontend("providers", "admissions", "shared", "application-status.ts");
        var review = Frontend("components", "modules", "admissions", "apply", "ReviewStep.tsx");

        status.ShouldContain("applicationStandingLabel");
        status.ShouldContain("Place accepted");
        status.ShouldContain("Place turned down");
        review.ShouldContain("applicationStandingLabel(");
    }

    [Fact]
    public void The_panel_has_the_provider_it_needs()
    {
        /* A panel whose provider nobody wrapped throws on render rather than
           quietly doing nothing, and it would do it on the screen of a family
           who have just been offered a place. */
        var journey = Frontend("components", "modules", "admissions", "apply", "ApplyJourney.tsx");

        journey.ShouldContain("WaitlistProvider");
    }
}
