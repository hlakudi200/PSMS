using psms.Domain.Admissions.Entities;
using Shouldly;
using System;
using Xunit;

namespace psms.Tests.Admissions;

/// <summary>
/// Which schools a prospective parent is offered.
/// <para>
/// The sign-up page asked them to type the school's name exactly. "School ABC"
/// and "SchoolABC" are the same school to a parent and different strings to us,
/// and the only feedback was a flat refusal that read as "your school is not on
/// this system". They pick from a list now.
/// </para>
/// <para>
/// A school is on that list when it is open to applications, which is both the
/// useful filter — a parent cannot apply to a closed school anyway — and the
/// careful one: a school that never opens admissions online never appears, so
/// this is not a directory of everyone who uses the product. These pin the rule
/// the list is built on.
/// </para>
/// </summary>
public class SchoolsAcceptingApplications_Tests
{
    private static Domain.Admissions.Entities.AdmissionSettings Settings(
        bool accepting, DateTime? opens = null, DateTime? closes = null)
    {
        var settings = new Domain.Admissions.Entities.AdmissionSettings(
            Guid.NewGuid(), 1, Guid.NewGuid(), 500m);

        settings.IsAcceptingApplications = accepting;
        settings.ApplicationOpenDate = opens;
        settings.ApplicationCloseDate = closes;

        return settings;
    }

    [Fact]
    public void A_school_with_the_switch_on_and_no_dates_is_open()
    {
        Settings(accepting: true).AreApplicationsOpen().ShouldBeTrue();
    }

    [Fact]
    public void A_school_with_the_switch_off_is_not()
    {
        Settings(accepting: false).AreApplicationsOpen().ShouldBeFalse();
    }

    [Fact]
    public void The_switch_alone_does_not_open_a_window_that_has_not_started()
    {
        // A school that set up next year's intake in advance should not be
        // taking applications for it today.
        Settings(accepting: true, opens: DateTime.UtcNow.AddDays(30))
            .AreApplicationsOpen().ShouldBeFalse();
    }

    [Fact]
    public void Nor_one_that_has_closed()
    {
        Settings(accepting: true, closes: DateTime.UtcNow.AddDays(-1))
            .AreApplicationsOpen().ShouldBeFalse();
    }

    [Fact]
    public void A_window_that_is_running_is_open()
    {
        Settings(accepting: true,
                 opens: DateTime.UtcNow.AddDays(-7),
                 closes: DateTime.UtcNow.AddDays(7))
            .AreApplicationsOpen().ShouldBeTrue();
    }

    [Fact]
    public void Leaving_the_switch_off_keeps_a_school_off_the_list_whatever_the_dates_say()
    {
        // The switch is the school's answer. Dates describe when, not whether.
        Settings(accepting: false,
                 opens: DateTime.UtcNow.AddDays(-7),
                 closes: DateTime.UtcNow.AddDays(7))
            .AreApplicationsOpen().ShouldBeFalse();
    }

    [Fact]
    public void New_settings_start_closed()
    {
        // A school that has just been set up has not decided to take
        // applications yet, and must not be advertised as though it had.
        new Domain.Admissions.Entities.AdmissionSettings(Guid.NewGuid(), 1, Guid.NewGuid(), 500m)
            .AreApplicationsOpen().ShouldBeFalse();
    }
}
