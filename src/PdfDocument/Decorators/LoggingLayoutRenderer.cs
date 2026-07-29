using System.Diagnostics;

namespace PdfDocument.Decorators;

/// <summary>
/// Decorator that adds logging around <see cref="ILayoutRenderer{T}"/> operations.
/// Wraps any <see cref="ILayoutRenderer{T}"/> and logs Render calls
/// with timing information.
/// </summary>
/// <typeparam name="T">The data model type (must implement <see cref="IPdfData"/>).</typeparam>
/// <remarks>
/// Usage:
/// <code>
/// var factory = new PdfPluginFactory();
/// factory.RegisterRenderer(new LoggingLayoutRenderer&lt;NFeData&gt;(new NFeRenderer()));
/// </code>
/// </remarks>
public sealed class LoggingLayoutRenderer<T> : ILayoutRenderer<T> where T : IPdfData
{
    private readonly ILayoutRenderer<T> _inner;
    private readonly Action<string> _log;

    /// <summary>
    /// Creates a logging decorator around the specified renderer.
    /// </summary>
    /// <param name="inner">The renderer to decorate.</param>
    /// <param name="log">
    /// Optional log sink. Defaults to <see cref="Console.WriteLine(string?)"/>.
    /// Pass any <c>Action&lt;string&gt;</c> for custom logging frameworks.
    /// </param>
    public LoggingLayoutRenderer(ILayoutRenderer<T> inner, Action<string>? log = null)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _log = log ?? Console.WriteLine;
    }

    /// <inheritdoc />
    public void Render(T data, string outputPath)
    {
        _log(
            $"[PdfDocument] Render<{typeof(T).Name}>(\"{Path.GetFileName(outputPath)}\") — started");

        var sw = Stopwatch.StartNew();
        try
        {
            _inner.Render(data, outputPath);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _log(
                $"[PdfDocument] Render<{typeof(T).Name}> FAILED after {sw.ElapsedMilliseconds}ms: {ex.GetType().Name}: {ex.Message}");
            throw;
        }

        sw.Stop();
        _log(
            $"[PdfDocument] Render<{typeof(T).Name}> completed in {sw.ElapsedMilliseconds}ms");
    }
}
