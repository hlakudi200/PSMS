using System.Linq;
using System.Text;
using UglyToad.PdfPig;

namespace psms.Tests.Assessment;

/// <summary>
/// Reads the words back out of a rendered PDF, so a test can assert on what a
/// report card actually says rather than only that bytes were produced.
/// </summary>
internal static class PdfText
{
    /// <summary>
    /// All the text on every page, one page per line. Words are joined with
    /// single spaces, so assert on a figure ("72.4%") or a heading rather than
    /// on exact spacing, which is a layout detail.
    /// </summary>
    public static string Extract(byte[] pdf)
    {
        using var document = PdfDocument.Open(pdf);

        var builder = new StringBuilder();

        foreach (var page in document.GetPages())
            builder.AppendLine(string.Join(" ", page.GetWords().Select(w => w.Text)));

        return builder.ToString();
    }
}
