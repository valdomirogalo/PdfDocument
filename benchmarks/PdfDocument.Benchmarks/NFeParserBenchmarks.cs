#pragma warning disable CA1822 // BenchmarkDotNet requires instance methods
using BenchmarkDotNet.Attributes;
using PdfDocument;
using PdfDocument.NFe;

namespace PdfDocument.Benchmarks;

/// <summary>
/// Benchmarks for NFe XML parsing.
/// </summary>
[MemoryDiagnoser]
[MinColumn, MaxColumn, MeanColumn]
public class NFeParserBenchmarks
{
    private string _xmlPath = "";

    [IterationSetup]
    public void Setup()
    {
        // Creates a temporary XML for benchmarks
        _xmlPath = Path.GetTempFileName();
        File.WriteAllText(_xmlPath, CreateNfeXml());
    }

    [IterationCleanup]
    public void Cleanup()
    {
        if (File.Exists(_xmlPath))
            File.Delete(_xmlPath);
    }

    [Benchmark(Description = "NFeParser.Parse (full)")]
    public NFeData Parse_Full()
    {
        return NFe.NFeParser.Parse(_xmlPath);
    }

    [Benchmark(Description = "NFeParser.Parse (missing fields)")]
    public static NFeData Parse_MissingFields()
    {
        // Creates another XML with missing fields
        string temp = Path.GetTempFileName();
        try
        {
            File.WriteAllText(temp, MinimalNfeXml());
            return NFe.NFeParser.Parse(temp);
        }
        finally
        {
            File.Delete(temp);
        }
    }

    /// <summary>
    /// Creates complete NFe XML for benchmark testing.
    /// </summary>
    private static string CreateNfeXml() => @"<?xml version=""1.0"" encoding=""utf-8""?>
<enviNFe versao=""4.00"" xmlns=""http://www.portalfiscal.inf.br/nfe"">
  <idLote>1</idLote>
  <indSinc>1</indSinc>
  <NFe xmlns=""http://www.portalfiscal.inf.br/nfe"">
    <infNFe Id=""NFe21250911111111000111558890000000051001234567"" versao=""4.00"">
      <ide>
        <cUF>21</cUF><cNF>00123456</cNF>
        <natOp>Compra P/Ind. sob o Regime de ""Drawback""</natOp>
        <mod>55</mod><serie>889</serie><nNF>5</nNF>
        <dhEmi>2025-09-01T00:00:00-03:00</dhEmi>
        <dhSaiEnt>2025-09-01T00:00:00-03:00</dhSaiEnt>
        <tpNF>1</tpNF><idDest>2</idDest><cMunFG>2111300</cMunFG>
        <tpImp>2</tpImp><tpEmis>1</tpEmis><cDV>0</cDV>
        <tpAmb>2</tpAmb><finNFe>1</finNFe><indFinal>1</indFinal><indPres>9</indPres>
      </ide>
      <emit>
        <CNPJ>11111111000111</CNPJ>
        <xNome>EMPRESA EXEMPLO LTDA</xNome>
        <xFant>EMPRESA EXEMPLO LTDA</xFant>
        <enderEmit>
          <xLgr>AVENIDA EXEMPLO</xLgr><nro>1000</nro>
          <xBairro>CENTRO</xBairro><cMun>2111300</cMun>
          <xMun>SAO LUIS</xMun><UF>MA</UF><CEP>00000000</CEP>
        </enderEmit>
        <IE>111111111</IE><CRT>3</CRT>
      </emit>
      <dest>
        <CNPJ>22222222000122</CNPJ>
        <xNome>NF-E EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL</xNome>
        <enderDest>
          <xLgr>RUA EXEMPLO</xLgr><nro>s/n</nro>
          <xBairro>CENTRO</xBairro><cMun>3304557</cMun>
          <xMun>RIO DE JANEIRO</xMun><UF>RJ</UF><CEP>00000000</CEP>
        </enderDest>
        <IE>22222222</IE>
      </dest>
      <det nItem=""1"">
        <prod>
          <cProd>96485451</cProd>
          <xProd>PESSEGO BASE CONCENTRADA</xProd>
          <NCM>76071120</NCM><CFOP>6101</CFOP>
          <uCom>KG</uCom><qCom>1.0000</qCom>
          <vUnCom>2.00</vUnCom><vProd>2.00</vProd>
          <cEANTrib>SEM GTIN</cEANTrib><uTrib>KG</uTrib>
          <qTrib>0.00</qTrib><vUnTrib>0.00</vUnTrib><indTot>1</indTot>
        </prod>
      </det>
      <total><ICMSTot>
        <vBC>0.00</vBC><vICMS>0.00</vICMS>
        <vProd>2.00</vProd><vNF>2.00</vNF>
      </ICMSTot></total>
      <transp><modFrete>1</modFrete><transporta>
        <CNPJ>33333333000133</CNPJ>
        <xNome>TRANSPORTADORA EXEMPLO LTDA</xNome>
        <IE>333333333333</IE>
        <xEnder>RUA EXEMPLO 100</xEnder>
        <xMun>GUARULHOS</xMun><UF>SP</UF>
      </transporta></transp>
      <pag><detPag>
        <tPag>01</tPag><vPag>2.00</vPag>
      </detPag></pag>
    </infNFe>
  </NFe>
</enviNFe>";

    /// <summary>
    /// XML mínimo com campos ausentes.
    /// </summary>
    private static string MinimalNfeXml() => @"<?xml version=""1.0"" encoding=""utf-8""?>
<enviNFe versao=""4.00"" xmlns=""http://www.portalfiscal.inf.br/nfe"">
  <NFe xmlns=""http://www.portalfiscal.inf.br/nfe"">
    <infNFe Id=""NFeTeste"" versao=""4.00"">
      <ide><mod>55</mod><serie>1</serie><nNF>1</nNF></ide>
    </infNFe>
  </NFe>
</enviNFe>";
}
