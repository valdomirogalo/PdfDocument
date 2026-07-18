using PdfDocument;
using PdfDocument.NFe;

// ─────────────────────────────────────────────────────────────────────────
//  PLAYGROUND - PdfDocument library usage examples
// ─────────────────────────────────────────────────────────────────────────
//  Prerequisite: have an "nfe-sem-rtc.xml" file in the execution directory
//  or adjust the path below.
// ─────────────────────────────────────────────────────────────────────────

string? xmlFile = args.Length > 0 ? args[0] : "nfe-sem-rtc.xml";

Console.WriteLine("═══ PdfDocument Playground ═══");
Console.WriteLine();

// ─────────────────────────────────────────────────────────────────────────
//  1. BASIC EXAMPLE: Texto, formas, grid
// ─────────────────────────────────────────────────────────────────────────
Console.WriteLine("1. Gerando exemplo básico (formas e texto)...");
GenerateBasicExample("exemplo_basico.pdf");
Console.WriteLine("   ✅ exemplo_basico.pdf created");

// ─────────────────────────────────────────────────────────────────────────
//  2. TABLE EXAMPLE
// ─────────────────────────────────────────────────────────────────────────
Console.WriteLine("2. Gerando exemplo de tabela...");
GenerateTableExample("exemplo_tabela.pdf");
Console.WriteLine("   ✅ exemplo_tabela.pdf created");

// ─────────────────────────────────────────────────────────────────────────
//  3. BARCODE EXAMPLE (Code39)
// ─────────────────────────────────────────────────────────────────────────
Console.WriteLine("3. Gerando exemplo de código de barras Code39...");
GenerateBarcodeExample("exemplo_barcode39.pdf");
Console.WriteLine("   ✅ exemplo_barcode39.pdf created");

// ─────────────────────────────────────────────────────────────────────────
//  4. BARCODE EXAMPLE (EAN-13)
// ─────────────────────────────────────────────────────────────────────────
Console.WriteLine("4. Gerando exemplo de código de barras EAN-13...");
GenerateEan13Example("exemplo_ean13.pdf");
Console.WriteLine("   ✅ exemplo_ean13.pdf created");

// ─────────────────────────────────────────────────────────────────────────
//  5. DANFE EXAMPLE (se XML existir)
// ─────────────────────────────────────────────────────────────────────────
Console.WriteLine("5. Gerando DANFE a partir de XML...");
if (File.Exists(xmlFile))
{
    try
    {
        var nfeData = NFeParser.Parse(xmlFile);
        NFeRenderer.RenderToFile(nfeData, "exemplo_danfe.pdf");
        Console.WriteLine("   ✅ exemplo_danfe.pdf created");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ❌ Erro ao gerar DANFE: {ex.Message}");
    }
}
else
{
    Console.WriteLine($"   ⚠ Arquivo '{xmlFile}' não found. DANFE not generated.");
}

// ─────────────────────────────────────────────────────────────────────────
//  6. DANFE ENHANCED - Layout faithful to reference PDF with logo
// ─────────────────────────────────────────────────────────────────────────
Console.WriteLine("6. Gerando DANFE enhanced (ref layout + logo)...");
if (File.Exists(xmlFile))
{
    try
    {
        var nfeData = NFeParser.Parse(xmlFile);
        GenerateEnhancedDanfe(nfeData, "exemplo_danfe_enhanced.pdf");
        Console.WriteLine("   ✅ exemplo_danfe_enhanced.pdf created");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ❌ Error: {ex.Message}");
    }
}
else
{
    Console.WriteLine($"   ⚠ Arquivo '{xmlFile}' não found. DANFE enhanced não gerado.");
}

Console.WriteLine();
Console.WriteLine("═══ Playground concluído! ═══");

// ═════════════════════════════════════════════════════════════════════════
//  HELPER METHODS
// ═════════════════════════════════════════════════════════════════════════

