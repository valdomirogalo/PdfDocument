using System.Xml.Linq;
using System.Xml;
using PdfDocument;

namespace PdfDocument.CTe;

/// <summary>
/// Parser for Conhecimento de Transporte Eletrônico (CTe) XML files in 3.00 format.
/// Extracts the data needed for DACTE generation.
/// Uses XDocument (LINQ to XML) instead of XmlDocument for lower memory
/// footprint and faster queries — no XPath overhead per field.
/// </summary>
public sealed class CTeParser : IDataParser<CTeData>
{
    private static readonly XNamespace Ns = "http://www.portalfiscal.inf.br/cte";

    /// <summary>
    /// Checks whether this parser can handle the given file.
    /// Returns true for .xml files that contain CTe namespace.
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

        // Deeper check: verify it contains CTe namespace
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
                        || reader.Name == "CTe"
                        || reader.Name == "infCte";
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
    /// Loads and extracts data from a CTe XML file.
    /// </summary>
    public CTeData Parse(string xmlPath)
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

        XElement? infCte = doc.Descendants(Ns + "infCte").FirstOrDefault()
            ?? throw new InvalidOperationException(
                "Could not find infCte node in XML. Verify the XML is a valid CTe.");

        XElement? ide = infCte.Element(Ns + "ide");

        // Elements inside infModal or directly under infCte — use Descendants
        // to handle both structural variants of the CTe XML layout.
        XElement? compl = infCte.Descendants(Ns + "compl").FirstOrDefault();
        XElement? emit = infCte.Descendants(Ns + "emit").FirstOrDefault();
        XElement? rem = infCte.Descendants(Ns + "rem").FirstOrDefault();
        XElement? dest = infCte.Descendants(Ns + "dest").FirstOrDefault();
        XElement? vPrest = infCte.Descendants(Ns + "vPrest").FirstOrDefault();
        XElement? imp = infCte.Descendants(Ns + "imp").FirstOrDefault();
        XElement? infCTeNorm = infCte.Descendants(Ns + "infCTeNorm").FirstOrDefault();
        XElement? infCarga = infCTeNorm?.Element(Ns + "infCarga");
        XElement? infDoc = infCTeNorm?.Element(Ns + "infDoc");
        XElement? infModal = infCTeNorm?.Element(Ns + "infModal");
        XElement? rodo = infModal?.Element(Ns + "rodo");

