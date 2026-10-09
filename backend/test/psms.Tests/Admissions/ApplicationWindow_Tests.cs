using Abp.UI;
using psms.Admissions.AdmissionSettings;
using psms.Admissions.AdmissionSettings.Dto;
using psms.Admissions.Shared;
using psms.Domain.Academic.Entities;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

namespace psms.Tests.Admissions;

/// <summary>
/// The application window has to describe a period a parent could apply in.
/// <para>
/// Nothing checked it. A real row on a live tenant said: 2027 intake, Grade 11,
/// window open 03 Feb 2025, close 04 Oct 2025, applications switched on. A year
/// and a half before the year it was for, and already closed.
/// </para>
/// <para>
/// That last part is not cosmetic. Whether a school appears to a prospective
/// parent at all is <c>AreApplicationsOpen()</c>, which weighs the switch
/// against these dates — so a principal could set that up, read "Open" on their
/// own screen, and be invisible to every applicant with nothing saying why.
/// </para>
/// </summary>
public class ApplicationWindow_Tests : psmsTestBase
{
    private readonly IAdmissionSettingsAppService _settings;
    private Guid _yearId;
    private DateTime _yearEnds;

    public ApplicationWindow_Tests()
    {
        LoginAsDefaultTenantAdmin();
        _settings = Resolve<IAdmissionSettingsAppService>();

        _yearId = Guid.NewGuid();
        _yearEnds = new DateTime(2027, 12, 15);

        UsingDbContext(1, context =>
        {
            context.AcademicYears.Add(new AcademicYear(_yearId, 1, 2027,
                new DateTime(2027, 2, 3), _yearEnds) { YearName = "2027 Academic Year" });
            context.SaveChanges();
        });
    }

    private CreateAdmissionSettingsDto Window(
        DateTime? opens, DateTime? closes, bool accepting = true, Guid? gradeId = null) =>
        new CreateAdmissionSettingsDto
        {
            AcademicYearId = _yearId,
            GradeId = gradeId,
            ApplicationFeeAmount = 500m,
            IsAcceptingApplications = accepting,
            ApplicationOpenDate = opens,
            ApplicationCloseDate = closes,
            OfferExpiryDays = 14,
        };

    [Fact]
    public async Task A_window_that_closes_before_it_opens_is_refused()
    {
        var refusal = await Should.ThrowAsync<UserFriendlyException>(
            () => _settings.CreateAsync(Window(
                opens: DateTime.UtcNow.Date.AddDays(30),
                closes: DateTime.UtcNow.Date.AddDays(10))));

        refusal.Message.ShouldBe(AdmissionsExceptionCodes.InvalidApplicationWindow);
        refusal.Details.ShouldContain("cannot close before they open");
    }

    [Fact]
    public async Task A_window_that_closes_after_the_school_year_is_over_is_refused()
    {
        // Nobody can apply for a year that has finished.
        var refusal = await Should.ThrowAsync<UserFriendlyException>(
            () => _settings.CreateAsync(Window(
                opens: new DateTime(2027, 1, 10),
                closes: _yearEnds.AddDays(1))));

        refusal.Message.ShouldBe(AdmissionsExceptionCodes.InvalidApplicationWindow);
        refusal.Details.ShouldContain("already ended");
    }

    [Fact]
    public async Task Being_switched_on_with_a_window_that_already_closed_is_refused()
    {
        // The exact state found on the live tenant, and the one that makes a
        // school invisible while its own screen says Open.
        var refusal = await Should.ThrowAsync<UserFriendlyException>(
            () => _settings.CreateAsync(Window(
                opens: DateTime.UtcNow.Date.AddYears(-2),
                closes: DateTime.UtcNow.Date.AddDays(-1),
                accepting: true)));

        refusal.Message.ShouldBe(AdmissionsExceptionCodes.InvalidApplicationWindow);
        refusal.Details.ShouldContain("no parent can see this school");
    }

    [Fact]
    public async Task The_same_closed_window_is_fine_with_applications_switched_off()
    {
        // Turning the switch off is how a window that has been and gone is
        // recorded. There is no contradiction to complain about.
        var saved = await _settings.CreateAsync(Window(
            opens: DateTime.UtcNow.Date.AddYears(-2),
            closes: DateTime.UtcNow.Date.AddDays(-1),
            accepting: false));

        saved.ShouldNotBeNull();
    }

    [Fact]
    public async Task A_window_that_opens_before_the_year_it_is_for_is_allowed()
    {
        // Applications for a year open during the year before it. That is the
        // normal case and must not be mistaken for the bug above.
        var saved = await _settings.CreateAsync(Window(
            opens: DateTime.UtcNow.Date.AddDays(-30),
            closes: new DateTime(2027, 1, 31)));

        saved.ShouldNotBeNull();
    }

    [Fact]
    public async Task A_window_with_no_dates_at_all_is_allowed()
    {
        // Then the switch is the whole answer, which is what most schools do.
        var saved = await _settings.CreateAsync(Window(opens: null, closes: null));

        saved.ShouldNotBeNull();
    }

    [Fact]
    public async Task Editing_a_row_is_held_to_the_same_rules()
    {
        // The defect reached the database through an editor, so the check has
        // to sit on the way back in as well as on the way in.
        var saved = await _settings.CreateAsync(Window(opens: null, closes: null));

        var refusal = await Should.ThrowAsync<UserFriendlyException>(
            () => _settings.UpdateAsync(saved.Id, new UpdateAdmissionSettingsDto
            {
                IsAcceptingApplications = true,
                ApplicationCloseDate = DateTime.UtcNow.Date.AddDays(-1),
            }));

        refusal.Message.ShouldBe(AdmissionsExceptionCodes.InvalidApplicationWindow);
    }
}
