using psms.Assessment.Reports.Pdf;
using psms.Domain.Assessment;
using psms.Domain.Shared.Enums;
using Shouldly;
using System.Collections.Generic;
using Xunit;

namespace psms.Tests.Assessment;

/// <summary>
/// RC-19. A Foundation Phase report card reports in national codes and their
/// achievement descriptions, and in nothing else.
/// <para>
/// National Protocol §17(4): a percentage is provided for from Grade 4 onward.
/// For Grades R–3 the Protocol gives the code and its description as the
/// reporting instrument, so a Grade 1 card printing "68.0%" is reporting on a
/// scale the phase does not use — on a document that is the school's legal
/// record of the learner's year.
/// </para>
/// </summary>
[Collection(PdfRenderCollection.Name)]
public class FoundationPhaseReportCard_Tests
{
    private static ReportPdfData Card(bool reportsPercentages) => new ReportPdfData
    {
        SchoolName = "Riverside Primary School",
        PrimaryColor = "#C8102E",
        SecondaryColor = "#003D73",
        StudentName = "Ayanda Nkosi",
        AdmissionNumber = "STU-2026-004",
        ClassName = "Grade 1A",
        TermName = "Term 1",
        AcademicYearName = "2026 Academic Year",
        ReportType = "Term 1 Report",
        GeneratedDate = "08 Oct 2026",
        ReportCardNumber = "2026/GR1A/STU-2026-004/T1",
        ReportsPercentages = reportsPercentages,
        OverallPercentage = 68.4m,
        OverallAchievementLevel = CapsAchievementLevel.Level5,
        ClassPosition = 4,
        TotalStudentsInClass = 30,
        DaysInTerm = 60,
        DaysPresent = 58,
        DaysAbsent = 2,
        Subjects = new List<SubjectEntry>
        {
            new SubjectEntry
            {
                SubjectName = "English Home Language", SubjectCode = "EHL01",
                TermMark = 68m, FinalMark = 68m, ClassAverage = 61.2m, SubjectPosition = 7,
                AchievementLevel = CapsAchievementLevel.Level5,
                TeacherName = "L. Mokoena", TeacherComment = "Reads with growing confidence.",
            },
            new SubjectEntry
            {
                SubjectName = "Mathematics", SubjectCode = "MAT01",
                TermMark = 81m, FinalMark = 81m, ClassAverage = 66.8m, SubjectPosition = 2,
                AchievementLevel = CapsAchievementLevel.Level7,
                TeacherName = "L. Mokoena", TeacherComment = "Confident with number bonds.",
            },
        },
    };

    [Theory]
    [InlineData(SouthAfricanGradeLevel.GradeR)]
    [InlineData(SouthAfricanGradeLevel.Grade1)]
    [InlineData(SouthAfricanGradeLevel.Grade2)]
    [InlineData(SouthAfricanGradeLevel.Grade3)]
    public void The_foundation_phase_does_not_report_percentages(SouthAfricanGradeLevel grade)
    {
        ReportingScale.ReportsPercentages(grade).ShouldBeFalse();
    }

    [Theory]
    [InlineData(SouthAfricanGradeLevel.Grade4)]
    [InlineData(SouthAfricanGradeLevel.Grade7)]
    [InlineData(SouthAfricanGradeLevel.Grade9)]
    [InlineData(SouthAfricanGradeLevel.Grade10)]
    [InlineData(SouthAfricanGradeLevel.Grade12)]
    public void Every_other_phase_still_reports_percentages(SouthAfricanGradeLevel grade)
    {
        ReportingScale.ReportsPercentages(grade).ShouldBeTrue();
    }

    [Fact]
    public void A_grade_whose_record_has_gone_keeps_percentages()
    {
        // An older card whose class or grade was removed. Percentages are the
        // behaviour for every phase but one; stripping them on a guess would
        // quietly rewrite a Grade 8 card.
        ReportingScale.ReportsPercentages((SouthAfricanGradeLevel?)null).ShouldBeTrue();
    }

    [Fact]
    public void A_foundation_phase_card_prints_no_percentage_anywhere()
    {
        var text = PdfText.Extract(ReportPdfGenerator.Generate(Card(reportsPercentages: false)));

        // The marks themselves.
        text.ShouldNotContain("68.0");
        text.ShouldNotContain("81.0");
        // The class comparison and the overall aggregate.
        text.ShouldNotContain("61.2");
        text.ShouldNotContain("68.4");
        text.ShouldNotContain("Overall Average");
        // The percentage column headings. Matched on the "(%)" they carry
        // rather than on the word — "CLASS" also appears in "Class Teacher",
        // and Shouldly compares without case.
        text.ShouldNotContain("MARK (%)");
        text.ShouldNotContain("AVG (%)");
    }

    [Fact]
    public void A_foundation_phase_card_reports_the_code_and_its_description()
    {
        var text = PdfText.Extract(ReportPdfGenerator.Generate(Card(reportsPercentages: false)));

        text.ShouldContain("ACHIEVEMENT");
        text.ShouldContain("Substantial achievement");   // level 5
        text.ShouldContain("Outstanding achievement");   // level 7
        text.ShouldContain("English Home Language");
        text.ShouldContain("Mathematics");
    }

    [Fact]
    public void A_foundation_phase_card_still_carries_everything_else_a_card_must()
    {
        var text = PdfText.Extract(ReportPdfGenerator.Generate(Card(reportsPercentages: false)));

        // Dropping the percentages must not drop the rest of RE-002.
        text.ShouldContain("Riverside Primary School");
        text.ShouldContain("Ayanda Nkosi");
        text.ShouldContain("Grade 1A");
        text.ShouldContain("2026/GR1A/STU-2026-004/T1");
        text.ShouldContain("DAYS PRESENT");
        text.ShouldContain("Class Teacher");
        text.ShouldContain("Principal");
        // §25(8)(b): the codes still have to be explained.
        text.ShouldContain("ACHIEVEMENT LEVELS");
    }

    [Fact]
    public void A_card_outside_the_phase_is_untouched()
    {
        var text = PdfText.Extract(ReportPdfGenerator.Generate(Card(reportsPercentages: true)));

        text.ShouldContain("68.0");
        text.ShouldContain("61.2");
        text.ShouldContain("Overall Average");
    }

    [Fact]
    public void A_foundation_phase_card_fits_on_one_page()
    {
        using var pdf = UglyToad.PdfPig.PdfDocument.Open(
            ReportPdfGenerator.Generate(Card(reportsPercentages: false)));

        pdf.NumberOfPages.ShouldBe(1);
    }
}
