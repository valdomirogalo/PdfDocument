namespace PdfDocument;

/// <summary>
/// Represents an individual page within a PDF document.
/// Contains dimensions and a <see cref="PdfCanvas"/> for drawing.
/// </summary>
/// <param name="width">Width in points (default: 612 = landscape A4 / letter).</param>
/// <param name="height">Height in points (default: 792 = letter).</param>
public class PdfPage(double width = PdfConstants.DefaultPageWidth,
                     double height = PdfConstants.DefaultPageHeight)
{
    /// <summary>Page width in points (1/72 inch).</summary>
    public double Width { get; } = width;

    /// <summary>Page height in points.</summary>
    public double Height { get; } = height;

    /// <summary>Canvas where drawing commands are recorded.</summary>
    public PdfCanvas Canvas { get; } = new PdfCanvas();
}
