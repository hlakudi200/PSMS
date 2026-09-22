using System;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace psms.Assessment.Reports.Pdf;

/// <summary>
/// Generates SA school report card PDFs using QuestPDF.
/// Layout matches the frontend print view: school header, student info grid,
/// bordered subject table with overall footer, attendance cells, comment boxes,
/// signature lines, and CAPS legend.
/// </summary>
public static class ReportPdfGenerator
{
    // Colours. The neutrals stay fixed — a report card has to stay legible in
    // black and white — while the school's primary colour carries the identity:
    // the logo rule, the school name and the table headers.
    private static readonly string HeaderBg = Colors.Grey.Lighten3;
    private static readonly string BorderCol = Colors.Black;
    private static readonly string LightBorder = Colors.Grey.Lighten2;

    /// <summary>
    /// A CAPS level as it is printed in the marks table: the numeral alone.
    /// The words for it are in the legend at the foot of the card, which is
    /// what the legend is for — spelling "Outstanding" into a column this
    /// narrow wrapped it over four lines.
    /// </summary>
    private static string Level(psms.Domain.Shared.Enums.CapsAchievementLevel? level) =>
        level.HasValue ? ((int)level.Value).ToString() : "-";

    /// <summary>
    /// A mark as it is printed: one decimal, rounded half away from zero, and
    /// always with a full stop.
    /// <para>
    /// Interpolating a decimal directly took the <i>server's</i> culture, so
    /// the same report printed "72.4%" on one host and "72,4%" on another, and
    /// never matched the browser print view, which is always a full stop.
    /// </para>
    /// </summary>
    private static string Mark(decimal? value) =>
        value.HasValue
            ? value.Value.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)
            : "-";

    /// <summary>
    /// RC-15. Marks a final mark that is the School-Based Assessment component
    /// only, because the examination is external.
    /// </summary>
    private const string ExternalExamMarker = " †";

    /// <summary>
    /// What that marker means, printed under the table. NPPPPR §31(1): in Grade
    /// 12 the school-based assessment is 25% of the total mark and the external
    /// assessment 75%, and the external paper is set and marked by the
    /// Department of Basic Education, not by the school.
    /// </summary>
    private const string ExternalExamNote =
        "† School-based assessment component only. The National Senior Certificate "
        + "examination is set and marked externally by the Department of Basic Education "
        + "and is not included in this mark. The final result is issued on the "
        + "Department's statement of results.";

    /// <summary>Largest logo we will place in the header, in points.</summary>
    private const float LogoMaxHeight = 52f;
    private const float LogoMaxWidth = 150f;

    /// <summary>
    /// The colour if it is one QuestPDF will accept, otherwise the stock PSMS
    /// primary. Branding is validated on the way in, but a report card must not
    /// be the thing that falls over if a bad value ever reaches it.
    /// </summary>
    private static string SafeColor(string hex)
    {
        var value = (hex ?? string.Empty).Trim();
        if (!value.StartsWith("#")) value = "#" + value;

        return System.Text.RegularExpressions.Regex.IsMatch(value, "^#[0-9A-Fa-f]{6}$")
            ? value.ToUpperInvariant()
            : psms.Domain.Tenancy.BrandingDefaults.PrimaryColor;
    }

    /// <summary>
    /// The brand colour, darkened until it is legible as ink on white paper.
    /// A pale brand — gold, mint, sky — set straight as text is almost invisible
    /// on a printed page, so it is shaded down while staying recognisably the
    /// school's colour. A colour that is already dark enough is returned as-is.
    /// </summary>
    private static string InkOnWhite(string hex)
    {
        var value = SafeColor(hex).Substring(1);
        var r = Convert.ToInt32(value.Substring(0, 2), 16);
        var g = Convert.ToInt32(value.Substring(2, 2), 16);
        var b = Convert.ToInt32(value.Substring(4, 2), 16);

        // Shade toward black until the colour carries enough weight against
        // white. 0.35 keeps strong mid-tones (a royal blue, a deep purple)
        // untouched while pulling pastels down to something readable.
        for (var i = 0; i < 8 && Luminance(r, g, b) > 0.35f; i++)
        {
            r = (int)(r * 0.8f);
            g = (int)(g * 0.8f);
            b = (int)(b * 0.8f);
        }

        return $"#{r:X2}{g:X2}{b:X2}";
    }

    private static float Luminance(int r, int g, int b)
    {
        float Channel(int c)
        {
            var srgb = c / 255f;
            return srgb <= 0.03928f ? srgb / 12.92f : (float)Math.Pow((srgb + 0.055f) / 1.055f, 2.4);
        }

        return 0.2126f * Channel(r) + 0.7152f * Channel(g) + 0.0722f * Channel(b);
    }

    /// <summary>
    /// Black or white, whichever reads better on the given background. Mirrors
    /// the frontend's getReadableForeground so a branded header looks the same
    /// on screen and on paper; 0.179 is the crossover where the two give equal
    /// contrast.
    /// </summary>
    private static string ReadableOn(string hex)
    {
        var value = (hex ?? string.Empty).Replace("#", string.Empty);
        if (value.Length < 6 || !System.Text.RegularExpressions.Regex.IsMatch(value, "^[0-9A-Fa-f]{6}$"))
            return Colors.White;

        float Channel(int offset)
        {
            var srgb = Convert.ToInt32(value.Substring(offset, 2), 16) / 255f;
            return srgb <= 0.03928f
                ? srgb / 12.92f
                : (float)Math.Pow((srgb + 0.055f) / 1.055f, 2.4);
        }

        var luminance = 0.2126f * Channel(0) + 0.7152f * Channel(2) + 0.0722f * Channel(4);
        return luminance > 0.179f ? "#1F1F1F" : Colors.White;
    }

    static ReportPdfGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public static byte[] Generate(ReportPdfData data)
    {
        // Gate the brand colour once here so every use below is safe.
        data.PrimaryColor = SafeColor(data.PrimaryColor);
        data.SecondaryColor = SafeColor(data.SecondaryColor);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginVertical(30);
                page.MarginHorizontal(25);
                // Ligatures off, for every text style on the page. The font's
                // "ti" ligature is one glyph that the embedded ToUnicode map
                // points at U+0000, so the text drew correctly and came back out
                // of the file as "Posi on", "Substan al", "Na onal". A report
                // card is a document people copy from, search, and read with a
                // screen reader; verified against two independent extractors.
                page.DefaultTextStyle(x => x
                    .FontSize(10)
                    .DisableFontFeature(FontFeatures.StandardLigatures));

                page.Header().Element(c => ComposeHeader(c, data));
                page.Content().Element(c => ComposeContent(c, data));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    // ─── HEADER: School name, report type, term/year ───
    private static void ComposeHeader(IContainer container, ReportPdfData data)
    {
        container.Column(column =>
        {
            // The school's logo, when it has one. Constrained so a tall or wide
            // upload cannot push the rest of the card off the page.
            if (data.LogoBytes != null && data.LogoBytes.Length > 0)
            {
                column.Item().AlignCenter()
                    .MaxHeight(LogoMaxHeight).MaxWidth(LogoMaxWidth)
                    .Image(data.LogoBytes).FitArea();
                column.Item().Height(6);
            }

            // School name — large, uppercase, centred, in the school's colour
            column.Item().AlignCenter()
                .Text(data.SchoolName?.ToUpperInvariant() ?? "SCHOOL REPORT CARD")
                .Bold().FontSize(18).LetterSpacing(0.05f).FontColor(InkOnWhite(data.PrimaryColor));

            // Report type
            column.Item().AlignCenter()
                .Text(data.ReportType ?? "Student Report Card")
                .SemiBold().FontSize(13);

            // Term — Year
            if (!string.IsNullOrWhiteSpace(data.TermName) || !string.IsNullOrWhiteSpace(data.AcademicYearName))
            {
                var termYear = string.Join(" — ",
                    new[] { data.TermName, data.AcademicYearName }.Where(s => !string.IsNullOrWhiteSpace(s)));
                column.Item().AlignCenter().Text(termYear).FontSize(10);
            }

            column.Item().Height(4);

            // Double rule — the heavy line takes the school's colour, the hairline
            // stays black so the card still reads as a document in mono.
            column.Item().LineHorizontal(2).LineColor(InkOnWhite(data.PrimaryColor));
            column.Item().Height(2);
            column.Item().LineHorizontal(0.5f).LineColor(BorderCol);

            column.Item().Height(8);
        });
    }

    // ─── CONTENT ───
    private static void ComposeContent(IContainer container, ReportPdfData data)
    {
        container.Column(column =>
        {
            // ── Student Info Grid (2 columns) ──
            column.Item().Element(c => ComposeStudentInfo(c, data));
            column.Item().Height(10);

            // ── Subject Table ──
            column.Item().Element(c => ComposeSubjectTable(c, data));

            // ── RC-15: what the dagger means, when anything carries one ──
            if (data.Subjects.Any(s => s.AwaitsExternalExamination))
            {
                column.Item().Height(4);
                // Deliberately not italic: the italic face's "ti" ligature
                // carries no usable mapping, so copying the note out of the PDF
                // — or reading it with a screen reader — turns "National" into
                // "Na onal".
                column.Item().Text(ExternalExamNote).FontSize(7.5f).FontColor(Colors.Grey.Darken2);
            }

            column.Item().Height(10);

            // ── Attendance ──
            column.Item().Element(c => ComposeAttendance(c, data));
            column.Item().Height(10);

            // ── RC-16: the promotion decision ──
            // NPPPPR §(2b)(c): "the decision reached at the meeting contemplated
            // above must be reflected on the learner's report card." Printing
            // attendance, marks and comments while omitting the one thing the
            // policy names was a direct breach.
            if (!string.IsNullOrWhiteSpace(data.PromotionDecision))
            {
                column.Item().Element(c => ComposePromotion(c, data));
                column.Item().Height(10);
            }

            // ── Comments ──
            column.Item().Element(c => ComposeComments(c, data));
            column.Item().Height(16);

            // ── Signature Lines ──
            column.Item().Element(ComposeSignatures);
            column.Item().Height(12);

            // ── CAPS Legend ──
            column.Item().Element(ComposeLegend);
        });
    }

    // ─── Student Info ───
    private static void ComposeStudentInfo(IContainer container, ReportPdfData data)
    {
        container.Border(1).BorderColor(BorderCol).Padding(8).Row(row =>
        {
            // Left column
            row.RelativeItem().Column(c =>
            {
                InfoRow(c, "Student Name:", data.StudentName);
                InfoRow(c, "Admission No:", data.AdmissionNumber ?? "N/A");
                InfoRow(c, "Class:", data.ClassName ?? "N/A");
            });

            // Right column
            row.RelativeItem().Column(c =>
            {
                InfoRow(c, "Date Generated:", data.GeneratedDate ?? "N/A");
                var posText = data.ClassPosition.HasValue
                    ? $"{data.ClassPosition} of {data.TotalStudentsInClass ?? 0}"
                    : "N/A";
                InfoRow(c, "Class Position:", posText);
                InfoRow(c, "Overall:", data.OverallPercentage.HasValue ? $"{Mark(data.OverallPercentage)}%" : "N/A");
            });
        });
    }

    private static void InfoRow(ColumnDescriptor column, string label, string value)
    {
        column.Item().BorderBottom(0.5f).BorderColor(LightBorder).PaddingVertical(2).Row(r =>
        {
            r.ConstantItem(100).Text(label).Bold().FontSize(9);
            r.RelativeItem().Text(value ?? "").FontSize(9);
        });
    }

    // ─── Subject Table with overall footer ───
    private static void ComposeSubjectTable(IContainer container, ReportPdfData data)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2.6f); // Subject
                columns.RelativeColumn(0.9f); // Code
                columns.RelativeColumn(1.1f); // Term Mark
                columns.RelativeColumn(1.1f); // Exam Mark
                columns.RelativeColumn(1.1f); // Final Mark
                columns.RelativeColumn(0.6f); // Level
                columns.RelativeColumn(1.1f); // Class average (RC-06)
                columns.RelativeColumn(0.8f); // Position in class (RC-06)
                columns.RelativeColumn(1.6f); // Teacher
                columns.RelativeColumn(2.3f); // Comment
            });

            // Header row
            table.Header(header =>
            {
                // The subject table's header row carries the school's colour, with a
                // foreground picked for contrast so a dark navy and a pale gold
                // brand both stay readable.
                var brandBg = data.PrimaryColor;
                var brandFg = ReadableOn(brandBg);
                IContainer BrandedHeader(IContainer cell) =>
                    cell.Border(1).BorderColor(BorderCol).Background(brandBg).Padding(4);

                header.Cell().Element(BrandedHeader).Text("SUBJECT").Bold().FontSize(8).FontColor(brandFg);
                header.Cell().Element(BrandedHeader).Text("CODE").Bold().FontSize(8).FontColor(brandFg);
                header.Cell().Element(BrandedHeader).AlignCenter().Text("TERM\nMARK (%)").Bold().FontSize(8).FontColor(brandFg);
                header.Cell().Element(BrandedHeader).AlignCenter().Text("EXAM\nMARK (%)").Bold().FontSize(8).FontColor(brandFg);
                header.Cell().Element(BrandedHeader).AlignCenter().Text("FINAL\nMARK (%)").Bold().FontSize(8).FontColor(brandFg);
                header.Cell().Element(BrandedHeader).AlignCenter().Text("LVL").Bold().FontSize(8).FontColor(brandFg);
                header.Cell().Element(BrandedHeader).AlignCenter().Text("CLASS\nAVG (%)").Bold().FontSize(8).FontColor(brandFg);
                header.Cell().Element(BrandedHeader).AlignCenter().Text("POS").Bold().FontSize(8).FontColor(brandFg);
                header.Cell().Element(BrandedHeader).Text("TEACHER").Bold().FontSize(8).FontColor(brandFg);
                header.Cell().Element(BrandedHeader).Text("COMMENT").Bold().FontSize(8).FontColor(brandFg);
            });

            // Data rows
            foreach (var s in data.Subjects)
            {
                table.Cell().Element(DataCellStyle).Text(s.SubjectName ?? "").SemiBold().FontSize(9);
                table.Cell().Element(DataCellStyle).Text(s.SubjectCode ?? "-").FontSize(9);
                table.Cell().Element(DataCellStyle).AlignCenter().Text(Mark(s.TermMark)).FontSize(9);
                table.Cell().Element(DataCellStyle).AlignCenter().Text(Mark(s.ExamMark)).FontSize(9);
                table.Cell().Element(DataCellStyle).AlignCenter()
                    // RC-15: the dagger marks a mark that is the school-based
                    // component only, explained in the note under the table.
                    .Text(s.AwaitsExternalExamination ? Mark(s.FinalMark) + ExternalExamMarker : Mark(s.FinalMark))
                    .SemiBold().FontSize(9);
                table.Cell().Element(DataCellStyle).AlignCenter().Text(Level(s.AchievementLevel)).FontSize(9);
                // RC-06: the class comparison. Blank until the cohort pass has
                // run, rather than a zero that reads as a class average of 0%.
                table.Cell().Element(DataCellStyle).AlignCenter().Text(Mark(s.ClassAverage)).FontSize(9);
                table.Cell().Element(DataCellStyle).AlignCenter().Text(s.SubjectPosition?.ToString() ?? "-").FontSize(9);
                table.Cell().Element(DataCellStyle).Text(s.TeacherName ?? "-").FontSize(8);
                table.Cell().Element(DataCellStyle).Text(s.TeacherComment ?? "-").FontSize(8);
            }

            // Overall footer row. RC-07: this prints the overall stored on the
            // report, which is the same number the screen shows. It used to be
            // recomputed here from the rendered rows, so an edited mark or a
            // subject with no final mark made the printed card contradict the
            // report it was printed from.
            var overallAvg = Mark(data.OverallPercentage);

            // Span 4 columns for label
            table.Cell().ColumnSpan(4)
                .Border(1).BorderColor(BorderCol)
                .Background(Colors.Grey.Lighten4)
                .Padding(4).AlignRight()
                .Text("Overall Average:").Bold().FontSize(9);
            table.Cell()
                .Border(1).BorderColor(BorderCol)
                .Background(Colors.Grey.Lighten4)
                .Padding(4).AlignCenter()
                .Text($"{overallAvg}%").Bold().FontSize(9);
            table.Cell()
                .Border(1).BorderColor(BorderCol)
                .Background(Colors.Grey.Lighten4)
                .Padding(4).AlignCenter()
                .Text(Level(data.OverallAchievementLevel)).Bold().FontSize(9);
            table.Cell().ColumnSpan(4)
                .Border(1).BorderColor(BorderCol)
                .Background(Colors.Grey.Lighten4)
                .Padding(4).Text("").FontSize(9);
        });
    }

    // ─── RC-16: the promotion decision, which the year-end card exists to carry ───
    private static void ComposePromotion(IContainer container, ReportPdfData data)
    {
        var brandBg = data.PrimaryColor;
        var brandFg = ReadableOn(brandBg);

        container.Border(1).BorderColor(BorderCol).Column(column =>
        {
            column.Item().Background(brandBg).Padding(4)
                .Text("PROMOTION DECISION").Bold().FontSize(8).FontColor(brandFg);

            column.Item().Padding(6).Column(body =>
            {
                var line = string.IsNullOrWhiteSpace(data.PromotedToGradeName)
                    ? data.PromotionDecision
                    : $"{data.PromotionDecision} to {data.PromotedToGradeName}";

                body.Item().Text(line).Bold().FontSize(11);

                if (!string.IsNullOrWhiteSpace(data.PromotionReason))
                {
                    body.Item().Height(3);
                    body.Item().Text(data.PromotionReason).FontSize(9);
                }
            });
        });
    }

    private static IContainer HeaderCellStyle(IContainer cell)
    {
        return cell.Border(1).BorderColor(BorderCol).Background(HeaderBg).Padding(4);
    }

    private static IContainer DataCellStyle(IContainer cell)
    {
        return cell.Border(1).BorderColor(BorderCol).Padding(4);
    }

    // ─── Attendance cells ───
    private static void ComposeAttendance(IContainer container, ReportPdfData data)
    {
        var totalDays = data.DaysPresent + data.DaysAbsent;

        container.Row(row =>
        {
            AttendanceCell(row, "DAYS PRESENT", data.DaysPresent.ToString());
            AttendanceCell(row, "DAYS ABSENT", data.DaysAbsent.ToString());
            AttendanceCell(row, "DAYS LATE", data.DaysLate.ToString());
            AttendanceCell(row, "TOTAL SCHOOL DAYS", totalDays.ToString());
        });
    }

    private static void AttendanceCell(RowDescriptor row, string label, string value)
    {
        row.RelativeItem().Border(1).BorderColor(BorderCol).Padding(6).Column(c =>
        {
            c.Item().AlignCenter().Text(label).Bold().FontSize(8);
            c.Item().AlignCenter().Text(value).Bold().FontSize(16);
        });
    }

    // ─── Comments ───
    private static void ComposeComments(IContainer container, ReportPdfData data)
    {
        container.Column(column =>
        {
            CommentBox(column, "CLASS TEACHER'S COMMENT", data.TeacherComment);
            column.Item().Height(6);
            CommentBox(column, "PRINCIPAL'S COMMENT", data.PrincipalComment);

            if (!string.IsNullOrWhiteSpace(data.ParentComment))
            {
                column.Item().Height(6);
                CommentBox(column, "PARENT'S COMMENT", data.ParentComment);
            }
        });
    }

    private static void CommentBox(ColumnDescriptor column, string label, string comment)
    {
        column.Item().Border(1).BorderColor(BorderCol).Column(c =>
        {
            // Label bar
            c.Item().BorderBottom(1).BorderColor(LightBorder).Padding(4)
                .Text(label).Bold().FontSize(8);

            // Content area — minimum height so it looks like a form field even when empty
            c.Item().MinHeight(36).Padding(6)
                .Text(comment ?? "").FontSize(10);
        });
    }

    // ─── Signature Lines ───
    private static void ComposeSignatures(IContainer container)
    {
        container.Row(row =>
        {
            SignatureBlock(row, "Class Teacher");
            row.ConstantItem(30); // spacer
            SignatureBlock(row, "Principal");
            row.ConstantItem(30); // spacer
            SignatureBlock(row, "Parent / Guardian");
        });
    }

    private static void SignatureBlock(RowDescriptor row, string title)
    {
        row.RelativeItem().Column(c =>
        {
            c.Item().Height(30); // space for signature
            c.Item().LineHorizontal(1).LineColor(BorderCol);
            c.Item().Height(3);
            c.Item().AlignCenter().Text(title).FontSize(9);
        });
    }

    // ─── CAPS Legend Table ───
    private static void ComposeLegend(IContainer container)
    {
        container.Border(1).BorderColor(BorderCol).Padding(6).Column(column =>
        {
            column.Item().Text("CAPS ACHIEVEMENT LEVEL DESCRIPTORS").Bold().FontSize(8);
            column.Item().Height(4);

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn(); // L7
                    cols.RelativeColumn(); // L6
                    cols.RelativeColumn(); // L5
                    cols.RelativeColumn(); // L4
                    cols.RelativeColumn(); // L3
                    cols.RelativeColumn(); // L2
                    cols.RelativeColumn(); // L1
                });

                // RC-07: the levels and their bands come from CapsAchievementScale,
                // so the printed legend cannot drift from the levels actually
                // awarded above it.
                var levels = psms.Domain.Assessment.CapsAchievementScale.Descending;

                // Header
                table.Header(h =>
                {
                    foreach (var level in levels)
                    {
                        h.Cell().Border(0.5f).BorderColor(LightBorder)
                            .Background(HeaderBg).Padding(2).AlignCenter()
                            .Text($"Level {(int)level}").Bold().FontSize(7);
                    }
                });

                // Descriptions
                foreach (var level in levels)
                {
                    var (low, high) = psms.Domain.Assessment.CapsAchievementScale.RangeFor(level);
                    var descriptor = psms.Domain.Assessment.CapsAchievementScale.ShortDescriptorFor(level);

                    table.Cell().Border(0.5f).BorderColor(LightBorder)
                        .Padding(2).AlignCenter()
                        .Text($"{descriptor}\n{low}–{high}%").FontSize(7);
                }
            });
        });
    }

    // ─── Footer: page numbers + print date ───
    private static void ComposeFooter(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().AlignLeft()
                .Text($"Printed: {DateTime.Now:dd MMM yyyy}").FontSize(8).FontColor(Colors.Grey.Medium);

            row.RelativeItem().AlignCenter().Text(text =>
            {
                text.Span("Page ").FontSize(8).FontColor(Colors.Grey.Medium);
                text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                text.Span(" of ").FontSize(8).FontColor(Colors.Grey.Medium);
                text.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
            });

            row.RelativeItem(); // balance
        });
    }
}
