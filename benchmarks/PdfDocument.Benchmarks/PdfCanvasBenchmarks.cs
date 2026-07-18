using BenchmarkDotNet.Attributes;
using PdfDocument;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;

namespace PdfDocument.Benchmarks;

/// <summary>
/// Benchmarks for PdfCanvas operations: throughput and memory allocation.
/// </summary>
[MemoryDiagnoser]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public class PdfCanvasBenchmarks
{
    private PdfCanvas _canvas = null!;

    [IterationSetup]
    public void Setup()
    {
        _canvas = new PdfCanvas();
    }

    // ── DrawLine ───────────────────────────────────────────────────────

    [Benchmark(Description = "DrawLine 1k calls")]
    public void DrawLine_1000()
    {
        for (int i = 0; i < BenchConstants.OperationsPerInvoke; i++)
            _canvas.DrawLine(i, i + 1, i + 10, i + 20);
    }

    [Benchmark(Description = "DrawLine 10k calls", OperationsPerInvoke = 10_000)]
    public void DrawLine_10000()
    {
        for (int i = 0; i < 10_000; i++)
            _canvas.DrawLine(i, i + 1, i + 10, i + 20);
    }

    // ── DrawRectangle ──────────────────────────────────────────────────

    [Benchmark(Description = "DrawRectangle 1k calls")]
    public void DrawRectangle_1000()
    {
        for (int i = 0; i < BenchConstants.OperationsPerInvoke; i++)
            _canvas.DrawRectangle(i, i, 100, 50);
    }

    // ── FillRectangle ──────────────────────────────────────────────────

    [Benchmark(Description = "FillRectangle 1k calls")]
    public void FillRectangle_1000()
    {
        for (int i = 0; i < BenchConstants.OperationsPerInvoke; i++)
            _canvas.FillRectangle(i, i, 100, 50);
    }

    // ── DrawText ───────────────────────────────────────────────────────

    [Benchmark(Description = "DrawText (ASCII) 1k calls")]
    public void DrawText_Ascii_1000()
    {
        for (int i = 0; i < BenchConstants.OperationsPerInvoke; i++)
            _canvas.DrawText("Hello World ABC 123", 10, 100);
    }

    [Benchmark(Description = "DrawText (UTF8/WinAnsi) 1k calls")]
    public void DrawText_Unicode_1000()
    {
        for (int i = 0; i < BenchConstants.OperationsPerInvoke; i++)
            _canvas.DrawText("Preço R$ 1.234,56 — Café & Cia ®", 10, 100);
    }

    // ── DrawTextAligned ────────────────────────────────────────────────

    [Benchmark(Description = "DrawTextAligned 1k calls")]
    public void DrawTextAligned_1000()
    {
        for (int i = 0; i < BenchConstants.OperationsPerInvoke; i++)
            _canvas.DrawTextAligned("Aligned", i % 500, i % 700, 100, 20,
                PdfCanvas.TextAlign.Center, 10);
    }

    // ── DrawCell (with border) ─────────────────────────────────────────

    [Benchmark(Description = "DrawCell 1k calls (with border)")]
    public void DrawCell_WithBorder_1000()
    {
        for (int i = 0; i < BenchConstants.OperationsPerInvoke; i++)
            _canvas.DrawCell("Cell", i % 500, i % 700, 80, 20,
                PdfCanvas.TextAlign.Left, 10, true);
    }

    // ── DrawGrid ───────────────────────────────────────────────────────

    [Benchmark(Description = "DrawGrid 10x10")]
    public void DrawGrid_10x10()
    {
        _canvas.DrawGrid(0, 0, 500, 500, 10, 10);
    }

    [Benchmark(Description = "DrawGrid 50x50")]
    public void DrawGrid_50x50()
    {
        _canvas.DrawGrid(0, 0, 500, 500, 50, 50);
    }

    // ── DrawBarcode ────────────────────────────────────────────────────

    [Benchmark(Description = "DrawBarcode (Code39, 10 chars)")]
    public void DrawBarcode_Code39_10chars()
    {
        var bars = Code39.Generate("ABC123DEF4");
        _canvas.DrawBarcode(bars, 10, 10, 1.5, 40);
    }

    // ── DrawTable ──────────────────────────────────────────────────────

    [Benchmark(Description = "DrawTable 5x4")]
    public void DrawTable_5x4()
    {
        var data = new string[,]
        {
            { "Prod", "Qtde", "Vlr Unit", "Total" },
            { "A", "10", "5,00", "50,00" },
            { "B", "5", "12,00", "60,00" },
            { "C", "20", "3,00", "60,00" },
            { "D", "2", "100,00", "200,00" },
        };
        _canvas.DrawTable(data, 10, 100, [80.0, 60, 80, 80], 20);
    }

    // ── GetContent (leitura do buffer) ─────────────────────────────────

    [Benchmark(Description = "GetContent after 1k DrawLine")]
    public void GetContent_AfterDraws()
    {
        for (int i = 0; i < BenchConstants.OperationsPerInvoke; i++)
            _canvas.DrawLine(i, i + 1, i + 10, i + 20);
        _ = _canvas.GetContent();
    }
}
