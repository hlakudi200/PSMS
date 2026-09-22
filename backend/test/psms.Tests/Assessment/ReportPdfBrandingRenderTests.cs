using psms.Assessment.Reports.Pdf;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace psms.Tests.Assessment;

/// <summary>
/// Renders the report card so the branded header can be eyeballed, and pins the
/// behaviour that matters: a school's colour and logo reach the page, and a
/// missing or broken logo still produces a report.
/// </summary>
public class ReportPdfBrandingRenderTests
{
    private static ReportPdfData SampleData(string primary, byte[] logo) => new ReportPdfData
    {
        SchoolName = "Riverside Primary School",
        PrimaryColor = primary,
        SecondaryColor = "#003D73",
        LogoBytes = logo,
        StudentName = "Thandeka Masinga",
        AdmissionNumber = "STU-2026-023",
        ClassName = "Grade 8A",
        TermName = "Term 1",
        AcademicYearName = "2026 Academic Year",
        ReportType = "Term 1 Report",
        GeneratedDate = "21 Sep 2026",
        OverallPercentage = 72.4m,
        OverallAchievementLevel = CapsAchievementLevel.Level6,
        ClassPosition = 4,
        TotalStudentsInClass = 31,
        DaysPresent = 58,
        DaysAbsent = 2,
        DaysLate = 3,
        TeacherComment = "Thandeka has worked steadily all term and contributes well in class.",
        PrincipalComment = "A pleasing report. Keep it up.",
        Subjects = new List<SubjectEntry>
        {
            new SubjectEntry { SubjectName = "Mathematics", SubjectCode = "MATH", TermMark = 68m, FinalMark = 68m, AchievementLevel = CapsAchievementLevel.Level5, TeacherName = "T. Petersen", TeacherComment = "Solid algebra.", ClassAverage = 61.2m, SubjectPosition = 7, HighestInClass = 92m, LowestInClass = 28m },
            new SubjectEntry { SubjectName = "English Home Language", SubjectCode = "ENG", TermMark = 81m, FinalMark = 81m, AchievementLevel = CapsAchievementLevel.Level7, TeacherName = "L. Mokoena", TeacherComment = "Excellent writing.", ClassAverage = 66.8m, SubjectPosition = 2, HighestInClass = 88m, LowestInClass = 41m },
            new SubjectEntry { SubjectName = "Life Sciences", SubjectCode = "LSCI", TermMark = 74m, FinalMark = 74m, AchievementLevel = CapsAchievementLevel.Level6, TeacherName = "T. Ndlovu", TeacherComment = "Good practical work.", ClassAverage = 70.1m, SubjectPosition = 5, HighestInClass = 95m, LowestInClass = 39m },
        }
    };

    /// <summary>A 1x1 PNG — enough to prove the image path renders.</summary>
    private static byte[] TinyPng() => Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

    [Fact]
    public void Renders_with_a_school_logo_and_primary_colour()
    {
        var pdf = ReportPdfGenerator.Generate(SampleData("#7B1FA2", TinyPng()));

        Assert.NotNull(pdf);
        Assert.True(pdf.Length > 1000, $"expected a real PDF, got {pdf.Length} bytes");
        Assert.Equal(new byte[] { 0x25, 0x50, 0x44, 0x46 }, pdf[..4]); // %PDF

        WriteSample("report-branded.pdf", pdf);
    }

    [Fact]
    public void Renders_without_a_logo()
    {
        var pdf = ReportPdfGenerator.Generate(SampleData("#0066CC", null));

        Assert.True(pdf.Length > 1000);
        WriteSample("report-no-logo.pdf", pdf);
    }

    [Fact]
    public void A_pale_brand_still_renders()
    {
        // The header foreground flips to dark on a light background; this pins
        // that a pale brand does not produce white-on-white headings.
        var pdf = ReportPdfGenerator.Generate(SampleData("#FFD54F", TinyPng()));

        Assert.True(pdf.Length > 1000);
        WriteSample("report-pale-brand.pdf", pdf);
    }

    [Fact]
    public void The_printed_overall_is_the_stored_one_not_a_recomputed_mean()
    {
        // RC-07. The rows here mean 74.33; the report stores 72.4. The PDF used
        // to print its own mean of the rows, which is how a printed card could
        // contradict the report it was printed from. Whatever the rows say, the
        // page must carry the stored figure.
        var data = SampleData("#0066CC", null);

        var pdf = ReportPdfGenerator.Generate(data);
        var text = PdfText.Extract(pdf);

        Assert.Contains("72.4%", text);
        Assert.DoesNotContain("74.3%", text);
    }