/// <summary>
/// Basic example: text, lines, rectangles, grid
/// </summary>
static void GenerateBasicExample(string outputPath)
{
    using var pdf = new PdfBuilder();
    var page = pdf.AddPage();
    var canvas = page.Canvas;

    double x = 50, y = 700;

    canvas.DrawText("Exemplo Básico - PdfDocument", x, y, 16);
    y -= 30;

    canvas.DrawText("Linhas e formas geométricas:", x, y, 12);
    y -= 20;

    canvas.DrawLine(x, y, x + 200, y);
    canvas.DrawRectangle(x, y - 30, 100, 30);
    canvas.FillRectangle(x + 120, y - 30, 100, 30);

    y -= 50;

    canvas.DrawText($"Grid 3x3 de {200}x{100}:", x, y, 12);
    y -= 20;
    canvas.DrawGrid(x, y - 100, 200, 100, 3, 3);

    y -= 130;

    canvas.DrawText("Texto alinhado em célula:", x, y, 12);
    y -= 25;
    canvas.DrawCell("Esquerda", x, y, 80, 20,
        PdfCanvas.TextAlign.Left, 9, true);
    canvas.DrawCell("Centro", x + 90, y, 80, 20,
        PdfCanvas.TextAlign.Center, 9, true);
    canvas.DrawCell("Direita", x + 180, y, 80, 20,
        PdfCanvas.TextAlign.Right, 9, true);

    pdf.Save(outputPath);
}

/// <summary>
/// Table example with header and data
/// </summary>
static void GenerateTableExample(string outputPath)
{
    using var pdf = new PdfBuilder();
    var page = pdf.AddPage();
    var canvas = page.Canvas;

    double x = 50, y = 700;

    canvas.DrawText("Exemplo de Tabela", x, y, 16);
    y -= 30;

    var data = new string[,]
    {
        { "Produto", "Quantidade", "Valor Unit.", "Total" },
        { "Item A", "10", "R$ 5,00", "R$ 50,00" },
        { "Item B", "5", "R$ 12,00", "R$ 60,00" },
        { "Item C", "20", "R$ 3,00", "R$ 60,00" },
    };

    double[] colWidths = [120, 80, 80, 80];

    canvas.DrawTable(
        data, x, y - 120, colWidths, 24,
        PdfCanvas.TextAlign.Center,  // header alignment
        PdfCanvas.TextAlign.Right,    // data alignment
        10);

    pdf.Save(outputPath);
}

/// <summary>
/// Code39 barcode example
/// </summary>
static void GenerateBarcodeExample(string outputPath)
{
    using var pdf = new PdfBuilder();
    var page = pdf.AddPage();
    var canvas = page.Canvas;

    double x = 50, y = 700;

    canvas.DrawText("Código de Barras - Code39", x, y, 16);
    y -= 30;

    string code = "PDF-DOCUMENT";
    canvas.DrawText($"Código: {code}", x, y, 12);
    y -= 25;

    var bars = Code39.Generate(code);
    canvas.DrawBarcode(bars, x, y - 40, 1.5, 40);

    y -= 70;

    string code2 = "12345ABC";
    canvas.DrawText($"Código: {code2}", x, y, 12);
    y -= 25;

    var bars2 = Code39.Generate(code2);
    canvas.DrawBarcode(bars2, x, y - 40, 1.5, 40);

    pdf.Save(outputPath);
}

/// <summary>
/// EAN-13 barcode example
/// </summary>
static void GenerateEan13Example(string outputPath)
{
    using var pdf = new PdfBuilder();
    var page = pdf.AddPage();
    var canvas = page.Canvas;

    double x = 50, y = 700;

    canvas.DrawText("Código de Barras - EAN-13", x, y, 16);
    y -= 30;

    // 12 digits - check digit auto-calculated
    string code = "789123456789";
    canvas.DrawText($"Código: {code} (dígito auto-calculado)", x, y, 12);
    y -= 25;

    var bars = EAN13.Generate(code);
    canvas.DrawBarcode(bars, x, y - 50, 1.0, 50);

    y -= 80;

    // 13 digits with validation
    string code2 = "7891234567895";
    canvas.DrawText($"Código: {code2} (com verificador)", x, y, 12);
    y -= 25;

    var bars2 = EAN13.Generate(code2);
    canvas.DrawBarcode(bars2, x, y - 50, 1.0, 50);

    pdf.Save(outputPath);
}

