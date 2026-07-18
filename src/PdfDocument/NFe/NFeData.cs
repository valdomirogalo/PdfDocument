namespace PdfDocument.NFe;

/// <summary>
/// Data extracted from a Nota Fiscal Eletrônica (NFe) XML for DANFE generation.
/// Expanded to support full DANFE reference layout.
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
    public string DhSaiEnt { get; init; } = "";       // dhSaiEnt (saída/entrada)
    public string TpNf { get; init; } = "";
    public string IdDest { get; init; } = "";
    public string CMunFg { get; init; } = "";
    public string TpAmb { get; init; } = "";
    public string FinNfe { get; init; } = "";
    public string IndFinal { get; init; } = "";
    public string IndPres { get; init; } = "";
    public string NProt { get; init; } = "";           // Protocolo de autorização
    public string DhAutor { get; init; } = "";         // Data/hora autorização

    // ── Issuer (Emitente) ───────────────────────────────────────────
    public string EmitCnpj { get; init; } = "";
    public string EmitXNome { get; init; } = "";
    public string EmitXFant { get; init; } = "";
    public string EmitIe { get; init; } = "";
    public string EmitIeSt { get; init; } = "";        // IE do substituto tributário
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
    public string DestFone { get; init; } = "";        // Fone/Fax

    // ── Product (first item) ────────────────────────────────────────
    public string CProd { get; init; } = "";
    public string XProd { get; init; } = "";
    public string Ncm { get; init; } = "";
    public string Cst { get; init; } = "";             // CST do produto
    public string Cfop { get; init; } = "";
    public string UCom { get; init; } = "";
    public string QCom { get; init; } = "";
    public string VUnCom { get; init; } = "";
    public string VProd { get; init; } = "";
    public string VProdDesc { get; init; } = "";       // Desconto por produto
    public string VBcProd { get; init; } = "";         // BC do ICMS do produto
    public string VIcmsProd { get; init; } = "";       // ICMS do produto
    public string VIpiProd { get; init; } = "";        // IPI do produto
    public string PIcms { get; init; } = "";           // Alíquota ICMS
    public string PIpi { get; init; } = "";            // Alíquota IPI

    // ── Totals ──────────────────────────────────────────────────────
    public string VBc { get; init; } = "";
    public string VIcms { get; init; } = "";
    public string VBcSt { get; init; } = "";           // BC ICMS ST
    public string VSt { get; init; } = "";             // ICMS ST
    public string VProdTotal { get; init; } = "";
    public string VFrete { get; init; } = "";
    public string VSeg { get; init; } = "";
    public string VDesc { get; init; } = "";
    public string VOutro { get; init; } = "";
    public string VIpi { get; init; } = "";            // Valor IPI
    public string VNf { get; init; } = "";
    public string VAproxTrib { get; init; } = "";      // Valor aproximado tributos

    // ── Carrier (Transportadora) ────────────────────────────────────
    public string TransCnpj { get; init; } = "";
    public string TransXNome { get; init; } = "";
    public string TransIe { get; init; } = "";
    public string TransXEnder { get; init; } = "";
    public string TransXMun { get; init; } = "";
    public string TransUf { get; init; } = "";
    public string ModFrete { get; init; } = "";        // Modalidade do frete
    public string TransPlaca { get; init; } = "";      // Placa do veículo
    public string TransUFVeic { get; init; } = "";     // UF da placa
    public string TransAntt { get; init; } = "";       // Código ANTT
    public string TransQVol { get; init; } = "";       // Quantidade de volumes
    public string TransEspecie { get; init; } = "";    // Espécie
    public string TransMarca { get; init; } = "";      // Marca
    public string TransNumVol { get; init; } = "";     // Numeração
    public string TransPesoB { get; init; } = "";      // Peso bruto
    public string TransPesoL { get; init; } = "";      // Peso líquido

    // ── Payment ─────────────────────────────────────────────────────
    public string TPag { get; init; } = "";
    public string VPag { get; init; } = "";

    // ── Additional Info ─────────────────────────────────────────────
    public string InfCpl { get; init; } = "";          // Informações complementares
    public string InfAdic { get; init; } = "";         // Informações adicionais do fisco
}
