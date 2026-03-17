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
    // Colours
    private static readonly string HeaderBg = Colors.Grey.Lighten3;
    private static readonly string BorderCol = Colors.Black;
    private static readonly string LightBorder = Colors.Grey.Lighten2;

    static ReportPdfGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public static byte[] Generate(ReportPdfData data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginVertical(30);
                page.MarginHorizontal(25);
                page.DefaultTextStyle(x => x.FontSize(10));

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
            // School name — large, uppercase, centred
            column.Item().AlignCenter()
                .Text(data.SchoolName?.ToUpperInvariant() ?? "SCHOOL REPORT CARD")
                .Bold().FontSize(18).LetterSpacing(0.05f);

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

            // Double rule
            column.Item().LineHorizontal(2).LineColor(BorderCol);
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
            column.Item().Height(10);

            // ── Attendance ──
            column.Item().Element(c => ComposeAttendance(c, data));
            column.Item().Height(10);

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
                InfoRow(c, "Overall:", data.OverallPercentage.HasValue ? $"{data.OverallPercentage:F1}%" : "N/A");
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
                columns.RelativeColumn(3);   // Subject
                columns.RelativeColumn(1);   // Code
                columns.RelativeColumn(1.2f); // Term Mark
                columns.RelativeColumn(1.2f); // Exam Mark
                columns.RelativeColumn(1.2f); // Final Mark
                columns.RelativeColumn(0.8f); // Level
                columns.RelativeColumn(2);   // Teacher
                columns.RelativeColumn(2.5f); // Comment
            });

            // Header row
            table.Header(header =>
            {
                header.Cell().Element(HeaderCellStyle).Text("SUBJECT").Bold().FontSize(8);
                header.Cell().Element(HeaderCellStyle).Text("CODE").Bold().FontSize(8);
                header.Cell().Element(HeaderCellStyle).AlignCenter().Text("TERM\nMARK (%)").Bold().FontSize(8);
                header.Cell().Element(HeaderCellStyle).AlignCenter().Text("EXAM\nMARK (%)").Bold().FontSize(8);
                header.Cell().Element(HeaderCellStyle).AlignCenter().Text("FINAL\nMARK (%)").Bold().FontSize(8);
                header.Cell().Element(HeaderCellStyle).AlignCenter().Text("LEVEL").Bold().FontSize(8);
                header.Cell().Element(HeaderCellStyle).Text("TEACHER").Bold().FontSize(8);
                header.Cell().Element(HeaderCellStyle).Text("COMMENT").Bold().FontSize(8);
            });

            // Data rows
            foreach (var s in data.Subjects)
            {
                table.Cell().Element(DataCellStyle).Text(s.SubjectName ?? "").SemiBold().FontSize(9);
                table.Cell().Element(DataCellStyle).Text(s.SubjectCode ?? "-").FontSize(9);
                table.Cell().Element(DataCellStyle).AlignCenter().Text(s.TermMark.HasValue ? $"{s.TermMark:F1}" : "-").FontSize(9);
                table.Cell().Element(DataCellStyle).AlignCenter().Text(s.ExamMark.HasValue ? $"{s.ExamMark:F1}" : "-").FontSize(9);
                table.Cell().Element(DataCellStyle).AlignCenter().Text(s.FinalMark.HasValue ? $"{s.FinalMark:F1}" : "-").SemiBold().FontSize(9);
                table.Cell().Element(DataCellStyle).AlignCenter().Text(s.AchievementLevel ?? "-").FontSize(9);
                table.Cell().Element(DataCellStyle).Text(s.TeacherName ?? "-").FontSize(8);
                table.Cell().Element(DataCellStyle).Text(s.TeacherComment ?? "-").FontSize(8);
            }

            // Overall footer row
            var subjectsWithFinal = data.Subjects.Where(s => s.FinalMark.HasValue).ToList();
            var overallAvg = subjectsWithFinal.Any()
                ? $"{subjectsWithFinal.Average(s => s.FinalMark!.Value):F1}"
                : "-";

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
                .Text(data.OverallAchievementLevel ?? "-").Bold().FontSize(9);
            table.Cell().ColumnSpan(2)
                .Border(1).BorderColor(BorderCol)
                .Background(Colors.Grey.Lighten4)
                .Padding(4).Text("").FontSize(9);
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

                // Header
                table.Header(h =>
                {
                    foreach (var lv in new[] { "Level 7", "Level 6", "Level 5", "Level 4", "Level 3", "Level 2", "Level 1" })
                    {
                        h.Cell().Border(0.5f).BorderColor(LightBorder)
                            .Background(HeaderBg).Padding(2).AlignCenter()
                            .Text(lv).Bold().FontSize(7);
                    }
                });

                // Descriptions
                var descs = new[]
                {
                    "Outstanding\n80–100%",
                    "Meritorious\n70–79%",
                    "Substantial\n60–69%",
                    "Adequate\n50–59%",
                    "Moderate\n40–49%",
                    "Elementary\n30–39%",
                    "Not Achieved\n0–29%",
                };
                foreach (var desc in descs)
                {
                    table.Cell().Border(0.5f).BorderColor(LightBorder)
                        .Padding(2).AlignCenter()
                        .Text(desc).FontSize(7);
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
