using psms.Domain.Assessment;
using psms.Domain.Assessment.Entities;
using psms.Domain.Shared.Enums;
using Shouldly;
using System;
using System.Collections.Generic;
using Xunit;

namespace psms.Tests.Assessment;

/// <summary>
/// RC-23. Exempting a learner from a task — National Protocol §8(9).
/// <para>
/// "A learner who is not able to offer the Physical Education Task (PET) as a
/// fifth component in Life Orientation may be exempted … provided a valid
/// medical reason is submitted. If the learner's request for exemption is
/// successful, <b>his or her marks for Life Orientation will be recalculated in
/// terms of four tasks</b>."
/// </para>
/// <para>
/// The recalculation is not a separate sum. An exempted task carries no
/// percentage, so it drops out of the subject average and the remaining four
/// carry the mark. What the system was missing was any way to say a task had
/// been excused: <c>MarkStatus.Exempted</c> existed and was honoured in two
/// places, and nothing could produce it.
/// </para>
/// </summary>
public class PetExemption_Tests
{
    private static Mark TaskWithMark(decimal percentage)
    {
        var mark = new Mark(Guid.NewGuid(), 1, Guid.NewGuid(), Guid.NewGuid());
        mark.RecordMark(percentage, 100m, teacherUserId: 1);
        return mark;
    }

    [Fact]
    public void Exempting_a_task_leaves_it_with_no_mark_to_count()
    {
        var mark = TaskWithMark(64m);

        mark.Exempt("Medical certificate on file, 12 March.");

        mark.Status.ShouldBe(MarkStatus.Exempted);
        mark.Percentage.ShouldBeNull();
        mark.AchievementLevel.ShouldBeNull();
        mark.ExemptionReason.ShouldBe("Medical certificate on file, 12 March.");
    }

    [Fact]
    public void The_raw_mark_survives_as_the_record_of_what_was_entered()
    {
        // Same philosophy as moderation: RawMark is the audit of what was
        // actually recorded, and the percentage is what stops counting.
        var mark = TaskWithMark(64m);

        mark.Exempt("Medical certificate on file.");

        mark.RawMark.ShouldBe(64m);
    }

    [Fact]
    public void An_exemption_has_to_say_what_it_was_granted_on()
    {
        // §8(9) grants it "provided a valid medical reason is submitted", so an
        // exemption with nothing recorded against it is not one the policy
        // describes.
        var mark = TaskWithMark(64m);

        Should.Throw<ArgumentException>(() => mark.Exempt(null));
        Should.Throw<ArgumentException>(() => mark.Exempt("   "));
        mark.Status.ShouldNotBe(MarkStatus.Exempted);
    }

    [Fact]
    public void An_exemption_can_be_withdrawn()
    {
        var mark = TaskWithMark(64m);
        mark.Exempt("Submitted in error.");

        mark.ClearExemption();

        mark.Status.ShouldBe(MarkStatus.Pending);
        mark.ExemptionReason.ShouldBeNull();
    }

    [Fact]
    public void Withdrawing_an_exemption_that_was_never_granted_changes_nothing()
    {
        var mark = TaskWithMark(64m);

        mark.ClearExemption();

        mark.Status.ShouldBe(MarkStatus.Completed);
        mark.Percentage.ShouldBe(64m);
    }

    [Fact]
    public void Exempting_is_not_the_same_as_being_absent()
    {
        var mark = TaskWithMark(64m);

        mark.Exempt("Medical certificate on file.");

        // Absence is a learner who missed a task they owed; an exemption is a
        // task they never owed.
        mark.WasAbsent.ShouldBeFalse();
        mark.Status.ShouldBe(MarkStatus.Exempted);
    }

    /* ─── what the exemption is for: the §8(9) recalculation ─── */

    [Fact]
    public void Life_orientation_over_five_tasks_and_then_over_four()
    {
        // Five tasks: 60, 70, 80, 50 and a PET of 20.
        var allFive = new List<AssessmentContribution>
        {
            new AssessmentContribution(60m, 1m, false),
            new AssessmentContribution(70m, 1m, false),
            new AssessmentContribution(80m, 1m, false),
            new AssessmentContribution(50m, 1m, false),
            new AssessmentContribution(20m, 1m, false, isPracticalAssessmentTask: true),
        };

        // Life Orientation in the FET phase is 100% school-based (§31(2)).
        var withPet = SubjectMarkAggregator.Aggregate(
            allFive, sbaPercentage: 100, examPercentage: 0, examinationIsExternal: false);

        withPet.FinalMark.ShouldBe(56m);          // (60+70+80+50+20)/5

        // Exempted, the PET is simply not among the marks that reach the
        // aggregator — the contribution query takes completed marks only.
        var exempted = allFive.GetRange(0, 4);

        var withoutPet = SubjectMarkAggregator.Aggregate(
            exempted, sbaPercentage: 100, examPercentage: 0, examinationIsExternal: false);

        withoutPet.FinalMark.ShouldBe(65m);       // (60+70+80+50)/4
    }

    [Fact]
    public void Scoring_the_exempted_task_zero_would_punish_the_learner()
    {
        // The thing the exemption exists to prevent, kept here so the
        // difference is visible rather than asserted in the abstract.
        var withZero = new List<AssessmentContribution>
        {
            new AssessmentContribution(60m, 1m, false),
            new AssessmentContribution(70m, 1m, false),
            new AssessmentContribution(80m, 1m, false),
            new AssessmentContribution(50m, 1m, false),
            new AssessmentContribution(0m, 1m, false),
        };

        var punished = SubjectMarkAggregator.Aggregate(
            withZero, sbaPercentage: 100, examPercentage: 0, examinationIsExternal: false);

        punished.FinalMark.ShouldBe(52m);         // against 65 when properly exempted
    }
}