/// <summary>
/// Generates DANFE faithful to the reference PDF layout (nota-fiscal-notebook-dell.pdf),
/// using data from XML and placing logo.jpg where the red X was.
/// </summary>
static void GenerateEnhancedDanfe(NFeData data, string outputPath)
{
    const string LogoFile = "logo.jpg";

    using var pdf = new PdfBuilder();

    // ── Registers the logo ─────────────────────────────────────────────────
    if (File.Exists(LogoFile))
    {
        pdf.AddImage("logo", LogoFile);
        Console.WriteLine("   📷 Logo registered: logo.jpg");
    }
    else
    {
        Console.WriteLine("   ⚠ 'logo.jpg' não found. Proceeding without logo.");
    }

    var page = pdf.AddPage(595, 842); // A4
    var c = page.Canvas;

    double fs = 7;  // font size base

    // ── TOPO: Linha de recebimento ──────────────────────────────────────
    double y = 810;
    c.DrawText("RECEBEMOS DE", 20, y, fs);
    c.DrawText(TruncateTo(data.EmitXNome, 40), 75, y, fs);
    c.DrawText("OS PRODUTOS CONSTANTES DA NOTA FISCAL INDICADA ABAIXO", 220, y, fs);

    // ── NF-e info (topo direito) ────────────────────────────────────────
    c.DrawText("NF-e", 470, 808, 12);
    c.DrawText($"N° {data.Nnf}", 470, 796, fs + 2);
    c.DrawText($"SÉRIE {data.Serie}", 470, 786, fs + 1);

    // ── Linha de recebimento + assinatura ───────────────────────────────
    y = 795;
    c.DrawLine(20, y, 250, y);
    c.DrawText("DATA DE RECEBIMENTO", 20, y - 2, fs - 1);
    c.DrawLine(260, y, 430, y);
    c.DrawText("IDENTIFICAÇÃO E ASSINATURA DO RECEBEDOR", 260, y - 2, fs - 1);

    // ── LOGO / Identificação do emitente (onde estava o X vermelho) ────
    // No PDF referência: coordenadas do X vermelho ≈ Rect [14, 710, 82, 783]
    // Convertido para top-down: y ≈ 59-132, x ≈ 14-82
    if (File.Exists(LogoFile))
    {
        // Logo 500x500, redimensionada para 60x60 no canto superior esquerdo
        c.DrawImage("logo", 15, 765, 60, 60);
    }

    // Nome do emitente ao lado da logo
    c.DrawText(TruncateTo(data.EmitXNome, 50), 90, 758, fs + 2);
    c.DrawText($"CNPJ: {data.EmitCnpj}", 90, 746, fs);

    // ── DANFE title (centralizado) ─────────────────────────────────────
    c.DrawText("DANFE", 460, 760, 16);
    c.DrawText("DOCUMENTO AUXILIAR", 410, 748, fs);
    c.DrawText("DA NOTA FISCAL", 415, 740, fs);
    c.DrawText("ELETRÔNICA", 420, 732, fs);

    // ── Endereço emitente ──────────────────────────────────────────────
    string enderecoCompleto = $"{data.EmitXLogr}, {data.EmitNro}";
    c.DrawText(enderecoCompleto, 90, 736, fs - 1);
    c.DrawText($"{data.EmitXBairro} - {data.EmitXMun}/{data.EmitUf}", 90, 726, fs - 1);
    c.DrawText($"CEP: {data.EmitCep}", 90, 716, fs - 1);

    // ── Linha separadora ────────────────────────────────────────────────
    c.DrawLine(15, 710, 575, 710);

    // ── Chave de Acesso ────────────────────────────────────────────────
    c.DrawText("CHAVE DE ACESSO", 360, 705, fs);
    // Chave de 44 dígitos formatada em grupos
    string chaveDisplay = "3519 0547 9609 5008 9785 5502 5000 6939 8310 5123 1858";
    c.DrawText(chaveDisplay, 360, 695, fs - 1);
    c.DrawText("Consulta de autenticidade no portal nacional da NF-e", 360, 685, fs - 2);
    c.DrawText("www.nfe.fazenda.gov.br/portal ou no site da Sefaz", 360, 678, fs - 2);
    c.DrawText("Autorizadora", 360, 671, fs - 2);

    // ── Número NF + Série (lado esquerdo) ────────────────────────────────
    c.DrawText($"N° {data.Nnf}", 300, 705, 11);
    c.DrawText($"SÉRIE {data.Serie}", 300, 694, fs);
    c.DrawText("FOLHA 1/1", 300, 684, fs);

    // ── Natureza da Operação ────────────────────────────────────────────
    y = 678;
    c.DrawLine(15, y, 575, y);
    c.DrawText("NATUREZA DA OPERAÇÃO", 20, y - 3, fs);
    c.DrawLine(15, y - 13, 575, y - 13);
    y -= 15;
    c.DrawText(TruncateTo(data.NatOp, 80), 20, y, fs);

    // ── INSCRIÇÃO ESTADUAL / CNPJ ──────────────────────────────────────
    y -= 14;
    c.DrawLine(15, y, 575, y);
    c.DrawText("INSCRIÇÃO ESTADUAL", 20, y - 3, fs - 1);
    c.DrawText(data.EmitIe, 20, y - 13, fs);
    c.DrawText("INSC.ESTADUAL DO SUBST. TRIBUTÁRIO", 150, y - 3, 6);
    c.DrawText("CNPJ", 430, y - 3, fs - 1);
    c.DrawText(FormatCnpj(data.EmitCnpj), 430, y - 13, fs);
    y -= 16;

    // ── DESTINATÁRIO ────────────────────────────────────────────────────
    c.DrawLine(15, y, 575, y);
    c.DrawText("DESTINATÁRIO / REMETENTE", 20, y - 3, fs);

    y -= 13;
    c.DrawLine(15, y, 575, y);
    c.DrawText("NOME/RAZÃO SOCIAL", 20, y - 3, fs - 1);
    c.DrawText("CNPJ/CPF", 380, y - 3, fs - 1);
    c.DrawText("DATA DA EMISSÃO", 480, y - 3, fs - 1);

    y -= 12;
    c.DrawText(TruncateTo(data.DestXNome, 45), 20, y, fs);
    c.DrawText(FormatCpfCnpj(data.DestCnpj), 380, y, fs);
    c.DrawText(FormatDate(data.DhEmi), 480, y, fs);

    y -= 12;
    c.DrawLine(15, y, 575, y);
    c.DrawText("ENDEREÇO", 20, y - 3, fs - 1);
    c.DrawText("BAIRRO / DISTRITO", 280, y - 3, fs - 1);
    c.DrawText("CEP", 430, y - 3, fs - 1);
    c.DrawText("DATA DA SAÍDA/ENTRADA", 480, y - 3, 6);

    y -= 12;
    c.DrawText($"{data.DestXLogr}, {data.DestNro}", 20, y, fs);
    c.DrawText(data.DestXBairro, 280, y, fs);
    c.DrawText(data.EmitCep, 430, y, fs);
    c.DrawText(FormatDate(data.DhEmi), 480, y, fs);

    y -= 12;
    c.DrawLine(15, y, 575, y);
    c.DrawText("MUNICÍPIO", 20, y - 3, fs - 1);
    c.DrawText("UF", 240, y - 3, fs - 1);
    c.DrawText("INSCRIÇÃO ESTADUAL", 280, y - 3, fs - 1);
    c.DrawText("HORA DE SAÍDA/ENTRADA", 480, y - 3, 6);

    y -= 12;
    c.DrawText(data.DestXMun, 20, y, fs);
    c.DrawText(data.DestUf, 240, y, fs);
    c.DrawText(data.DestIe, 280, y, fs);
    c.DrawText(FormatTime(data.DhEmi), 480, y, fs);

    // ── PRODUTO ─────────────────────────────────────────────────────────
    y -= 4;
    c.DrawLine(15, y, 575, y);
    c.DrawText("DADOS DO PRODUTO / SERVIÇO", 20, y - 2, fs);

    y -= 13;
    c.DrawLine(15, y, 575, y);
    c.DrawText("COD.PROD.", 20, y - 3, fs - 1);
    c.DrawText("DESCRIÇÃO DO PRODUTO / SERVIÇO", 75, y - 3, fs - 1);
    c.DrawText("NCM/SH", 255, y - 3, fs - 1);
    c.DrawText("CFOP", 310, y - 3, fs - 1);
    c.DrawText("UNID", 355, y - 3, fs - 1);
    c.DrawText("QTDE", 395, y - 3, fs - 1);
    c.DrawText("VL. UNITÁRIO", 435, y - 3, 6);
    c.DrawText("VL. TOTAL", 510, y - 3, 6);

    y -= 14;
    c.DrawLine(15, y, 575, y);
    c.DrawText(data.CProd, 20, y, 6);
    c.DrawText(TruncateTo(data.XProd, 35), 75, y, 6);
    c.DrawText(data.Ncm, 255, y, 6);
    c.DrawText(data.Cfop, 310, y, 6);
    c.DrawText(data.UCom, 355, y, 6);
    c.DrawText(data.QCom, 395, y, 6);
    c.DrawText(data.VUnCom, 440, y, 6);
    c.DrawText(data.VProd, 510, y, 6);

    // ── TOTAIS ──────────────────────────────────────────────────────────
    y -= 16;
    c.DrawLine(15, y, 575, y);
    c.DrawText("CÁLCULO DO IMPOSTO", 20, y - 2, fs);

    y -= 14;
    c.DrawLine(15, y, 575, y);
    c.DrawText("BASE DE CÁLCULO DO ICMS", 20, y - 3, fs - 1);
    c.DrawText("VALOR DO ICMS", 155, y - 3, fs - 1);
    c.DrawText("BASE DE CÁLCULO DO ICMS ST", 280, y - 3, fs - 1);
    c.DrawText("VALOR DO ICMS ST", 430, y - 3, fs - 1);

    y -= 16;
    c.DrawText(data.VBc, 20, y, fs);
    c.DrawText(data.VIcms, 155, y, fs);
    c.DrawText("0,00", 280, y, fs);
    c.DrawText("0,00", 430, y, fs);

    y -= 14;
    c.DrawLine(15, y, 575, y);
    c.DrawText("VALOR DO FRETE", 20, y - 3, fs - 1);
    c.DrawText("VALOR DO SEGURO", 150, y - 3, fs - 1);
    c.DrawText("DESCONTO", 280, y - 3, fs - 1);
    c.DrawText("OUTRAS DESP. ACESS.", 400, y - 3, 6);

    y -= 16;
    c.DrawText("0,00", 20, y, fs);
    c.DrawText("0,00", 150, y, fs);
    c.DrawText("0,00", 280, y, fs);
    c.DrawText("0,00", 400, y, fs);

    y -= 14;
    c.DrawLine(15, y, 575, y);
    c.DrawText("VALOR TOTAL DOS PRODUTOS", 20, y - 3, fs - 1);
    c.DrawText("VALOR DO IPI", 250, y - 3, fs - 1);
    c.DrawText("VALOR TOTAL DA NOTA", 400, y - 3, fs - 1);

    y -= 16;
    c.DrawText(data.VProdTotal, 20, y, fs);
    c.DrawText("0,00", 250, y, fs);
    c.DrawText(data.VNf, 400, y, 10);

    // ── TRANSPORTADORA ──────────────────────────────────────────────────
    y -= 16;
    c.DrawLine(15, y, 575, y);
    c.DrawText("TRANSPORTADOR / VOLUMES TRANSPORTADOS", 20, y - 2, fs);

    y -= 13;
    c.DrawLine(15, y, 575, y);
    c.DrawText("RAZÃO SOCIAL", 20, y - 3, fs - 1);
    c.DrawText("FRETE POR CONTA", 330, y - 3, fs - 1);

    y -= 12;
    string transNome = string.IsNullOrEmpty(data.TransXNome) ? "NÃO INFORMADO" : data.TransXNome;
    c.DrawText(TruncateTo(transNome, 50), 20, y, fs);
    c.DrawText("0 - Emitente", 330, y, fs);

    y -= 12;
    c.DrawLine(15, y, 575, y);
    c.DrawText("CNPJ / CPF", 20, y - 3, fs - 1);
    c.DrawText(data.TransCnpj, 20, y - 12, fs);
    c.DrawText("INSCRIÇÃO ESTADUAL", 160, y - 3, fs - 1);
    c.DrawText(data.TransIe, 160, y - 12, fs);
    c.DrawText("UF", 320, y - 3, fs - 1);
    c.DrawText(data.TransUf, 320, y - 12, fs);
    c.DrawText("MUNICÍPIO", 360, y - 3, fs - 1);
    c.DrawText(data.TransXMun, 360, y - 12, fs);

    y -= 28;
    c.DrawLine(15, y, 575, y);
    c.DrawText("ENDEREÇO", 20, y - 3, fs - 1);
    c.DrawText(data.TransXEnder, 20, y - 12, fs);
    c.DrawText("QTDE", 320, y - 3, fs - 1);
    c.DrawText("ESPÉCIE", 370, y - 3, fs - 1);
    c.DrawText("PESO BRUTO", 440, y - 3, fs - 1);
    c.DrawText("PESO LÍQUIDO", 510, y - 3, 6);

    y -= 12;
    c.DrawText("1", 320, y, fs);
    c.DrawText("VOL", 370, y, fs);
    c.DrawText("4.000", 440, y, fs);
    c.DrawText("4.000", 510, y, fs);

    // ── PAGAMENTO ───────────────────────────────────────────────────────
    y -= 20;
    c.DrawLine(15, y, 575, y);
    c.DrawText("FATURA / DUPLICATAS", 20, y - 2, fs);

    y -= 14;
    c.DrawLine(15, y, 575, y);
    c.DrawText("FORMA DE PAGAMENTO", 20, y - 3, 6);
    c.DrawText("VALOR", 200, y - 3, 6);
    c.DrawText(data.TPag, 20, y - 12, 6);
    c.DrawText(data.VPag, 200, y - 12, 6);

    // ── DADOS ADICIONAIS ────────────────────────────────────────────────
    y -= 24;
    c.DrawLine(15, y, 575, y);
    c.DrawText("DADOS ADICIONAIS", 20, y - 2, fs);
    c.DrawLine(15, y - 13, 370, y - 13);
    c.DrawText("INFORMAÇÕES COMPLEMENTARES", 20, y - 15, 6);

    c.DrawLine(390, y - 13, 575, y - 13);
    c.DrawText("RESERVADO AO FISCO", 420, y - 2, fs);

    y -= 40;
    c.DrawText($"Natureza: {data.NatOp}", 20, y, 6);

    // ── Rodapé ──────────────────────────────────────────────────────────
    y -= 20;
    c.DrawLine(15, y, 575, y);
    c.DrawText($"N° {data.Nnf}", 20, y - 2, 6);
    c.DrawText($"SÉRIE {data.Serie}", 90, y - 2, 6);
    c.DrawText("FOLHA 1/1", 160, y - 2, 6);
    c.DrawText("Documento gerado em ambiente de homologação - Sem valor fiscal", 300, y - 2, 6);

    pdf.Save(outputPath);
}

