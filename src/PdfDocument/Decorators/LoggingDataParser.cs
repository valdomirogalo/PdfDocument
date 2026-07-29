using System.Diagnostics;

namespace PdfDocument.Decorators;

/// <summary>
/// Decorator that adds logging around <see cref="IDataParser{T}"/> operations.
/// Wraps any <see cref="IDataParser{T}"/> and logs CanParse / Parse calls
/// with timing information.
/// </summary>
/// <typeparam name="T">The data model type (must implement <see cref="IPdfData"/>).</typeparam>
/// <remarks>
/// Usage:
/// <code>
/// var factory = new PdfPluginFactory();
/// factory.RegisterParser(new LoggingDataParser&lt;NFeData&gt;(new NFeParser()));
/// </code>
/// </remarks>
public sealed class LoggingDataParser<T> : IDataParser<T> where T : IPdfData
{
    private readonly IDataParser<T> _inner;
    private readonly Action<string> _log;

    /// <summary>
    /// Creates a logging decorator around the specified parser.
    /// </summary>
    /// <param name="inner">The parser to decorate.</param>
    /// <param name="log">
    /// Optional log sink. Defaults to <see cref="Console.WriteLine(string?)"/>.
    /// Pass any <c>Action&lt;string&gt;</c> for custom logging frameworks.
    /// </param>
    public LoggingDataParser(IDataParser<T> inner, Action<string>? log = null)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _log = log ?? Console.WriteLine;
    }

    /// <inheritdoc />
    public bool CanParse(string inputPath)
    {
        bool can = _inner.CanParse(inputPath);
        _log(
            $"[PdfDocument] CanParse<{typeof(T).Name}>(\"{Path.GetFileName(inputPath)}\") = {can}");
        return can;
    }

    /// <inheritdoc />
    public T Parse(string inputPath)
    {
        _log(
            $"[PdfDocument] Parse<{typeof(T).Name}>(\"{Path.GetFileName(inputPath)}\") — started");

        var sw = Stopwatch.StartNew();
        T result;
        try
        {
            result = _inner.Parse(inputPath);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _log(
                $"[PdfDocument] Parse<{typeof(T).Name}> FAILED after {sw.ElapsedMilliseconds}ms: {ex.GetType().Name}: {ex.Message}");
            throw;
        }

        sw.Stop();
        _log(
            $"[PdfDocument] Parse<{typeof(T).Name}> completed in {sw.ElapsedMilliseconds}ms");

        return result;
    }
}