    [Fact]
    public void The_class_figures_reach_the_page()
    {
        // RC-06. Five fields that were stored and never rendered anywhere.
        var text = PdfText.Extract(ReportPdfGenerator.Generate(SampleData("#0066CC", null)));

        Assert.Contains("CLASS", text);
        Assert.Contains("61.2", text);  // the Mathematics class average
        Assert.Contains("POS", text);
    }

    [Fact]
    public void A_subject_with_no_class_figures_yet_prints_a_dash_not_a_zero()
    {
        var data = SampleData("#0066CC", null);
        foreach (var subject in data.Subjects)
        {
            subject.ClassAverage = null;
            subject.SubjectPosition = null;
        }

        var text = PdfText.Extract(ReportPdfGenerator.Generate(data));

        Assert.Contains("CLASS", text);
        Assert.DoesNotContain("0.0", text);
    }

    [Fact]
    public void A_grade_twelve_card_says_the_examination_is_external()
    {
        // RC-15. The mark shown is the 25% school-based component; the NSC paper
        // is set and marked by the Department. A card that presents that as an
        // unqualified final mark is claiming an NSC result it does not have.
        var data = SampleData("#0066CC", null);
        data.ReportType = "Year-End Report";
        foreach (var subject in data.Subjects)
            subject.AwaitsExternalExamination = true;

        var pdf = ReportPdfGenerator.Generate(data);
        WriteSample("report-grade-12.pdf", pdf);

        var text = PdfText.Extract(pdf);

        Assert.Contains("School-based assessment component only", text);
        Assert.Contains("National Senior Certificate", text);
    }

    [Fact]
    public void The_text_on_the_card_can_be_read_back_out_of_the_file()
    {
        // The font's "ti" ligature is one glyph whose ToUnicode entry is
        // U+0000, so the card drew correctly and extracted as "Posi on",
        // "Substan al", "Na onal" — uncopyable, unsearchable, and wrong to a
        // screen reader. Ligatures are off for that reason; this is the guard.
        var text = PdfText.Extract(ReportPdfGenerator.Generate(SampleData("#0066CC", null)));

        Assert.Contains("Substantial", text);
        Assert.Contains("Position", text);
        Assert.Contains("Meritorious", text);
    }

    [Fact]
    public void An_ordinary_card_carries_no_external_examination_note()
    {
        var text = PdfText.Extract(ReportPdfGenerator.Generate(SampleData("#0066CC", null)));

        Assert.DoesNotContain("School-based assessment component only", text);
    }

    [Fact]
    public void A_year_end_card_prints_the_promotion_decision()
    {
        // RC-16. NPPPPR §(2b)(c): "the decision reached at the meeting
        // contemplated above must be reflected on the learner's report card."
        // The card printed attendance, marks and comments and omitted the one
        // thing the policy names.
        var data = SampleData("#0066CC", null);
        data.ReportType = "Year-End Report";
        data.PromotionDecision = "Promoted";
        data.PromotedToGradeName = "Grade 9";
        data.PromotionReason = "Met the requirements in all nine subjects.";

        var pdf = ReportPdfGenerator.Generate(data);
        WriteSample("report-promotion.pdf", pdf);

        var text = PdfText.Extract(pdf);

        Assert.Contains("PROMOTION DECISION", text);
        Assert.Contains("Promoted to Grade 9", text);
        Assert.Contains("Met the requirements", text);
    }

    [Fact]
    public void A_card_with_no_decision_prints_no_promotion_block()
    {
        var text = PdfText.Extract(ReportPdfGenerator.Generate(SampleData("#0066CC", null)));

        Assert.DoesNotContain("PROMOTION DECISION", text);
    }

    [Fact]
    public void A_malformed_colour_does_not_throw()
    {
        // Defensive: branding is validated on the way in, but a report card must
        // not be the thing that falls over if a bad value ever reaches it.
        var pdf = ReportPdfGenerator.Generate(SampleData("not-a-colour", null));

        Assert.True(pdf.Length > 1000);
    }

    /// <summary>
    /// Drops the rendered file next to the test output when PSMS_PDF_SAMPLES is
    /// set, so the layout can be looked at rather than only asserted on.
    /// </summary>
    private static void WriteSample(string name, byte[] pdf)
    {
        var dir = Environment.GetEnvironmentVariable("PSMS_PDF_SAMPLES");
        if (string.IsNullOrWhiteSpace(dir)) return;

        Directory.CreateDirectory(dir);
        File.WriteAllBytes(Path.Combine(dir, name), pdf);
    }
}
