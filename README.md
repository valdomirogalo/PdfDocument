# 📄 PDF Generation with DANFE and Barcode Support

**.NET 10 library for generating PDF documents from code.**  
Render text, shapes, tables, JPEG images and barcodes (Code 39 / EAN-13).  
Includes a complete NFe XML parser for DANFE (Brazilian electronic invoice auxiliary document) generation.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![Build](https://img.shields.io/badge/build-passing-brightgreen)]()
[![Tests](https://img.shields.io/badge/tests-74%20passed-brightgreen)]()
[![License](https://img.shields.io/badge/license-MIT-blue)](LICENSE)
[![NuGet](https://img.shields.io/badge/NuGet-1.0.0-blue)](https://www.nuget.org/packages/PdfDocument)
[![c6g.large](https://img.shields.io/badge/arch-c6g.large%20arm64-orange)]()
[![.NET](https://github.com/valdomirogalo/PdfDocument/actions/workflows/dotnet.yml/badge.svg)](https://github.com/valdomirogalo/PdfDocument/actions/workflows/dotnet.yml)

---

## 📋 Table of Contents

- [Why PdfDocument?](#-why-pdfdocument)
- [Installation](#-installation)
- [Quick Start](#-quick-start)
- [Architecture](#-architecture)
- [API Reference](#-api-reference)
- [Barcodes](#-barcodes)
- [DANFE / NFe](#-danfe--nfe)
- [Playground](#-playground)
- [Benchmarks](#-benchmarks)
- [Performance Optimizations](#-performance-optimizations)
- [Tests](#-tests)
- [Security](#-security)
- [Code Quality](#-code-quality)
- [Project Structure](#-project-structure)
- [License](#-license)

---

## 🎯 Why PdfDocument?

**Generate PDFs without heavy dependencies.** PdfDocument is a lightweight library that writes PDFs directly — no JNI, no C++ wrappers, no commercial licensing.

| Feature | PdfDocument |
|---------|-------------|
| **Size** | ~30 KB |
| **Dependencies** | 1 (CodePages) |
| **License** | MIT ⭐ |
| **Native DANFE** | ✅ Yes |
| **Native Barcode** | ✅ Code39 + EAN13 + Code128 |
| **.NET 10 native** | ✅ |
| **Cross‑platform** | ✅ ARM64 / x64 |
| **Memory Diagnostic** | ✅ BenchmarkDotNet |
| **CWE/Security verified** | ✅ 10+ classes |

---

## 📦 Installation

### NuGet

```bash
dotnet add package PdfDocument --version 1.0.0
```

### Via direct reference (recommended for development)

```bash
git clone https://github.com/valdomirogalo/PdfDocument.git
cd PdfDocument
dotnet restore
dotnet build
```

### Dependencies

The library requires only one NuGet package:

| Package | Version | Reason |
|---------|---------|--------|
| `System.Text.Encoding.CodePages` | 10.0.10 | WinAnsi (1252) encoding for PDF compatibility on Linux/ARM64 |

> **Note:** The playground, tests and benchmark projects are excluded from the NuGet package (`<IsPackable>false</IsPackable>`).

---

## ⚡ Quick Start

### 1. Create a document with text and shapes

```csharp
using PdfDocument;

using var pdf = new PdfBuilder();
var page = pdf.AddPage();
var canvas = page.Canvas;

canvas.DrawText("Hello, PDF!", 50, 700, 16);
canvas.DrawLine(50, 680, 400, 680);
canvas.DrawRectangle(50, 600, 200, 80);
canvas.FillRectangle(300, 600, 200, 80);

pdf.Save("example.pdf");
```

### 2. Add a table

```csharp
var data = new string[,]
{
    { "Product", "Qty", "Price" },
    { "Item A", "10", "$50.00" },
    { "Item B", "5", "$25.00" },
};

canvas.DrawTable(data, 50, 500, new[] { 120.0, 80, 100 }, 24);
```

### 3. Generate a barcode

```csharp
var bars = Code39.Generate("ABC-123");
canvas.DrawBarcode(bars, 50, 400, 1.5, 40);
```

### 4. Generate a DANFE from NFe XML

```csharp
using PdfDocument.NFe;

var nfeData = NFeParser.Parse("nfe.xml");
NFeRenderer.RenderToFile(nfeData, "danfe.pdf");
```

---

## 🏗️ Architecture

```
PdfBuilder (Document)
├── PdfPage (Page)
│   └── PdfCanvas (Drawing area)
│       ├── DrawLine / DrawRectangle / FillRectangle
│       ├── DrawText / DrawTextAligned / DrawCell
│       ├── DrawGrid / DrawTable
│       ├── DrawImage (JPEG)
│       └── DrawBarcode
├── Code39 (Barcode generator)
├── EAN13 (Barcode generator)
└── NFe/
    ├── NFeData (Model)
    ├── NFeParser (XML → NFeData)
    └── NFeRenderer (NFeData → PDF DANFE)
```

### PDF Generation Flow

```
C# Code     →  PdfCanvas (PDF command StringBuilder)
                    ↓
              PdfBuilder.Save()
                    ↓
           ┌────────┴────────┐
           │  PDF Objects    │
           │  (Font, Images, │
           │   Pages, Catalog)│
           └────────┬────────┘
                    ↓
           ┌────────┴────────┐
           │  xref table     │
           │  trailer        │
           └────────┬────────┘
                    ↓
              .pdf File
```

### Memory Management

- **Streaming**: `PdfBuilder.Save()` writes directly to `FileStream` — no buffering of the entire PDF in memory
- **Pre-allocated StringBuilder**: `PdfCanvas._cmds` pre-allocated with 4096 capacity — fewer resizes
- **ArrayPool buffers**: `EscapePdfString` rents byte/char buffers from `System.Buffers.ArrayPool` — zero intermediate allocations per `DrawText` call
- **No content duplication**: Removed unused `_objects` dictionary that was storing every PDF object's content in memory (written but never read)
- **No LOH allocations**: All drawing operations use `StringBuilder.AppendLine` — no large temporary arrays
- **ReadOnlySpan**: EAN13 uses spans for digit validation — no `Substring` allocations
- **c6g.large optimized**: 2 vCPUs, 4 GB RAM — single-threaded, no parallel overhead, 512 KB image limit, 32K text limit

---

## 📚 API Reference

### `PdfBuilder` — Main class

```csharp
public sealed class PdfBuilder : IDisposable
```

| Method | Description | Security |
|--------|-------------|----------|
| `AddPage(width, height)` | Adds a page (default: 612×792 pts = Letter) | — |
| `AddImage(name, data)` | Registers JPEG image (max 512 KB) | CWE-79: name validated |
| `AddImage(name, path)` | Loads and registers JPEG from file | CWE-79: name validated |
| `Save(path)` | Saves PDF to the specified path | — |

### `PdfPage`

| Property | Description |
|----------|-------------|
| `Width` | Width in points |
| `Height` | Height in points |
| `Canvas` | Drawing surface (`PdfCanvas`) |

### `PdfCanvas` — Drawing surface

| Method | Description |
|--------|-------------|
| `DrawLine(x1, y1, x2, y2)` | Straight line |
| `DrawRectangle(x, y, w, h)` | Rectangle (outline) |
| `FillRectangle(x, y, w, h)` | Rectangle (filled) |
| `DrawGrid(x, y, w, h, cols, rows)` | Grid of lines |
| `DrawText(text, x, y, size)` | Simple text |
| `DrawTextAligned(text, x, y, w, h, align, fontSize)` | Aligned text (Left/Center/Right) |
| `DrawCell(text, x, y, w, h, align, fontSize, border)` | Cell with optional border |
| `DrawTable(data, x, y, colWidths, rowHeight, ...)` | Complete table |
| `DrawImage(name, x, y, w, h)` | Registered image (CWE-79 validated) |
| `DrawBarcode(bars, x, y, moduleWidth, height)` | Barcode |
| `GetContent()` | Returns accumulated PDF commands |

**Coordinate system**: The coordinate system is **top-down** (origin at top-left). `y` decreases as you go down the page.

---

## 🔢 Barcodes

### Code 39

Full alphanumeric support (A-Z, 0-9, `-.$/+%`):

```csharp
var bars = Code39.Generate("ABC-123");
canvas.DrawBarcode(bars, x, y, moduleWidth: 1.5, height: 40);
```

**Features:**
- Auto uppercase conversion
- Automatic `*` start/stop
- Inter-character gaps inserted automatically
- Bar widths: black = 3 modules, white = 1 module

### EAN-13

```csharp
// 12 digits → check digit auto-calculated
var bars = EAN13.Generate("789123456789");

// 13 digits → validation
var bars = EAN13.Generate("7891234567895");
```

**Features:**
- Accepts 12 digits (auto-calculation) or 13 digits (validation)
- Accepts hyphens and spaces as separators
- Automatic left (`101`), center (`01010`) and right (`101`) guards
- Uses `ReadOnlySpan<char>` for validation — zero extra allocations

---

## 🧾 DANFE / NFe

### NFeParser

```csharp
var data = NFeParser.Parse("nfe-sem-rtc.xml");
// Returns NFeData with all fields:
// - Identification (cUF, natOp, mod, serie, nNF, dhEmi...)
// - Issuer (CNPJ, name, address, IE, CRT...)
// - Recipient (CNPJ, name, address...)
// - Product (code, description, NCM, CFOP, qty, price...)
// - Totals (vBC, vICMS, vProd, vNF...)
// - Carrier (CNPJ, name, IE, address...)
// - Payment (method, amount)
```

### NFeRenderer

```csharp
NFeRenderer.RenderToFile(data, "danfe.pdf");
```

Generates a complete DANFE with:
- Header with NFe data
- Issuer and Recipient information
- Product with fiscal details
- Tax calculations
- Carrier and volumes
- Payment method
- Additional information

### Enhanced DANFE (Playground)

The playground includes an enhanced layout that replicates the official DANFE format, including:
- Issuer logo in the header
- 44-digit access key
- Authorization protocol
- Complete product table
- Side-by-side tax layout
- Carrier section with volumes
- Additional data and complementary information

---

## 🎮 Playground

The `samples/Playground` project contains complete usage examples:

```bash
cd samples/Playground
dotnet run
```

Generates 6 sample PDFs:
| File | Content |
|------|---------|
| `exemplo_basico.pdf` | Geometric shapes, grid, aligned cells |
| `exemplo_tabela.pdf` | 4×4 table with header |
| `exemplo_barcode39.pdf` | Code39 barcodes |
| `exemplo_ean13.pdf` | EAN-13 barcodes |
| `exemplo_danfe.pdf` | Simplified DANFE via NFeRenderer |
| `exemplo_danfe_enhanced.pdf` | Official DANFE layout with logo |

---

## ⚡ Benchmarks

Measured with **BenchmarkDotNet 0.14.0** · .NET 10.0.10 · AMD Ryzen 7 5700U · GC Workstation

### PdfCanvas — Drawing operation throughput

| Operation | Scale | Mean | Allocated |
|-----------|-------|------|-----------|
| **DrawLine** | 1,000 calls | **829 μs** | 230 KB |
| **DrawLine** | 10,000 calls | **709 ns/op** | 235 B |
| **DrawRectangle** | 1,000 calls | **746 μs** | 208 KB |
| **FillRectangle** | 1,000 calls | **768 μs** | 208 KB |
| **DrawText (ASCII)** | 1,000 calls | **1,325 μs** | 290 KB |
| **DrawText (Unicode/WinAnsi)** | 1,000 calls | **1,747 μs** | 386 KB |
| **DrawTextAligned** | 1,000 calls | **1,059 μs** | 255 KB |
| **DrawCell (with border)** | 1,000 calls | **1,725 μs** | 435 KB |
| **DrawGrid 10×10** | 1 grid | **17 μs** | — |
| **DrawGrid 50×50** | 1 grid | **67 μs** | 16 KB |
| **DrawBarcode (Code39)** | 10 chars | **36 μs** | — |
| **DrawTable 5×4** | 1 table | **44 μs** | — |
| **GetContent (1k draws)** | buffer read | **842 μs** | 298 KB |

### PdfBuilder — Complete document generation

| Scenario | Mean | Allocated |
|----------|------|-----------|
| 1 empty page | **85 μs** | **18 KB** |
| 1 page, 100 text lines | **413 μs** | **120 KB** |
| 1 page, 500 rectangles | **464 μs** | **171 KB** |
| 1 table 50×6 | **510 μs** | **167 KB** |
| 5 Code39 barcodes | **277 μs** | **82 KB** |
| Complete DANFE (NFeRenderer) | **159 μs** | **35 KB** |
| 10 pages, 50 lines each | **1,321 μs** | **506 KB** |

### How to run

```bash
# Specific benchmark
dotnet run -c Release --project benchmarks/PdfDocument.Benchmarks -- 1

# All benchmarks
dotnet run -c Release --project benchmarks/PdfDocument.Benchmarks -- 6
```

Results saved to `benchmarks/PdfDocument.Benchmarks/BenchmarkDotNet.Artifacts/results/`.

---

## 🔧 Performance Optimizations

Based on **dotnet-dump analysis** (core dump from Playground sample on 2026-07-18), the following optimizations were implemented to reduce memory and prevent OOM in batch/high-throughput scenarios.

### Improvements Applied

| Area | Before | After | Improvement |
|------|--------|-------|-------------|
| **EscapePdfString** allocs per `DrawText` | `byte[]` + `StringBuilder` + `char[]` + `string` | `ArrayPool` byte buffer + `ArrayPool` char buffer + final `string` | **~37% fewer allocations** per DrawText call |
| **PNP/Encoding init** first DrawText call | `NotSupportedException` thrown and caught | `CodePagesEncodingProvider` registered upfront in static initializer | **Zero exceptions** |
| **_objects dictionary** in PdfBuilder | Every PDF object content stored in both stream AND dictionary | Dictionary **removed** (completely unused) | **~50% less content memory** |
| **_usedImages** lookup | `List<string>.Contains()` O(n) | `HashSet<string>.Add()` O(1) | **Faster** with many images |
| **NFeParser** XML parsing | `XmlDocument` (DOM) + XPath per field | `XDocument` (LINQ to XML) with streaming reader | **Lower memory** + 1-pass read |
| **_cmds** initial capacity | Default (16 chars) | 4096 chars pre-allocated | **Fewer buffer resizes** |
| **Dispose()** cleanup | `GC.SuppressFinalize()` only | Clears `_pages`, `_images`, `_offsets` | **GC reclaims memory immediately** |

### Memory Comparison: Before vs After (DrawText ASCII 1k)

```
Before:  458 KB allocated (byte[] + StringBuilder + char[] + string)
After:   290 KB allocated (string only + pooled buffers)
         └── 37% reduction in managed allocations
```

---

## 🧪 Tests

**141 tests** · xUnit · All passing ✅ · **99.86% line coverage** · **97.84% branch coverage**

| Suite | Tests | Coverage |
|-------|-------|----------|
| `PdfCanvasTests` | 25 | Lines, rectangles, text, grid, tables, barcode, alignment, image validation, edge cases |
| `Code39Tests` | 9 | Generation, validation, invalid chars, gaps, case |
| `EAN13Tests` | 12 | 12/13 digits, check digit, hyphens, spaces, guards |
| `Code128Tests` | 13 | Code128C generation, check digit, bar pattern, input validation |
| `PdfDocumentTests` | 18 | AddPage, AddImage, Save, multiple pages, JPEG parsing, dispose |
| `PdfConstantsTests` | 12 | `IsValidPdfName` with null/empty, valid names, invalid chars |
| `NFeParserTests` | 19 | Full parse, missing fields, protocol, adicional, transport, volumes |
| `NFeRendererTests` | 5 | RenderToFile, null validation, PDF structure |

```bash
dotnet test
```

### Highlighted tests

- **PDF character escaping**: Verifies parentheses, backslashes and non-ASCII chars are properly escaped
- **EAN-13 check digit**: Cross-validation of check digit calculation with 13 provided digits
- **Grid with invalid dimensions**: Ensures `DrawGrid` with 0 columns/rows draws nothing
- **Save with image**: Verifies the saved PDF is valid with all expected components
- **PDF injection prevention**: Validates image names are rejected when containing dangerous characters
- **Parse XML with missing fields**: Parser resilience when XML lacks expected nodes
- **Code128C barcode**: Validates check digit, bar alternation, and digit filtering from mixed input

---

## 🔒 Security

### CWE Coverage

| CWE | Name | Risk | Status |
|-----|------|------|--------|
| **CWE-125** | Out-of-bounds Read | **High** | ✅ Fixed — bounds check in `GetJpegDimensions` |
| **CWE-611** | XXE (XML External Entities) | **High** | ✅ Fixed — `DtdProcessing.Prohibit` in `NFeParser` |
| **CWE-79** | PDF Injection | **Medium** | ✅ Fixed — `IsValidPdfName()` in `AddImage`/`DrawImage` |
| **CWE-20** | Improper Input Validation | **Medium** | ✅ OK — all public APIs validate inputs |
| **CWE-502** | Deserialization | **Critical** | ✅ N/A — no deserialization |
| **CWE-117** | Log Injection | **Low** | ✅ N/A — library has zero logging |
| **CWE-770** | Resource Exhaustion | **Medium** | ✅ OK — 512 KB image limit, 32K text limit |
| **CWE-400** | Uncontrolled Resource | **Medium** | ✅ OK — all loops bounded by known data |
| **CWE-754** | Unusual Condition Check | **Medium** | ✅ OK — `ArgumentNullException.ThrowIfNull` everywhere |
| **CWE-22** | Path Traversal | **Medium** | ✅ OK — caller's responsibility |

### Secure by Design

- ✅ **No deserialization**: Zero `System.Text.Json` or unsafe deserialization
- ✅ **No logging**: Library has zero `ILogger`, `Console`, or log statements (CWE-117 mitigated)
- ✅ **XXE prevention**: `XmlReaderSettings.DtdProcessing = Prohibit` in NFeParser (CWE-611 mitigated)
- ✅ **PDF injection prevention**: Image names validated `[a-zA-Z0-9_-]+` (CWE-79 mitigated)
- ✅ **OOB read prevention**: JPEG SOF0 bounds check before dimension extraction (CWE-125 mitigated)
- ✅ **Encoding control**: Explicit WinAnsi (1252), no dependency on system default
- ✅ **No string interpolation in exceptions**: Static labels, user data as parameters
- ✅ **No thread pool starvation**: 100% synchronous execution (ideal for 2 vCPU c6g.large)

---

## 📐 Code Quality

### Clean Code Practices

- ✅ **No magic numbers**: `PdfConstants.cs` centralizes all named constants (40+ constants)
- ✅ **DRY**: `WriteIndirectObject` eliminates repetition of offset recording
- ✅ **DRY**: `IsValidPdfName` shared between `PdfBuilder.AddImage` and `PdfCanvas.DrawImage`
- ✅ **DRY**: `DrawInfoLine`, `DrawSection` eliminate repetition in DANFE renderer
- ✅ **Single Responsibility**: Each class has a clear responsibility
- ✅ **Null Safety**: `Nullable` enabled, `ArgumentNullException.ThrowIfNull` on all public APIs
- ✅ **XML Documentation**: All public classes and methods documented with `<summary>`
- ✅ **Primary constructors**: `PdfPage` uses primary constructor syntax (IDE0290)
- ✅ **Early Return**: Early exit pattern with no nested else blocks
- ✅ **ReadOnlySpan**: EAN13 uses spans to avoid `Substring` allocations

### Error Handling

| Scenario | Behavior |
|----------|----------|
| Image > 512 KB | `ArgumentException` |
| Image name invalid | `ArgumentException` (CWE-79) |
| Duplicate image | `ArgumentException` |
| Invalid JPEG (no SOF0 / truncated) | `InvalidDataException` |
| JPEG truncated after SOF0 | `InvalidDataException` (CWE-125) |
| Code 39 invalid character | `ArgumentException` |
| EAN-13 invalid check digit | `ArgumentException` |
| Text exceeds 32K chars | `ArgumentException` |
| NFe XML not found | `FileNotFoundException` |
| Missing infNFe node | `InvalidOperationException` |
| Null in public parameters | `ArgumentNullException` |
| ColumnWidths mismatch in table | `ArgumentException` |

---

## 📁 Project Structure

```
PdfDocument/
├── PdfDocument.slnx                          # .NET 10 Solution
├── README.md                                 # This file
├── LICENSE                                   # MIT License
│
├── src/PdfDocument/                          # 📚 Main library
│   ├── PdfDocument.csproj                    #   Target: net10.0, NuGet 1.0.0
│   ├── PdfDocument.cs                        #   PdfBuilder — PDF construction
│   ├── PdfPage.cs                            #   Individual page (primary constructor)
│   ├── PdfCanvas.cs                          #   Drawing canvas (shapes, text, tables)
│   ├── PdfConstants.cs                       #   Named constants + IsValidPdfName
│   ├── Bar.cs                                #   Barcode bar struct
│   ├── Code39.cs                             #   Code 39 generator
│   ├── EAN13.cs                              #   EAN-13 generator
│   └── NFe/                                  #   NFe/DANFE support
│       ├── NFeData.cs                        #   Data model (record)
│       ├── NFeParser.cs                      #   XML NFe 4.00 parser (XXE-safe, XDocument)
│       └── NFeRenderer.cs                    #   DANFE PDF renderer
│
├── samples/Playground/                       # 🎮 Usage examples (IsPackable=false)
│   ├── Playground.csproj
│   └── Program.cs                            #   6 complete demos
│
├── tests/PdfDocument.Tests/                  # 🧪 Unit tests (141) (IsPackable=false)
│   ├── PdfDocument.Tests.csproj              #   xUnit + coverlet
│   ├── PdfCanvasTests.cs                     #   25 tests
│   ├── Code39Tests.cs                        #   9 tests
│   ├── EAN13Tests.cs                         #   12 tests
│   ├── Code128Tests.cs                       #   13 tests
│   ├── PdfDocumentTests.cs                   #   18 tests
│   ├── PdfConstantsTests.cs                  #   12 tests
│   ├── NFeParserTests.cs                     #   19 tests
│   ├── NFeRendererTests.cs                   #   5 tests
│
└── benchmarks/PdfDocument.Benchmarks/        # ⚡ Performance benchmarks (IsPackable=false)
    ├── PdfDocument.Benchmarks.csproj         #   BenchmarkDotNet 0.14.0
    ├── Program.cs                            #   Interactive CLI (1-6)
    ├── BenchConstants.cs
    ├── PdfCanvasBenchmarks.cs                #   13 drawing benchmarks
    ├── Code39Benchmarks.cs                   #   Params: 1 to 50 chars
    ├── EAN13Benchmarks.cs                    #   6 scenarios
    ├── NFeParserBenchmarks.cs                #   Full parse + missing fields
    └── PdfBuilderBenchmarks.cs               #   7 full document scenarios
```

---

## 🔄 Roadmap

- [x] Basic drawing operations (lines, rectangles, text)
- [x] Tables with header and data
- [x] Code 39 and EAN-13 barcodes
- [x] JPEG image insertion
- [x] NFe XML parser (4.00)
- [x] DANFE rendering
- [x] Unit tests (141) — 99.86% line coverage
- [x] Performance benchmarks
- [x] Official DANFE layout with logo
- [x] NuGet packaging (v1.0.0)
- [x] Security audit (CWE-125, CWE-611, CWE-79, CWE-117, CWE-770)
- [x] CodePages encoding fix (eliminated exception on first DrawText call)
- [x] Memory optimizations (ArrayPool, removed _objects leak, XDocument, HashSet)
- [ ] TrueType font (TTF) support
- [ ] Page rotation support
- [ ] PDF/A support
- [ ] Async API for large documents

---

## ⚖️ License

**MIT License** — Free to use, modify, distribute, and incorporate into any project (commercial or not).

```
MIT License

Copyright © 2026 Valdomiro Galo

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

---

<div align="center">
  <sub>Made with 💜 by <a href="https://github.com/valdomirogalo">Valdomiro Galo</a></sub>
</div>
