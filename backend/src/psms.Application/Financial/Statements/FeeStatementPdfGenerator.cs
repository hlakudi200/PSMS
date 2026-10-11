using System;
using System.Globalization;
using System.Linq;
using psms.Financial.Statements.Dto;
using psms.Shared.Pdf;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace psms.Financial.Statements;

/// <summary>
/// Prints a fee statement with QuestPDF, under the same letterhead as the
/// report card: logo, school name in the school's colour, double rule.
/// </summary>
public static class FeeStatementPdfGenerator
{
    private static readonly string HeaderBg = Colors.Grey.Lighten3;
    private static readonly string LightBorder = Colors.Grey.Lighten2;

    private const float LogoMaxHeight = 36f;
    private const float LogoMaxWidth = 120f;

    static FeeStatementPdfGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <param name="addressedTo">Who the statement is for — the parent's name, or null for a staff copy.</param>
    public static byte[] Generate(FeeStatementDto statement, SchoolLetterhead letterhead, string addressedTo)
    {
        var ink = PdfBranding.InkOnWhite(letterhead.PrimaryColor);
        var multipleChildren = statement.Children.Count > 1;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginVertical(30);
                page.MarginHorizontal(25);
                // Ligatures off: the font's "ti" ligature extracts as U+0000,
                // so copied text read "Sta ement". Same fix as the report card.
                page.DefaultTextStyle(x => x
                    .FontSize(9)
                    .DisableFontFeature(FontFeatures.StandardLigatures));

                page.Header().Element(c => ComposeHeader(c, statement, letterhead, ink));
                page.Content().Element(c => ComposeContent(c, statement, addressedTo, multipleChildren, ink));
                page.Footer().AlignCenter().Text(text =>
                {
                    text.DefaultTextStyle(x => x.FontSize(7.5f).FontColor(Colors.Grey.Darken1));
                    text.Span($"Generated {DateTime.UtcNow.ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture)} UTC  ·  Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposeHeader(
        IContainer container, FeeStatementDto statement, SchoolLetterhead letterhead, string ink)
    {
        container.Column(column =>
        {
            if (PdfBranding.IsRenderableImage(letterhead.LogoBytes))
            {
                column.Item().AlignCenter()
                    .MaxHeight(LogoMaxHeight).MaxWidth(LogoMaxWidth)
                    .Image(letterhead.LogoBytes).FitArea();
                column.Item().Height(3);
            }

            column.Item().AlignCenter()
                .Text((letterhead.SchoolName ?? "School").ToUpperInvariant())
                .Bold().FontSize(16).LetterSpacing(0.05f).FontColor(ink);

            column.Item().AlignCenter()
                .Text($"Fee Statement  ·  {statement.PeriodLabel}")
                .SemiBold().FontSize(10.5f);

            column.Item().Height(3);
            column.Item().LineHorizontal(2).LineColor(ink);
            column.Item().Height(2);
            column.Item().LineHorizontal(0.5f).LineColor(Colors.Black);
            column.Item().Height(6);
        });
    }

    private static void ComposeContent(
        IContainer container, FeeStatementDto statement, string addressedTo, bool multipleChildren, string ink)
    {
        container.Column(column =>
        {
            column.Spacing(8);

            column.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    if (!string.IsNullOrWhiteSpace(addressedTo))
                        c.Item().Text(t => { t.Span("Statement for: ").SemiBold(); t.Span(addressedTo); });
                    c.Item().Text(t =>
                    {
                        t.Span(multipleChildren ? "Learners: " : "Learner: ").SemiBold();
                        t.Span(string.Join(", ", statement.Children.Select(Describe)));
                    });
                });
                row.ConstantItem(170).Column(c =>
                {
                    c.Item().AlignRight().Text(t => { t.Span("Period: ").SemiBold(); t.Span($"{Day(statement.PeriodStart)} – {Day(statement.PeriodEnd)}"); });
                    c.Item().AlignRight().Text(t => { t.Span("Balances as at: ").SemiBold(); t.Span(Day(statement.StatementDate)); });
                });
            });

            column.Item().Element(c => ComposeSummary(c, statement, multipleChildren));
            column.Item().Element(c => ComposeAmountDue(c, statement, ink));
            column.Item().Element(c => ComposeLines(c, statement, multipleChildren));
        });
    }

    private static void ComposeSummary(IContainer container, FeeStatementDto statement, bool multipleChildren)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                for (var i = 0; i < 6; i++) columns.RelativeColumn(2);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCell).Text("Learner").SemiBold();
                foreach (var title in new[] { "Opening", "Charged", "Credits", "Paid", "Refunded", "Closing" })
                    header.Cell().Element(HeaderCell).AlignRight().Text(title).SemiBold();
            });

            foreach (var child in statement.Children)
            {
                table.Cell().Element(BodyCell).Text(child.StudentName ?? "-");
                MoneyCells(table, child.OpeningBalance, child.Charged, child.Credits, child.Paid, child.Refunded, child.ClosingBalance);
            }

            if (multipleChildren)
            {
                table.Cell().Element(BodyCell).Text("Family total").Bold();
                MoneyCells(table, statement.OpeningBalance, statement.TotalCharged, statement.TotalCredits,
                    statement.TotalPaid, statement.TotalRefunded, statement.ClosingBalance, bold: true);
            }
        });
    }

    private static void ComposeAmountDue(IContainer container, FeeStatementDto statement, string ink)
    {
        container.Border(1).BorderColor(ink).Padding(8).Row(row =>
        {
            row.RelativeItem().Column(c =>
            {
                c.Item().Text(statement.ClosingBalance < 0 ? "In credit" : "Balance owing").SemiBold();
                c.Item().Text(Money(Math.Abs(statement.ClosingBalance), statement.Currency)).Bold().FontSize(14).FontColor(ink);
            });
            row.RelativeItem().AlignRight().Column(c =>
            {
                c.Item().AlignRight().Text("Of which overdue").SemiBold();
                c.Item().AlignRight().Text(Money(statement.AmountOverdue, statement.Currency)).Bold().FontSize(14)
                    .FontColor(statement.AmountOverdue > 0 ? Colors.Red.Darken2 : Colors.Black);
            });
        });
    }

    private static void ComposeLines(IContainer container, FeeStatementDto statement, bool multipleChildren)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                // Wide enough that "11 Sep 2026" stays on one line.
                columns.ConstantColumn(66);
                if (multipleChildren) columns.RelativeColumn(2);
                columns.RelativeColumn(4);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCell).Text("Date").SemiBold();
                if (multipleChildren) header.Cell().Element(HeaderCell).Text("Learner").SemiBold();
                header.Cell().Element(HeaderCell).Text("Description").SemiBold();
                header.Cell().Element(HeaderCell).Text("Reference").SemiBold();
                header.Cell().Element(HeaderCell).AlignRight().Text("Debit").SemiBold();
                header.Cell().Element(HeaderCell).AlignRight().Text("Credit").SemiBold();
                header.Cell().Element(HeaderCell).AlignRight().Text("Balance").SemiBold();
            });

            table.Cell().Element(BodyCell).Text(Day(statement.PeriodStart));
            if (multipleChildren) table.Cell().Element(BodyCell).Text(string.Empty);
            table.Cell().Element(BodyCell).Text("Opening balance").Italic();
            table.Cell().Element(BodyCell).Text(string.Empty);
            table.Cell().Element(BodyCell).Text(string.Empty);
            table.Cell().Element(BodyCell).Text(string.Empty);
            table.Cell().Element(BodyCell).AlignRight().Text(Amount(statement.OpeningBalance));

            foreach (var line in statement.Lines)
            {
                table.Cell().Element(BodyCell).Text(Day(line.Date));
                if (multipleChildren) table.Cell().Element(BodyCell).Text(line.StudentName ?? "-");
                table.Cell().Element(BodyCell).Column(c =>
                {
                    c.Item().Text(line.Description);
                    foreach (var allocation in line.Allocations.Where(a => !string.IsNullOrWhiteSpace(a.FeeName)))
                        c.Item().Text($"→ {allocation.FeeName}: {Amount(allocation.Amount)}")
                            .FontSize(7.5f).FontColor(Colors.Grey.Darken2);
                });
                table.Cell().Element(BodyCell).Text(line.Reference ?? string.Empty);
                table.Cell().Element(BodyCell).AlignRight().Text(line.Debit > 0 ? Amount(line.Debit) : string.Empty);
                table.Cell().Element(BodyCell).AlignRight().Text(line.Credit > 0 ? Amount(line.Credit) : string.Empty);
                table.Cell().Element(BodyCell).AlignRight().Text(Amount(line.Balance));
            }

            if (statement.Lines.Count == 0)
            {
                table.Cell().ColumnSpan(multipleChildren ? 7u : 6u).Element(BodyCell)
                    .Text("No transactions in this period.").Italic().FontColor(Colors.Grey.Darken1);
            }
        });
    }

    private static void MoneyCells(TableDescriptor table, decimal opening, decimal charged, decimal credits,
        decimal paid, decimal refunded, decimal closing, bool bold = false)
    {
        foreach (var value in new[] { opening, charged, credits, paid, refunded, closing })
        {
            var text = table.Cell().Element(BodyCell).AlignRight().Text(Amount(value));
            if (bold) text.Bold();
        }
    }

    private static IContainer HeaderCell(IContainer container) =>
        container.Background(HeaderBg).BorderBottom(0.5f).BorderColor(Colors.Black).PaddingVertical(3).PaddingHorizontal(4);

    private static IContainer BodyCell(IContainer container) =>
        container.BorderBottom(0.5f).BorderColor(LightBorder).PaddingVertical(3).PaddingHorizontal(4);

    private static string Describe(FeeStatementChildDto child)
    {
        var details = string.Join(", ", new[] { child.GradeName, child.AdmissionNumber }
            .Where(x => !string.IsNullOrWhiteSpace(x)));
        return string.IsNullOrEmpty(details) ? child.StudentName : $"{child.StudentName} ({details})";
    }

    private static string Day(DateTime date) => date.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);

    /// <summary>
    /// Always a full stop and a comma separator, whatever the server's culture —
    /// the report card learnt that the hard way ("72,4%" on one host).
    /// </summary>
    private static string Amount(decimal value) => value.ToString("#,##0.00;-#,##0.00", CultureInfo.InvariantCulture);

    private static string Money(decimal value, string currency) =>
        (currency == "ZAR" ? "R " : $"{currency} ") + Amount(value);
}
