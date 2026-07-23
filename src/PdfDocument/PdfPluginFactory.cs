using System.Reflection;

namespace PdfDocument;

/// <summary>
/// Factory that discovers, registers and composes data parsers and layout renderers
/// to generate PDFs from various input formats.
/// </summary>
/// <remarks>
/// Usage — programmatic registration:
/// <code>
/// var factory = new PdfPluginFactory();
/// factory.RegisterParser(new NFeParser());
/// factory.RegisterRenderer(new NFeRenderer());
/// factory.Generate("nota.xml", "danfe.pdf");
/// </code>
///
/// Usage — assembly scanning:
/// <code>
/// var factory = new PdfPluginFactory();
/// factory.LoadPlugins("./plugins");
/// factory.Generate("input.xml", "output.pdf");
/// </code>
/// </remarks>
public sealed class PdfPluginFactory
{
    private readonly List<IDataParser> _parsers = [];
    private readonly Dictionary<Type, ILayoutRenderer> _renderers = [];

    // ── Non-generic wrappers (type-erased for storage) ──────────────────

    private interface IDataParser
    {
        Type DataType { get; }
        IPdfData Parse(string inputPath);
        bool CanParse(string inputPath);
    }

    private interface ILayoutRenderer
    {
        Type DataType { get; }
        void Render(IPdfData data, string outputPath);
    }

    private sealed class DataParserWrapper<T>(IDataParser<T> parser) : IDataParser where T : IPdfData
    {
        public Type DataType => typeof(T);
        public IPdfData Parse(string inputPath) => parser.Parse(inputPath);
        public bool CanParse(string inputPath) => parser.CanParse(inputPath);
    }

    private sealed class LayoutRendererWrapper<T>(ILayoutRenderer<T> renderer) : ILayoutRenderer where T : IPdfData
    {
        public Type DataType => typeof(T);
        public void Render(IPdfData data, string outputPath) => renderer.Render((T)data, outputPath);
    }

    // ── Registration ───────────────────────────────────────────────────

    /// <summary>
    /// Registers a data parser for a specific data type.
    /// </summary>
    public void RegisterParser<T>(IDataParser<T> parser) where T : IPdfData
    {
        ArgumentNullException.ThrowIfNull(parser);
        _parsers.Add(new DataParserWrapper<T>(parser));
    }

    /// <summary>
    /// Registers a layout renderer for a specific data type.
    /// Only one renderer per data type is allowed; subsequent registrations replace the previous one.
    /// </summary>
    public void RegisterRenderer<T>(ILayoutRenderer<T> renderer) where T : IPdfData
    {
        ArgumentNullException.ThrowIfNull(renderer);
        _renderers[typeof(T)] = new LayoutRendererWrapper<T>(renderer);
    }

    // ── Assembly scanning ──────────────────────────────────────────────

    /// <summary>
    /// Scans all .dll files in the given directory and registers any types
    /// implementing <see cref="IDataParser{T}"/> or <see cref="ILayoutRenderer{T}"/>.
    /// </summary>
    /// <param name="directoryPath">Directory containing plugin assemblies.</param>
    /// <exception cref="DirectoryNotFoundException">If the directory does not exist.</exception>
    public void LoadPlugins(string directoryPath)
    {
        ArgumentNullException.ThrowIfNull(directoryPath);

        if (!Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException($"Plugin directory not found: {directoryPath}");

        foreach (string dll in Directory.EnumerateFiles(directoryPath, "*.dll"))
        {
            try
            {
                var assembly = Assembly.LoadFrom(dll);
                RegisterFromAssembly(assembly);
            }
            catch (BadImageFormatException)
            {
                // Skip files that are not .NET assemblies
            }
            catch (FileLoadException)
            {
                // Skip assemblies that fail to load (e.g. mismatched runtime)
            }
        }
    }

    /// <summary>
    /// Scans a single assembly for plugin types and registers them.
    /// </summary>
    public void LoadPlugin(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        RegisterFromAssembly(assembly);
    }

    private void RegisterFromAssembly(Assembly assembly)
    {
        foreach (var type in assembly.GetExportedTypes())
        {
            if (type.IsAbstract || type.IsInterface)
                continue;

            foreach (var iface in type.GetInterfaces())
            {
                if (!iface.IsGenericType)
                    continue;

                var genericDef = iface.GetGenericTypeDefinition();

                if (genericDef == typeof(IDataParser<>))
                {
                    var dataType = iface.GetGenericArguments()[0];
                    var wrapperType = typeof(DataParserWrapper<>).MakeGenericType(dataType);
                    var instance = Activator.CreateInstance(type)
                        ?? throw new InvalidOperationException(
                            $"Could not instantiate parser '{type.FullName}'.");
                    var wrapper = (IDataParser)Activator.CreateInstance(wrapperType, instance)!;
                    _parsers.Add(wrapper);
                }
                else if (genericDef == typeof(ILayoutRenderer<>))
                {
                    var dataType = iface.GetGenericArguments()[0];
                    var wrapperType = typeof(LayoutRendererWrapper<>).MakeGenericType(dataType);
                    var instance = Activator.CreateInstance(type)
                        ?? throw new InvalidOperationException(
                            $"Could not instantiate renderer '{type.FullName}'.");
                    var wrapper = (ILayoutRenderer)Activator.CreateInstance(wrapperType, instance)!;
                    _renderers[dataType] = wrapper;
                }
            }
        }
    }

    // ── Generation ─────────────────────────────────────────────────────

    /// <summary>
    /// Generates a PDF from an input file. The factory automatically discovers
    /// the appropriate parser (via <see cref="IDataParser{T}.CanParse"/>) and
    /// the matching layout renderer for the parsed data type.
    /// </summary>
    /// <param name="inputPath">Path to the input file (XML, JSON, CSV, etc.).</param>
    /// <param name="outputPath">Path where the PDF will be saved.</param>
    /// <exception cref="InvalidOperationException">
    /// If no parser can handle the input, or no renderer is registered for the parsed data type.
    /// </exception>
    public void Generate(string inputPath, string outputPath)
    {
        ArgumentNullException.ThrowIfNull(inputPath);
        ArgumentNullException.ThrowIfNull(outputPath);

        // Find a parser that can handle this input
        foreach (var parser in _parsers)
        {
            if (parser.CanParse(inputPath))
            {
                var data = parser.Parse(inputPath);

                if (_renderers.TryGetValue(parser.DataType, out var renderer))
                {
                    renderer.Render(data, outputPath);
                    return;
                }

                throw new InvalidOperationException(
                    $"No layout renderer registered for data type '{parser.DataType.Name}'. " +
                    "Register one with RegisterRenderer<T>() or ensure the plugin assembly is loaded.");
            }
        }

        throw new InvalidOperationException(
            $"No data parser found that can handle '{Path.GetFileName(inputPath)}'. " +
            "Register one with RegisterParser<T>() or ensure the plugin assembly is loaded.");
    }
}
