using System.Xml;

namespace PdfDocument.Tests;

public sealed class NFeParserTests
{
    [Fact]
    public void Parse_ShouldThrow_WhenXmlNotFound()
    {
        // Act & Assert
        Assert.Throws<FileNotFoundException>(() =>
            NFe.NFeParser.Parse("/caminho/inexistente.xml"));
    }

    [Fact]
    public void Parse_ShouldThrow_WhenPathIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            NFe.NFeParser.Parse(null!));
    }

    [Fact]
    public void Parse_ShouldExtractBasicFields()
    {
        // Arrange
        string tempXml = Path.GetTempFileName();
        File.WriteAllText(tempXml, MinimalNfeXml());

        try
        {
            // Act
            var data = NFe.NFeParser.Parse(tempXml);

            // Assert
            Assert.NotNull(data);
            Assert.Equal("21", data.CUf);
            Assert.Equal("55", data.Mod);
            Assert.Equal("889", data.Serie);
            Assert.Equal("5", data.Nnf);
            Assert.Equal("Compra P/Ind. sob o Regime de \"Drawback\"", data.NatOp);
        }
        finally
        {
            File.Delete(tempXml);
        }
    }

    [Fact]
    public void Parse_ShouldExtractEmitenteFields()
    {
        // Arrange
        string tempXml = Path.GetTempFileName();
        File.WriteAllText(tempXml, MinimalNfeXml());

        try
        {
            // Act
            var data = NFe.NFeParser.Parse(tempXml);

            // Assert
            Assert.Equal("11111111000111", data.EmitCnpj);
            Assert.Equal("EMPRESA EXEMPLO LTDA", data.EmitXNome);
            Assert.Equal("111111111", data.EmitIe);
            Assert.Equal("3", data.EmitCrt);
        }
        finally
        {
            File.Delete(tempXml);
        }
    }

    [Fact]
    public void Parse_ShouldExtractDestinatarioFields()
    {
        // Arrange
        string tempXml = Path.GetTempFileName();
        File.WriteAllText(tempXml, MinimalNfeXml());

        try
        {
            // Act
            var data = NFe.NFeParser.Parse(tempXml);

            // Assert
            Assert.Equal("22222222000122", data.DestCnpj);
            Assert.Contains("HOMOLOGACAO", data.DestXNome);
            Assert.Equal("RJ", data.DestUf);
        }
        finally
        {
            File.Delete(tempXml);
        }
    }

    [Fact]
    public void Parse_ShouldExtractProdutoFields()
    {
        // Arrange
        string tempXml = Path.GetTempFileName();
        File.WriteAllText(tempXml, MinimalNfeXml());

        try
        {
            // Act
            var data = NFe.NFeParser.Parse(tempXml);

            // Assert
            Assert.Equal("96485451", data.CProd);
            Assert.Equal("PESSEGO BASE CONCENTRADA", data.XProd);
            Assert.Equal("76071120", data.Ncm);
            Assert.Equal("6101", data.Cfop);
            Assert.Equal("KG", data.UCom);
            Assert.Equal("1.0000", data.QCom);
            Assert.Equal("2.00", data.VUnCom);
            Assert.Equal("2.00", data.VProd);
        }
        finally
        {
            File.Delete(tempXml);
        }
    }

    [Fact]
    public void Parse_ShouldExtractTotaisFields()
    {
        // Arrange
        string tempXml = Path.GetTempFileName();
        File.WriteAllText(tempXml, MinimalNfeXml());

        try
        {
            // Act
            var data = NFe.NFeParser.Parse(tempXml);

            // Assert
            Assert.Equal("0.00", data.VBc);
            Assert.Equal("0.00", data.VIcms);
            Assert.Equal("2.00", data.VProdTotal);
            Assert.Equal("2.00", data.VNf);
        }
        finally
        {
            File.Delete(tempXml);
        }
    }

    [Fact]
    public void Parse_ShouldExtractTransportadoraFields()
    {
        // Arrange
        string tempXml = Path.GetTempFileName();
        File.WriteAllText(tempXml, MinimalNfeXml());

        try
        {
            // Act
            var data = NFe.NFeParser.Parse(tempXml);

            // Assert
            Assert.Equal("33333333000133", data.TransCnpj);
            Assert.Equal("TRANSPORTADORA EXEMPLO LTDA", data.TransXNome);
            Assert.Equal("SP", data.TransUf);
        }
        finally
        {
            File.Delete(tempXml);
        }
    }

    [Fact]
    public void Parse_ShouldExtractPagamentoFields()
    {
        // Arrange
        string tempXml = Path.GetTempFileName();
        File.WriteAllText(tempXml, MinimalNfeXml());

        try
        {
            // Act
            var data = NFe.NFeParser.Parse(tempXml);

            // Assert
            Assert.Equal("01", data.TPag);
            Assert.Equal("2.00", data.VPag);
        }
        finally
        {
            File.Delete(tempXml);
        }
    }

    [Fact]
    public void Parse_ShouldReturnEmptyStrings_ForMissingNodes()
    {
        // Arrange
        string tempXml = Path.GetTempFileName();
        File.WriteAllText(tempXml, NfeWithMissingFields());

        try
        {
            // Act
            var data = NFe.NFeParser.Parse(tempXml);

            // Assert
            Assert.Empty(data.CUf); // Missing field
            Assert.Equal("55", data.Mod); // Present field
        }
        finally
        {
            File.Delete(tempXml);
        }
    }

    [Fact]
    public void Parse_ShouldThrow_WhenInfNFeNotFound()
    {
        // Arrange
        string tempXml = Path.GetTempFileName();
        File.WriteAllText(tempXml, "<root><other /></root>");

        try
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                NFe.NFeParser.Parse(tempXml));
        }
        finally
        {
            File.Delete(tempXml);
        }
    }

    /// <summary>
    /// Creates a minimal NFe XML for testing.
    /// </summary>
    private static string MinimalNfeXml() => @"<?xml version=""1.0"" encoding=""utf-8""?>
