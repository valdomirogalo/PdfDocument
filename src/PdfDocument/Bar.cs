namespace PdfDocument;

/// <summary>
/// Represents an individual bar in a barcode.
/// </summary>
public struct Bar
{
    /// <summary>Bar width in module multiples.</summary>
    public int Width;

    /// <summary>
    /// <see langword="true"/> for black bar; <see langword="false"/> for white space.
    /// </summary>
    public bool Black;
}
