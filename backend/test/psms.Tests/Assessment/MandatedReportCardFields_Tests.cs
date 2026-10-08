using psms.Assessment.Reports.Pdf;
using psms.Domain.Shared.Enums;
using Shouldly;
using System.Collections.Generic;
using Xunit;

namespace psms.Tests.Assessment;

/// <summary>
/// RC-21. The fields the National Protocol names as essential on a report card.
/// <para>
/// §25(8): "(a) Personal details: Name of the learner, grade and class of the
/// learner, date of birth, school attendance profile. (b) Official school
/// details: Year and term, name of the school, date, signature and comment of
/// parent or guardian, teacher and principal, dates of closing and opening of
/// school, school stamp, explanation of the codes of the national coding
/// system. (c) Performance details … (d) Constructive feedback: … comments
/// about the learner's performance in relation to his or her previous
/// performance."
/// </para>
/// <para>
/// RC-17 built the card's field list from our own RE-002, which was written
/// from this and is not quite the same list. These are the ones that were
/// missing.
/// </para>
/// </summary>
[Collection(PdfRenderCollection.Name)]
public class MandatedReportCardFields_Tests
{
    private static ReportPdfData Card() => new ReportPdfData
    {
        SchoolName = "Riverside Secondary School",
        PrimaryColor = "#C8102E",
        SecondaryColor = "#003D73",
        StudentName = "Thandeka Masinga",
        AdmissionNumber = "STU-2026-023",
        ClassName = "11C (Arts Stream)",
        GradeName = "Grade 11",
        DateOfBirth = "14 Mar 2009",
        SchoolOpensOn = "15 Jan 2026",
        SchoolClosesOn = "27 Mar 2026",
        PreviousPerformance = "Term 1: 64.2% (up 3.2)",
        TermName = "Term 2",
        AcademicYearName = "2026 Academic Year",
        ReportType = "Term 2 Report",
        GeneratedDate = "08 Oct 2026",
        ReportCardNumber = "2026/GR11C/STU-2026-023/T2",
        DaysInTerm = 60,
        DaysPresent = 58,
        DaysAbsent = 2,
        DaysLate = 1,
        OverallPercentage = 67.4m,
        OverallAchievementLevel = CapsAchievementLevel.Level6,
        ParentComment = "Thank you, we have discussed this at home.",
        Subjects = new List<SubjectEntry>
        {
            new SubjectEntry
            {
                SubjectName = "Mathematics", SubjectCode = "MAT01",
                TermMark = 68m, FinalMark = 68m,
                AchievementLevel = CapsAchievementLevel.Level5,
                TeacherName = "T. Petersen", TeacherComment = "Solid algebra.",
            },
        },
    };

    private static string TextOf(ReportPdfData data) =>
        PdfText.Extract(ReportPdfGenerator.Generate(data));

    [Fact]
    public void The_card_carries_the_learners_date_of_birth()
    {
        TextOf(Card()).ShouldContain("14 Mar 2009");
    }

    [Fact]
    public void The_card_names_the_grade_as_well_as_the_class()
    {
        // "11C (Arts Stream)" does not say Grade 11 to a reader who does not
        // already know the school's class names.
        var text = TextOf(Card());

        text.ShouldContain("Grade 11");
        text.ShouldContain("11C (Arts Stream)");
    }

    [Fact]
    public void The_card_gives_the_dates_the_school_opened_and_closed()
    {
        var text = TextOf(Card());

        text.ShouldContain("15 Jan 2026");
        text.ShouldContain("27 Mar 2026");
    }

    [Fact]
    public void The_card_says_how_this_term_compares_with_the_last()
    {
        TextOf(Card()).ShouldContain("Term 1: 64.2%");
    }

    [Fact]
    public void A_first_card_of_the_year_simply_omits_the_comparison()
    {
        // Nothing to compare with, so nothing is claimed.
        var data = Card();
        data.PreviousPerformance = null;

        var text = TextOf(data);

        text.ShouldNotContain("Previously");
        text.ShouldContain("Thandeka Masinga");
    }

    [Fact]
    public void A_parents_comment_is_printed_when_there_is_one()
    {
        TextOf(Card()).ShouldContain("Thank you, we have discussed this at home.");
    }

    [Fact]
    public void The_card_still_fits_on_one_page_with_all_of_it()
    {
        using var pdf = UglyToad.PdfPig.PdfDocument.Open(ReportPdfGenerator.Generate(Card()));

        pdf.NumberOfPages.ShouldBe(1);
    }

    [Fact]
    public void A_card_missing_the_new_details_still_renders()
    {
        // Older cards, and cards whose class or term has since been removed.
        var data = Card();
        data.GradeName = null;
        data.DateOfBirth = null;
        data.SchoolOpensOn = null;
        data.SchoolClosesOn = null;
        data.PreviousPerformance = null;

        var text = TextOf(data);

        text.ShouldContain("Thandeka Masinga");
        text.ShouldContain("11C (Arts Stream)");
    }
}
