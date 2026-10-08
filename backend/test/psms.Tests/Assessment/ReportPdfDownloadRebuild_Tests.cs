using NSubstitute;
using NSubstitute.ClearExtensions;
using psms.Assessment.Reports;
using psms.Domain.Academic.Entities;
using psms.Domain.Assessment.Entities;
using psms.Domain.Shared.Enums;
using psms.Domain.Shared.Storage;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

namespace psms.Tests.Assessment;

/// <summary>
/// Downloading a card whose contents have moved on rebuilds the file first.
/// <para>
/// The first attempt at this queued a background job and threw a "try again in
/// a moment" message to say so. ABP writes the job row through the same unit of
/// work as the request, so the throw rolled the row back: nothing was ever
/// rebuilt, and the download refused for good. Retrying — which the message
/// asked for — queued another row and threw that one away too.
/// </para>
/// <para>
/// Nothing in a domain test could have caught that; it only shows up when a
/// real unit of work is in play. So these run against the application service.
/// </para>
/// </summary>
[Collection(PdfRenderCollection.Name)]
public class ReportPdfDownloadRebuild_Tests : psmsTestBase
{
    private const string Key = "reports/1/year/term/A001_card.pdf";
    private const string SignedUrl = "https://storage.test/signed/card.pdf?token=abc";

    private readonly IReportAppService _reports;
    private readonly IFileStorageService _storage;

    public ReportPdfDownloadRebuild_Tests()
    {
        LoginAsDefaultTenantAdmin();

        _reports = Resolve<IReportAppService>();

        _storage = Resolve<IFileStorageService>();
        _storage.ClearSubstitute();
        _storage.DefaultBucketName.Returns("psms-files");
        _storage.UploadAsync(Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<string>())
            .Returns(_ => Task.FromResult(Key));
        _storage.CreateSignedDownloadUrlAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(_ => Task.FromResult(SignedUrl));
    }

    /// <summary>
    /// A card with a stored PDF, where <paramref name="stale"/> says whether the
    /// card has changed since that file was made.
    /// <para>
    /// The learner, class, grade and year are seeded alongside it because the
    /// PDF is really rendered here — this is the path under test, not a stub.
    /// </para>
    /// </summary>
    private Guid SeedCard(bool stale)
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

            context.Grades.Add(new Grade(gradeId, 1,
                SouthAfricanGradeLevel.Grade8, "Grade 8", SouthAfricanSchoolPhase.Senior));

            context.Classes.Add(new Class(classId, 1, "8A", gradeId, yearId, 30));

            context.Students.Add(new Student(studentId, 1, "Ayanda", "Nkosi",
                new DateTime(2012, 1, 1), Gender.Female, "A001", DateTime.Today, gradeId, classId));

            var report = new Report(id, 1, studentId, classId, yearId, ReportType.Term1);

            report.PdfObjectKey = "reports/1/year/term/A001_old.pdf";
            report.PdfGeneratedDate = DateTime.UtcNow.AddHours(-2);
            report.CreationTime = DateTime.UtcNow.AddHours(-3);
            report.LastModificationTime = stale
                ? DateTime.UtcNow                      // signed after the file was made
                : DateTime.UtcNow.AddHours(-2);

            context.Reports.Add(report);
            context.SaveChanges();
        });

        return id;
    }

    [Fact]
    public async Task A_stale_card_is_rebuilt_and_the_link_still_comes_back()
    {
        var id = SeedCard(stale: true);

        var url = await _reports.GetReportPdfUrlAsync(id);

        url.ShouldBe(SignedUrl);
        await _storage.Received(1).UploadAsync(
            Arg.Any<string>(), Arg.Any<byte[]>(), "application/pdf");
    }

    [Fact]
    public async Task The_rebuilt_file_is_the_one_the_link_points_at()
    {
        // The rebuild has to be recorded on the card, or the next download
        // signs the old key and hands back the stale file anyway.
        var id = SeedCard(stale: true);

        await _reports.GetReportPdfUrlAsync(id);

        var stored = UsingDbContext(1, context => context.Reports.Find(id));
        stored.PdfObjectKey.ShouldBe(Key);
    }

    [Fact]
    public async Task A_second_download_does_not_rebuild_again()
    {
        // The rebuild writes PdfGeneratedDate, so the card stops being stale.
        // While it did not, every download rebuilt — or, as shipped, every
        // download refused.
        var id = SeedCard(stale: true);

        await _reports.GetReportPdfUrlAsync(id);
        await _reports.GetReportPdfUrlAsync(id);

        await _storage.Received(1).UploadAsync(
            Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<string>());
    }

    [Fact]
    public async Task A_current_card_is_handed_over_without_rebuilding()
    {
        var id = SeedCard(stale: false);

        var url = await _reports.GetReportPdfUrlAsync(id);

        url.ShouldBe(SignedUrl);
        await _storage.DidNotReceive().UploadAsync(
            Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<string>());
    }

}
