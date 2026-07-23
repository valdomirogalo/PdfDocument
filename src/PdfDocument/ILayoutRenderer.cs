namespace PdfDocument;

/// <summary>
/// Renders a data model into a PDF document using a specific visual layout.
/// Each implementation produces a specific layout (DANFE, invoice, report, etc.).
/// </summary>
/// <typeparam name="T">The data model type (must implement <see cref="IPdfData"/>).</typeparam>
public interface ILayoutRenderer<in T> where T : IPdfData
{
    /// <summary>
    /// Renders the data into a PDF file at the specified path.
    /// </summary>
    void Render(T data, string outputPath);
}
