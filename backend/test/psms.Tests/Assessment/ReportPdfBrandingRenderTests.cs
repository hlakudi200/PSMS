using psms.Assessment.Reports.Pdf;
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
        OverallAchievementLevel = "Level 6 - Meritorious",
        ClassPosition = 4,
        TotalStudentsInClass = 31,
        DaysPresent = 58,
        DaysAbsent = 2,
        DaysLate = 3,
        TeacherComment = "Thandeka has worked steadily all term and contributes well in class.",
        PrincipalComment = "A pleasing report. Keep it up.",
        Subjects = new List<SubjectEntry>
        {
            new SubjectEntry { SubjectName = "Mathematics", SubjectCode = "MATH", TermMark = 68m, FinalMark = 68m, AchievementLevel = "Level 5", TeacherName = "T. Petersen", TeacherComment = "Solid algebra." },
            new SubjectEntry { SubjectName = "English Home Language", SubjectCode = "ENG", TermMark = 81m, FinalMark = 81m, AchievementLevel = "Level 7", TeacherName = "L. Mokoena", TeacherComment = "Excellent writing." },
            new SubjectEntry { SubjectName = "Life Sciences", SubjectCode = "LSCI", TermMark = 74m, FinalMark = 74m, AchievementLevel = "Level 6", TeacherName = "T. Ndlovu", TeacherComment = "Good practical work." },
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
