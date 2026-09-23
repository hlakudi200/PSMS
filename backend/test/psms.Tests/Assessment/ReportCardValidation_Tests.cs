using psms.Domain.Assessment.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Linq;
using Xunit;

namespace psms.Tests.Assessment;

/// <summary>
/// RC-17. RE-002's <c>ValidateSAReportCard()</c>, which the business rules
/// referenced and no code implemented, and RE-003's signature requirement.
/// </summary>
public class ReportCardValidation_Tests
{
    private static Report Complete()
    {
        var report = new Report(
            Guid.NewGuid(), 1, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), ReportType.Term1);

        report.AssignReportCardNumber("2026/GR8A/STU-2026-023/T1");
        report.RecalculateOverall(new decimal?[] { 72m });
        report.RecordAttendance(present: 58, absent: 2, late: 3, daysInTerm: 60);
        report.TeacherComment = "Worked steadily all term.";
        report.PrincipalComment = "A pleasing report.";

        return report;
    }

    [Fact]
    public void A_complete_card_is_missing_nothing()
    {
        Assert.Empty(Complete().ValidateSAReportCard(subjectCount: 6, markedSubjectCount: 6));
    }

    [Fact]
    public void A_card_with_no_number_is_named_as_missing_one()
    {
        var report = Complete();
        report.ReportCardNumber = null;

        var missing = report.ValidateSAReportCard(6, 6);

        Assert.Contains(missing, m => m.Contains("report card number"));
    }

    [Fact]
    public void The_number_is_stamped_once_and_does_not_change_under_the_card()
    {
        var report = Complete();

        report.AssignReportCardNumber("something/else");

        // A card's number is how it is referred to afterwards.
        Assert.Equal("2026/GR8A/STU-2026-023/T1", report.ReportCardNumber);
    }

    [Theory]
    [InlineData(58, 2, 60, true)]    // adds up
    [InlineData(58, 2, 61, false)]   // a school day nobody accounted for
    [InlineData(40, 2, 60, false)]   // eighteen days unexplained
    public void Attendance_has_to_reconcile_against_the_school_days(
        int present, int absent, int daysInTerm, bool reconciles)
    {
        // RE-002's ATTENDANCE_MISMATCH. Before this there was no school-day
        // total on the card, so attendance could say anything.
        var report = Complete();
        report.RecordAttendance(present, absent, late: 0, daysInTerm: daysInTerm);

        var missing = report.ValidateSAReportCard(6, 6);

        Assert.Equal(reconciles, !missing.Any(m => m.Contains("attendance that adds up")));
    }

    [Fact]
    public void A_card_with_no_school_day_total_has_nothing_to_reconcile_against()
    {
        var report = Complete();
        report.RecordAttendance(58, 2, 3, daysInTerm: null);

        Assert.Contains(
            report.ValidateSAReportCard(6, 6),
            m => m.Contains("school days in the period"));
    }

    [Fact]
    public void A_late_arrival_is_not_a_third_bucket()
    {
        // A learner who arrived late still attended, so they are counted present
        // and the day is not counted twice.
        var report = Complete();
        report.RecordAttendance(present: 58, absent: 2, late: 58, daysInTerm: 60);

        Assert.Empty(report.ValidateSAReportCard(6, 6));
    }

    [Fact]
    public void The_comments_RE_002_names_are_both_required()
    {
        var report = Complete();
        report.TeacherComment = null;
        report.PrincipalComment = "   ";

        var missing = report.ValidateSAReportCard(6, 6);

        Assert.Contains(missing, m => m.Contains("class teacher's comment"));
        Assert.Contains(missing, m => m.Contains("principal's comment"));
    }

    [Fact]
    public void A_card_with_no_subjects_or_no_marks_is_caught_here_too()
    {
        Assert.Contains(Complete().ValidateSAReportCard(0, 0), m => m.Contains("any subjects"));
        Assert.Contains(Complete().ValidateSAReportCard(6, 0), m => m.Contains("a mark in any subject"));
    }

    // ─── RE-003: the signatures ───

    [Fact]
    public void A_card_is_signed_off_only_when_both_have_signed()
    {
        var report = Complete();
        Assert.False(report.IsSignedOff());

        report.SignAsTeacher(42);
        Assert.False(report.IsSignedOff());

        report.SignAsPrincipal(7);
        Assert.True(report.IsSignedOff());

        Assert.Equal(42, report.TeacherSignedByUserId);
        Assert.Equal(7, report.PrincipalSignedByUserId);
        Assert.NotNull(report.TeacherSignedDate);
        Assert.NotNull(report.PrincipalSignedDate);
    }

    [Fact]
    public void Signatures_can_be_withdrawn_when_the_card_changes_under_them()
    {
        var report = Complete();
        report.SignAsTeacher(42);
        report.SignAsPrincipal(7);

        report.ClearSignatures();

        Assert.False(report.IsSignedOff());
        Assert.Null(report.TeacherSignedDate);
        Assert.Null(report.PrincipalSignedDate);
    }

    [Fact]
    public void A_blank_behaviour_comment_is_stored_as_nothing()
    {
        var report = Complete();

        report.RecordConduct(
            ConductDiligenceRating.VeryGood, ConductDiligenceRating.Good, "   ");

        Assert.Equal(ConductDiligenceRating.VeryGood, report.ConductRating);
        Assert.Equal(ConductDiligenceRating.Good, report.DiligenceRating);
        Assert.Null(report.BehaviourComments);
    }
}
