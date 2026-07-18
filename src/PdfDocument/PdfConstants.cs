using System.Globalization;

namespace PdfDocument;

/// <summary>
/// Named constants used throughout the PdfDocument library.
/// Eliminates magic numbers and magic strings, improving maintainability.
/// </summary>
internal static class PdfConstants
{
    // ── Page ─────────────────────────────────────────────────────────
    internal const double DefaultPageWidth = 612.0;
    internal const double DefaultPageHeight = 792.0;

    // ── Image ─────────────────────────────────────────────────────────
    internal const int MaxImageSizeBytes = 512 * 1024; // 512 KB

    // ── Text ──────────────────────────────────────────────────────────
    internal const double TextWidthFactor = 0.6;
    internal const double TextVerticalAdjust = 0.3;
    internal const int MaxTextLength = 32768;

    // ── Default positioning (DANFE) ───────────────────────────────────
    internal const double DefaultMarginX = 40.0;
    internal const double DefaultMarginY = 720.0;
    internal const double DefaultLineHeight = 14.0;
    internal const double DefaultFontSize = 9.0;
    internal const double TitleFontSize = 14.0;
    internal const double SectionFontSize = 11.0;
    internal const double FooterFontSize = 8.0;
    internal const double PageWidth = 560.0; // Usable page width

    // ── DANFE spacing ─────────────────────────────────────────────────
    internal const double TitleSpacing = 25.0;
    internal const double DividerSpacing = 15.0;
    internal const double SectionSpacing = 18.0;
    internal const double FooterLineSpacing = 10.0;

    // ── Barcode Code 39 ───────────────────────────────────────────────
    internal const int BarBlackWidth = 3;
    internal const int BarWhiteWidth = 1;
    internal const int InterCharGapWidth = 1;

    // ── EAN-13 ────────────────────────────────────────────────────────
    internal const int Ean13DigitCount = 13;
    internal const int Ean12DigitCount = 12;
    internal const int EanPatternCount = 6;

    // ── JPEG markers ──────────────────────────────────────────────────
    internal const byte JpegMarkerPrefix = 0xFF;
    internal const byte JpegSof0Marker = 0xC0;

    // ── PDF structural constants ──────────────────────────────────────
    internal const string PdfHeader = "%PDF-1.4";
    internal const string XrefMarker = "xref";
    internal const string TrailerMarker = "trailer";
    internal const string StartXrefMarker = "startxref";
    internal const string EofMarker = "%%EOF";

    // ── Font ──────────────────────────────────────────────────────────
    internal const string DefaultFontDefinition =
        "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>";

    // ── Number formatting ──────────────────────────────────────────────
    internal static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

    // ── Name validation (CWE-79) ────────────────────────────────────────
    /// <summary>
    /// Validates that a PDF name contains only safe characters (alphanumeric, dash, underscore).
    /// Prevents PDF injection through object names.
    /// </summary>
    internal static bool IsValidPdfName(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;
        foreach (char c in name)
        {
            if (!char.IsLetterOrDigit(c) && c != '-' && c != '_')
                return false;
        }
        return true;
    }
}
