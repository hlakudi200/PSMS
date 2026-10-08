using psms.Domain.Assessment.Entities;
using psms.Domain.Shared.Enums;
using Shouldly;
using System;
using Xunit;

namespace psms.Tests.Assessment;

/// <summary>
/// The stored PDF has to be the card it claims to be.
/// <para>
/// The file was produced once and kept, and nothing rebuilt it when the card
/// changed. Download a report card, then sign it, and the stored file was still
/// the unsigned one — and publishing only built a PDF where <i>none</i> existed,
/// so that unsigned file is what reached the family. A card issued with blank
/// signature lines is exactly what RE-003 exists to prevent.
/// </para>
/// <para>
/// The same held for anything else printed on the card: a comment, a conduct
/// rating, a promotion decision, an attendance figure recorded after the file
/// was made.
/// </para>
/// </summary>
public class StaleReportPdf_Tests
{
    private static Report Card()
    {
        var report = new Report(Guid.NewGuid(), 1, Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), ReportType.Term1);
        report.CreationTime = new DateTime(2026, 10, 1, 8, 0, 0, DateTimeKind.Utc);
        return report;
    }

    [Fact]
    public void A_card_with_no_pdf_is_not_stale()
    {
        // There is nothing to be out of date. "Not generated" is its own answer
        // and the caller already has a message for it.
        Card().PdfIsStale().ShouldBeFalse();
    }

    [Fact]
    public void A_pdf_made_after_the_last_change_is_current()
    {
        var report = Card();
        report.LastModificationTime = new DateTime(2026, 10, 1, 9, 0, 0, DateTimeKind.Utc);
        report.SetPdfObjectKey("reports/abc.pdf");
        report.PdfGeneratedDate = new DateTime(2026, 10, 1, 9, 5, 0, DateTimeKind.Utc);

        report.PdfIsStale().ShouldBeFalse();
    }

    [Fact]
    public void A_card_signed_after_its_pdf_was_made_is_stale()
    {
        // The case that started this: download, then sign, then download again
        // and get the unsigned file back.
        var report = Card();
        report.SetPdfObjectKey("reports/abc.pdf");
        report.PdfGeneratedDate = new DateTime(2026, 10, 1, 9, 0, 0, DateTimeKind.Utc);

        report.LastModificationTime = new DateTime(2026, 10, 1, 10, 0, 0, DateTimeKind.Utc);

        report.PdfIsStale().ShouldBeTrue();
    }

    [Fact]
    public void Writing_the_pdf_does_not_make_the_card_call_it_stale()
    {
        // Storing the file is itself a change to the report, so the save that
        // records it also moves LastModificationTime. Without a little slack
        // every card would declare its own freshly built PDF out of date.
        var report = Card();
        report.SetPdfObjectKey("reports/abc.pdf");

        var now = new DateTime(2026, 10, 1, 9, 0, 0, DateTimeKind.Utc);
        report.PdfGeneratedDate = now;
        report.LastModificationTime = now.AddMilliseconds(400);

        report.PdfIsStale().ShouldBeFalse();
    }

    [Fact]
    public void A_pdf_from_before_this_was_tracked_counts_as_stale()
    {
        // Every card produced before the generation date existed. Rebuilding
        // once is cheap; serving an unsigned card is not.
        var report = Card();
        report.SetPdfObjectKey("reports/abc.pdf");
        report.PdfGeneratedDate = null;
        report.LastModificationTime = new DateTime(2026, 10, 1, 9, 0, 0, DateTimeKind.Utc);

        report.PdfIsStale().ShouldBeTrue();
    }

    [Fact]
    public void A_card_never_modified_falls_back_to_when_it_was_created()
    {
        var report = Card();
        report.SetPdfObjectKey("reports/abc.pdf");
        report.PdfGeneratedDate = report.CreationTime.AddMinutes(5);
        report.LastModificationTime = null;

        report.PdfIsStale().ShouldBeFalse();
    }

    [Fact]
    public void Storing_a_pdf_records_when_it_was_made()
    {
        var byKey = Card();
        byKey.SetPdfObjectKey("reports/abc.pdf");
        byKey.PdfGeneratedDate.ShouldNotBeNull();

        var byUrl = Card();
        byUrl.SetPdfUrl("https://example.test/abc.pdf");
        byUrl.PdfGeneratedDate.ShouldNotBeNull();
    }
}
