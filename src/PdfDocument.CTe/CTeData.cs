using PdfDocument;

namespace PdfDocument.CTe;

/// <summary>
/// Data extracted from a Conhecimento de Transporte Eletrônico (CTe) XML for DACTE generation.
/// </summary>
public sealed record CTeData : IPdfData
{
    // ── Identification ───────────────────────────────────────────────
    public string CUf { get; init; } = "";
    public string CCT { get; init; } = "";
    public string Cfop { get; init; } = "";
    public string NatOp { get; init; } = "";
    public string Mod { get; init; } = "";
    public string Serie { get; init; } = "";
    public string NCt { get; init; } = "";
    public string DhEmi { get; init; } = "";
    public string TpImp { get; init; } = "";
    public string TpEmis { get; init; } = "";
    public string CDv { get; init; } = "";
    public string TpAmb { get; init; } = "";
    public string TpCte { get; init; } = "";
    public string ProcEmi { get; init; } = "";
    public string VerProc { get; init; } = "";
    public string CMunEnv { get; init; } = "";
    public string XMunEnv { get; init; } = "";
    public string UFEnv { get; init; } = "";
    public string Modal { get; init; } = "";
    public string TpServ { get; init; } = "";
    public string CMunIni { get; init; } = "";
    public string XMunIni { get; init; } = "";
    public string UFIni { get; init; } = "";
    public string CMunFim { get; init; } = "";
    public string XMunFim { get; init; } = "";
    public string UFFim { get; init; } = "";
    public string Retira { get; init; } = "";
    public string IndIEToma { get; init; } = "";
    public string Toma { get; init; } = "";

    // ── Complementary Info ───────────────────────────────────────────
    public string XEmi { get; init; } = "";
    public string Fluxo { get; init; } = "";
    public string XObs { get; init; } = "";

    // ── Issuer (Transport Company) ────────────────────────────────────
    public string EmitCnpj { get; init; } = "";
    public string EmitIe { get; init; } = "";
    public string EmitXNome { get; init; } = "";
    public string EmitXFant { get; init; } = "";
    public string EmitXLogr { get; init; } = "";
    public string EmitNro { get; init; } = "";
    public string EmitXBairro { get; init; } = "";
    public string EmitCMun { get; init; } = "";
    public string EmitXMun { get; init; } = "";
    public string EmitCep { get; init; } = "";
    public string EmitUf { get; init; } = "";
    public string EmitFone { get; init; } = "";

    // ── Sender ───────────────────────────────────────────────────────
    public string RemCpfCnpj { get; init; } = "";
    public string RemIe { get; init; } = "";
    public string RemXNome { get; init; } = "";
    public string RemFone { get; init; } = "";
    public string RemXLogr { get; init; } = "";
    public string RemNro { get; init; } = "";
    public string RemXBairro { get; init; } = "";
    public string RemCMun { get; init; } = "";
    public string RemXMun { get; init; } = "";
    public string RemCep { get; init; } = "";
    public string RemUf { get; init; } = "";
    public string RemCPais { get; init; } = "";
    public string RemXPais { get; init; } = "";

    // ── Recipient ────────────────────────────────────────────────────
    public string DestCnpj { get; init; } = "";
    public string DestIe { get; init; } = "";
    public string DestXNome { get; init; } = "";
    public string DestFone { get; init; } = "";
    public string DestXLogr { get; init; } = "";
    public string DestNro { get; init; } = "";
    public string DestXBairro { get; init; } = "";
    public string DestCMun { get; init; } = "";
    public string DestXMun { get; init; } = "";
    public string DestCep { get; init; } = "";
    public string DestUf { get; init; } = "";
    public string DestCPais { get; init; } = "";
    public string DestXPais { get; init; } = "";

    // ── Service Values ───────────────────────────────────────────────
    public string VTPrest { get; init; } = "";
    public string VRec { get; init; } = "";

    // ── Tax (ICMS) ───────────────────────────────────────────────────
    public string Cst { get; init; } = "";
    public string IndSN { get; init; } = "";

    // ── Cargo ────────────────────────────────────────────────────────
    public string VCarga { get; init; } = "";
    public string ProPred { get; init; } = "";
    public string XOutCat { get; init; } = "";
    public string CUnid { get; init; } = "";
    public string TpMed { get; init; } = "";
    public string QCarga { get; init; } = "";

    // ── Documents ────────────────────────────────────────────────────
    public string TpDoc { get; init; } = "";
    public string DescOutros { get; init; } = "";
    public string NDoc { get; init; } = "";
    public string DEmi { get; init; } = "";
    public string VDocFisc { get; init; } = "";

    // ── Road Transport ───────────────────────────────────────────────
    public string Rntrc { get; init; } = "";
}
