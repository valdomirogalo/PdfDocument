using System.Xml.Linq;
using System.Xml;
using PdfDocument;

namespace PdfDocument.NFe;

/// <summary>
/// Parser for Nota Fiscal Eletrônica (NFe) XML files in 4.00 format.
/// Extracts the data needed for DANFE generation (expanded layout).
/// Uses XDocument (LINQ to XML) instead of XmlDocument for lower memory
/// footprint and faster queries — no XPath overhead per field.
/// </summary>
public sealed class NFeParser : IDataParser<NFeData>
{
    private static readonly XNamespace Ns = "http://www.portalfiscal.inf.br/nfe";

    /// <summary>
    /// Checks whether this parser can handle the given file.
    /// Returns true for .xml files that contain NFe namespace.
    /// </summary>
    public bool CanParse(string inputPath)
    {
        if (string.IsNullOrEmpty(inputPath))
            return false;

        if (!File.Exists(inputPath))
            return false;

        // Quick check: only accept .xml extension
        if (!inputPath.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            return false;

        // Deeper check: verify it contains NFe namespace
        try
        {
            using var reader = XmlReader.Create(inputPath, new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            });
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    return reader.NamespaceURI == Ns.NamespaceName
                        || reader.Name == "NFe"
                        || reader.Name == "infNFe";
                }
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    /// <summary>
    /// Loads and extracts data from an NFe XML file.
    /// </summary>
    public NFeData Parse(string xmlPath)
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
        XElement? ide = infNFe.Element(Ns + "ide");
        XElement? emit = infNFe.Element(Ns + "emit");
        XElement? dest = infNFe.Element(Ns + "dest");
        XElement? det = infNFe.Element(Ns + "det");
        XElement? total = infNFe.Element(Ns + "total");
        XElement? transp = infNFe.Element(Ns + "transp");
        XElement? pag = infNFe.Element(Ns + "pag");
        XElement? infAdic = infNFe.Element(Ns + "infAdic");

        return new NFeData
        {
            // ── Identification ─────────────────────────────────────────
            CUf = Field(ide, "cUF"),
            NatOp = Field(ide, "natOp"),
            Mod = Field(ide, "mod"),
            Serie = Field(ide, "serie"),
            Nnf = Field(ide, "nNF"),
            DhEmi = Field(ide, "dhEmi"),
            DhSaiEnt = Field(ide, "dhSaiEnt"),
            TpNf = Field(ide, "tpNF"),
            IdDest = Field(ide, "idDest"),
            CMunFg = Field(ide, "cMunFG"),
            TpAmb = Field(ide, "tpAmb"),
            FinNfe = Field(ide, "finNFe"),
            IndFinal = Field(ide, "indFinal"),
            IndPres = Field(ide, "indPres"),

            // ── Protocol ───────────────────────────────────────────────
            NProt = SubField(prot, "infProt", "nProt"),
            DhAutor = SubField(prot, "infProt", "dhRecbto"),

            // ── Issuer ─────────────────────────────────────────────────
            EmitCnpj = Field(emit, "CNPJ"),
            EmitXNome = Field(emit, "xNome"),
            EmitXFant = Field(emit, "xFant"),
            EmitIe = Field(emit, "IE"),
            EmitIeSt = Field(emit, "IEST"),
            EmitCrt = Field(emit, "CRT"),
            EmitXLogr = SubField(emit, "enderEmit", "xLgr"),
            EmitNro = SubField(emit, "enderEmit", "nro"),
            EmitXBairro = SubField(emit, "enderEmit", "xBairro"),
            EmitCMun = SubField(emit, "enderEmit", "cMun"),
            EmitXMun = SubField(emit, "enderEmit", "xMun"),
            EmitUf = SubField(emit, "enderEmit", "UF"),
            EmitCep = SubField(emit, "enderEmit", "CEP"),

            // ── Recipient ──────────────────────────────────────────────
            DestCnpj = Field(dest, "CNPJ"),
            DestXNome = Field(dest, "xNome"),
            DestXLogr = SubField(dest, "enderDest", "xLgr"),
            DestNro = SubField(dest, "enderDest", "nro"),
            DestXBairro = SubField(dest, "enderDest", "xBairro"),
            DestCMun = SubField(dest, "enderDest", "cMun"),
            DestXMun = SubField(dest, "enderDest", "xMun"),
            DestUf = SubField(dest, "enderDest", "UF"),
            DestIe = Field(dest, "IE"),
            DestFone = SubField(dest, "enderDest", "fone"),

            // ── Product (first item) ───────────────────────────────────
            CProd = SubField(det, "prod", "cProd"),
            XProd = SubField(det, "prod", "xProd"),
            Ncm = SubField(det, "prod", "NCM"),
            Cst = ParseCst(det),
            Cfop = SubField(det, "prod", "CFOP"),
            UCom = SubField(det, "prod", "uCom"),
            QCom = SubField(det, "prod", "qCom"),
            VUnCom = SubField(det, "prod", "vUnCom"),
            VProd = SubField(det, "prod", "vProd"),
            VProdDesc = SubField(det, "prod", "vDesc"),
            VBcProd = SubSubField(det, "imposto", "ICMS", "vBC"),
            VIcmsProd = SubSubField(det, "imposto", "ICMS", "vICMS"),
            VIpiProd = SubSubField(det, "imposto", "IPI", "vIPI"),
            PIcms = SubSubField(det, "imposto", "ICMS", "pICMS"),
            PIpi = SubSubField(det, "imposto", "IPI", "pIPI"),

            // ── Totals ──────────────────────────────────────────────────
            VBc = SubField(total, "ICMSTot", "vBC"),
            VIcms = SubField(total, "ICMSTot", "vICMS"),
            VBcSt = SubField(total, "ICMSTot", "vBCST"),
            VSt = SubField(total, "ICMSTot", "vST"),
            VProdTotal = SubField(total, "ICMSTot", "vProd"),
            VFrete = SubField(total, "ICMSTot", "vFrete"),
            VSeg = SubField(total, "ICMSTot", "vSeg"),
            VDesc = SubField(total, "ICMSTot", "vDesc"),
            VOutro = SubField(total, "ICMSTot", "vOutro"),
            VIpi = SubField(total, "ICMSTot", "vIPI"),
            VNf = SubField(total, "ICMSTot", "vNF"),
            VAproxTrib = SubField(total, "ICMSTot", "vTotTrib"),

            // ── Carrier ─────────────────────────────────────────────────
            TransCnpj = SubField(transp, "transporta", "CNPJ"),
            TransXNome = SubField(transp, "transporta", "xNome"),
            TransIe = SubField(transp, "transporta", "IE"),
            TransXEnder = SubField(transp, "transporta", "xEnder"),
            TransXMun = SubField(transp, "transporta", "xMun"),
            TransUf = SubField(transp, "transporta", "UF"),
            ModFrete = Field(transp, "modFrete"),
            TransPlaca = SubField(transp, "veicTransp", "placa"),
            TransUFVeic = SubField(transp, "veicTransp", "UF"),
            TransAntt = SubFieldOrNull(transp, "reboque", "RNTC")
                     ?? SubField(transp, "veicTransp", "RNTC"),
            TransQVol = SubField(transp, "vol", "qVol"),
            TransEspecie = SubField(transp, "vol", "esp"),
            TransMarca = SubField(transp, "vol", "marca"),
            TransNumVol = SubField(transp, "vol", "nVol"),
            TransPesoB = SubField(transp, "vol", "pesoB"),
            TransPesoL = SubField(transp, "vol", "pesoL"),

            // ── Payment ─────────────────────────────────────────────────
            TPag = SubField(pag, "detPag", "tPag"),
            VPag = SubField(pag, "detPag", "vPag"),

            // ── Additional Info ─────────────────────────────────────────
            InfCpl = SubField(infAdic, "infCpl"),
            InfAdic = SubField(infAdic, "infAdFisco"),
        };
    }

