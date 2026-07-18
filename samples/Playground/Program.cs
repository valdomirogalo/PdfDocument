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
Console.ReadKey();

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
    /// <summary>
    /// Generates DANFE faithful to the reference PDF (nota-fiscal-notebook-dell.pdf) layout.
    /// All sections in the exact order and column layout of the reference.
    /// </summary>
    static void GenerateEnhancedDanfe(NFeData data, string outputPath)
    {
        const string LogoFile = "logo.jpg";

        using var pdf = new PdfBuilder();

        // ── Registers the logo ─────────────────────────────────────────────
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

        // ═══════════════════════════════════════════════════════════════════
        //  Helpers de layout
        // ═══════════════════════════════════════════════════════════════════

        void DrawTableRow(ref double yRef, double rowHeight, string[] texts, double[] colWidths,
            double startX = 15, double fontSize = 7, PdfCanvas.TextAlign align = PdfCanvas.TextAlign.Left)
        {
            double cellBottom = yRef - rowHeight;
            double x = startX;
            for (int i = 0; i < texts.Length; i++)
            {
                c.DrawCell(texts[i] ?? "", x, cellBottom, colWidths[i], rowHeight, align, fontSize, true);
                x += colWidths[i];
            }
            yRef = cellBottom;
        }

        void DrawSectionTitle(ref double yRef, string title, double rowHeight = 10, double fontSize = 7)
        {
            double cellBottom = yRef - rowHeight;
            c.DrawCell(title, 15, cellBottom, 560, rowHeight, PdfCanvas.TextAlign.Center, fontSize, true);
            yRef = cellBottom;
        }

        const double RowH = 11;          // altura padrão das linhas
        const double RowHlabel = 9;      // altura linhas de rótulo/cabeçalho

        double y = 820;

        // ═══════════════════════════════════════════════════════════════════
        //  CABEÇALHO — Layout idêntico à imagem PNG de referência
        // ═══════════════════════════════════════════════════════════════════

        // ── (1) LINHA TOPO: "RECEBEMOS DE..." (full width) + NF-e (dir) ─
        y = 815;
        c.DrawText($"RECEBEMOS DE {TruncateTo(data.EmitXNome, 40)} OS PRODUTOS CONSTANTES DA NOTA FISCAL INDICADA ABAIXO", 15, y, 7);
        c.DrawText("NF-e", 500, y, 12);

        // ── (2) N° + SÉRIE no canto direito (abaixo do NF-e e da linha) ─
        y = 783;
        c.DrawText($"N° {data.Nnf}", 500, y, 9);
        y = 773;
        c.DrawText($"SÉRIE {data.Serie}", 500, y, 8);

        // ── (3) LINHA DE RECEBIMENTO ─────────────────────────────────────
        y = 793;
        c.DrawLine(15, y, 245, y);
        c.DrawText("DATA DE RECEBIMENTO", 15, y - 8, 6);
        c.DrawLine(255, y, 430, y);
        c.DrawText("IDENTIFICAÇÃO E ASSINATURA DO RECEBEDOR", 255, y - 8, 6);

        // ── (4) LOGO + EMITENTE (esq)  |  DANFE (dir) ─────────────────
        y = 760;

        // ── Esquerda: logo (60x60) + dados do emitente ──────────────────
        if (File.Exists(LogoFile))
            c.DrawImage("logo", 15, y - 40, 60, 60);

        c.DrawText(TruncateTo(data.EmitXNome, 50), 90, y, 9);
        y -= 11;
        c.DrawText($"CNPJ: {FormatCnpj(data.EmitCnpj)}", 90, y, 7);
        y -= 10;
        string enderecoLinha1 = $"{data.EmitXLogr}, {data.EmitNro}";
        c.DrawText(enderecoLinha1, 90, y, 6);
        y -= 9;
        c.DrawText($"{data.EmitXBairro} - {data.EmitXMun}/{data.EmitUf}", 90, y, 6);
        y -= 9;
        c.DrawText($"CEP: {data.EmitCep}", 90, y, 6);

        // ── Direita: DANFE + CHAVE DE ACESSO + N°/SÉRIE + barcode ────
        y = 760;
        c.DrawText("DANFE", 460, y, 16);
        c.DrawText("DOCUMENTO AUXILIAR", 410, y - 14, 7);
        c.DrawText("DA NOTA FISCAL", 415, y - 24, 7);
        c.DrawText("ELETRÔNICA", 420, y - 34, 7);
        c.DrawText("0 - ENTRADA", 410, y - 44, 6);
        c.DrawText("1 - SAÍDA", 410, y - 52, 6);

        // ── Bloco CHAVE DE ACESSO (lado direito) ────────────────────────
        c.DrawText("CHAVE DE ACESSO", 360, y - 62, 7);
        string chave = "3519 0547 9609 5008 9785 5502 5000 6939 8310 5123 1858";
        c.DrawText(chave, 360, y - 74, 6);

        // N°/SÉRIE/FOLHA (abaixo de CHAVE, lado direito)
        c.DrawText($"N°{data.Nnf}", 470, y - 64, 11);
        c.DrawText($"SÉRIE {data.Serie}", 470, y - 77, 7);
        c.DrawText("FOLHA 1/1", 470, y - 87, 7);

        // Barcode abaixo da chave
        try
        {
            string chaveLimpa = chave.Replace(" ", "");
            // Code128C → padrão DANFE real (mais compacto que Code39, 44 dígitos)
            var barcodeBars = Code128.Generate(chaveLimpa);
            c.DrawBarcode(barcodeBars, 360, y - 50, 0.6, 24);  // Code128 mais compacto
        }
        catch { }

        // Consulta abaixo do barcode
        c.DrawText("Consulta de autenticidade no portal nacional da NF-e", 360, y - 110, 5);
        c.DrawText("www.nfe.fazenda.gov.br/portal ou no site da Sefaz", 360, y - 118, 5);
        c.DrawText("Autorizadora", 360, y - 126, 5);

        // ── (5) SEPARADOR HORIZONTAL ────────────────────────────────────
        y = y - 130;
        c.DrawLine(15, y, 575, y);
        y -= 3;

        // ═══════════════════════════════════════════════════════════════════
        //  SEÇÕES EM TABELA
        // ═══════════════════════════════════════════════════════════════════

        // ── NATUREZA DA OPERAÇÃO | PROTOCOLO DE AUTORIZAÇÃO DE USO ────
        DrawSectionTitle(ref y, "NATUREZA DA OPERAÇÃO");
        // Linha dupla: Natureza (esq) + Protocolo (dir)
        double yNat = y + RowH;  // salva y antes da DrawTableRow
        DrawTableRow(ref y, RowH, [TruncateTo(data.NatOp, 40)], [360]);
        // Protocolo na mesma linha, lado direito
        double yProt = yNat;  // mesma linha
        string protText = string.IsNullOrEmpty(data.NProt) ? "" : $"{data.NProt} {FormatDate(data.DhAutor)} {FormatTime(data.DhAutor)}";
        c.DrawCell(TruncateTo(protText, 30), 380, yProt - RowH, 195, RowH, PdfCanvas.TextAlign.Left, 7, true);
        y = yProt - RowH;  // avança y

        // ── INSCRIÇÃO ESTADUAL (3 colunas: IE | IE SUBST. TRIB. | CNPJ) ─
        DrawSectionTitle(ref y, "INSCRIÇÃO ESTADUAL");
        string ieSt = string.IsNullOrEmpty(data.EmitIeSt) ? "" : data.EmitIeSt;
        DrawTableRow(ref y, RowHlabel,
            ["INSCRIÇÃO ESTADUAL", "INSC.ESTADUAL DO SUBST. TRIBUTÁRIO", "CNPJ"],
            [230, 180, 150], fontSize: 5);
        DrawTableRow(ref y, RowH,
            [data.EmitIe, ieSt, FormatCnpj(data.EmitCnpj)],
            [230, 180, 150]);

        // ── DESTINATÁRIO / REMETENTE ──────────────────────────────────────
        DrawSectionTitle(ref y, "DESTINATÁRIO / REMETENTE");

        DrawTableRow(ref y, RowHlabel,
            ["NOME/RAZÃO SOCIAL", "CNPJ/CPF", "DATA DA EMISSÃO"],
            [360, 100, 100], fontSize: 5);
        DrawTableRow(ref y, RowH,
            [TruncateTo(data.DestXNome, 45), FormatCpfCnpj(data.DestCnpj), FormatDate(data.DhEmi)],
            [360, 100, 100]);

        DrawTableRow(ref y, RowHlabel,
            ["ENDEREÇO", "BAIRRO / DISTRITO", "CEP", "DATA DA SAÍDA/ENTRADA"],
            [260, 150, 70, 80], fontSize: 5);
        DrawTableRow(ref y, RowH,
            [$"{data.DestXLogr}, {data.DestNro}", data.DestXBairro, data.EmitCep,
             string.IsNullOrEmpty(data.DhSaiEnt) ? FormatDate(data.DhEmi) : FormatDate(data.DhSaiEnt)],
            [260, 150, 70, 80]);

        DrawTableRow(ref y, RowHlabel,
            ["MUNICÍPIO", "FONE/FAX", "UF", "INSCRIÇÃO ESTADUAL", "HORA DE SAÍDA/ENTRADA"],
            [150, 100, 50, 130, 130], fontSize: 5);
        DrawTableRow(ref y, RowH,
            [data.DestXMun, data.DestFone, data.DestUf, data.DestIe,
             string.IsNullOrEmpty(data.DhSaiEnt) ? FormatTime(data.DhEmi) : FormatTime(data.DhSaiEnt)],
            [150, 100, 50, 130, 130]);

        // ── FATURA / DUPLICATAS ───────────────────────────────────────────
        y -= 4;
        DrawSectionTitle(ref y, "FATURA / DUPLICATAS");
        DrawTableRow(ref y, RowHlabel, ["FATURA"], [560], fontSize: 6);
        DrawTableRow(ref y, RowHlabel,
            ["Núm. Duplicata/Parcela", "Vencimento", "Valor",
             "Núm. Duplicata/Parcela", "Vencimento", "Valor",
             "Núm. Duplicata/Parcela", "Vencimento", "Valor"],
            [62, 63, 62, 62, 63, 62, 62, 63, 62], fontSize: 5);
        DrawTableRow(ref y, RowH,
            ["", "", "", "", "", "", "", "", ""],
            [62, 63, 62, 62, 63, 62, 62, 63, 62], fontSize: 6);

        // ── CÁLCULO DO IMPOSTO ────────────────────────────────────────────
        y -= 4;
        DrawSectionTitle(ref y, "CÁLCULO DO IMPOSTO");

        DrawTableRow(ref y, RowHlabel,
            ["BASE DE CÁLCULO DO ICMS", "VALOR DO ICMS", "BASE DE CÁLCULO DO ICMS ST", "VALOR DO ICMS ST",
             "VALOR APROXIMADO DOS TRIBUTOS", "VALOR TOTAL DOS PRODUTOS"],
            [110, 70, 110, 70, 100, 100], fontSize: 5);
        DrawTableRow(ref y, RowH,
            [data.VBc, data.VIcms, data.VBcSt, data.VSt, data.VAproxTrib, data.VProdTotal],
            [110, 70, 110, 70, 100, 100]);

        DrawTableRow(ref y, RowHlabel,
            ["VALOR DO FRETE", "VALOR DO SEGURO", "DESCONTO", "OUTRAS DESPESAS ACESSÓRIAS",
             "VALOR DO IPI", "VALOR TOTAL DA NOTA"],
            [95, 75, 75, 125, 70, 120], fontSize: 5);
        DrawTableRow(ref y, RowH,
            [data.VFrete, data.VSeg, data.VDesc, data.VOutro, data.VIpi, data.VNf],
            [95, 75, 75, 125, 70, 120]);

        // ── TRANSPORTADOR ──────────────────────────────────────────────────
        y -= 4;
        DrawSectionTitle(ref y, "TRANSPORTADOR / VOLUMES TRANSPORTADOS");

        string transNome = string.IsNullOrEmpty(data.TransXNome) ? "NÃO INFORMADO" : data.TransXNome;
        DrawTableRow(ref y, RowHlabel,
            ["RAZÃO SOCIAL", "FRETE POR CONTA", "CÓDIGO ANTT", "PLACA DO VEÍCULO", "UF", "CNPJ / CPF"],
            [210, 90, 60, 90, 30, 80], fontSize: 5);
        DrawTableRow(ref y, RowH,
            [TruncateTo(transNome, 35), data.ModFrete == "0" ? "0 - Emitente" : data.ModFrete, data.TransAntt,
             data.TransPlaca, data.TransUFVeic, data.TransCnpj],
            [210, 90, 60, 90, 30, 80]);

        DrawTableRow(ref y, RowHlabel,
            ["ENDEREÇO", "MUNICÍPIO", "UF", "INSCRIÇÃO ESTADUAL"],
            [260, 150, 50, 100], fontSize: 5);
        DrawTableRow(ref y, RowH,
            [data.TransXEnder, data.TransXMun, data.TransUf, data.TransIe],
            [260, 150, 50, 100]);

        DrawTableRow(ref y, RowHlabel,
            ["QUANTIDADE", "ESPÉCIE", "MARCA", "NUMERAÇÃO", "PESO BRUTO", "PESO LÍQUIDO"],
            [70, 70, 130, 130, 80, 80], fontSize: 5);
        string qVol = string.IsNullOrEmpty(data.TransQVol) ? "1" : data.TransQVol;
        string esp = string.IsNullOrEmpty(data.TransEspecie) ? "VOLUMES" : data.TransEspecie;
        string pesoB = string.IsNullOrEmpty(data.TransPesoB) ? "" : data.TransPesoB;
        string pesoL = string.IsNullOrEmpty(data.TransPesoL) ? "" : data.TransPesoL;
        DrawTableRow(ref y, RowH,
            [qVol, esp, data.TransMarca, data.TransNumVol, pesoB, pesoL],
            [70, 70, 130, 130, 80, 80]);

        // ── DADOS DO PRODUTO / SERVIÇO ─────────────────────────────────────
        y -= 4;
        DrawSectionTitle(ref y, "DADOS DO PRODUTO / SERVIÇO");

        DrawTableRow(ref y, RowHlabel,
            ["COD.PROD.", "DESCRIÇÃO DO PRODUTO / SERVIÇO", "NCM/SH", "CST", "CFOP", "UNID",
             "QTDE", "VL. UNITÁRIO", "VL. TOTAL", "VL. DESCONTO", "BC.ICMS", "VL.ICMS", "V.IPI",
             "ALÍQ.ICMS", "ALÍQ.IPI"],
            [45, 145, 42, 25, 28, 25, 28, 42, 42, 35, 35, 35, 30, 25, 25], fontSize: 4.5f);
        DrawTableRow(ref y, RowH,
            [data.CProd, TruncateTo(data.XProd, 30), data.Ncm, data.Cst, data.Cfop, data.UCom,
             data.QCom, data.VUnCom, data.VProd, data.VProdDesc, data.VBcProd, data.VIcmsProd,
             data.VIpiProd, data.PIcms, data.PIpi],
            [45, 145, 42, 25, 28, 25, 28, 42, 42, 35, 35, 35, 30, 25, 25], fontSize: 5);

        // ── CÁLCULO DO ISSQN ───────────────────────────────────────────────
        y -= 4;
        DrawSectionTitle(ref y, "CÁLCULO DO ISSQN");

        DrawTableRow(ref y, RowHlabel,
            ["INSCRIÇÃO MUNICIPAL", "VALOR TOTAL DOS SERVIÇOS", "BASE DE CÁLCULO DO ISSQN", "VALOR DO ISSQN"],
            [140, 140, 140, 140], fontSize: 5);
        DrawTableRow(ref y, RowH,
            ["", "0,00", "0,00", "0,00"],
            [140, 140, 140, 140]);

        // ── DADOS ADICIONAIS ───────────────────────────────────────────────
        y -= 4;
        DrawSectionTitle(ref y, "DADOS ADICIONAIS");

        DrawTableRow(ref y, RowHlabel,
            ["INFORMAÇÕES COMPLEMENTARES", "RESERVADO AO FISCO"],
            [355, 205], fontSize: 5);
        string infCpl = string.IsNullOrEmpty(data.InfCpl) ? $"Natureza: {data.NatOp}" : data.InfCpl;
        DrawTableRow(ref y, RowH * 3,
            [infCpl, data.InfAdic],
            [355, 205], fontSize: 6, align: PdfCanvas.TextAlign.Left);

        // ── Rodapé ──────────────────────────────────────────────────────────
        y -= 4;
        DrawTableRow(ref y, RowH,
            [$"N° {data.Nnf}", $"SÉRIE {data.Serie}", "FOLHA 1/1",
             "Documento gerado em ambiente de homologação - Sem valor fiscal"],
            [70, 70, 80, 340], fontSize: 6);

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
