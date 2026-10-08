using psms.Assessment.Reports.Pdf;
using psms.Domain.Shared.Enums;
using Shouldly;
using System;
using System.Collections.Generic;
using Xunit;

namespace psms.Tests.Assessment;

/// <summary>
/// RC-21. The school stamp, §25(8)(b).
/// <para>
/// The Protocol names it among the essential components of a report card. It is
/// not the logo: the logo is the school's mark on a screen, the stamp is what
/// authenticates a document on paper, which is why it sits beside the
/// signatures rather than in the header.
/// </para>
/// </summary>
[Collection(PdfRenderCollection.Name)]
public class SchoolStamp_Tests
{
    /// <summary>A 1x1 PNG — enough to prove the image path renders.</summary>
    private static byte[] TinyPng() => Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

    private static ReportPdfData Card(byte[] stamp) => new ReportPdfData
    {
        SchoolName = "Riverside Secondary School",
        PrimaryColor = "#C8102E",
        SecondaryColor = "#003D73",
        StampBytes = stamp,
        StudentName = "Thandeka Masinga",
        AdmissionNumber = "STU-2026-023",
        ClassName = "11C (Arts Stream)",
        GradeName = "Grade 11",
        DateOfBirth = "14 Mar 2009",
        TermName = "Term 2",
        AcademicYearName = "2026 Academic Year",
        ReportType = "Term 2 Report",
        GeneratedDate = "08 Oct 2026",
        ReportCardNumber = "2026/GR11C/STU-2026-023/T2",
        TeacherSignedBy = "T. Petersen",
        TeacherSignedDate = "08 Oct 2026",
        PrincipalSignedBy = "M. Dlamini",
        PrincipalSignedDate = "08 Oct 2026",
        DaysInTerm = 60,
        DaysPresent = 58,
        OverallPercentage = 67.4m,
        OverallAchievementLevel = CapsAchievementLevel.Level6,
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

    [Fact]
    public void A_stamped_card_says_so_beside_the_signatures()
    {
        var text = PdfText.Extract(ReportPdfGenerator.Generate(Card(TinyPng())));

        text.ShouldContain("School Stamp");
        // It belongs with what it authenticates.
        text.ShouldContain("Class Teacher");
        text.ShouldContain("Principal");
    }

    [Fact]
    public void A_school_with_no_stamp_prints_the_card_it_always_did()
    {
        // No labelled gap where a stamp should be — most schools will not have
        // uploaded one, and an empty box reads as something missing.
        var text = PdfText.Extract(ReportPdfGenerator.Generate(Card(null)));

        text.ShouldNotContain("School Stamp");
        text.ShouldContain("Thandeka Masinga");
        text.ShouldContain("Class Teacher");
    }

    [Fact]
    public void An_empty_stamp_is_treated_as_no_stamp()
    {
        var text = PdfText.Extract(ReportPdfGenerator.Generate(Card(Array.Empty<byte>())));

        text.ShouldNotContain("School Stamp");
    }

    [Fact]
    public void A_stamped_card_still_fits_on_one_page()
    {
        using var pdf = UglyToad.PdfPig.PdfDocument.Open(ReportPdfGenerator.Generate(Card(TinyPng())));

        pdf.NumberOfPages.ShouldBe(1);
    }

    [Fact]
    public void A_stamp_that_will_not_render_does_not_cost_the_school_the_card()
    {
        // Whatever is in the bucket is not guaranteed to be a usable image.
        var data = Card(new byte[] { 0x00, 0x01, 0x02, 0x03 });

        Should.NotThrow(() => ReportPdfGenerator.Generate(data));
    }
}