    /// <summary>Reads a child field value from a parent element, or empty string.</summary>
    private static string Field(XElement? parent, string child)
        => (string?)parent?.Element(Ns + child) ?? "";

    /// <summary>Reads a nested field: parent → middle → child.</summary>
    private static string SubField(XElement? parent, string middle, string child)
        => (string?)parent?.Element(Ns + middle)?.Element(Ns + child) ?? "";

    /// <summary>Reads a nested field returning null instead of empty string.</summary>
    private static string? SubFieldOrNull(XElement? parent, string middle, string child)
        => (string?)parent?.Element(Ns + middle)?.Element(Ns + child);

    /// <summary>Reads a deeply nested field: parent → m1 → m2 → child.</summary>
    private static string SubSubField(XElement? parent, string m1, string m2, string child)
        => (string?)parent?.Element(Ns + m1)?.Element(Ns + m2)?.Element(Ns + child) ?? "";

    /// <summary>Reads a single-level subfield (middle is optional).</summary>
    private static string SubField(XElement? parent, string child)
        => (string?)parent?.Element(Ns + child) ?? "";

    /// <summary>Parses CST from ICMS or IPI.</summary>
    private static string ParseCst(XElement? det)
    {
        if (det == null) return "";
        var imposto = det.Element(Ns + "imposto");
        if (imposto == null) return "";
        return (string?)imposto.Element(Ns + "ICMS")?.Element(Ns + "CST")
            ?? (string?)imposto.Element(Ns + "IPI")?.Element(Ns + "CST")
            ?? "";
    }
}
