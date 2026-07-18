using System.Xml.Linq;
using System.Xml;

namespace PdfDocument.NFe;

/// <summary>
/// Parser for Nota Fiscal Eletrônica (NFe) XML files in 4.00 format.
/// Extracts the data needed for DANFE generation (expanded layout).
/// Uses XDocument (LINQ to XML) instead of XmlDocument for lower memory
/// footprint and faster queries — no XPath overhead per field.
/// </summary>
public static class NFeParser
{
    private static readonly XNamespace Ns = "http://www.portalfiscal.inf.br/nfe";

    /// <summary>
    /// Loads and extracts data from an NFe XML file.
    /// </summary>
    public static NFeData Parse(string xmlPath)
    {
        ArgumentNullException.ThrowIfNull(xmlPath);

        if (!File.Exists(xmlPath))
            throw new FileNotFoundException($"XML file not found: {xmlPath}", xmlPath);

        // CWE-611: disable DTD to prevent XXE attacks
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        };

        using var reader = XmlReader.Create(xmlPath, settings);
        var doc = XDocument.Load(reader);

        XElement? infNFe = doc.Descendants(Ns + "infNFe").FirstOrDefault()
            ?? throw new InvalidOperationException(
                "Could not find infNFe node in XML. Verify the XML is a valid NFe.");

        XElement? prot = doc.Descendants(Ns + "prot").FirstOrDefault();

