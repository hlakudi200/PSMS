using System;
using QuestPDF.Helpers;

namespace psms.Shared.Pdf;

/// <summary>
/// Colour and image rules shared by every document the school prints — report
/// cards and fee statements. They lived privately in the report card generator;
/// a second document has to make the same decisions, so they are decided once.
/// </summary>
public static class PdfBranding
{
    /// <summary>
    /// The colour if it is one QuestPDF will accept, otherwise the stock PSMS
    /// primary. Branding is validated on the way in, but a printed document must
    /// not be the thing that falls over if a bad value ever reaches it.
    /// </summary>
    public static string SafeColor(string hex)
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
    public static string InkOnWhite(string hex)
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

    /// <summary>
    /// Black or white, whichever reads better on the given background. Mirrors
    /// the frontend's getReadableForeground so a branded header looks the same
    /// on screen and on paper; 0.179 is the crossover where the two give equal
    /// contrast.
    /// </summary>
    public static string ReadableOn(string hex)
    {
        var value = (hex ?? string.Empty).Replace("#", string.Empty);
        if (value.Length < 6 || !System.Text.RegularExpressions.Regex.IsMatch(value, "^[0-9A-Fa-f]{6}$"))
            return Colors.White;

        var luminance = Luminance(
            Convert.ToInt32(value.Substring(0, 2), 16),
            Convert.ToInt32(value.Substring(2, 2), 16),
            Convert.ToInt32(value.Substring(4, 2), 16));

        return luminance > 0.179f ? "#1F1F1F" : Colors.White;
    }

    /// <summary>
    /// Whether these bytes are an image QuestPDF will actually draw.
    /// <para>
    /// The logo and the stamp are whatever a school uploaded, fetched over HTTP
    /// at render time. Handing QuestPDF something that is not an image throws
    /// out of document composition and takes the whole document with it — so a
    /// school with a corrupt logo could not print anything at all, and no
    /// message would say why. Decided once here rather than guessed at each
    /// call site.
    /// </para>
    /// </summary>
    public static bool IsRenderableImage(byte[] bytes)
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

    private static float Luminance(int r, int g, int b)
    {
        float Channel(int c)
        {
            var srgb = c / 255f;
            return srgb <= 0.03928f ? srgb / 12.92f : (float)Math.Pow((srgb + 0.055f) / 1.055f, 2.4);
        }

        return 0.2126f * Channel(r) + 0.7152f * Channel(g) + 0.0722f * Channel(b);
    }
}
