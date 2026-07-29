using PdfDocument;

namespace PdfDocument.CTe;

/// <summary>
/// Renders CTe data into a PDF document (DACTE).
/// </summary>
public sealed class CTeRenderer : ILayoutRenderer<CTeData>
{
    /// <summary>
    /// Generates a DACTE PDF from CTe data and saves it to the specified path.
    /// </summary>
    /// <param name="data">CTe data extracted from XML.</param>
    /// <param name="outputPath">Output PDF file path.</param>
    /// <exception cref="ArgumentNullException">If any parameter is null.</exception>
    public void Render(CTeData data, string outputPath)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(outputPath);

        using var pdf = new PdfBuilder();
        var page = pdf.AddPage();
        var canvas = page.Canvas;

        double x = CTeConstants.DefaultMarginX;
        double y = CTeConstants.DefaultMarginY;

        // ── Header ──────────────────────────────────────────────────────
        canvas.DrawText(
            "DACTE - Documento Auxiliar do Conhecimento de Transporte Eletrônico",
            x, y, CTeConstants.TitleFontSize);
        y -= CTeConstants.TitleSpacing;

        // Divider line
        canvas.DrawLine(x, y, CTeConstants.PageWidth, y);
        y -= CTeConstants.DividerSpacing;

        // ── CTe data ────────────────────────────────────────────────────
        DrawInfoLine(canvas, ref y, $"Natureza da Operação: {data.NatOp}");
        DrawInfoLine(canvas, ref y, $"CTe: {data.Mod}   Série: {data.Serie}   Número: {data.NCt}");
        DrawInfoLine(canvas, ref y, $"Emissão: {data.DhEmi}   Ambiente: {data.TpAmb}");
        DrawInfoLine(canvas, ref y,
            $"Tipo CTe: {data.TpCte}   Modal: {data.Modal}   Serviço: {data.TpServ}");

        y -= CTeConstants.SectionSpacing;

        // ── Issuer (Transportadora) ─────────────────────────────────────
        DrawSection(canvas, ref y, "--- EMITENTE (TRANSPORTADORA) ---");
        DrawInfoLine(canvas, ref y, $"CNPJ: {data.EmitCnpj}   IE: {data.EmitIe}");
        DrawInfoLine(canvas, ref y, data.EmitXNome);
        DrawInfoLine(canvas, ref y,
            $"{data.EmitXLogr}, {data.EmitNro} - {data.EmitXBairro} - {data.EmitXMun}/{data.EmitUf}");
        if (!string.IsNullOrEmpty(data.EmitFone))
            DrawInfoLine(canvas, ref y, $"Fone: {data.EmitFone}");

        y -= CTeConstants.SectionSpacing;

        // ── Route: Origin → Destination ─────────────────────────────────
        DrawSection(canvas, ref y, "--- PERCURSO ---");
        DrawInfoLine(canvas, ref y, $"Origem:  {data.XMunIni}/{data.UFIni}");
        DrawInfoLine(canvas, ref y, $"Destino: {data.XMunFim}/{data.UFFim}");
        DrawInfoLine(canvas, ref y, $"Município do Envio: {data.XMunEnv}/{data.UFEnv}");

        y -= CTeConstants.SectionSpacing;

        // ── Sender (Remetente) ──────────────────────────────────────────
        DrawSection(canvas, ref y, "--- REMETENTE ---");
        DrawInfoLine(canvas, ref y, $"CPF/CNPJ: {data.RemCpfCnpj}   IE: {data.RemIe}");
        DrawInfoLine(canvas, ref y, data.RemXNome);
        DrawInfoLine(canvas, ref y,
            $"{data.RemXLogr}, {data.RemNro} - {data.RemXBairro} - {data.RemXMun}/{data.RemUf}");
        if (!string.IsNullOrEmpty(data.RemFone))
            DrawInfoLine(canvas, ref y, $"Fone: {data.RemFone}");

        y -= CTeConstants.SectionSpacing;

