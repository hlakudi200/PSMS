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

    private async Task<Abp.Application.Services.Dto.ListResultDto<AdmissionSettingsDto>> SeedAsync(
        Guid yearId, Guid? gradeId) =>
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

        all.Items.Select(x => x.AcademicYearId).Distinct()
            .ShouldBe(new[] { _year2027, _year2026 }, ignoreOrder: true);
    }

    [Fact]
    public async Task No_grade_means_every_grade_gets_its_own_settings()
    {
        /* It used to mean one "any grade" row, and almost nothing on these
           settings is true of a whole school at once — the capacity least of
           all, and a six-year-old and a sixteen-year-old cannot sit the same
           admission assessment. Nobody could apply through one either: creating
           an application needs a grade, so the row was a dead end. */
        var written = await SeedAsync(_year2027, null);

        written.Items.Count.ShouldBe(2);
        written.Items.Select(x => x.GradeId)
            .ShouldBe(new Guid?[] { _grade8, _grade11 }, ignoreOrder: true);
        written.Items.ShouldAllBe(x => x.GradeId != null);
    }

    [Fact]
    public async Task And_leaves_a_grade_the_school_has_already_set_up_alone()
    {
        // Somebody decided Grade 11 takes 20 learners. Filling in the rest of
        // the school must not quietly reset that.
        await _settings.CreateAsync(new CreateAdmissionSettingsDto
        {
            AcademicYearId = _year2027,
            GradeId = _grade11,
            ApplicationFeeAmount = 500m,
            MaxCapacity = 20,
            OfferExpiryDays = 14,
        });

        var written = await SeedAsync(_year2027, null);

        written.Items.Single().GradeId.ShouldBe(_grade8);

        var all = await _settings.GetAllAsync(_year2027, _grade11);
        all.Items.Single().MaxCapacity.ShouldBe(20);
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
    public async Task A_leftover_year_wide_row_sits_above_the_grades()
    {
        /* New settings are always written per grade, but schools configured
           before that have an "any grade" row in the database. It still sorts
           to the top, where the action that turns it into real per-grade
           settings is waiting. */
        var legacy = Guid.NewGuid();
        UsingDbContext(1, context =>
        {
            context.AdmissionSettings.Add(
                new psms.Domain.Admissions.Entities.AdmissionSettings(legacy, 1, _year2027, 500m)
                {
                    GradeId = null,
                    OfferExpiryDays = 14,
                });
            context.SaveChanges();
        });

        await SeedAsync(_year2027, _grade11);

        var all = await _settings.GetAllAsync(_year2027);

        all.Items.First().GradeId.ShouldBeNull();
    }

    [Fact]
    public async Task And_can_be_turned_into_the_per_grade_settings_it_stood_for()
    {
        var legacy = Guid.NewGuid();
        UsingDbContext(1, context =>
        {
            context.AdmissionSettings.Add(
                new psms.Domain.Admissions.Entities.AdmissionSettings(legacy, 1, _year2027, 750m)
                {
                    GradeId = null,
                    MaxCapacity = 100,
                    IsAcceptingApplications = true,
                    OfferExpiryDays = 21,
                });
            context.SaveChanges();
        });

        var written = await _settings.ExpandToEveryGradeAsync(legacy);

        written.Items.Count.ShouldBe(2);
        written.Items.ShouldAllBe(x => x.ApplicationFeeAmount == 750m);
        written.Items.ShouldAllBe(x => x.OfferExpiryDays == 21);

        // And the row nobody could apply through is gone.
        var all = await _settings.GetAllAsync(_year2027);
        all.Items.ShouldAllBe(x => x.GradeId != null);
    }

    [Fact]
    public async Task A_school_with_no_settings_gets_an_empty_list_not_an_error()
    {
        (await _settings.GetAllAsync()).Items.ShouldBeEmpty();
    }
}
