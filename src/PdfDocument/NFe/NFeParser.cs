using System.Xml;

namespace PdfDocument.NFe;

/// <summary>
/// Parser for Nota Fiscal Eletrônica (NFe) XML files in 4.00 format.
/// Extracts the data needed for DANFE generation.
/// </summary>
public static class NFeParser
{
    private const string NfeNamespace = "http://www.portalfiscal.inf.br/nfe";
    private const string NfePrefix = "nfe";

    /// <summary>
    /// Loads and extracts data from an NFe XML file.
    /// </summary>
    /// <param name="xmlPath">Path to the XML file.</param>
    /// <returns>Structured NFe data.</returns>
    /// <exception cref="ArgumentNullException">If the path is null.</exception>
    /// <exception cref="FileNotFoundException">If the file does not exist.</exception>
    /// <exception cref="InvalidOperationException">If the infNFe node is not found.</exception>
    public static NFeData Parse(string xmlPath)
    {
        ArgumentNullException.ThrowIfNull(xmlPath);

        if (!File.Exists(xmlPath))
            throw new FileNotFoundException($"XML file not found: {xmlPath}", xmlPath);

        // CWE-611: disable DTD processing to prevent XXE attacks
        var doc = new XmlDocument { XmlResolver = null };
        doc.Load(xmlPath);

        var ns = new XmlNamespaceManager(doc.NameTable);
        ns.AddNamespace(NfePrefix, NfeNamespace);

        XmlNode? infNFe = doc.SelectSingleNode($"//{NfePrefix}:infNFe", ns) ?? throw new InvalidOperationException(
                "Could not find infNFe node in XML. Verify the XML is a valid NFe.");
        return new NFeData
        {
            // ── Identification ───────────────────────────────────────────
            CUf = GetNodeText(infNFe, $"{NfePrefix}:ide/{NfePrefix}:cUF", ns),
            NatOp = GetNodeText(infNFe, $"{NfePrefix}:ide/{NfePrefix}:natOp", ns),
            Mod = GetNodeText(infNFe, $"{NfePrefix}:ide/{NfePrefix}:mod", ns),
            Serie = GetNodeText(infNFe, $"{NfePrefix}:ide/{NfePrefix}:serie", ns),
            Nnf = GetNodeText(infNFe, $"{NfePrefix}:ide/{NfePrefix}:nNF", ns),
            DhEmi = GetNodeText(infNFe, $"{NfePrefix}:ide/{NfePrefix}:dhEmi", ns),
            TpNf = GetNodeText(infNFe, $"{NfePrefix}:ide/{NfePrefix}:tpNF", ns),
            IdDest = GetNodeText(infNFe, $"{NfePrefix}:ide/{NfePrefix}:idDest", ns),
            CMunFg = GetNodeText(infNFe, $"{NfePrefix}:ide/{NfePrefix}:cMunFG", ns),
            TpAmb = GetNodeText(infNFe, $"{NfePrefix}:ide/{NfePrefix}:tpAmb", ns),
            FinNfe = GetNodeText(infNFe, $"{NfePrefix}:ide/{NfePrefix}:finNFe", ns),
            IndFinal = GetNodeText(infNFe, $"{NfePrefix}:ide/{NfePrefix}:indFinal", ns),
            IndPres = GetNodeText(infNFe, $"{NfePrefix}:ide/{NfePrefix}:indPres", ns),

            // ── Issuer ─────────────────────────────────────────────────
            EmitCnpj = GetNodeText(infNFe, $"{NfePrefix}:emit/{NfePrefix}:CNPJ", ns),
            EmitXNome = GetNodeText(infNFe, $"{NfePrefix}:emit/{NfePrefix}:xNome", ns),
            EmitXFant = GetNodeText(infNFe, $"{NfePrefix}:emit/{NfePrefix}:xFant", ns),
            EmitIe = GetNodeText(infNFe, $"{NfePrefix}:emit/{NfePrefix}:IE", ns),
            EmitCrt = GetNodeText(infNFe, $"{NfePrefix}:emit/{NfePrefix}:CRT", ns),
            EmitXLogr = GetNodeText(infNFe, $"{NfePrefix}:emit/{NfePrefix}:enderEmit/{NfePrefix}:xLgr", ns),
            EmitNro = GetNodeText(infNFe, $"{NfePrefix}:emit/{NfePrefix}:enderEmit/{NfePrefix}:nro", ns),
            EmitXBairro = GetNodeText(infNFe, $"{NfePrefix}:emit/{NfePrefix}:enderEmit/{NfePrefix}:xBairro", ns),
            EmitCMun = GetNodeText(infNFe, $"{NfePrefix}:emit/{NfePrefix}:enderEmit/{NfePrefix}:cMun", ns),
            EmitXMun = GetNodeText(infNFe, $"{NfePrefix}:emit/{NfePrefix}:enderEmit/{NfePrefix}:xMun", ns),
            EmitUf = GetNodeText(infNFe, $"{NfePrefix}:emit/{NfePrefix}:enderEmit/{NfePrefix}:UF", ns),
            EmitCep = GetNodeText(infNFe, $"{NfePrefix}:emit/{NfePrefix}:enderEmit/{NfePrefix}:CEP", ns),

            // ── Recipient ──────────────────────────────────────────────
            DestCnpj = GetNodeText(infNFe, $"{NfePrefix}:dest/{NfePrefix}:CNPJ", ns),
            DestXNome = GetNodeText(infNFe, $"{NfePrefix}:dest/{NfePrefix}:xNome", ns),
            DestXLogr = GetNodeText(infNFe, $"{NfePrefix}:dest/{NfePrefix}:enderDest/{NfePrefix}:xLgr", ns),
            DestNro = GetNodeText(infNFe, $"{NfePrefix}:dest/{NfePrefix}:enderDest/{NfePrefix}:nro", ns),
            DestXBairro = GetNodeText(infNFe, $"{NfePrefix}:dest/{NfePrefix}:enderDest/{NfePrefix}:xBairro", ns),
            DestCMun = GetNodeText(infNFe, $"{NfePrefix}:dest/{NfePrefix}:enderDest/{NfePrefix}:cMun", ns),
            DestXMun = GetNodeText(infNFe, $"{NfePrefix}:dest/{NfePrefix}:enderDest/{NfePrefix}:xMun", ns),
            DestUf = GetNodeText(infNFe, $"{NfePrefix}:dest/{NfePrefix}:enderDest/{NfePrefix}:UF", ns),
            DestIe = GetNodeText(infNFe, $"{NfePrefix}:dest/{NfePrefix}:IE", ns),

            // ── Product ─────────────────────────────────────────────────
            CProd = GetNodeText(infNFe, $"{NfePrefix}:det/{NfePrefix}:prod/{NfePrefix}:cProd", ns),
            XProd = GetNodeText(infNFe, $"{NfePrefix}:det/{NfePrefix}:prod/{NfePrefix}:xProd", ns),
            Ncm = GetNodeText(infNFe, $"{NfePrefix}:det/{NfePrefix}:prod/{NfePrefix}:NCM", ns),
            Cfop = GetNodeText(infNFe, $"{NfePrefix}:det/{NfePrefix}:prod/{NfePrefix}:CFOP", ns),
            UCom = GetNodeText(infNFe, $"{NfePrefix}:det/{NfePrefix}:prod/{NfePrefix}:uCom", ns),
            QCom = GetNodeText(infNFe, $"{NfePrefix}:det/{NfePrefix}:prod/{NfePrefix}:qCom", ns),
            VUnCom = GetNodeText(infNFe, $"{NfePrefix}:det/{NfePrefix}:prod/{NfePrefix}:vUnCom", ns),
            VProd = GetNodeText(infNFe, $"{NfePrefix}:det/{NfePrefix}:prod/{NfePrefix}:vProd", ns),

            // ── Totals ──────────────────────────────────────────────────
            VBc = GetNodeText(infNFe, $"{NfePrefix}:total/{NfePrefix}:ICMSTot/{NfePrefix}:vBC", ns),
            VIcms = GetNodeText(infNFe, $"{NfePrefix}:total/{NfePrefix}:ICMSTot/{NfePrefix}:vICMS", ns),
            VProdTotal = GetNodeText(infNFe, $"{NfePrefix}:total/{NfePrefix}:ICMSTot/{NfePrefix}:vProd", ns),
            VNf = GetNodeText(infNFe, $"{NfePrefix}:total/{NfePrefix}:ICMSTot/{NfePrefix}:vNF", ns),

            // ── Carrier ─────────────────────────────────────────────────
            TransCnpj = GetNodeText(infNFe, $"{NfePrefix}:transp/{NfePrefix}:transporta/{NfePrefix}:CNPJ", ns),
            TransXNome = GetNodeText(infNFe, $"{NfePrefix}:transp/{NfePrefix}:transporta/{NfePrefix}:xNome", ns),
            TransIe = GetNodeText(infNFe, $"{NfePrefix}:transp/{NfePrefix}:transporta/{NfePrefix}:IE", ns),
            TransXEnder = GetNodeText(infNFe, $"{NfePrefix}:transp/{NfePrefix}:transporta/{NfePrefix}:xEnder", ns),
            TransXMun = GetNodeText(infNFe, $"{NfePrefix}:transp/{NfePrefix}:transporta/{NfePrefix}:xMun", ns),
            TransUf = GetNodeText(infNFe, $"{NfePrefix}:transp/{NfePrefix}:transporta/{NfePrefix}:UF", ns),

            // ── Payment ─────────────────────────────────────────────────
            TPag = GetNodeText(infNFe, $"{NfePrefix}:pag/{NfePrefix}:detPag/{NfePrefix}:tPag", ns),
            VPag = GetNodeText(infNFe, $"{NfePrefix}:pag/{NfePrefix}:detPag/{NfePrefix}:vPag", ns),
        };
    }

    /// <summary>
    /// Extracts text from a child XML node, returning empty string if not found.
    /// </summary>
    private static string GetNodeText(XmlNode parent, string xpath, XmlNamespaceManager ns)
    {
        var node = parent.SelectSingleNode(xpath, ns);
        return node?.InnerText ?? "";
    }
}
