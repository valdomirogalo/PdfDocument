using PdfDocument;

namespace PdfDocument.NFe;

/// <summary>
/// Renders NFe data into a PDF document (DANFE).
/// </summary>
public sealed class NFeRenderer : ILayoutRenderer<NFeData>
{
    /// <summary>
    /// Generates a DANFE PDF from NFe data and saves it to the specified path.
    /// </summary>
    /// <param name="data">NFe data extracted from XML.</param>
    /// <param name="outputPath">Output PDF file path.</param>
    /// <exception cref="ArgumentNullException">If any parameter is null.</exception>
    public void Render(NFeData data, string outputPath)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(outputPath);

        using var pdf = new PdfBuilder();
        var page = pdf.AddPage();
        var canvas = page.Canvas;

        double x = NFeConstants.DefaultMarginX;
        double y = NFeConstants.DefaultMarginY;

        // ── Header ──────────────────────────────────────────────────────
        canvas.DrawText(
            "DANFE - Documento Auxiliar da Nota Fiscal Eletrônica",
            x, y, NFeConstants.TitleFontSize);
        y -= NFeConstants.TitleSpacing;

        // Divider line
        canvas.DrawLine(x, y, NFeConstants.PageWidth, y);
        y -= NFeConstants.DividerSpacing;

        // ── NFe data ────────────────────────────────────────────────────
        DrawInfoLine(canvas, ref y, $"Natureza da Operação: {data.NatOp}");
        DrawInfoLine(canvas, ref y, $"NFe: {data.Mod}   Série: {data.Serie}   Número: {data.Nnf}");
        DrawInfoLine(canvas, ref y, $"Emissão: {data.DhEmi}");
        DrawInfoLine(canvas, ref y,
            $"Tipo: {data.TpNf}   Destino: {data.IdDest}   Ambiente: {data.TpAmb}");

        y -= NFeConstants.SectionSpacing;

        // ── Issuer ──────────────────────────────────────────────────────
        DrawSection(canvas, ref y, "--- EMITENTE ---");
        DrawInfoLine(canvas, ref y, $"CNPJ: {data.EmitCnpj}");
        DrawInfoLine(canvas, ref y, data.EmitXNome);
        DrawInfoLine(canvas, ref y,
            $"{data.EmitXLogr}, {data.EmitNro} - {data.EmitXBairro} - {data.EmitXMun}/{data.EmitUf}");
        DrawInfoLine(canvas, ref y, $"IE: {data.EmitIe}   CRT: {data.EmitCrt}");

        y -= NFeConstants.SectionSpacing;

        // ── Recipient ───────────────────────────────────────────────────
        DrawSection(canvas, ref y, "--- DESTINATÁRIO ---");
        DrawInfoLine(canvas, ref y, $"CNPJ: {data.DestCnpj}");
        DrawInfoLine(canvas, ref y, data.DestXNome);
        DrawInfoLine(canvas, ref y,
            $"{data.DestXLogr}, {data.DestNro} - {data.DestXBairro} - {data.DestXMun}/{data.DestUf}");
        DrawInfoLine(canvas, ref y, $"IE: {data.DestIe}");

        y -= NFeConstants.SectionSpacing;

        // ── Product ─────────────────────────────────────────────────────
        DrawSection(canvas, ref y, "--- PRODUTO ---");
        DrawInfoLine(canvas, ref y, $"Código: {data.CProd}   Descrição: {data.XProd}");
        DrawInfoLine(canvas, ref y,
            $"NCM: {data.Ncm}   CFOP: {data.Cfop}   Quantidade: {data.QCom} {data.UCom}");
        DrawInfoLine(canvas, ref y, $"Valor Unit.: {data.VUnCom}   Valor Total: {data.VProd}");

        y -= NFeConstants.SectionSpacing;

        // ── Totals ──────────────────────────────────────────────────────
        DrawSection(canvas, ref y, "--- TOTAIS ---");
        DrawInfoLine(canvas, ref y, $"Base de Cálculo ICMS: {data.VBc}   ICMS: {data.VIcms}");
        DrawInfoLine(canvas, ref y, $"Valor dos Produtos: {data.VProdTotal}   Valor da NF: {data.VNf}");

        y -= NFeConstants.SectionSpacing;

        // ── Carrier ─────────────────────────────────────────────────────
        DrawSection(canvas, ref y, "--- TRANSPORTADORA ---");
        DrawInfoLine(canvas, ref y, $"CNPJ: {data.TransCnpj}   {data.TransXNome}");
        DrawInfoLine(canvas, ref y, $"{data.TransXEnder} - {data.TransXMun}/{data.TransUf}");

        y -= NFeConstants.SectionSpacing;

        // ── Payment ─────────────────────────────────────────────────────
        DrawSection(canvas, ref y, "--- PAGAMENTO ---");
        DrawInfoLine(canvas, ref y, $"Forma: {data.TPag}   Valor: {data.VPag}");

        y -= NFeConstants.SectionSpacing;

        // ── Footer ──────────────────────────────────────────────────────
        canvas.DrawLine(x, y + NFeConstants.FooterLineSpacing, NFeConstants.PageWidth, y + NFeConstants.FooterLineSpacing);
        y -= NFeConstants.FooterLineSpacing;
        canvas.DrawText(
            "Documento gerado em ambiente de homologação - Sem valor fiscal",
            x, y, NFeConstants.FooterFontSize);

        pdf.Save(outputPath);
    }

    /// <summary>
    /// Draws an info line on the canvas and advances the Y position.
    /// </summary>
    private static void DrawInfoLine(PdfCanvas canvas, ref double y, string text)
    {
        canvas.DrawText(text, NFeConstants.DefaultMarginX, y, NFeConstants.DefaultFontSize);
        y -= NFeConstants.DefaultLineHeight;
    }

    /// <summary>
    /// Draws a section title on the canvas and advances the Y position.
    /// </summary>
    private static void DrawSection(PdfCanvas canvas, ref double y, string title)
    {
        canvas.DrawText(title, NFeConstants.DefaultMarginX, y, NFeConstants.SectionFontSize);
        y -= NFeConstants.DefaultLineHeight;
    }
}
