using psms.Domain.Assessment;
using psms.Domain.Assessment.Entities;
using Shouldly;
using System;
using Xunit;

namespace psms.Tests.Assessment;

/// <summary>
/// RC-26. The procedure around retaining a learner — NPPPPR §(2b) and §(2c).
/// <para>
/// §(2b): a special meeting of subject staff, then a meeting with the parent
/// "before the learner's school report is handed to them", and written
/// confirmation by the parent. §(2c): the parent may appeal "not later than
/// three (3) days after the official opening of schools", and the head of
/// department determines it "within fourteen (14) working days of receiving a
/// request to appeal".
/// </para>
/// </summary>
public class RetentionProcedure_Tests
{
    private static RetentionProcedure New() =>
        new RetentionProcedure(Guid.NewGuid(), 1, Guid.NewGuid());

    /* ─────────── §(2b)(b): the parent comes before the card ─────────── */

    [Fact]
    public void A_card_is_not_ready_to_hand_over_until_the_parent_has_been_seen()
    {
        var procedure = New();

        procedure.ParentHasBeenMet().ShouldBeFalse();

        procedure.ParentMeetingDate = new DateTime(2026, 11, 20);

        procedure.ParentHasBeenMet().ShouldBeTrue();
    }

    [Fact]
    public void The_staff_meeting_alone_does_not_make_it_ready()
    {
        // §(2b)(a) and (b) are two meetings, and it is the second one the
        // Protocol puts before the report reaches the family.
        var procedure = New();
        procedure.StaffMeetingDate = new DateTime(2026, 11, 10);

        procedure.ParentHasBeenMet().ShouldBeFalse();
    }

    /* ─────────── §(2c): three days to lodge ─────────── */

    [Fact]
    public void An_appeal_is_due_three_days_after_schools_open()
    {
        // Calendar days, not working days — the policy says "three (3) days"
        // where it says "fourteen (14) working days" a clause later.
        AppealDeadlines.LodgingDeadline(new DateTime(2027, 1, 13))
            .ShouldBe(new DateTime(2027, 1, 16));
    }

    [Fact]
    public void An_appeal_lodged_in_time_is_not_flagged()
    {
        var procedure = New();
        procedure.AppealDeadline = new DateTime(2027, 1, 16);
        procedure.AppealLodgedDate = new DateTime(2027, 1, 15);

        procedure.AppealWasLate().ShouldBeFalse();
    }

    [Fact]
    public void An_appeal_lodged_on_the_deadline_is_in_time()
    {
        var procedure = New();
        procedure.AppealDeadline = new DateTime(2027, 1, 16);
        procedure.AppealLodgedDate = new DateTime(2027, 1, 16);

        procedure.AppealWasLate().ShouldBeFalse();
    }

    [Fact]
    public void A_late_appeal_is_recorded_as_late_rather_than_refused()
    {
        // It is still a parent asking. Whether to hear it is the department's
        // call, not this system's.
        var procedure = New();
        procedure.AppealDeadline = new DateTime(2027, 1, 16);
        procedure.AppealLodgedDate = new DateTime(2027, 1, 20);

        procedure.AppealWasLate().ShouldBeTrue();
        procedure.AppealLodgedDate.ShouldNotBeNull();
    }

    [Fact]
    public void Without_a_known_opening_date_nothing_is_called_late()
    {
        var procedure = New();
        procedure.AppealLodgedDate = new DateTime(2027, 1, 20);

        procedure.AppealWasLate().ShouldBeFalse();
    }

    /* ─────────── §(2c): fourteen working days to determine ─────────── */

    [Fact]
    public void Fourteen_working_days_skips_the_weekends()
    {
        // Lodged Monday 18 January 2027. Fourteen working days later is
        // Friday 5 February — four weekend days fall inside the run.
        var lodged = new DateTime(2027, 1, 18);
        lodged.DayOfWeek.ShouldBe(DayOfWeek.Monday);

        var deadline = AppealDeadlines.DeterminationDeadline(lodged);

        deadline.ShouldBe(new DateTime(2027, 2, 5));
        deadline.DayOfWeek.ShouldBe(DayOfWeek.Friday);
    }

    [Fact]
    public void The_clock_starts_the_day_after_the_appeal_arrives()
    {
        // "within fourteen working days of receiving a request" — the day it
        // lands is day zero.
        AppealDeadlines.AddWorkingDays(new DateTime(2027, 1, 18), 1)
            .ShouldBe(new DateTime(2027, 1, 19));
    }

    [Fact]
    public void A_deadline_counted_from_a_friday_lands_on_a_weekday()
    {
        var deadline = AppealDeadlines.DeterminationDeadline(new DateTime(2027, 1, 15));

        deadline.DayOfWeek.ShouldNotBe(DayOfWeek.Saturday);
        deadline.DayOfWeek.ShouldNotBe(DayOfWeek.Sunday);
    }

    [Fact]
    public void A_determination_still_inside_its_window_is_not_overdue()
    {
        var procedure = New();
        procedure.AppealLodgedDate = new DateTime(2027, 1, 18);
        procedure.AppealDeterminationDeadline = new DateTime(2027, 2, 5);

        procedure.DeterminationIsOverdue(new DateTime(2027, 2, 5)).ShouldBeFalse();
    }

    [Fact]
    public void A_determination_past_its_window_is_overdue()
    {
        var procedure = New();
        procedure.AppealLodgedDate = new DateTime(2027, 1, 18);
        procedure.AppealDeterminationDeadline = new DateTime(2027, 2, 5);

        procedure.DeterminationIsOverdue(new DateTime(2027, 2, 8)).ShouldBeTrue();
    }

    [Fact]
    public void Once_determined_it_stops_being_overdue()
    {
        var procedure = New();
        procedure.AppealLodgedDate = new DateTime(2027, 1, 18);
        procedure.AppealDeterminationDeadline = new DateTime(2027, 2, 5);
        procedure.AppealDeterminedDate = new DateTime(2027, 2, 3);

        procedure.DeterminationIsOverdue(new DateTime(2027, 3, 1)).ShouldBeFalse();
    }

    [Fact]
    public void With_no_appeal_lodged_nothing_is_overdue()
    {
        New().DeterminationIsOverdue(new DateTime(2030, 1, 1)).ShouldBeFalse();
    }

    [Fact]
    public void The_two_limits_are_the_ones_the_policy_states()
    {
        AppealDeadlines.DaysToLodge.ShouldBe(3);
        AppealDeadlines.WorkingDaysToDetermine.ShouldBe(14);
    }
}
