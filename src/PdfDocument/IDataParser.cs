namespace PdfDocument;

/// <summary>
/// Parses an input file into a strongly-typed data model.
/// Each implementation handles a specific input format (XML, JSON, CSV, etc.)
/// and produces a specific <see cref="IPdfData"/> model.
/// </summary>
/// <typeparam name="T">The data model type (must implement <see cref="IPdfData"/>).</typeparam>
public interface IDataParser<out T> where T : IPdfData
{
    /// <summary>
    /// Parses the input file and returns the extracted data.
    /// </summary>
    T Parse(string inputPath);

    /// <summary>
    /// Checks whether this parser can handle the given input file.
    /// Typically inspects the file extension, MIME type, or file header.
    /// </summary>
    bool CanParse(string inputPath);
}
