using psms.Domain.Assessment.Entities;
using psms.Domain.Shared.Enums;
using Shouldly;
using System;
using System.Linq;
using Xunit;

namespace psms.Tests.Assessment;

/// <summary>
/// RE-002. The class teacher's comment is a named field of a South African
/// report card, and a card cannot be issued without one.
/// <para>
/// The endpoint that writes it has existed since RC-10, and so has the check
/// that refuses to publish without it. What did not exist was anywhere to type
/// it: the card's page rendered the comment when it was already there and
/// offered no box, and the only "teacher comment" on the screen was the
/// per-subject one inside the marks modal, which is a different field.
/// </para>
/// <para>
/// So every card in the system was one field short of issuable, and the only
/// way past it was an API client. These pin the rule the screen now serves.
/// </para>
/// </summary>
public class TheClassTeachersComment_Tests
{
    /// <summary>A card complete in every respect except what a test removes.</summary>
    private static Report Complete()
    {
        var report = new Report(Guid.NewGuid(), 1, Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), ReportType.Term1);

        report.ReportCardNumber = "2026/GRADE10A/STU-001/T1/ABCD";
        report.OverallAchievementLevel = CapsAchievementLevel.Level4;
        report.ConductRating = ConductDiligenceRating.Good;
        report.RecordAttendance(present: 40, absent: 2, late: 1, daysInTerm: 42);
        report.TeacherComment = "Ayanda has worked steadily and reads with growing confidence.";
        report.PrincipalComment = "A pleasing term.";

        return report;
    }

    private static System.Collections.Generic.IReadOnlyList<string> Missing(Report report) =>
        report.ValidateSAReportCard(subjectCount: 7, markedSubjectCount: 7);

    [Fact]
    public void A_card_with_every_field_is_ready_to_issue()
    {
        Missing(Complete()).ShouldBeEmpty();
    }

    [Fact]
    public void Without_the_class_teachers_comment_it_is_not()
    {
        var report = Complete();
        report.TeacherComment = null;

        Missing(report).ShouldContain("the class teacher's comment");
    }

    [Fact]
    public void Whitespace_is_not_a_comment()
    {
        // A box someone tabbed through is not a comment on a learner's year.
        var report = Complete();
        report.TeacherComment = "   ";

        Missing(report).ShouldContain("the class teacher's comment");
    }

    [Fact]
    public void The_principals_comment_does_not_stand_in_for_it()
    {
        // The two are separate fields on the card and separate people's words.
        // Only the principal's had anywhere to be typed, which is exactly how a
        // card could look nearly finished and still refuse to publish.
        var report = Complete();
        report.TeacherComment = null;
        report.PrincipalComment = "A pleasing term.";

        Missing(report).ShouldContain("the class teacher's comment");
    }

    [Fact]
    public void A_card_short_only_of_that_comment_says_so_and_nothing_else()
    {
        // The message a person reads names what is still needed, so it has to
        // name this and not bury it among things that are already there.
        var report = Complete();
        report.TeacherComment = null;

        Missing(report).ShouldBe(new[] { "the class teacher's comment" });
    }

    [Fact]
    public void Writing_it_completes_the_card()
    {
        var report = Complete();
        report.TeacherComment = null;
        Missing(report).ShouldNotBeEmpty();

        report.TeacherComment = "Ayanda should keep up the reading over the holidays.";

        Missing(report).ShouldBeEmpty();
    }

    [Fact]
    public void A_published_card_stops_accepting_comments()
    {
        // Which is why the screen shows the comment rather than a box once the
        // card has gone out: the server refuses the change, and an editor whose
        // Save can only fail is worse than none.
        var report = Complete();
        report.Generate();
        report.SubmitForApproval();
        report.Approve(1);

        report.AcceptsComments().ShouldBeTrue();

        report.Publish();

        report.AcceptsComments().ShouldBeFalse();
    }
}