<enviNFe versao=""4.00"" xmlns=""http://www.portalfiscal.inf.br/nfe"">
  <idLote>1</idLote>
  <indSinc>1</indSinc>
  <NFe xmlns=""http://www.portalfiscal.inf.br/nfe"">
    <infNFe Id=""NFeTeste"" versao=""4.00"">
      <ide>
        <cUF>21</cUF>
        <cNF>00123456</cNF>
        <natOp>Compra P/Ind. sob o Regime de ""Drawback""</natOp>
        <mod>55</mod>
        <serie>889</serie>
        <nNF>5</nNF>
        <dhEmi>2025-09-01T00:00:00-03:00</dhEmi>
        <dhSaiEnt>2025-09-01T00:00:00-03:00</dhSaiEnt>
        <tpNF>1</tpNF>
        <idDest>2</idDest>
        <cMunFG>2111300</cMunFG>
        <tpImp>2</tpImp>
        <tpEmis>1</tpEmis>
        <cDV>0</cDV>
        <tpAmb>2</tpAmb>
        <finNFe>1</finNFe>
        <indFinal>1</indFinal>
        <indPres>9</indPres>
        <indIntermed>0</indIntermed>
        <procEmi>0</procEmi>
        <verProc>3.1.0.0</verProc>
      </ide>
      <emit>
        <CNPJ>11111111000111</CNPJ>
        <xNome>EMPRESA EXEMPLO LTDA</xNome>
        <xFant>EMPRESA EXEMPLO LTDA</xFant>
        <enderEmit>
          <xLgr>AVENIDA EXEMPLO</xLgr>
          <nro>1000</nro>
          <xBairro>CENTRO</xBairro>
          <cMun>2111300</cMun>
          <xMun>SAO LUIS</xMun>
          <UF>MA</UF>
          <CEP>00000000</CEP>
        </enderEmit>
        <IE>111111111</IE>
        <CRT>3</CRT>
      </emit>
      <dest>
        <CNPJ>22222222000122</CNPJ>
        <xNome>NF-E EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL</xNome>
        <enderDest>
          <xLgr>RUA EXEMPLO</xLgr>
          <nro>s/n</nro>
          <xBairro>CENTRO</xBairro>
          <cMun>3304557</cMun>
          <xMun>RIO DE JANEIRO</xMun>
          <UF>RJ</UF>
          <CEP>00000000</CEP>
        </enderDest>
        <IE>22222222</IE>
      </dest>
      <det nItem=""1"">
        <prod>
          <cProd>96485451</cProd>
          <xProd>PESSEGO BASE CONCENTRADA</xProd>
          <NCM>76071120</NCM>
          <CFOP>6101</CFOP>
          <uCom>KG</uCom>
          <qCom>1.0000</qCom>
          <vUnCom>2.00</vUnCom>
          <vProd>2.00</vProd>
        </prod>
      </det>
      <total>
        <ICMSTot>
          <vBC>0.00</vBC>
          <vICMS>0.00</vICMS>
          <vProd>2.00</vProd>
          <vNF>2.00</vNF>
        </ICMSTot>
      </total>
      <transp>
        <modFrete>1</modFrete>
        <transporta>
          <CNPJ>33333333000133</CNPJ>
          <xNome>TRANSPORTADORA EXEMPLO LTDA</xNome>
          <IE>333333333333</IE>
          <xEnder>RUA EXEMPLO 100</xEnder>
          <xMun>GUARULHOS</xMun>
          <UF>SP</UF>
        </transporta>
      </transp>
      <pag>
        <detPag>
          <tPag>01</tPag>
          <vPag>2.00</vPag>
        </detPag>
      </pag>
    </infNFe>
  </NFe>
</enviNFe>";

    /// <summary>
    /// Creates an NFe XML with missing fields to test resilience.
    /// </summary>
    private static string NfeWithMissingFields() => @"<?xml version=""1.0"" encoding=""utf-8""?>
<enviNFe versao=""4.00"" xmlns=""http://www.portalfiscal.inf.br/nfe"">
  <NFe xmlns=""http://www.portalfiscal.inf.br/nfe"">
    <infNFe Id=""NFeTeste"" versao=""4.00"">
      <ide>
        <mod>55</mod>
        <serie>1</serie>
        <nNF>1</nNF>
      </ide>
    </infNFe>
  </NFe>
</enviNFe>";
}