        return new NFeData
        {
            // ── Identification ─────────────────────────────────────────
            CUf = (string?)infNFe.Element(Ns + "ide")?.Element(Ns + "cUF") ?? "",
            NatOp = (string?)infNFe.Element(Ns + "ide")?.Element(Ns + "natOp") ?? "",
            Mod = (string?)infNFe.Element(Ns + "ide")?.Element(Ns + "mod") ?? "",
            Serie = (string?)infNFe.Element(Ns + "ide")?.Element(Ns + "serie") ?? "",
            Nnf = (string?)infNFe.Element(Ns + "ide")?.Element(Ns + "nNF") ?? "",
            DhEmi = (string?)infNFe.Element(Ns + "ide")?.Element(Ns + "dhEmi") ?? "",
            DhSaiEnt = (string?)infNFe.Element(Ns + "ide")?.Element(Ns + "dhSaiEnt") ?? "",
            TpNf = (string?)infNFe.Element(Ns + "ide")?.Element(Ns + "tpNF") ?? "",
            IdDest = (string?)infNFe.Element(Ns + "ide")?.Element(Ns + "idDest") ?? "",
            CMunFg = (string?)infNFe.Element(Ns + "ide")?.Element(Ns + "cMunFG") ?? "",
            TpAmb = (string?)infNFe.Element(Ns + "ide")?.Element(Ns + "tpAmb") ?? "",
            FinNfe = (string?)infNFe.Element(Ns + "ide")?.Element(Ns + "finNFe") ?? "",
            IndFinal = (string?)infNFe.Element(Ns + "ide")?.Element(Ns + "indFinal") ?? "",
            IndPres = (string?)infNFe.Element(Ns + "ide")?.Element(Ns + "indPres") ?? "",

            // ── Protocol ───────────────────────────────────────────────
            NProt = (string?)prot?.Element(Ns + "infProt")?.Element(Ns + "nProt") ?? "",
            DhAutor = (string?)prot?.Element(Ns + "infProt")?.Element(Ns + "dhRecbto") ?? "",

            // ── Issuer ─────────────────────────────────────────────────
            EmitCnpj = (string?)infNFe.Element(Ns + "emit")?.Element(Ns + "CNPJ") ?? "",
            EmitXNome = (string?)infNFe.Element(Ns + "emit")?.Element(Ns + "xNome") ?? "",
            EmitXFant = (string?)infNFe.Element(Ns + "emit")?.Element(Ns + "xFant") ?? "",
            EmitIe = (string?)infNFe.Element(Ns + "emit")?.Element(Ns + "IE") ?? "",
            EmitIeSt = (string?)infNFe.Element(Ns + "emit")?.Element(Ns + "IEST") ?? "",
            EmitCrt = (string?)infNFe.Element(Ns + "emit")?.Element(Ns + "CRT") ?? "",
            EmitXLogr = (string?)infNFe.Element(Ns + "emit")?.Element(Ns + "enderEmit")?.Element(Ns + "xLgr") ?? "",
            EmitNro = (string?)infNFe.Element(Ns + "emit")?.Element(Ns + "enderEmit")?.Element(Ns + "nro") ?? "",
            EmitXBairro = (string?)infNFe.Element(Ns + "emit")?.Element(Ns + "enderEmit")?.Element(Ns + "xBairro") ?? "",
            EmitCMun = (string?)infNFe.Element(Ns + "emit")?.Element(Ns + "enderEmit")?.Element(Ns + "cMun") ?? "",
            EmitXMun = (string?)infNFe.Element(Ns + "emit")?.Element(Ns + "enderEmit")?.Element(Ns + "xMun") ?? "",
            EmitUf = (string?)infNFe.Element(Ns + "emit")?.Element(Ns + "enderEmit")?.Element(Ns + "UF") ?? "",
            EmitCep = (string?)infNFe.Element(Ns + "emit")?.Element(Ns + "enderEmit")?.Element(Ns + "CEP") ?? "",

            // ── Recipient ──────────────────────────────────────────────
            DestCnpj = (string?)infNFe.Element(Ns + "dest")?.Element(Ns + "CNPJ") ?? "",
            DestXNome = (string?)infNFe.Element(Ns + "dest")?.Element(Ns + "xNome") ?? "",
            DestXLogr = (string?)infNFe.Element(Ns + "dest")?.Element(Ns + "enderDest")?.Element(Ns + "xLgr") ?? "",
            DestNro = (string?)infNFe.Element(Ns + "dest")?.Element(Ns + "enderDest")?.Element(Ns + "nro") ?? "",
            DestXBairro = (string?)infNFe.Element(Ns + "dest")?.Element(Ns + "enderDest")?.Element(Ns + "xBairro") ?? "",
            DestCMun = (string?)infNFe.Element(Ns + "dest")?.Element(Ns + "enderDest")?.Element(Ns + "cMun") ?? "",
            DestXMun = (string?)infNFe.Element(Ns + "dest")?.Element(Ns + "enderDest")?.Element(Ns + "xMun") ?? "",
            DestUf = (string?)infNFe.Element(Ns + "dest")?.Element(Ns + "enderDest")?.Element(Ns + "UF") ?? "",
            DestIe = (string?)infNFe.Element(Ns + "dest")?.Element(Ns + "IE") ?? "",
            DestFone = (string?)infNFe.Element(Ns + "dest")?.Element(Ns + "enderDest")?.Element(Ns + "fone") ?? "",

            // ── Product (first item) ───────────────────────────────────
            CProd = (string?)infNFe.Element(Ns + "det")?.Element(Ns + "prod")?.Element(Ns + "cProd") ?? "",
            XProd = (string?)infNFe.Element(Ns + "det")?.Element(Ns + "prod")?.Element(Ns + "xProd") ?? "",
            Ncm = (string?)infNFe.Element(Ns + "det")?.Element(Ns + "prod")?.Element(Ns + "NCM") ?? "",
            Cst = (string?)infNFe.Element(Ns + "det")?.Element(Ns + "imposto")?
                      .Element(Ns + "ICMS")?.Element(Ns + "CST")
                  ?? (string?)infNFe.Element(Ns + "det")?.Element(Ns + "imposto")?
                      .Element(Ns + "IPI")?.Element(Ns + "CST") ?? "",
            Cfop = (string?)infNFe.Element(Ns + "det")?.Element(Ns + "prod")?.Element(Ns + "CFOP") ?? "",
            UCom = (string?)infNFe.Element(Ns + "det")?.Element(Ns + "prod")?.Element(Ns + "uCom") ?? "",
            QCom = (string?)infNFe.Element(Ns + "det")?.Element(Ns + "prod")?.Element(Ns + "qCom") ?? "",
            VUnCom = (string?)infNFe.Element(Ns + "det")?.Element(Ns + "prod")?.Element(Ns + "vUnCom") ?? "",
            VProd = (string?)infNFe.Element(Ns + "det")?.Element(Ns + "prod")?.Element(Ns + "vProd") ?? "",
            VProdDesc = (string?)infNFe.Element(Ns + "det")?.Element(Ns + "prod")?.Element(Ns + "vDesc") ?? "",
            VBcProd = (string?)infNFe.Element(Ns + "det")?.Element(Ns + "imposto")?
                          .Element(Ns + "ICMS")?.Element(Ns + "vBC") ?? "",
            VIcmsProd = (string?)infNFe.Element(Ns + "det")?.Element(Ns + "imposto")?
                            .Element(Ns + "ICMS")?.Element(Ns + "vICMS") ?? "",
            VIpiProd = (string?)infNFe.Element(Ns + "det")?.Element(Ns + "imposto")?
                           .Element(Ns + "IPI")?.Element(Ns + "vIPI") ?? "",
            PIcms = (string?)infNFe.Element(Ns + "det")?.Element(Ns + "imposto")?
                        .Element(Ns + "ICMS")?.Element(Ns + "pICMS") ?? "",
            PIpi = (string?)infNFe.Element(Ns + "det")?.Element(Ns + "imposto")?
                       .Element(Ns + "IPI")?.Element(Ns + "pIPI") ?? "",

            // ── Totals ──────────────────────────────────────────────────
            VBc = (string?)infNFe.Element(Ns + "total")?.Element(Ns + "ICMSTot")?.Element(Ns + "vBC") ?? "",
            VIcms = (string?)infNFe.Element(Ns + "total")?.Element(Ns + "ICMSTot")?.Element(Ns + "vICMS") ?? "",
            VBcSt = (string?)infNFe.Element(Ns + "total")?.Element(Ns + "ICMSTot")?.Element(Ns + "vBCST") ?? "",
            VSt = (string?)infNFe.Element(Ns + "total")?.Element(Ns + "ICMSTot")?.Element(Ns + "vST") ?? "",
            VProdTotal = (string?)infNFe.Element(Ns + "total")?.Element(Ns + "ICMSTot")?.Element(Ns + "vProd") ?? "",
            VFrete = (string?)infNFe.Element(Ns + "total")?.Element(Ns + "ICMSTot")?.Element(Ns + "vFrete") ?? "",
            VSeg = (string?)infNFe.Element(Ns + "total")?.Element(Ns + "ICMSTot")?.Element(Ns + "vSeg") ?? "",
            VDesc = (string?)infNFe.Element(Ns + "total")?.Element(Ns + "ICMSTot")?.Element(Ns + "vDesc") ?? "",
            VOutro = (string?)infNFe.Element(Ns + "total")?.Element(Ns + "ICMSTot")?.Element(Ns + "vOutro") ?? "",
            VIpi = (string?)infNFe.Element(Ns + "total")?.Element(Ns + "ICMSTot")?.Element(Ns + "vIPI") ?? "",
            VNf = (string?)infNFe.Element(Ns + "total")?.Element(Ns + "ICMSTot")?.Element(Ns + "vNF") ?? "",
            VAproxTrib = (string?)infNFe.Element(Ns + "total")?.Element(Ns + "ICMSTot")?.Element(Ns + "vTotTrib") ?? "",

            // ── Carrier ─────────────────────────────────────────────────
            TransCnpj = (string?)infNFe.Element(Ns + "transp")?.Element(Ns + "transporta")?.Element(Ns + "CNPJ") ?? "",
            TransXNome = (string?)infNFe.Element(Ns + "transp")?.Element(Ns + "transporta")?.Element(Ns + "xNome") ?? "",
            TransIe = (string?)infNFe.Element(Ns + "transp")?.Element(Ns + "transporta")?.Element(Ns + "IE") ?? "",
            TransXEnder = (string?)infNFe.Element(Ns + "transp")?.Element(Ns + "transporta")?.Element(Ns + "xEnder") ?? "",
            TransXMun = (string?)infNFe.Element(Ns + "transp")?.Element(Ns + "transporta")?.Element(Ns + "xMun") ?? "",
            TransUf = (string?)infNFe.Element(Ns + "transp")?.Element(Ns + "transporta")?.Element(Ns + "UF") ?? "",
            ModFrete = (string?)infNFe.Element(Ns + "transp")?.Element(Ns + "modFrete") ?? "",
            TransPlaca = (string?)infNFe.Element(Ns + "transp")?.Element(Ns + "veicTransp")?.Element(Ns + "placa") ?? "",
            TransUFVeic = (string?)infNFe.Element(Ns + "transp")?.Element(Ns + "veicTransp")?.Element(Ns + "UF") ?? "",
            TransAntt = (string?)infNFe.Element(Ns + "transp")?.Element(Ns + "reboque")?.Element(Ns + "RNTC")
                     ?? (string?)infNFe.Element(Ns + "transp")?.Element(Ns + "veicTransp")?.Element(Ns + "RNTC") ?? "",
            TransQVol = (string?)infNFe.Element(Ns + "transp")?.Element(Ns + "vol")?.Element(Ns + "qVol") ?? "",
            TransEspecie = (string?)infNFe.Element(Ns + "transp")?.Element(Ns + "vol")?.Element(Ns + "esp") ?? "",
            TransMarca = (string?)infNFe.Element(Ns + "transp")?.Element(Ns + "vol")?.Element(Ns + "marca") ?? "",
            TransNumVol = (string?)infNFe.Element(Ns + "transp")?.Element(Ns + "vol")?.Element(Ns + "nVol") ?? "",
            TransPesoB = (string?)infNFe.Element(Ns + "transp")?.Element(Ns + "vol")?.Element(Ns + "pesoB") ?? "",
            TransPesoL = (string?)infNFe.Element(Ns + "transp")?.Element(Ns + "vol")?.Element(Ns + "pesoL") ?? "",

            // ── Payment ─────────────────────────────────────────────────
            TPag = (string?)infNFe.Element(Ns + "pag")?.Element(Ns + "detPag")?.Element(Ns + "tPag") ?? "",
            VPag = (string?)infNFe.Element(Ns + "pag")?.Element(Ns + "detPag")?.Element(Ns + "vPag") ?? "",

            // ── Additional Info ─────────────────────────────────────────
            InfCpl = (string?)infNFe.Element(Ns + "infAdic")?.Element(Ns + "infCpl") ?? "",
            InfAdic = (string?)infNFe.Element(Ns + "infAdic")?.Element(Ns + "infAdFisco") ?? "",
        };
    }
}
