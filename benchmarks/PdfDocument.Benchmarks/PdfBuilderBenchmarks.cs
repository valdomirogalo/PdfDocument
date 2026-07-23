using BenchmarkDotNet.Attributes;
using PdfDocument;
using PdfDocument.NFe;

namespace PdfDocument.Benchmarks;

/// <summary>
/// Benchmarks for complete PDF document generation.
/// </summary>
[MemoryDiagnoser]
[MinColumn, MaxColumn, MeanColumn]
public class PdfBuilderBenchmarks
{
    private readonly NFeRenderer _nfeRenderer = new();
    private string _outputDir = "";

    [IterationSetup]
    public void Setup()
    {
        _outputDir = Path.Combine(Path.GetTempPath(), "pdf_bench_" + Guid.NewGuid().ToString()[..8]);
        Directory.CreateDirectory(_outputDir);
    }

    [IterationCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_outputDir))
            Directory.Delete(_outputDir, true);
    }

    // ── Empty document ────────────────────────────────────────────────

    [Benchmark(Description = "PdfBuilder (empty, 1 page)")]
    public string Build_Empty1Page()
    {
        string path = Path.Combine(_outputDir, "empty.pdf");
        using var pdf = new PdfBuilder();
        pdf.AddPage();
        pdf.Save(path);
        return path;
    }

    // ── Document with text ────────────────────────────────────────────

    [Benchmark(Description = "PdfBuilder (text, 1 page, 100 lines)")]
    public string Build_Text1Page()
    {
        string path = Path.Combine(_outputDir, "text.pdf");
        using var pdf = new PdfBuilder();
        var page = pdf.AddPage();
        var c = page.Canvas;
        for (int i = 0; i < 100; i++)
            c.DrawText($"Line number {i}: The quick brown fox jumps over the lazy dog", 20, 800 - i * 8, 8);
        pdf.Save(path);
        return path;
    }

    // ── Document with shapes ───────────────────────────────────────────

    [Benchmark(Description = "PdfBuilder (shapes, 1 page, 500 rects)")]
    public string Build_Shapes1Page()
    {
        string path = Path.Combine(_outputDir, "shapes.pdf");
        using var pdf = new PdfBuilder();
        var page = pdf.AddPage();
        var c = page.Canvas;
        for (int i = 0; i < 500; i++)
            c.DrawRectangle(i % 500, i % 800, 10, 10);
        pdf.Save(path);
        return path;
    }

    // ── Document with table ───────────────────────────────────────────

    [Benchmark(Description = "PdfBuilder (table, 50x6)")]
    public string Build_Table()
    {
        string path = Path.Combine(_outputDir, "table.pdf");
        using var pdf = new PdfBuilder();
        var page = pdf.AddPage();
        var c = page.Canvas;

        var data = new string[50, 6];
        for (int r = 0; r < 50; r++)
            for (int col = 0; col < 6; col++)
                data[r, col] = $"R{r}C{col}";

        c.DrawTable(data, 20, 700, [50.0, 80, 60, 60, 60, 60], 14);
        pdf.Save(path);
        return path;
    }

    // ── Document with barcode ──────────────────────────────────────────

    [Benchmark(Description = "PdfBuilder (barcode Code39, 5 codes)")]
    public string Build_WithBarcode()
    {
        string path = Path.Combine(_outputDir, "barcode.pdf");
        using var pdf = new PdfBuilder();
        var page = pdf.AddPage();
        var c = page.Canvas;

        for (int i = 0; i < 5; i++)
        {
            var bars = Code39.Generate($"CODE{i}TEST");
            c.DrawBarcode(bars, 20, 700 - i * 70, 1.5, 40);
        }

        pdf.Save(path);
        return path;
    }

    // ── DANFE completo (via NFeRenderer) ───────────────────────────────

    [Benchmark(Description = "PdfBuilder (DANFE via NFeRenderer)")]
    public string Build_Danfe()
    {
        string path = Path.Combine(_outputDir, "danfe.pdf");
        var data = CreateNFeData();
        _nfeRenderer.Render(data, path);
        return path;
    }

    // ── Multi-page ─────────────────────────────────────────────────────

    [Benchmark(Description = "PdfBuilder (10 pages, 50 lines each)")]
    public string Build_MultiPage()
    {
        string path = Path.Combine(_outputDir, "multipage.pdf");
        using var pdf = new PdfBuilder();
        for (int p = 0; p < 10; p++)
        {
            var page = pdf.AddPage();
            var c = page.Canvas;
            for (int i = 0; i < 50; i++)
                c.DrawText($"Page {p + 1}, Line {i}: Lorem ipsum dolor sit amet", 20, 800 - i * 16, 10);
        }
        pdf.Save(path);
        return path;
    }

    private static NFe.NFeData CreateNFeData()
    {
        return new NFe.NFeData
        {
            CUf = "21",
            NatOp = "VENDA MERCADORIA",
            Mod = "55",
            Serie = "889",
            Nnf = "5",
            DhEmi = "2025-09-01T00:00:00-03:00",
            TpNf = "1",
            IdDest = "2",
            CMunFg = "2111300",
            TpAmb = "2",
            FinNfe = "1",
            IndFinal = "1",
            IndPres = "9",
            EmitCnpj = "11111111000111",
            EmitXNome = "EMPRESA EXEMPLO LTDA",
            EmitXFant = "EMPRESA EXEMPLO",
            EmitIe = "111111111",
            EmitCrt = "3",
            EmitXLogr = "AVENIDA EXEMPLO",
            EmitNro = "1000",
            EmitXBairro = "CENTRO",
            EmitCMun = "2111300",
            EmitXMun = "SAO LUIS",
            EmitUf = "MA",
            EmitCep = "00000000",
            DestCnpj = "22222222000122",
            DestXNome = "NF-E EMITIDA EM AMBIENTE DE HOMOLOGACAO",
            DestXLogr = "RUA EXEMPLO",
            DestNro = "s/n",
            DestXBairro = "CENTRO",
            DestCMun = "3304557",
            DestXMun = "RIO DE JANEIRO",
            DestUf = "RJ",
            DestIe = "22222222",
            CProd = "96485451",
            XProd = "PESSEGO BASE CONCENTRADA",
            Ncm = "76071120",
            Cfop = "6101",
            UCom = "KG",
            QCom = "1.0000",
            VUnCom = "2.00",
            VProd = "2.00",
            VBc = "0.00",
            VIcms = "0.00",
            VProdTotal = "2.00",
            VNf = "2.00",
            TransCnpj = "33333333000133",
            TransXNome = "TRANSPORTADORA EXEMPLO LTDA",
            TransIe = "333333333333",
            TransXEnder = "RUA EXEMPLO 100",
            TransXMun = "GUARULHOS",
            TransUf = "SP",
            TPag = "01",
            VPag = "2.00",
        };
    }
}
