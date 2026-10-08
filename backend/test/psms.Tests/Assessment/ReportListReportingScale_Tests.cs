using NSubstitute;
using NSubstitute.ClearExtensions;
using psms.Assessment.Reports;
using psms.Assessment.Reports.Dto;
using psms.Domain.Academic.Entities;
using psms.Domain.Assessment.Entities;
using psms.Domain.Shared.Enums;
using psms.Domain.Shared.Storage;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace psms.Tests.Assessment;

/// <summary>
/// RC-19. The list of report cards reports each learner on their own phase's
/// scale.
/// <para>
/// National Protocol §17(4) provides for a percentage from Grade 4 onward; for
/// Grades R–3 the reporting instrument is the national code and its achievement
/// description. The Protocol does not rank Foundation Phase learners against one
/// another either.
/// </para>
/// <para>
/// The card itself and the printed PDF both honoured that. The list did not —
/// it had no idea which phase a row belonged to, so every Grade R learner
/// carried an overall percentage and a class position in two columns, in front
/// of whoever had the list open.
/// </para>
/// </summary>

public class ReportListReportingScale_Tests : psmsTestBase
{
    private readonly IReportAppService _reports;

    public ReportListReportingScale_Tests()
    {
        LoginAsDefaultTenantAdmin();
        _reports = Resolve<IReportAppService>();

        var storage = Resolve<IFileStorageService>();
        storage.ClearSubstitute();
        storage.DefaultBucketName.Returns("psms-files");
    }

    /// <summary>A card for a learner in <paramref name="grade"/>.</summary>
    private Guid SeedCard(SouthAfricanGradeLevel grade, string className, decimal overall, int position)
    {
        var id = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var classId = Guid.NewGuid();
        var gradeId = Guid.NewGuid();
        var yearId = Guid.NewGuid();

        UsingDbContext(1, context =>
        {
            context.AcademicYears.Add(new AcademicYear(yearId, 1, 2026,
                new DateTime(2026, 1, 1), new DateTime(2026, 12, 1)));

            context.Grades.Add(new Grade(gradeId, 1, grade, className,
                psms.Domain.Academic.SchoolPhases.PhaseFor(grade)));

            context.Classes.Add(new Class(classId, 1, className + "A", gradeId, yearId, 30));

            context.Students.Add(new Student(studentId, 1, "Ayanda", "Nkosi",
                new DateTime(2019, 1, 1), Gender.Female, "A" + position, DateTime.Today, gradeId, classId));

            var report = new Report(id, 1, studentId, classId, yearId, ReportType.Term1);
            report.OverallPercentage = overall;
            report.ClassPosition = position;

            context.Reports.Add(report);
            context.SaveChanges();
        });

        return id;
    }

    private async Task<ReportListDto> RowAsync(Guid id)
    {
        var page = await _reports.GetAllAsync(new GetReportsInput { MaxResultCount = 100 });
        return page.Items.FirstOrDefault(r => r.Id == id);
    }

    [Fact]
    public async Task A_Grade_R_row_is_marked_as_not_reporting_percentages()
    {
        var id = SeedCard(SouthAfricanGradeLevel.GradeR, "Grade R", 68m, 5);

        var row = await RowAsync(id);

        row.ShouldNotBeNull();
        row.ReportsPercentages.ShouldBeFalse();
    }

    [Fact]
    public async Task So_is_a_Grade_3_row_the_last_of_the_phase()
    {
        var id = SeedCard(SouthAfricanGradeLevel.Grade3, "Grade 3", 71m, 2);

        (await RowAsync(id)).ReportsPercentages.ShouldBeFalse();
    }

    [Fact]
    public async Task A_Grade_4_row_reports_percentages_the_phase_boundary()
    {
        // §17(4) turns the percentage on at Grade 4, not before it.
        var id = SeedCard(SouthAfricanGradeLevel.Grade4, "Grade 4", 67m, 4);

        (await RowAsync(id)).ReportsPercentages.ShouldBeTrue();
    }

    [Fact]
    public async Task A_Grade_10_row_reports_percentages()
    {
        var id = SeedCard(SouthAfricanGradeLevel.Grade10, "Grade 10", 47m, 6);

        (await RowAsync(id)).ReportsPercentages.ShouldBeTrue();
    }

    [Fact]
    public async Task The_figures_themselves_are_left_on_the_row()
    {
        // The flag says how to read the row, not what the school recorded. A
        // Foundation Phase card still has a mark behind it — the screen is what
        // must not put a percentage and a ranking in front of a parent, and
        // blanking the data here would hide it from the school's own reports
        // as well.
        var id = SeedCard(SouthAfricanGradeLevel.GradeR, "Grade R", 68m, 5);

        var row = await RowAsync(id);

        row.OverallPercentage.ShouldBe(68m);
        row.ClassPosition.ShouldBe(5);
        row.ReportsPercentages.ShouldBeFalse();
    }

    [Fact]
    public async Task A_list_spanning_two_phases_marks_each_row_for_itself()
    {
        // The real case: a school's list is not one phase.
        var foundation = SeedCard(SouthAfricanGradeLevel.Grade1, "Grade 1", 74m, 1);
        var intermediate = SeedCard(SouthAfricanGradeLevel.Grade5, "Grade 5", 58m, 9);

        (await RowAsync(foundation)).ReportsPercentages.ShouldBeFalse();
        (await RowAsync(intermediate)).ReportsPercentages.ShouldBeTrue();
    }
}
