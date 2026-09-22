using psms.Domain.Assessment.Entities;
using psms.Domain.Shared.Enums;
using System;
using Xunit;

namespace psms.Tests.Assessment;

/// <summary>
/// RC-04. A report's PDF is private and reached through a short-lived signed
/// URL, so the only durable thing is the storage key. These pin how that key is
/// found — including for reports generated before the change, which stored a
/// permanent public URL instead.
/// </summary>
public class ReportPdfObjectKey_Tests
{
    private const string Bucket = "psms-files";

    private static Report NewReport() => new Report(
        Guid.NewGuid(), 1, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), ReportType.Term1);

    [Fact]
    public void A_report_with_no_pdf_has_none()
    {
        var report = NewReport();

        Assert.False(report.HasPdf());
        Assert.Null(report.ResolvePdfObjectKey(Bucket));
    }

    [Fact]
    public void A_generated_pdf_resolves_to_its_key()
    {
        var report = NewReport();
        report.SetPdfObjectKey("reports/1/year/term/STU-001_abc.pdf");

        Assert.True(report.HasPdf());
        Assert.Equal("reports/1/year/term/STU-001_abc.pdf", report.ResolvePdfObjectKey(Bucket));
    }

    [Fact]
    public void A_legacy_public_url_still_resolves()
    {
        // Written before RC-04: the column held the whole public link.
        var report = NewReport();
        report.SetPdfUrl($"https://xyz.supabase.co/storage/v1/object/public/{Bucket}/reports/1/y/t/STU-002_def.pdf");

        Assert.True(report.HasPdf());
        Assert.Equal("reports/1/y/t/STU-002_def.pdf", report.ResolvePdfObjectKey(Bucket));
    }

    [Fact]
    public void The_key_wins_over_a_legacy_url()
    {
        var report = NewReport();
        report.SetPdfUrl($"https://xyz.supabase.co/storage/v1/object/public/{Bucket}/old.pdf");
        report.SetPdfObjectKey("new.pdf");

        Assert.Equal("new.pdf", report.ResolvePdfObjectKey(Bucket));
    }

    [Fact]
    public void A_url_from_another_bucket_does_not_resolve()
    {
        // Better to fail the download than to sign a key that is not ours.
        var report = NewReport();
        report.SetPdfUrl("https://xyz.supabase.co/storage/v1/object/public/materials/some-file.pdf");

        Assert.Null(report.ResolvePdfObjectKey(Bucket));
    }
}
