namespace PdfDocument.NFe;

/// <summary>
/// Renders NFe data into a PDF document (DANFE).
/// </summary>
public static class NFeRenderer
{
    /// <summary>
    /// Generates a DANFE PDF from NFe data and saves it to the specified path.
    /// </summary>
    /// <param name="data">NFe data extracted from XML.</param>
    /// <param name="outputPath">Output PDF file path.</param>
    /// <exception cref="ArgumentNullException">If any parameter is null.</exception>
    public static void RenderToFile(NFeData data, string outputPath)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(outputPath);

        using var pdf = new PdfBuilder();
        var page = pdf.AddPage();
        var canvas = page.Canvas;

        double x = PdfConstants.DefaultMarginX;
        double y = PdfConstants.DefaultMarginY;

        // ── Header ──────────────────────────────────────────────────────
        canvas.DrawText(
            "DANFE - Documento Auxiliar da Nota Fiscal Eletrônica",
            x, y, PdfConstants.TitleFontSize);
        y -= PdfConstants.TitleSpacing;

        // Divider line
        canvas.DrawLine(x, y, PdfConstants.PageWidth, y);
        y -= PdfConstants.DividerSpacing;

        // ── NFe data ────────────────────────────────────────────────────
        DrawInfoLine(canvas, ref y, $"Natureza da Operação: {data.NatOp}");
        DrawInfoLine(canvas, ref y, $"NFe: {data.Mod}   Série: {data.Serie}   Número: {data.Nnf}");
        DrawInfoLine(canvas, ref y, $"Emissão: {data.DhEmi}");
        DrawInfoLine(canvas, ref y,
            $"Tipo: {data.TpNf}   Destino: {data.IdDest}   Ambiente: {data.TpAmb}");

        y -= PdfConstants.SectionSpacing;

        // ── Issuer ──────────────────────────────────────────────────────
        DrawSection(canvas, ref y, "--- EMITENTE ---");
        DrawInfoLine(canvas, ref y, $"CNPJ: {data.EmitCnpj}");
        DrawInfoLine(canvas, ref y, data.EmitXNome);
        DrawInfoLine(canvas, ref y,
            $"{data.EmitXLogr}, {data.EmitNro} - {data.EmitXBairro} - {data.EmitXMun}/{data.EmitUf}");
        DrawInfoLine(canvas, ref y, $"IE: {data.EmitIe}   CRT: {data.EmitCrt}");

        y -= PdfConstants.SectionSpacing;

        // ── Recipient ───────────────────────────────────────────────────
        DrawSection(canvas, ref y, "--- DESTINATÁRIO ---");
        DrawInfoLine(canvas, ref y, $"CNPJ: {data.DestCnpj}");
        DrawInfoLine(canvas, ref y, data.DestXNome);
        DrawInfoLine(canvas, ref y,
            $"{data.DestXLogr}, {data.DestNro} - {data.DestXBairro} - {data.DestXMun}/{data.DestUf}");
        DrawInfoLine(canvas, ref y, $"IE: {data.DestIe}");

        y -= PdfConstants.SectionSpacing;

        // ── Product ─────────────────────────────────────────────────────
        DrawSection(canvas, ref y, "--- PRODUTO ---");
        DrawInfoLine(canvas, ref y, $"Código: {data.CProd}   Descrição: {data.XProd}");
        DrawInfoLine(canvas, ref y,
            $"NCM: {data.Ncm}   CFOP: {data.Cfop}   Quantidade: {data.QCom} {data.UCom}");
        DrawInfoLine(canvas, ref y, $"Valor Unit.: {data.VUnCom}   Valor Total: {data.VProd}");

        y -= PdfConstants.SectionSpacing;

        // ── Totals ──────────────────────────────────────────────────────
        DrawSection(canvas, ref y, "--- TOTAIS ---");
        DrawInfoLine(canvas, ref y, $"Base de Cálculo ICMS: {data.VBc}   ICMS: {data.VIcms}");
        DrawInfoLine(canvas, ref y, $"Valor dos Produtos: {data.VProdTotal}   Valor da NF: {data.VNf}");

        y -= PdfConstants.SectionSpacing;

        // ── Carrier ─────────────────────────────────────────────────────
        DrawSection(canvas, ref y, "--- TRANSPORTADORA ---");
        DrawInfoLine(canvas, ref y, $"CNPJ: {data.TransCnpj}   {data.TransXNome}");
        DrawInfoLine(canvas, ref y, $"{data.TransXEnder} - {data.TransXMun}/{data.TransUf}");

        y -= PdfConstants.SectionSpacing;

        // ── Payment ─────────────────────────────────────────────────────
        DrawSection(canvas, ref y, "--- PAGAMENTO ---");
        DrawInfoLine(canvas, ref y, $"Forma: {data.TPag}   Valor: {data.VPag}");

        y -= PdfConstants.SectionSpacing;

        // ── Footer ──────────────────────────────────────────────────────
        canvas.DrawLine(x, y + PdfConstants.FooterLineSpacing, PdfConstants.PageWidth, y + PdfConstants.FooterLineSpacing);
        y -= PdfConstants.FooterLineSpacing;
        canvas.DrawText(
            "Documento gerado em ambiente de homologação - Sem valor fiscal",
            x, y, PdfConstants.FooterFontSize);

        pdf.Save(outputPath);
    }

    /// <summary>
    /// Draws an info line on the canvas and advances the Y position.
    /// </summary>
    private static void DrawInfoLine(PdfCanvas canvas, ref double y, string text)
    {
        canvas.DrawText(text, PdfConstants.DefaultMarginX, y, PdfConstants.DefaultFontSize);
        y -= PdfConstants.DefaultLineHeight;
    }

    /// <summary>
    /// Draws a section title on the canvas and advances the Y position.
    /// </summary>
    private static void DrawSection(PdfCanvas canvas, ref double y, string title)
    {
        canvas.DrawText(title, PdfConstants.DefaultMarginX, y, PdfConstants.SectionFontSize);
        y -= PdfConstants.DefaultLineHeight;
    }
}