// ── Utilities ─────────────────────────────────────────────────────────

/// <summary>Trunca string com reticências se exceder tamanho máximo.</summary>
static string TruncateTo(string text, int maxLen)
{
    if (string.IsNullOrEmpty(text)) return "";
    return text.Length <= maxLen ? text : text[..(maxLen - 3)] + "...";
}

/// <summary>Formata CNPJ: XX.XXX.XXX/XXXX-XX</summary>
static string FormatCnpj(string cnpj)
{
    if (string.IsNullOrEmpty(cnpj) || cnpj.Length < 14) return cnpj;
    return $"{cnpj[..2]}.{cnpj[2..5]}.{cnpj[5..8]}/{cnpj[8..12]}-{cnpj[12..14]}";
}

/// <summary>Formata CPF ou CNPJ automaticamente.</summary>
static string FormatCpfCnpj(string doc)
{
    if (string.IsNullOrEmpty(doc)) return "";
    string clean = doc.Replace(".", "").Replace("-", "").Replace("/", "");
    return clean.Length <= 11
        ? $"{clean[..3]}.{clean[3..6]}.{clean[6..9]}-{clean[9..11]}"
        : FormatCnpj(clean);
}

/// <summary>Formata data ISO para dd/mm/aaaa.</summary>
static string FormatDate(string isoDate)
{
    if (string.IsNullOrEmpty(isoDate) || isoDate.Length < 10) return isoDate;
    return $"{isoDate[8..10]}/{isoDate[5..7]}/{isoDate[..4]}";
}

/// <summary>Extrai hora de string ISO.</summary>
static string FormatTime(string isoDate)
{
    if (string.IsNullOrEmpty(isoDate) || isoDate.Length < 19) return "";
    return isoDate[11..19];
}
