using psms.Domain.Assessment.Entities;
using psms.Domain.Shared.Enums;
using System;
using Xunit;

namespace psms.Tests.Assessment;

/// <summary>
/// RC-10. A report card that has been issued is a document, not a record we may
/// keep editing. The National Protocol §25(3): "schools should ensure that there
/// are no errors, erasures or corrections that will compromise the legal status
/// of the report cards."
/// </summary>
public class ReportLifecycle_Tests
{
    private static Report NewReport() => new Report(
        Guid.NewGuid(), 1, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), ReportType.Term1);

    private static Report At(ReportStatus status)
    {
        var report = NewReport();
        report.Status = status;

        return report;
    }

    [Theory]
    [InlineData(ReportStatus.Draft)]
    [InlineData(ReportStatus.Generated)]
    [InlineData(ReportStatus.PendingApproval)]
    [InlineData(ReportStatus.Approved)]
    public void A_comment_can_still_be_written_up_to_publication(ReportStatus status)
    {
        // A comment can be added while the card is with the approver — unlike
        // the marks, which close at approval because they are what was approved.
        Assert.True(At(status).AcceptsComments());
    }

    [Fact]
    public void A_published_card_accepts_no_more_comments()
    {
        // The comments on a card a parent has already downloaded could be
        // silently rewritten afterwards. Three endpoints had no status guard at
        // all.
        Assert.False(At(ReportStatus.Published).AcceptsComments());
    }

    [Theory]
    [InlineData(ReportStatus.Approved)]
    [InlineData(ReportStatus.Published)]
    public void An_issued_card_is_closed_to_the_cohort_pass(ReportStatus status)
    {
        Assert.True(At(status).IsLockedForRestatement());
    }

    [Theory]
    [InlineData(ReportStatus.Draft)]
    [InlineData(ReportStatus.Generated)]
    [InlineData(ReportStatus.PendingApproval)]
    public void A_card_still_in_progress_is_not(ReportStatus status)
    {
        Assert.False(At(status).IsLockedForRestatement());
    }

    [Fact]
    public void Returning_a_report_truncates_the_reason_to_the_column_it_is_kept_in()
    {
        var report = At(ReportStatus.PendingApproval);
        var reason = new string('x', Report.MaxPrincipalCommentLength + 500);

        report.ReturnForRevision(reason);

        // It used to truncate at a magic 1000 that happened to match the column.
        Assert.Equal(Report.MaxPrincipalCommentLength, report.PrincipalComment.Length);
        Assert.Equal(ReportStatus.Generated, report.Status);
    }

    [Fact]
    public void A_short_reason_is_kept_whole()
    {
        var report = At(ReportStatus.PendingApproval);

        report.ReturnForRevision("Mathematics mark looks wrong.");

        Assert.Equal("Mathematics mark looks wrong.", report.PrincipalComment);
    }

    // ─── RC-16: the promotion decision ───

    [Fact]
    public void Only_the_year_end_card_carries_the_promotion_decision()
    {
        Assert.True(new Report(
            Guid.NewGuid(), 1, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), ReportType.YearEnd)
            .CarriesPromotionDecision());

        Assert.False(NewReport().CarriesPromotionDecision());
    }

    [Fact]
    public void A_retained_learner_has_no_destination_grade()
    {
        var report = NewReport();
        var nextGrade = Guid.NewGuid();

        report.RecordPromotion(PromotionDecision.Promoted, nextGrade);
        Assert.Equal(nextGrade, report.PromotedToGradeId);

        // The decision is revisited and the learner is retained instead. They
        // stay where they are, so the destination must not survive.
        report.RecordPromotion(PromotionDecision.Retained, nextGrade, "Parent meeting held 3 Dec.");

        Assert.Null(report.PromotedToGradeId);
        Assert.Equal("Parent meeting held 3 Dec.", report.PromotionReason);
    }

    [Fact]
    public void An_empty_reason_is_stored_as_nothing_rather_than_blank_text()
    {
        var report = NewReport();

        report.RecordPromotion(PromotionDecision.Promoted, Guid.NewGuid(), "   ");

        Assert.Null(report.PromotionReason);
    }
}
