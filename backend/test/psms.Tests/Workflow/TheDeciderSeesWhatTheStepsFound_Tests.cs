using psms.Shared;
using Shouldly;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace psms.Tests.Workflow;

/// <summary>
/// The principal decides on what the steps actually found, and a teacher is not
/// handed an identity number on the way.
/// <para>
/// Two things about the same method, found by walking admissions end to end on
/// the deployed system.
/// </para>
/// <para>
/// The assessment dialog tells the teacher "the principal reads this before
/// deciding". The principal's screen showed the applicant, the parents and the
/// documents, and nothing at all about the interview or the assessment — so a
/// result recorded was a result nobody used.
/// </para>
/// <para>
/// And the summary renders the linked record for anyone who can view the
/// workflow instance, which is wider than the admissions permissions admit. A
/// teacher refused <c>Application/Get</c> and
/// <c>ApplicationDocument/GetAllByApplication</c> outright was still handed the
/// learner's full thirteen-digit identity number through here, while the admin
/// screen masked the same field.
/// </para>
/// </summary>
public class TheDeciderSeesWhatTheStepsFound_Tests
{
    private static string Source()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, "src")))
            directory = directory.Parent;

        return File.ReadAllText(Path.Combine(directory.FullName,
            "src", "psms.Application", "Workflow", "Shared", "WorkflowEntitySummaryProvider.cs"));
    }

    private static string ApplicationSummaryBody()
    {
        var source = Source();
        var start = source.IndexOf("ApplicationSummary(Guid id)", StringComparison.Ordinal);
        start.ShouldBeGreaterThan(-1);
        var end = source.IndexOf("\n    private ", start + 1, StringComparison.Ordinal);
        return end > start ? source.Substring(start, end - start) : source.Substring(start);
    }

    [Fact]
    public void The_identity_number_is_masked_before_it_leaves()
    {
        var body = ApplicationSummaryBody();

        body.ShouldContain("PiiMasking.Mask(a.IdNumber)");
        body.ShouldContain("PiiMasking.Mask(a.PassportNumber)");
        // The unmasked field must not survive anywhere in the summary.
        body.ShouldNotContain("\"ID Number\", a.IdNumber");
        body.ShouldNotContain("\"Passport\", a.PassportNumber");
    }

    [Fact]
    public void Masking_leaves_enough_to_recognise_and_not_enough_to_use()
    {
        // The exact shape the admin screen already shows.
        PiiMasking.Mask("1503140001084").ShouldBe("*********1084");
        PiiMasking.Mask(null).ShouldBeNull();
        PiiMasking.Mask("").ShouldBe("");
    }

    [Fact]
    public void The_summary_carries_what_both_steps_produced()
    {
        var body = ApplicationSummaryBody();

        body.ShouldContain("Placement assessment");
        body.ShouldContain("Interview");
        // The figures the decision actually turns on.
        body.ShouldContain("MaxScore");
        body.ShouldContain("Percentage");
        body.ShouldContain("Recommended");
        body.ShouldContain("Rating");
    }

    [Fact]
    public void An_unmarked_assessment_says_so_rather_than_reading_as_zero()
    {
        /* A result of "0 / 50 (0%)" on a paper nobody has marked would read as
           a failed assessment, which is the worst possible way to be wrong on a
           screen somebody decides a child's place from. */
        var body = ApplicationSummaryBody();

        body.ShouldContain("Not marked yet");
        body.ShouldContain("CompletedDate.HasValue");
    }

    [Theory]
    [InlineData(null, "—")]
    [InlineData("   ", "—")]
    [InlineData("Solid paper.", "Solid paper.")]
    [InlineData("First line\nsecond line", "First line second line")]
    public void A_note_in_a_table_cell_stays_on_one_line(string input, string expected)
    {
        var trim = typeof(psms.Workflow.Shared.WorkflowEntitySummaryProvider)
            .GetMethod("Trim", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        trim.ShouldNotBeNull();
        trim.Invoke(null, new object[] { input, 160 }).ShouldBe(expected);
    }

    [Fact]
    public void A_long_note_is_cut_rather_than_pushing_the_table_off_the_screen()
    {
        var trim = typeof(psms.Workflow.Shared.WorkflowEntitySummaryProvider)
            .GetMethod("Trim", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var result = (string)trim.Invoke(null, new object[] { new string('x', 400), 160 });

        result.Length.ShouldBe(160);
        result.ShouldEndWith("…");
    }
}
