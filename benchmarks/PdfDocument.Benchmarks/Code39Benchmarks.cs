#pragma warning disable CA1822 // BenchmarkDotNet requires instance methods
using BenchmarkDotNet.Attributes;
using PdfDocument;

namespace PdfDocument.Benchmarks;

/// <summary>
/// Benchmarks for barcode generation Code39.
/// </summary>
[MemoryDiagnoser]
[MinColumn, MaxColumn, MeanColumn]
public class Code39Benchmarks
{
    [Params(1, 5, 10, 20, 50)]
    public int TextLength { get; set; }

    private string _text = "";

    [IterationSetup]
    public void Setup()
    {
        _text = new string('A', TextLength);
    }

    [Benchmark(Description = "Code39.Generate")]
    public List<Bar> Generate()
    {
        return Code39.Generate(_text);
    }

    // ── Mixed characters ───────────────────────────────────────────────

    [Benchmark(Description = "Code39.Generate (10 mixed chars)", OperationsPerInvoke = 100)]
    public static List<Bar> Generate_Mixed()
    {
        return Code39.Generate("ABC-123/XYZ");
    }

    // ── With validation (caractere inválido é exceção, não medimos) ───────

    [Benchmark(Description = "Code39.Generate (50 alphanumeric)")]
    public static List<Bar> Generate_Long()
    {
        return Code39.Generate("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-.$/+%");
    }
}
