using psms.Admissions.Waitlists;
using Shouldly;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace psms.Tests.Admissions;

/// <summary>
/// A family can answer about their own child's place on the waiting list, and
/// nobody else's.
/// <para>
/// An applicant holds the waitlist view, accept and decline permissions so
/// they can do exactly that. Nothing checked the entry they named was theirs.
/// Verified on the deployed system with a real applicant account against
/// another family's application id:
/// </para>
/// <code>
/// Waitlist/GetByApplication -> 200 { "applicantName": "Sipho Waitlist289323",
///                                    "gradeName": "Grade 11", "position": 1 }
/// Waitlist/GetPosition      -> 200 { ... }
/// </code>
/// <para>
/// And the reads were the lesser half. <c>AcceptOffer</c> and
/// <c>DeclineOffer</c> take a waitlist id and <b>act</b> on it, so one family
/// could take, or throw away, a place the school had offered to another.
/// </para>
/// </summary>
public class AWaitlistEntryBelongsToOneFamily_Tests
{
    private static string Service()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, "src")))
            directory = directory.Parent;

        directory.ShouldNotBeNull("could not find the repository src folder");
        return File.ReadAllText(Path.Combine(directory.FullName,
            "src", "psms.Application", "Admissions", "Waitlists", "WaitlistAppService.cs"));
    }

    private static string Body(string signature)
    {
        var source = Service();
        var start = source.IndexOf(signature, StringComparison.Ordinal);
        start.ShouldBeGreaterThan(-1, $"{signature} not found");
        var end = source.IndexOf("\n    [AbpAuthorize", start + 1, StringComparison.Ordinal);
        return end > start ? source.Substring(start, end - start) : source.Substring(start);
    }

    [Theory]
    [InlineData("GetByApplicationAsync(Guid applicationId)")]
    [InlineData("GetPositionAsync(Guid applicationId)")]
    public void A_read_about_an_application_is_checked_against_the_caller(string signature)
    {
        Body(signature).ShouldContain("EnsureTheirsAsync(applicationId)");
    }

    [Theory]
    [InlineData("AcceptOfferAsync(Guid id)")]
    [InlineData("DeclineOfferAsync(Guid id)")]
    [InlineData("WithdrawAsync(Guid id)")]
    [InlineData("GetAsync(Guid id)")]
    public void An_act_on_an_entry_is_checked_against_the_caller(string signature)
    {
        /* These are the ones that matter. Answering for a family who is not
           yours is worse than reading about them. */
        Body(signature).ShouldContain("EnsureEntryIsTheirsAsync(id)");
    }

    [Fact]
    public void The_check_runs_before_anything_is_fetched_or_changed()
    {
        var body = Body("AcceptOfferAsync(Guid id)");
        var check = body.IndexOf("EnsureEntryIsTheirsAsync", StringComparison.Ordinal);
        var fetch = body.IndexOf("_waitlistRepository.GetAsync(id)", StringComparison.Ordinal);

        check.ShouldBeGreaterThan(-1);
        fetch.ShouldBeGreaterThan(check);
    }

    [Fact]
    public void Staff_are_not_narrowed_by_it()
    {
        /* The resolver answers null for anybody holding Applications.ViewAll,
           which no applicant holds and every admissions role does. A check that
           caught staff too would stop the school running its own waiting list. */
        Service().ShouldContain("if (!ownerOnly.HasValue) return;");
    }

    [Fact]
    public void A_stranger_is_told_the_same_thing_as_somebody_asking_about_nothing()
    {
        /* Different answers for "does not exist" and "is not yours" let the ids
           of other families' applications be discovered by asking. */
        Service().ShouldContain("\"Application not found.\"");
    }

    [Fact]
    public void Saying_yes_to_a_waitlist_place_is_saying_yes()
    {
        /* ApproveFromWaitlist moves the application to Approved with a fresh
           offer window on it. Without this the family would be shown that offer
           and asked to accept the very thing they had just accepted. */
        var body = Body("AcceptOfferAsync(Guid id)");

        body.ShouldContain("application.ApproveFromWaitlist()");
        body.ShouldContain("application.AcceptOffer(AbpSession.UserId)");
    }

    [Fact]
    public void The_whole_waiting_list_still_needs_ViewAll()
    {
        /* Already true and worth keeping true: a family may see their own place
           in the queue, never the queue. */
        var service = typeof(WaitlistAppService)
            .GetMethod(nameof(WaitlistAppService.GetAllAsync));

        service.GetCustomAttributes(typeof(Abp.Authorization.AbpAuthorizeAttribute), false)
            .Cast<Abp.Authorization.AbpAuthorizeAttribute>()
            .SelectMany(a => a.Permissions)
            .ShouldContain(psms.Authorization.PermissionNames.Admissions_Waitlist_ViewAll);
    }
}