        // ── Recipient (Destinatário) ────────────────────────────────────
        DrawSection(canvas, ref y, "--- DESTINATÁRIO ---");
        DrawInfoLine(canvas, ref y, $"CNPJ: {data.DestCnpj}   IE: {data.DestIe}");
        DrawInfoLine(canvas, ref y, data.DestXNome);
        DrawInfoLine(canvas, ref y,
            $"{data.DestXLogr}, {data.DestNro} - {data.DestXBairro} - {data.DestXMun}/{data.DestUf}");
        if (!string.IsNullOrEmpty(data.DestFone))
            DrawInfoLine(canvas, ref y, $"Fone: {data.DestFone}");

        y -= CTeConstants.SectionSpacing;

        // ── Cargo ───────────────────────────────────────────────────────
        DrawSection(canvas, ref y, "--- CARGA ---");
        DrawInfoLine(canvas, ref y,
            $"Produto Predominante: {data.ProPred}   Categoria: {data.XOutCat}");
        DrawInfoLine(canvas, ref y,
            $"Quantidade: {data.QCarga}   Unidade: {data.TpMed} ({data.CUnid})");
        DrawInfoLine(canvas, ref y, $"Valor da Carga: {data.VCarga}");

        y -= CTeConstants.SectionSpacing;

        // ── Documents ────────────────────────────────────────────────────
        DrawSection(canvas, ref y, "--- DOCUMENTOS FISCAIS ---");
        DrawInfoLine(canvas, ref y,
            $"Tipo: {data.TpDoc}   Descrição: {data.DescOutros}");
        DrawInfoLine(canvas, ref y,
            $"Número: {data.NDoc}   Emissão: {data.DEmi}   Valor: {data.VDocFisc}");

        y -= CTeConstants.SectionSpacing;

        // ── Values ───────────────────────────────────────────────────────
        DrawSection(canvas, ref y, "--- VALORES ---");
        DrawInfoLine(canvas, ref y,
            $"Valor Total do Serviço: {data.VTPrest}   Valor a Receber: {data.VRec}");

        y -= CTeConstants.SectionSpacing;

        // ── Tax ──────────────────────────────────────────────────────────
        DrawSection(canvas, ref y, "--- IMPOSTOS ---");
        DrawInfoLine(canvas, ref y,
            $"CST: {data.Cst}   Simples Nacional: {(data.IndSN == "1" ? "Sim" : "Não")}");

        y -= CTeConstants.SectionSpacing;

        // ── Road Transport ───────────────────────────────────────────────
        if (!string.IsNullOrEmpty(data.Rntrc))
        {
            DrawSection(canvas, ref y, "--- TRANSPORTE RODOVIÁRIO ---");
            DrawInfoLine(canvas, ref y, $"RNTRC: {data.Rntrc}");

            y -= CTeConstants.SectionSpacing;
        }

        // ── Additional Info ──────────────────────────────────────────────
        if (!string.IsNullOrEmpty(data.XObs))
        {
            DrawSection(canvas, ref y, "--- OBSERVAÇÕES ---");
            DrawInfoLine(canvas, ref y, data.XObs);

            y -= CTeConstants.SectionSpacing;
        }

        // ── Footer ──────────────────────────────────────────────────────
        canvas.DrawLine(x, y + CTeConstants.FooterLineSpacing,
            CTeConstants.PageWidth, y + CTeConstants.FooterLineSpacing);
        y -= CTeConstants.FooterLineSpacing;
        canvas.DrawText(
            "Documento gerado em ambiente de homologação - Sem valor fiscal",
            x, y, CTeConstants.FooterFontSize);

        pdf.Save(outputPath);
    }

    /// <summary>
    /// Draws an info line on the canvas and advances the Y position.
    /// </summary>
    private static void DrawInfoLine(PdfCanvas canvas, ref double y, string text)
    {
        canvas.DrawText(text, CTeConstants.DefaultMarginX, y, CTeConstants.DefaultFontSize);
        y -= CTeConstants.DefaultLineHeight;
    }

    /// <summary>
    /// Draws a section title on the canvas and advances the Y position.
    /// </summary>
    private static void DrawSection(PdfCanvas canvas, ref double y, string title)
    {
        canvas.DrawText(title, CTeConstants.DefaultMarginX, y, CTeConstants.SectionFontSize);
        y -= CTeConstants.DefaultLineHeight;
    }
}