        return new CTeData
        {
            // ── Identification ───────────────────────────────────────
            CUf = Field(ide, "cUF"),
            CCT = Field(ide, "cCT"),
            Cfop = Field(ide, "CFOP"),
            NatOp = Field(ide, "natOp"),
            Mod = Field(ide, "mod"),
            Serie = Field(ide, "serie"),
            NCt = Field(ide, "nCT"),
            DhEmi = Field(ide, "dhEmi"),
            TpImp = Field(ide, "tpImp"),
            TpEmis = Field(ide, "tpEmis"),
            CDv = Field(ide, "cDV"),
            TpAmb = Field(ide, "tpAmb"),
            TpCte = Field(ide, "tpCTe"),
            ProcEmi = Field(ide, "procEmi"),
            VerProc = Field(ide, "verProc"),
            CMunEnv = Field(ide, "cMunEnv"),
            XMunEnv = Field(ide, "xMunEnv"),
            UFEnv = Field(ide, "UFEnv"),
            Modal = Field(ide, "modal"),
            TpServ = Field(ide, "tpServ"),
            CMunIni = Field(ide, "cMunIni"),
            XMunIni = Field(ide, "xMunIni"),
            UFIni = Field(ide, "UFIni"),
            CMunFim = Field(ide, "cMunFim"),
            XMunFim = Field(ide, "xMunFim"),
            UFFim = Field(ide, "UFFim"),
            Retira = Field(ide, "retira"),
            IndIEToma = Field(ide, "indIEToma"),
            Toma = SubField(ide, "toma3", "toma"),

            // ── Complementary Info ───────────────────────────────────
            XEmi = Field(compl, "xEmi"),
            Fluxo = Field(compl, "fluxo"),
            XObs = Field(compl, "xObs"),

            // ── Issuer ───────────────────────────────────────────────
            EmitCnpj = Field(emit, "CNPJ"),
            EmitIe = Field(emit, "IE"),
            EmitXNome = Field(emit, "xNome"),
            EmitXFant = Field(emit, "xFant"),
            EmitXLogr = SubField(emit, "enderEmit", "xLgr"),
            EmitNro = SubField(emit, "enderEmit", "nro"),
            EmitXBairro = SubField(emit, "enderEmit", "xBairro"),
            EmitCMun = SubField(emit, "enderEmit", "cMun"),
            EmitXMun = SubField(emit, "enderEmit", "xMun"),
            EmitCep = SubField(emit, "enderEmit", "CEP"),
            EmitUf = SubField(emit, "enderEmit", "UF"),
            EmitFone = SubField(emit, "enderEmit", "fone"),

            // ── Sender ───────────────────────────────────────────────
            RemCpfCnpj = Field(rem, "CPF") != "" ? Field(rem, "CPF") : Field(rem, "CNPJ"),
            RemIe = Field(rem, "IE"),
            RemXNome = Field(rem, "xNome"),
            RemFone = Field(rem, "fone"),
            RemXLogr = SubField(rem, "enderReme", "xLgr"),
            RemNro = SubField(rem, "enderReme", "nro"),
            RemXBairro = SubField(rem, "enderReme", "xBairro"),
            RemCMun = SubField(rem, "enderReme", "cMun"),
            RemXMun = SubField(rem, "enderReme", "xMun"),
            RemCep = SubField(rem, "enderReme", "CEP"),
            RemUf = SubField(rem, "enderReme", "UF"),
            RemCPais = SubField(rem, "enderReme", "cPais"),
            RemXPais = SubField(rem, "enderReme", "xPais"),

            // ── Recipient ─────────────────────────────────────────────
            DestCnpj = Field(dest, "CNPJ"),
            DestIe = Field(dest, "IE"),
            DestXNome = Field(dest, "xNome"),
            DestFone = Field(dest, "fone"),
            DestXLogr = SubField(dest, "enderDest", "xLgr"),
            DestNro = SubField(dest, "enderDest", "nro"),
            DestXBairro = SubField(dest, "enderDest", "xBairro"),
            DestCMun = SubField(dest, "enderDest", "cMun"),
            DestXMun = SubField(dest, "enderDest", "xMun"),
            DestCep = SubField(dest, "enderDest", "CEP"),
            DestUf = SubField(dest, "enderDest", "UF"),
            DestCPais = SubField(dest, "enderDest", "cPais"),
            DestXPais = SubField(dest, "enderDest", "xPais"),

            // ── Service Values ───────────────────────────────────────
            VTPrest = Field(vPrest, "vTPrest"),
            VRec = Field(vPrest, "vRec"),

            // ── Tax (ICMS) ───────────────────────────────────────────
            Cst = ParseCst(imp),
            IndSN = SubSubField(imp, "ICMS", "ICMSSN", "indSN"),

            // ── Cargo ────────────────────────────────────────────────
            VCarga = Field(infCarga, "vCarga"),
            ProPred = Field(infCarga, "proPred"),
            XOutCat = Field(infCarga, "xOutCat"),
            CUnid = SubField(infCarga, "infQ", "cUnid"),
            TpMed = SubField(infCarga, "infQ", "tpMed"),
            QCarga = SubField(infCarga, "infQ", "qCarga"),

            // ── Documents ────────────────────────────────────────────
            TpDoc = SubField(infDoc, "infOutros", "tpDoc"),
            DescOutros = SubField(infDoc, "infOutros", "descOutros"),
            NDoc = SubField(infDoc, "infOutros", "nDoc"),
            DEmi = SubField(infDoc, "infOutros", "dEmi"),
            VDocFisc = SubField(infDoc, "infOutros", "vDocFisc"),

            // ── Road Transport ───────────────────────────────────────
            Rntrc = Field(rodo, "RNTRC"),
        };
    }

    /// <summary>Reads a child field value from a parent element, or empty string.</summary>
    private static string Field(XElement? parent, string child)
        => (string?)parent?.Element(Ns + child) ?? "";

    /// <summary>Reads a nested field: parent → middle → child.</summary>
    private static string SubField(XElement? parent, string middle, string child)
        => (string?)parent?.Element(Ns + middle)?.Element(Ns + child) ?? "";

    /// <summary>Reads a deeply nested field: parent → m1 → m2 → child.</summary>
    private static string SubSubField(XElement? parent, string m1, string m2, string child)
        => (string?)parent?.Element(Ns + m1)?.Element(Ns + m2)?.Element(Ns + child) ?? "";

    /// <summary>Parses CST from ICMS/ICMSSN.</summary>
    private static string ParseCst(XElement? imp)
    {
        if (imp == null) return "";
        var icms = imp.Element(Ns + "ICMS");
        if (icms == null) return "";
        return (string?)icms.Element(Ns + "ICMSSN")?.Element(Ns + "CST")
            ?? (string?)icms.Element(Ns + "ICMS00")?.Element(Ns + "CST")
            ?? (string?)icms.Element(Ns + "ICMS20")?.Element(Ns + "CST")
            ?? (string?)icms.Element(Ns + "ICMS45")?.Element(Ns + "CST")
            ?? (string?)icms.Element(Ns + "ICMS60")?.Element(Ns + "CST")
            ?? (string?)icms.Element(Ns + "ICMS90")?.Element(Ns + "CST")
            ?? "";
    }
}
