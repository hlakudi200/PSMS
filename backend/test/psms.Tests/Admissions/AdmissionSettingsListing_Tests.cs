using psms.Admissions.AdmissionSettings;
using psms.Admissions.AdmissionSettings.Dto;
using psms.Domain.Academic.Entities;
using psms.Domain.Shared.Enums;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace psms.Tests.Admissions;

/// <summary>
/// Reading the school's admission settings.
/// <para>
/// They could only be read one academic year at a time, and the screen that
/// lists them defaults its year filter to "All Years". So it asked for nothing,
/// showed nothing, and a school that had configured an intake was told it had
/// none — the row was in the database the whole time.
/// </para>
/// </summary>
public class AdmissionSettingsListing_Tests : psmsTestBase
{
    private readonly IAdmissionSettingsAppService _settings;

    private Guid _year2026;
    private Guid _year2027;
    private Guid _grade11;
    private Guid _grade8;

    public AdmissionSettingsListing_Tests()
    {
        LoginAsDefaultTenantAdmin();
        _settings = Resolve<IAdmissionSettingsAppService>();

        _year2026 = Guid.NewGuid();
        _year2027 = Guid.NewGuid();
        _grade11 = Guid.NewGuid();
        _grade8 = Guid.NewGuid();

        UsingDbContext(1, context =>
        {
            context.AcademicYears.Add(new AcademicYear(_year2026, 1, 2026,
                new DateTime(2026, 1, 15), new DateTime(2026, 12, 15)) { YearName = "2026" });
            context.AcademicYears.Add(new AcademicYear(_year2027, 1, 2027,
                new DateTime(2027, 2, 3), new DateTime(2027, 12, 15)) { YearName = "2027" });

            context.Grades.Add(new Grade(_grade8, 1,
                SouthAfricanGradeLevel.Grade8, "Grade 8", SouthAfricanSchoolPhase.Senior));
            context.Grades.Add(new Grade(_grade11, 1,
                SouthAfricanGradeLevel.Grade11, "Grade 11", SouthAfricanSchoolPhase.FET));

            context.SaveChanges();
        });
    }

    private async Task SeedAsync(Guid yearId, Guid? gradeId) =>
        await _settings.CreateAsync(new CreateAdmissionSettingsDto
        {
            AcademicYearId = yearId,
            GradeId = gradeId,
            ApplicationFeeAmount = 500m,
            OfferExpiryDays = 14,
        });

    [Fact]
    public async Task Asking_for_everything_returns_every_year()
    {
        // The case the screen was in: no year chosen, and a school with rows
        // was shown an empty table.
        await SeedAsync(_year2026, null);
        await SeedAsync(_year2027, _grade11);

        var all = await _settings.GetAllAsync();

        all.Items.Count.ShouldBe(2);
    }

    [Fact]
    public async Task A_year_narrows_it()
    {
        await SeedAsync(_year2026, null);
        await SeedAsync(_year2027, _grade11);

        var only2027 = await _settings.GetAllAsync(_year2027);

        only2027.Items.Count.ShouldBe(1);
        only2027.Items.Single().AcademicYearId.ShouldBe(_year2027);
    }

    [Fact]
    public async Task So_does_a_grade()
    {
        await SeedAsync(_year2027, _grade11);
        await SeedAsync(_year2027, _grade8);

        var only11 = await _settings.GetAllAsync(_year2027, _grade11);

        only11.Items.Single().GradeId.ShouldBe(_grade11);
    }

    [Fact]
    public async Task The_newest_year_comes_first()
    {
        await SeedAsync(_year2026, null);
        await SeedAsync(_year2027, null);

        var all = await _settings.GetAllAsync();

        all.Items.First().AcademicYearId.ShouldBe(_year2027);
    }

    [Fact]
    public async Task The_default_sits_above_the_grades_it_is_the_default_for()
    {
        await SeedAsync(_year2027, _grade11);
        await SeedAsync(_year2027, null);

        var all = await _settings.GetAllAsync(_year2027);

        all.Items.First().GradeId.ShouldBeNull();
    }

    [Fact]
    public async Task A_school_with_no_settings_gets_an_empty_list_not_an_error()
    {
        (await _settings.GetAllAsync()).Items.ShouldBeEmpty();
    }
}
