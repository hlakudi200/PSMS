using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

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
            builder.AppendLine(string.Join(" ", InReadingOrder(page.GetWords()).Select(w => w.Text)));

        return Unligature(builder.ToString());
    }

    /// <summary>
    /// The words sorted down the page and then across it.
    /// <para>
    /// PdfPig's <c>GetWords()</c> makes no promise about the order of words that
    /// sit close together, and it does not return them in the same order every
    /// time. That is invisible until a layout tightens: once the subject table
    /// and the signature block were a few points apart, a teacher's name in the
    /// table and the same name over the signature line began swapping places
    /// between runs, and an assertion that read "the text after the last
    /// 'Class Teacher'" failed about one run in three — on identical bytes.
    /// </para>
    /// <para>
    /// Sorting on the geometry the page actually has makes the reading stable,
    /// which is what a test needs. The sort is on the <i>baseline</i>, not the
    /// bounding box: a box is drawn round the glyphs, so "School-based" sits
    /// taller than "assessment" purely because it has ascenders, and sorting by
    /// the top of the box pulled the two apart and shuffled a sentence. Every
    /// word on a line shares a baseline whatever letters it happens to contain.
    /// The text is the last tiebreak, so the ordering is total rather than
    /// merely mostly-defined.
    /// </para>
    /// </summary>
    private static IEnumerable<Word> InReadingOrder(IEnumerable<Word> words) => words
        // PDF coordinates start at the bottom of the page, so "down the page"
        // is descending Y.
        .OrderByDescending(w => Math.Round(Baseline(w), 1))
        .ThenBy(w => w.BoundingBox.Left)
        .ThenBy(w => w.Text, StringComparer.Ordinal);

    /// <summary>
    /// Where the line this word sits on is written, rather than where its
    /// tallest letter reaches. Falls back to the bounding box for a word with
    /// no letters, which should not happen but must not throw if it does.
    /// </summary>
    private static double Baseline(Word word) =>
        word.Letters.Count > 0
            ? word.Letters[0].StartBaseLine.Y
            : word.BoundingBox.Bottom;

    /// <summary>
    /// Typographic ligatures back to their letters, so an assertion can be
    /// written the way the string is written in the source.
    /// <para>
    /// The embedded font renders "fi" as a single glyph, and it comes back out
    /// as U+FB01 — so "Certificate" extracts as "Certiﬁcate" and a
    /// perfectly correct assertion fails for reasons that have nothing to do
    /// with the report card.
    /// </para>
    /// </summary>
    private static string Unligature(string text) => text
        .Replace("ﬀ", "ff")
        .Replace("ﬁ", "fi")
        .Replace("ﬂ", "fl")
        .Replace("ﬃ", "ffi")
        .Replace("ﬄ", "ffl")
        .Replace("ﬅ", "st")
        .Replace("ﬆ", "st");
}
