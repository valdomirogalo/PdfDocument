namespace PdfDocument.NFe;

/// <summary>
/// Data extracted from a Nota Fiscal Eletrônica (NFe) XML for DANFE generation.
/// </summary>
public sealed record NFeData
{
    // ── Identification ───────────────────────────────────────────────
    public string CUf { get; init; } = "";
    public string NatOp { get; init; } = "";
    public string Mod { get; init; } = "";
    public string Serie { get; init; } = "";
    public string Nnf { get; init; } = "";
    public string DhEmi { get; init; } = "";
    public string TpNf { get; init; } = "";
    public string IdDest { get; init; } = "";
    public string CMunFg { get; init; } = "";
    public string TpAmb { get; init; } = "";
    public string FinNfe { get; init; } = "";
    public string IndFinal { get; init; } = "";
    public string IndPres { get; init; } = "";

    // ── Issuer (Emitente) ───────────────────────────────────────────
    public string EmitCnpj { get; init; } = "";
    public string EmitXNome { get; init; } = "";
    public string EmitXFant { get; init; } = "";
    public string EmitIe { get; init; } = "";
    public string EmitCrt { get; init; } = "";
    public string EmitXLogr { get; init; } = "";
    public string EmitNro { get; init; } = "";
    public string EmitXBairro { get; init; } = "";
    public string EmitCMun { get; init; } = "";
    public string EmitXMun { get; init; } = "";
    public string EmitUf { get; init; } = "";
    public string EmitCep { get; init; } = "";

    // ── Recipient (Destinatário) ────────────────────────────────────
    public string DestCnpj { get; init; } = "";
    public string DestXNome { get; init; } = "";
    public string DestXLogr { get; init; } = "";
    public string DestNro { get; init; } = "";
    public string DestXBairro { get; init; } = "";
    public string DestCMun { get; init; } = "";
    public string DestXMun { get; init; } = "";
    public string DestUf { get; init; } = "";
    public string DestIe { get; init; } = "";

    // ── Product (first item) ────────────────────────────────────────
    public string CProd { get; init; } = "";
    public string XProd { get; init; } = "";
    public string Ncm { get; init; } = "";
    public string Cfop { get; init; } = "";
    public string UCom { get; init; } = "";
    public string QCom { get; init; } = "";
    public string VUnCom { get; init; } = "";
    public string VProd { get; init; } = "";

    // ── Totals ──────────────────────────────────────────────────────
    public string VBc { get; init; } = "";
    public string VIcms { get; init; } = "";
    public string VProdTotal { get; init; } = "";
    public string VNf { get; init; } = "";

    // ── Carrier (Transportadora) ────────────────────────────────────
    public string TransCnpj { get; init; } = "";
    public string TransXNome { get; init; } = "";
    public string TransIe { get; init; } = "";
    public string TransXEnder { get; init; } = "";
    public string TransXMun { get; init; } = "";
    public string TransUf { get; init; } = "";

    // ── Payment ─────────────────────────────────────────────────────
    public string TPag { get; init; } = "";
    public string VPag { get; init; } = "";
}
