#pragma warning disable CA1822 // BenchmarkDotNet requires instance methods
using BenchmarkDotNet.Attributes;
using PdfDocument;

namespace PdfDocument.Benchmarks;

/// <summary>
/// Benchmarks for barcode generation EAN-13.
/// </summary>
[MemoryDiagnoser]
[MinColumn, MaxColumn, MeanColumn]
public class Ean13Benchmarks
{
    [Benchmark(Description = "EAN13.Generate (12 digits, auto check)")]
    public List<Bar> Generate_12Digits()
    {
        return EAN13.Generate("789123456789");
    }

    [Benchmark(Description = "EAN13.Generate (13 digits, validated)")]
    public List<Bar> Generate_13Digits()
    {
        return EAN13.Generate("7891234567895");
    }

    [Benchmark(Description = "EAN13.Generate (with hyphens)")]
    public List<Bar> Generate_WithHyphens()
    {
        return EAN13.Generate("789-123-456-789");
    }

    [Benchmark(Description = "EAN13.Generate (with spaces)")]
    public List<Bar> Generate_WithSpaces()
    {
        return EAN13.Generate("789 123 456 789");
    }

    // ── Various first digits to test pattern table ─────────

    [Benchmark(Description = "EAN13.Generate (first digit 0)")]
    public List<Bar> Generate_FirstDigit0()
    {
        return EAN13.Generate("0123456789012");
    }

    [Benchmark(Description = "EAN13.Generate (first digit 9)")]
    public List<Bar> Generate_FirstDigit9()
    {
        return EAN13.Generate("9987654321096");
    }
}
