namespace PdfDocument;

/// <summary>
/// Represents an individual page within a PDF document.
/// Contains dimensions, optional rotation and a <see cref="PdfCanvas"/> for drawing.
/// </summary>
/// <param name="width">Width in points (default: 612 = letter).</param>
/// <param name="height">Height in points (default: 792 = letter).</param>
public class PdfPage(double width = PdfConstants.DefaultPageWidth,
                     double height = PdfConstants.DefaultPageHeight)
{
    /// <summary>Page width in points (1/72 inch).</summary>
    public double Width { get; } = width;

    /// <summary>Page height in points.</summary>
    public double Height { get; } = height;

    /// <summary>
    /// Clockwise rotation in degrees. Must be a multiple of 90 (0, 90, 180, or 270).
    /// Default is 0 (no rotation). The rotation is applied by the PDF viewer;
    /// drawing coordinates remain unchanged.
    /// </summary>
    public int Rotation { get; init; }

    /// <summary>Canvas where drawing commands are recorded.</summary>
    public PdfCanvas Canvas { get; } = new PdfCanvas();
}
