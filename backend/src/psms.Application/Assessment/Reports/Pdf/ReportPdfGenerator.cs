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
    private const float LogoMaxHeight = 36f;
    private const float LogoMaxWidth = 120f;

    /// <summary>
    /// The colour if it is one QuestPDF will accept, otherwise the stock PSMS
    /// primary. Branding is validated on the way in, but a report card must not
    /// be the thing that falls over if a bad value ever reaches it.
    /// </summary>
    /// <summary>
    /// Whether these bytes are an image QuestPDF will actually draw.
    /// <para>
    /// The logo and the stamp are whatever a school uploaded, fetched over HTTP
    /// at render time. Handing QuestPDF something that is not an image throws
    /// out of document composition and takes the whole report card with it — so
    /// a school with a corrupt logo could not print any card at all, and no
    /// message would say why. Decided once here rather than guessed at each
    /// call site.
    /// </para>
    /// </summary>
    private static bool IsRenderableImage(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0) return false;

        try
        {
            // QuestPDF's own decoder, so this answers the question the renderer
            // will ask rather than a different one.
            return QuestPDF.Infrastructure.Image.FromBinaryData(bytes) != null;
        }
        catch (Exception)
        {
            return false;
        }
    }

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
            if (IsRenderableImage(data.LogoBytes))
            {
                column.Item().AlignCenter()
                    .MaxHeight(LogoMaxHeight).MaxWidth(LogoMaxWidth)
                    .Image(data.LogoBytes).FitArea();
                column.Item().Height(3);
            }

            // School name — large, uppercase, centred, in the school's colour
            column.Item().AlignCenter()
                .Text(data.SchoolName?.ToUpperInvariant() ?? "SCHOOL REPORT CARD")
                .Bold().FontSize(16).LetterSpacing(0.05f).FontColor(InkOnWhite(data.PrimaryColor));

            // Report type, term and year, on one line. These were three
            // stacked lines that largely repeated each other — "Term 1 Report"
            // directly above "Term 1 — 2026 Academic Year".
            var reportType = data.ReportType ?? "Student Report Card";

            // "Term 1 Report · Term 1 · 2026" says the term twice. The report
            // type usually names it already, so the term is only added when it
            // does not.
            var term = !string.IsNullOrWhiteSpace(data.TermName)
                && reportType.IndexOf(data.TermName, StringComparison.OrdinalIgnoreCase) < 0
                    ? data.TermName
                    : null;

            var subtitle = string.Join("  ·  ", new[] { reportType, term, data.AcademicYearName }
                .Where(x => !string.IsNullOrWhiteSpace(x)));

            column.Item().AlignCenter().Text(subtitle).SemiBold().FontSize(10.5f);

            // RC-21, §25(8)(b): "dates of closing and opening of school". These
            // describe the term rather than the learner, so they sit under the
            // term in the header instead of taking a row of the details grid.
            if (!string.IsNullOrWhiteSpace(data.SchoolOpensOn)
                || !string.IsNullOrWhiteSpace(data.SchoolClosesOn))
            {
                column.Item().AlignCenter()
                    .Text($"School opens {data.SchoolOpensOn ?? "?"}  ·  closes {data.SchoolClosesOn ?? "?"}")
                    .FontSize(8).FontColor(Colors.Grey.Darken2);
            }

            column.Item().Height(3);

            // Double rule — the heavy line takes the school's colour, the hairline
            // stays black so the card still reads as a document in mono.
            column.Item().LineHorizontal(2).LineColor(InkOnWhite(data.PrimaryColor));
            column.Item().Height(2);
            column.Item().LineHorizontal(0.5f).LineColor(BorderCol);

            column.Item().Height(5);
        });
    }

    // ─── CONTENT ───
    private static void ComposeContent(IContainer container, ReportPdfData data)
    {
        container.Column(column =>
        {
            // ── Student Info Grid (2 columns) ──
            column.Item().Element(c => ComposeStudentInfo(c, data));
            column.Item().Height(6);

            // ── Subject Table ──
            column.Item().Element(c => ComposeSubjectTable(c, data));

            // ── RC-15: what the dagger means, when anything carries one ──
            if (data.Subjects.Any(s => s.AwaitsExternalExamination))
            {
                column.Item().Height(3);
                // Deliberately not italic: the italic face's "ti" ligature
                // carries no usable mapping, so copying the note out of the PDF
                // — or reading it with a screen reader — turns "National" into
                // "Na onal".
                column.Item().Text(ExternalExamNote).FontSize(7.5f).FontColor(Colors.Grey.Darken2);
            }

            column.Item().Height(6);

            // ── Attendance ──
            column.Item().Element(c => ComposeAttendance(c, data));
            column.Item().Height(6);

            // ── RC-17: conduct and diligence (RE-002) ──
            if (!string.IsNullOrWhiteSpace(data.ConductRating)
                || !string.IsNullOrWhiteSpace(data.DiligenceRating)
                || !string.IsNullOrWhiteSpace(data.BehaviourComments))
            {
                column.Item().Element(c => ComposeConduct(c, data));
                column.Item().Height(6);
            }

            // ── RC-16: the promotion decision ──
            // NPPPPR §(2b)(c): "the decision reached at the meeting contemplated
            // above must be reflected on the learner's report card." Printing
            // attendance, marks and comments while omitting the one thing the
            // policy names was a direct breach.
            if (!string.IsNullOrWhiteSpace(data.PromotionDecision))
            {
                column.Item().Element(c => ComposePromotion(c, data));
                column.Item().Height(6);
            }

            // ── Comments ──
            column.Item().Element(c => ComposeComments(c, data));
            column.Item().Height(10);

            // ── Signature Lines ──
            column.Item().Element(c => ComposeSignatures(c, data));
            column.Item().Height(8);

            // ── How to check this card is real ──
            if (!string.IsNullOrWhiteSpace(data.VerificationUrl))
            {
                column.Item().Element(c => ComposeVerification(c, data));
                column.Item().Height(6);
            }

            // ── CAPS Legend ──
            column.Item().Element(ComposeLegend);
        });
    }

    // ─── Student Info ───
    private static void ComposeStudentInfo(IContainer container, ReportPdfData data)
    {
        container.Border(1).BorderColor(BorderCol).Padding(6).Column(grid =>
        {
            grid.Item().Row(row =>
            {
                // Left column — §25(8)(a): name, grade and class, date of birth.
                row.RelativeItem().Column(c =>
                {
                    InfoRow(c, "Student Name:", data.StudentName);
                    InfoRow(c, "Admission No:", data.AdmissionNumber ?? "N/A");
                    InfoRow(c, "Date of Birth:", data.DateOfBirth ?? "N/A");
                    // §25(8)(a) asks for the grade and the class. They are one
                    // fact about where the learner sits, so they share a line
                    // rather than spending two rows of a card that has to fit
                    // on one page.
                    InfoRow(c, "Grade / Class:", string.Join("  ·  ",
                        new[] { data.GradeName, data.ClassName }
                            .Where(x => !string.IsNullOrWhiteSpace(x))
                            .Distinct()));
                });

                // Right column
                row.RelativeItem().Column(c =>
                {
                    InfoRow(c, "Date Generated:", data.GeneratedDate ?? "N/A");
                    // RC-19. No class position in the Foundation Phase. It is a
                    // rank off the overall aggregate — the very number §17(4)(a)
                    // does not provide for and which this card already omits —
                    // so printing it reports that aggregate by another name, and
                    // ranks a five-year-old against their class while doing it.
                    if (data.ReportsPercentages)
                    {
                        var posText = data.ClassPosition.HasValue
                            ? $"{data.ClassPosition} of {data.TotalStudentsInClass ?? 0}"
                            : "N/A";
                        InfoRow(c, "Class Position:", posText);
                    }

                    // RC-19. No overall for the Foundation Phase. The aggregate
                    // is a percentage, and an averaged percentage is precisely
                    // what §17(4)(a) does not provide for — the phase reports a
                    // code and a description per subject, and the Protocol's
                    // reporting instrument for it (§18, Table 1) has no overall.
                    if (data.ReportsPercentages)
                    {
                        InfoRow(c, "Overall:",
                            data.OverallPercentage.HasValue ? $"{Mark(data.OverallPercentage)}%" : "N/A");
                    }

                    // §25(8)(d): this term read against the last one.
                    if (!string.IsNullOrWhiteSpace(data.PreviousPerformance))
                        InfoRow(c, "Previously:", data.PreviousPerformance);

                });
            });

            // RC-17: RE-002's report card number. Full width, because it is one
            // long unbroken string — in a half-width cell it wrapped over three
            // lines and cost the card more room than the whole info grid saved.
            grid.Item().PaddingTop(1.5f).Row(r =>
            {
                r.ConstantItem(88).Text("Report card no:").Bold().FontSize(8.5f);
                r.RelativeItem().Text(data.ReportCardNumber ?? "N/A").FontSize(8);
            });
        });
    }

    private static void InfoRow(ColumnDescriptor column, string label, string value)
    {
        column.Item().BorderBottom(0.5f).BorderColor(LightBorder).PaddingVertical(1f).Row(r =>
        {
            r.ConstantItem(88).Text(label).Bold().FontSize(8.5f);
            r.RelativeItem().Text(value ?? "").FontSize(8.5f);
        });
    }

    // ─── Subject Table with overall footer ───
    private static void ComposeSubjectTable(IContainer container, ReportPdfData data)
    {
        if (!data.ReportsPercentages)
        {
            ComposeFoundationPhaseSubjectTable(container, data);
            return;
        }

        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                // Widths are what decide the height of this table: a subject
                // name or a teacher's name that does not fit wraps, and the
                // whole row grows with it. "Engineering Graphics and Design"
                // and "Sipho Christopher Nkosi" were both wrapping while the
                // comment column — a row of dashes on most cards — held the
                // width they needed. The totals are unchanged; the space is
                // just where it is read.
                columns.RelativeColumn(3.1f);  // Subject
                columns.RelativeColumn(1.05f); // Code
                columns.RelativeColumn(1.05f); // Term Mark
                columns.RelativeColumn(1.05f); // Exam Mark
                columns.RelativeColumn(1.05f); // Final Mark
                columns.RelativeColumn(0.55f); // Level
                columns.RelativeColumn(1.05f); // Class average (RC-06)
                columns.RelativeColumn(0.75f); // Position in class (RC-06)
                columns.RelativeColumn(2.05f); // Teacher
                columns.RelativeColumn(1.5f);  // Comment
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
                    cell.Border(1).BorderColor(BorderCol).Background(brandBg).Padding(3);

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
                table.Cell().Element(DataCellStyle).Text(s.SubjectCode ?? "-").FontSize(8.5f);
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
                table.Cell().Element(DataCellStyle).Text(s.TeacherName ?? "-").FontSize(7.5f);
                table.Cell().Element(DataCellStyle).Text(s.TeacherComment ?? "-").FontSize(7.5f);
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

    /// <summary>
    /// RC-19. The Foundation Phase table: the national code and its achievement
    /// description, and no percentage anywhere.
    /// <para>
    /// National Protocol §17(4)(a) gives the phase codes and descriptions as the
    /// reporting instrument. A percentage is provided for from Grade 4 onward,
    /// so a Grade 1 card printing "68.0%" is reporting on a scale the phase does
    /// not use. The examination column goes too: the Foundation Phase is wholly
    /// school-based, so it was always structurally empty here.
    /// </para>
    /// </summary>
    private static void ComposeFoundationPhaseSubjectTable(IContainer container, ReportPdfData data)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3.0f);  // Subject
                columns.RelativeColumn(1.0f);  // Code
                columns.RelativeColumn(0.7f);  // Level
                columns.RelativeColumn(2.6f);  // Achievement description
                columns.RelativeColumn(2.0f);  // Teacher
                columns.RelativeColumn(2.2f);  // Comment
            });

            table.Header(header =>
            {
                var brandBg = data.PrimaryColor;
                var brandFg = ReadableOn(brandBg);
                IContainer BrandedHeader(IContainer cell) =>
                    cell.Border(1).BorderColor(BorderCol).Background(brandBg).Padding(3);

                header.Cell().Element(BrandedHeader).Text("SUBJECT").Bold().FontSize(7.5f).FontColor(brandFg);
                header.Cell().Element(BrandedHeader).Text("CODE").Bold().FontSize(7.5f).FontColor(brandFg);
                header.Cell().Element(BrandedHeader).AlignCenter().Text("LEVEL").Bold().FontSize(7.5f).FontColor(brandFg);
                header.Cell().Element(BrandedHeader).Text("ACHIEVEMENT").Bold().FontSize(7.5f).FontColor(brandFg);
                header.Cell().Element(BrandedHeader).Text("TEACHER").Bold().FontSize(7.5f).FontColor(brandFg);
                header.Cell().Element(BrandedHeader).Text("COMMENT").Bold().FontSize(7.5f).FontColor(brandFg);
            });

            foreach (var s in data.Subjects)
            {
                table.Cell().Element(DataCellStyle).Text(s.SubjectName ?? "").SemiBold().FontSize(8.5f);
                table.Cell().Element(DataCellStyle).Text(s.SubjectCode ?? "-").FontSize(8.5f);
                table.Cell().Element(DataCellStyle).AlignCenter()
                    .Text(Level(s.AchievementLevel)).SemiBold().FontSize(8.5f);
                table.Cell().Element(DataCellStyle)
                    .Text(AchievementDescription(s.AchievementLevel)).FontSize(8.5f);
                table.Cell().Element(DataCellStyle).Text(s.TeacherName ?? "-").FontSize(7.5f);
                table.Cell().Element(DataCellStyle).Text(s.TeacherComment ?? "-").FontSize(7.5f);
            }
        });
    }

    /// <summary>
    /// The full achievement description for a level — "Substantial achievement".
    /// The Foundation Phase reports this rather than a mark, so it is spelled out
    /// here rather than shortened the way the legend shortens it.
    /// </summary>
    private static string AchievementDescription(psms.Domain.Shared.Enums.CapsAchievementLevel? level) =>
        level.HasValue
            ? psms.Domain.Assessment.CapsAchievementScale.DescriptorFor(level.Value)
            : "-";

    // ─── RC-17: conduct and diligence, two of RE-002's named fields ───
    private static void ComposeConduct(IContainer container, ReportPdfData data)
    {
        container.Border(1).BorderColor(BorderCol).Column(column =>
        {
            column.Item().Background(HeaderBg).Padding(3)
                .Text("CONDUCT AND DILIGENCE").Bold().FontSize(7.5f);

            column.Item().Padding(4).Column(body =>
            {
                body.Item().Row(row =>
                {
                    row.RelativeItem().Text(t =>
                    {
                        t.Span("Conduct: ").SemiBold().FontSize(9);
                        t.Span(data.ConductRating ?? "-").FontSize(9);
                    });
                    row.RelativeItem().Text(t =>
                    {
                        t.Span("Diligence: ").SemiBold().FontSize(9);
                        t.Span(data.DiligenceRating ?? "-").FontSize(9);
                    });
                });

                if (!string.IsNullOrWhiteSpace(data.BehaviourComments))
                {
                    body.Item().Height(2);
                    body.Item().Text(data.BehaviourComments).FontSize(9);
                }
            });
        });
    }

    // ─── RC-16: the promotion decision, which the year-end card exists to carry ───
    private static void ComposePromotion(IContainer container, ReportPdfData data)
    {
        var brandBg = data.PrimaryColor;
        var brandFg = ReadableOn(brandBg);

        container.Border(1).BorderColor(BorderCol).Column(column =>
        {
            column.Item().Background(brandBg).Padding(3)
                .Text("PROMOTION DECISION").Bold().FontSize(7.5f).FontColor(brandFg);

            column.Item().Padding(4).Column(body =>
            {
                var line = string.IsNullOrWhiteSpace(data.PromotedToGradeName)
                    ? data.PromotionDecision
                    : $"{data.PromotionDecision} to {data.PromotedToGradeName}";

                body.Item().Text(line).Bold().FontSize(11);

                if (!string.IsNullOrWhiteSpace(data.PromotionReason))
                {
                    body.Item().Height(2);
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
        return cell.Border(1).BorderColor(BorderCol).Padding(2.5f);
    }

    // ─── Attendance cells ───
    private static void ComposeAttendance(IContainer container, ReportPdfData data)
    {
        // RC-17: the school days in the period, which RE-002 reconciles the
        // other two against — not the sum of our own two figures, which would
        // always agree with itself and prove nothing.
        var totalDays = data.DaysInTerm ?? (data.DaysPresent + data.DaysAbsent);

        // Nothing recorded is not the same as a learner who attended nothing.
        // The figures are plain ints, so an attendance nobody captured arrived
        // here as four zeros and the card stated, on a legal document, that the
        // child was present on none of zero school days. A dash says what is
        // true: the school has not recorded it.
        var recorded = totalDays > 0
            || data.DaysPresent > 0
            || data.DaysAbsent > 0
            || data.DaysLate > 0;

        string Days(int value) => recorded ? value.ToString() : "—";

        container.Row(row =>
        {
            AttendanceCell(row, "DAYS PRESENT", Days(data.DaysPresent));
            AttendanceCell(row, "DAYS ABSENT", Days(data.DaysAbsent));
            AttendanceCell(row, "DAYS LATE", Days(data.DaysLate));
            AttendanceCell(row, "SCHOOL DAYS", Days(totalDays));
        });
    }

    private static void AttendanceCell(RowDescriptor row, string label, string value)
    {
        row.RelativeItem().Border(1).BorderColor(BorderCol).Padding(4).Column(c =>
        {
            c.Item().AlignCenter().Text(label).Bold().FontSize(7.5f);
            c.Item().AlignCenter().Text(value).Bold().FontSize(13);
        });
    }

    // ─── Comments ───
    private static void ComposeComments(IContainer container, ReportPdfData data)
    {
        container.Column(column =>
        {
            CommentBox(column, "CLASS TEACHER'S COMMENT", data.TeacherComment);
            column.Item().Height(4);
            CommentBox(column, "PRINCIPAL'S COMMENT", data.PrincipalComment);

            if (!string.IsNullOrWhiteSpace(data.ParentComment))
            {
                column.Item().Height(4);
                CommentBox(column, "PARENT'S COMMENT", data.ParentComment);
            }
        });
    }

    private static void CommentBox(ColumnDescriptor column, string label, string comment)
    {
        column.Item().Border(1).BorderColor(BorderCol).Column(c =>
        {
            // Label bar
            c.Item().BorderBottom(1).BorderColor(LightBorder).Padding(3)
                .Text(label).Bold().FontSize(7.5f);

            // Content area — minimum height so it looks like a form field even
            // when empty, and still enough to write a line or two of comment by
            // hand on the printed card.
            c.Item().MinHeight(24).Padding(4)
                .Text(comment ?? "").FontSize(9);
        });
    }

    // ─── Signature Lines ───
    /// <summary>
    /// The strip a recipient uses to confirm the card is genuine.
    /// <para>
    /// This is the only part of the document that actually proves anything. The
    /// signatures above are a picture — anyone holding one card can lift them —
    /// and the PDF carries no signing certificate, so a mark in it can be
    /// edited. Scanning this checks the card against the school's own records.
    /// </para>
    /// </summary>
    private static void ComposeVerification(IContainer container, ReportPdfData data)
    {
        container
            .Background(Colors.Grey.Lighten4)
            .Border(0.5f).BorderColor(BorderCol)
            .Padding(5)
            .Row(row =>
            {
                var qr = TryRenderQr(data.VerificationUrl);
                if (qr != null)
                {
                    // 42pt is about 15mm printed, which every phone camera reads
                    // at arm's length and which costs the page a third less than
                    // the 58pt square did.
                    row.ConstantItem(42).Height(42).Image(qr).FitArea();
                    row.ConstantItem(8);
                }

                row.RelativeItem().AlignMiddle().Column(c =>
                {
                    c.Item().Text("Check this report card is genuine")
                        .SemiBold().FontSize(8.5f);
                    c.Item().Text("Scan the code, or visit the address below — the school's records "
                        + "confirm whether this card was issued, and to whom.")
                        .FontSize(7).FontColor(Colors.Grey.Darken1);
                    c.Item().Text(data.VerificationUrl).FontSize(6.5f).FontColor(Colors.Grey.Darken2);
                });
            });
    }

    /// <summary>
    /// The QR as a PNG, or null. A code that will not render must not cost the
    /// school the whole report card, so the strip simply loses its square and
    /// keeps the printed address.
    /// </summary>
    private static byte[] TryRenderQr(string url)
    {
        try
        {
            using var generator = new QRCoder.QRCodeGenerator();
            // Q corrects around 25% damage, which is what a code printed on paper
            // that gets folded, stamped and photocopied actually needs.
            using var data = generator.CreateQrCode(url, QRCoder.QRCodeGenerator.ECCLevel.Q);
            return new QRCoder.PngByteQRCode(data).GetGraphic(8);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static void ComposeSignatures(IContainer container, ReportPdfData data)
    {
        container.Row(row =>
        {
            // RC-17: RE-003 requires a teacher and a principal signature before
            // a card is issued, and they are recorded now rather than being
            // three blank lines that meant only that somebody had printed it.
            SignatureBlock(row, "Class Teacher", data.TeacherSignedBy, data.TeacherSignedDate, data.TeacherSignatureSvg);
            row.ConstantItem(30); // spacer
            SignatureBlock(row, "Principal", data.PrincipalSignedBy, data.PrincipalSignedDate, data.PrincipalSignatureSvg);
            row.ConstantItem(30); // spacer
            // Signed by hand on the printed card, so this one keeps its blank line.
            SignatureBlock(row, "Parent / Guardian", null, null, null);

            // RC-21, §25(8)(b): the school stamp, beside the signatures it
            // authenticates. Only when the school has uploaded one — a card
            // with no stamp prints as it always did rather than leaving a
            // labelled gap where one should be.
            if (IsRenderableImage(data.StampBytes))
            {
                row.ConstantItem(14);
                row.ConstantItem(54).AlignBottom().Column(c =>
                {
                    c.Item().Height(44).AlignCenter().Image(data.StampBytes).FitArea();
                    c.Item().Height(3);
                    c.Item().AlignCenter().Text("School Stamp").FontSize(7.5f)
                        .FontColor(Colors.Grey.Darken1);
                });
            }
        });
    }

    private static void SignatureBlock(
        RowDescriptor row, string title, string signedBy, string signedDate, string signatureSvg)
    {
        row.RelativeItem().Column(c =>
        {
            // RC-17: the name of whoever signed sits above the line, so a card
            // that was signed says who signed it. A line with nothing over it is
            // a card nobody has signed yet.
            if (!string.IsNullOrWhiteSpace(signedBy))
            {
                // Their handwriting, where they have drawn one. It sits above
                // the name rather than replacing it: the drawing is for a reader,
                // the typed name is what survives a signature that will not render.
                if (!string.IsNullOrWhiteSpace(signatureSvg))
                {
                    try
                    {
                        c.Item().Height(22).AlignCenter().Svg(signatureSvg).FitArea();
                    }
                    catch (Exception)
                    {
                        // A drawing that will not render must not cost the school
                        // the whole report card; the name and date still print.
                        c.Item().Height(22);
                    }
                }
                else
                {
                    c.Item().Height(12);
                }

                c.Item().AlignCenter().Text(signedBy).SemiBold().FontSize(9);
                c.Item().Height(2);
            }
            else
            {
                c.Item().Height(24); // space for a signature by hand
            }

            c.Item().LineHorizontal(1).LineColor(BorderCol);
            c.Item().Height(3);
            c.Item().AlignCenter().Text(title).FontSize(9);

            if (!string.IsNullOrWhiteSpace(signedDate))
                c.Item().AlignCenter().Text(signedDate).FontSize(7.5f).FontColor(Colors.Grey.Darken1);
        });
    }

    // ─── CAPS Legend ───
    /// <summary>
    /// The achievement scale, as one strip of running text.
    /// <para>
    /// This is a key to the LVL column, not data, and it was costing a second
    /// sheet of paper: a seven-column bordered table whose cells each wrapped to
    /// two lines took about 75pt, which is what pushed every card onto a page
    /// two that carried nothing else. Set as a run of "7 Outstanding 80–100%"
    /// groups it reads the same way and takes about 20pt.
    /// </para>
    /// </summary>
    private static void ComposeLegend(IContainer container)
    {
        // RC-07: the levels and their bands come from CapsAchievementScale, so
        // the printed legend cannot drift from the levels actually awarded above.
        var levels = psms.Domain.Assessment.CapsAchievementScale.Descending;

        container.Border(0.5f).BorderColor(BorderCol).Padding(5).Text(text =>
        {
            text.DefaultTextStyle(x => x.FontSize(7).FontColor(Colors.Grey.Darken3));

            text.Span("ACHIEVEMENT LEVELS   ").Bold().FontSize(7).FontColor(Colors.Black);

            var first = true;
            foreach (var level in levels)
            {
                var (low, high) = psms.Domain.Assessment.CapsAchievementScale.RangeFor(level);
                var descriptor = psms.Domain.Assessment.CapsAchievementScale.ShortDescriptorFor(level);

                if (!first) text.Span("   ·   ").FontColor(Colors.Grey.Medium);
                first = false;

                text.Span($"{(int)level} ").Bold().FontColor(Colors.Black);
                text.Span($"{descriptor} {low}–{high}%");
            }
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
