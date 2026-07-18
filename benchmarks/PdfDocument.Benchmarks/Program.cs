using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using PdfDocument.Benchmarks;

Console.WriteLine("═══════════════════════════════════════════════");
Console.WriteLine("  PdfDocument - Performance Benchmarks");
Console.WriteLine("═══════════════════════════════════════════════");
Console.WriteLine();
Console.WriteLine("Available benchmarks:");
Console.WriteLine("  1 - PdfCanvas (drawing operations)");
Console.WriteLine("  2 - Code39 (barcode generation)");
Console.WriteLine("  3 - EAN-13 (barcode generation)");
Console.WriteLine("  4 - NFeParser (XML parsing)");
Console.WriteLine("  5 - PdfBuilder (full document generation)");
Console.WriteLine("  6 - All (run all benchmarks)");
Console.WriteLine();

int choice = 0;
if (args.Length > 0 && int.TryParse(args[0], out int parsed))
    choice = parsed;

var config = DefaultConfig.Instance
    .WithOptions(ConfigOptions.DisableOptimizationsValidator)
    .AddDiagnoser(MemoryDiagnoser.Default);

if (choice == 0)
{
    Console.Write("Choose an option (1-6, default=6): ");
    string? input = Console.ReadLine();
    choice = int.TryParse(input, out parsed) ? parsed : 6;
}

Console.WriteLine();

try
{
    switch (choice)
    {
        case 1:
            BenchmarkRunner.Run<PdfCanvasBenchmarks>(config);
            break;
        case 2:
            BenchmarkRunner.Run<Code39Benchmarks>(config);
            break;
        case 3:
            BenchmarkRunner.Run<Ean13Benchmarks>(config);
            break;
        case 4:
            BenchmarkRunner.Run<NFeParserBenchmarks>(config);
            break;
        case 5:
            BenchmarkRunner.Run<PdfBuilderBenchmarks>(config);
            break;
        case 6:
            BenchmarkRunner.Run<PdfCanvasBenchmarks>(config);
            BenchmarkRunner.Run<Code39Benchmarks>(config);
            BenchmarkRunner.Run<Ean13Benchmarks>(config);
            BenchmarkRunner.Run<NFeParserBenchmarks>(config);
            BenchmarkRunner.Run<PdfBuilderBenchmarks>(config);
            break;
        default:
            Console.WriteLine("Invalid option. Use 1-6.");
            return 1;
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error running benchmarks: {ex.Message}");
    Console.WriteLine("Tip: Run in Release configuration:");
    Console.WriteLine("  dotnet run -c Release --project benchmarks/PdfDocument.Benchmarks");
    return 1;
}

Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════════");
Console.WriteLine("  Benchmarks completed!");
Console.WriteLine("  Results saved to:");
Console.WriteLine($"    ./benchmarks/PdfDocument.Benchmarks/BenchmarkDotNet.Artifacts/results");
Console.WriteLine("═══════════════════════════════════════════════");
return 0;
