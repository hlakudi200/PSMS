using psms.Assessment.Reports.Pdf;
using psms.Domain.Shared.Enums;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace psms.Tests.Assessment;

/// <summary>
/// A report card is one sheet of paper.
/// <para>
/// Schools print these in class sets and hand them out at parents' evening; a
/// card that runs to two pages doubles the paper, and the second sheet gets
/// separated from the first. The layout used to spill whenever the content grew
/// — the CAPS legend alone was pushing every card onto a page two that carried
/// nothing else — so the fit is pinned here across the range of cards the system
/// actually produces rather than left to be noticed in a print room.
/// </para>
/// <para>
/// These are deliberately pessimistic: the longest realistic subject lists, the
/// longest names, every optional block present at once.
/// </para>
/// </summary>
public class ReportPdfSinglePageTests
{
    /// <summary>
    /// The nine subjects of a Senior Phase card, with the longest subject and
    /// teacher names in the seeded curriculum, since those are what wrap a row
    /// onto a second line.
    /// </summary>
    private static List<SubjectEntry> Subjects(int count, bool externalExam = false)
    {
        var names = new[]
        {
            ("IsiZulu First Additional Language", "ZUL02", "Nomsa Patricia Dlamini"),
            ("English Home Language", "EHL01", "Lerato Mokoena"),
            ("Mathematical Literacy", "MLT01", "Thandiwe Grace Ndlovu"),
            ("Life Orientation", "LOR09", "Puleng Anna Van der Merwe"),
            ("Physical Sciences", "PHY04", "Kagiso James Moloi"),
            ("Engineering Graphics and Design", "EGD07", "Sipho Christopher Nkosi"),
            ("Geography", "GEO07", "Thabo Petersen"),
            ("Accounting", "ACC02", "Refilwe Mathebula"),
            ("Life Sciences", "LIF03", "Bongani Khumalo"),
            ("History", "HIS05", "Zanele Mahlangu"),
            ("Business Studies", "BUS06", "Ayanda Sithole"),
            ("Dramatic Arts", "DRA08", "Mpho Radebe"),
        };

        return Enumerable.Range(0, count).Select(i =>
        {
            var (name, code, teacher) = names[i % names.Length];
            return new SubjectEntry
            {
                SubjectName = name,
                SubjectCode = code,
                TermMark = 63m + i,
                FinalMark = 63m + i,
                AchievementLevel = CapsAchievementLevel.Level5,
                TeacherName = teacher,
                TeacherComment = "-",
                ClassAverage = 61.2m,
                SubjectPosition = 5,
                AwaitsExternalExamination = externalExam,
            };
        }).ToList();
    }

    private static ReportPdfData Card(int subjectCount) => new ReportPdfData
    {
        SchoolName = "Riverside Secondary School",
        PrimaryColor = "#C8102E",
        SecondaryColor = "#003D73",
        StudentName = "Dimakatso Mahlangu",
        AdmissionNumber = "STU-2026-187",
        ClassName = "Grade 11A (Science Stream)",
        TermName = "Term 1",
        AcademicYearName = "2026 Academic Year",
        ReportType = "Term 1 Report",
        GeneratedDate = "01 Oct 2026",
        // The real format: long enough that it wraps in the info grid.
        ReportCardNumber = "2026ACADEMICYEAR/GRADE11ASCIENCESTREAM/STU-2026-187/T1/9841B7BC",
        DaysInTerm = 60,
        DaysPresent = 58,
        DaysAbsent = 2,
        DaysLate = 3,
        OverallPercentage = 71.0m,
        OverallAchievementLevel = CapsAchievementLevel.Level6,
        ClassPosition = 5,
        TotalStudentsInClass = 8,
        Subjects = Subjects(subjectCount),
    };

    /// <summary>Everything optional, all at once — the heaviest card possible.</summary>
    private static ReportPdfData FullCard(int subjectCount)
    {
        var data = Card(subjectCount);
        data.ConductRating = "Very good";
        data.DiligenceRating = "Good";
        data.BehaviourComments =
            "Courteous and cooperative; sometimes slow to settle after break, but responds well to a reminder.";
        data.PromotionDecision = "Promoted";
        data.PromotionReason =
            "Meets the requirements of the National Policy pertaining to the Programme and Promotion Requirements.";
        data.PromotedToGradeName = "Grade 12";
        data.TeacherComment =
            "Dimakatso has worked steadily all term and contributes well in class discussion.";
        data.PrincipalComment = "A pleasing report. Keep it up.";
        data.ParentComment = "Thank you, we have discussed this at home.";
        data.TeacherSignedBy = "T. Petersen";
        data.TeacherSignedDate = "05 Oct 2026";
        data.PrincipalSignedBy = "M. Dlamini";
        data.PrincipalSignedDate = "06 Oct 2026";
        data.VerificationUrl = "https://psms-omega.vercel.app/verify/8Kx2mQvL9pR4tNwZ";
        return data;
    }

    private static int PageCount(ReportPdfData data)
    {
        using var pdf = UglyToad.PdfPig.PdfDocument.Open(ReportPdfGenerator.Generate(data));
        return pdf.NumberOfPages;
    }

    [Theory]
    [InlineData(4)]   // Foundation Phase
    [InlineData(7)]   // Intermediate Phase and most FET streams
    [InlineData(9)]   // Senior Phase, the NPPPPR §21(1) maximum
    public void A_plain_card_fits_on_one_page(int subjectCount)
    {
        PageCount(Card(subjectCount)).ShouldBe(1);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(7)]
    [InlineData(9)]
    public void A_card_carrying_every_optional_block_fits_on_one_page(int subjectCount)
    {
        // Conduct, promotion, three comments, both signatures and the
        // verification strip — a year-end Senior Phase card has all of it.
        PageCount(FullCard(subjectCount)).ShouldBe(1);
    }

    [Fact]
    public void A_grade_12_card_with_the_external_examination_note_fits_on_one_page()
    {
        var data = FullCard(7);
        data.Subjects = Subjects(7, externalExam: true);
        data.ReportType = "Year-End Report";

        PageCount(data).ShouldBe(1);
    }

    /// <summary>
    /// Past the point a card can hold, it must still render rather than throw or
    /// clip. Twelve subjects is beyond any CAPS phase; it is allowed its second
    /// page, and the test says so plainly so a future reader does not mistake
    /// this for the fit the cases above are pinning.
    /// </summary>
    [Fact]
    public void An_impossible_subject_list_still_renders()
    {
        PageCount(FullCard(12)).ShouldBeGreaterThanOrEqualTo(1);
    }
}
