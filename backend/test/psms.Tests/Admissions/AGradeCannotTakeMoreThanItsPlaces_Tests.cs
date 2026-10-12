using psms.Admissions.Shared;
using psms.Domain.Admissions.Entities;
using Shouldly;
using System;
using Xunit;

namespace psms.Tests.Admissions;

/// <summary>
/// A grade stops taking learners once its places are gone.
/// <para>
/// Capacity counts learners who have enrolled, not people who have applied —
/// so a grade with two places and three applicants reads "2 available" and is
/// quite right. Schools want more applicants than places; that is how they get
/// to choose.
/// </para>
/// <para>
/// What was missing sat one step later. Enrolment incremented the count and
/// never checked it, so those two places could seat a third child and a fourth.
/// <c>GradeCapacityFull</c> (ADM-028) was defined, the enrolment code cited it
/// in a comment, and nothing ever threw it. The available figure floors at
/// zero, so the overshoot did not even show.
/// </para>
/// </summary>
public class AGradeCannotTakeMoreThanItsPlaces_Tests
{
    private static AdmissionSettings WithPlaces(int? capacity, int enrolled)
    {
        var settings = new AdmissionSettings(Guid.NewGuid(), 1, Guid.NewGuid(), 0m)
        {
            MaxCapacity = capacity,
            CurrentEnrolledCount = enrolled,
        };
        return settings;
    }

    [Fact]
    public void Three_applicants_for_two_places_is_not_full()
    {
        // Nobody has enrolled yet. Both places are still there to be offered.
        var settings = WithPlaces(capacity: 2, enrolled: 0);

        settings.IsAtCapacity().ShouldBeFalse();
        settings.GetAvailableSpots().ShouldBe(2);
    }

    [Fact]
    public void One_enrolled_leaves_one_place()
    {
        var settings = WithPlaces(capacity: 2, enrolled: 1);

        settings.IsAtCapacity().ShouldBeFalse();
        settings.GetAvailableSpots().ShouldBe(1);
    }

    [Fact]
    public void And_the_second_closes_it()
    {
        var settings = WithPlaces(capacity: 2, enrolled: 2);

        settings.IsAtCapacity().ShouldBeTrue();
        settings.GetAvailableSpots().ShouldBe(0);
    }

    [Fact]
    public void A_grade_with_no_stated_limit_never_fills()
    {
        // Leaving the capacity empty means the school is not counting.
        var settings = WithPlaces(capacity: null, enrolled: 500);

        settings.IsAtCapacity().ShouldBeFalse();
    }

    [Fact]
    public void An_overshoot_still_reads_as_full_rather_than_as_room()
    {
        // Rows written before enrolment was checked can sit above their own
        // capacity. That must read as "no room", not as a negative number of
        // places, and certainly not as room for one more.
        var settings = WithPlaces(capacity: 2, enrolled: 3);

        settings.IsAtCapacity().ShouldBeTrue();
        settings.GetAvailableSpots().ShouldBe(0);
    }

    [Fact]
    public void The_refusal_has_a_code_of_its_own()
    {
        // ADM-028, so the screen can tell this apart from any other refusal.
        AdmissionsExceptionCodes.GradeCapacityFull.ShouldBe("ADM028_GRADE_CAPACITY_FULL");
    }
}
