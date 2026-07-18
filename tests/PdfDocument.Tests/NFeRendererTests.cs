using PdfDocument.NFe;

namespace PdfDocument.Tests;

public sealed class NFeRendererTests
{
    private static NFeData CreateSampleData()
    {
        return new NFeData
        {
            NatOp = "VENDA",
            Mod = "55",
            Serie = "1",
            Nnf = "123456",
            DhEmi = "2026-07-18T10:00:00",
            TpNf = "1",
            IdDest = "1",
            TpAmb = "2",
            EmitCnpj = "12345678000199",
            EmitXNome = "EMPRESA TESTE LTDA",
            EmitIe = "123456789",
            EmitCrt = "3",
            EmitXLogr = "RUA TESTE",
            EmitNro = "100",
            EmitXBairro = "CENTRO",
            EmitXMun = "SAO PAULO",
            EmitUf = "SP",
            EmitCep = "01001000",
            DestCnpj = "98765432000188",
            DestXNome = "CLIENTE TESTE LTDA",
            DestIe = "987654321",
            DestXLogr = "AV TESTE",
            DestNro = "200",
            DestXBairro = "JARDINS",
            DestXMun = "RIO DE JANEIRO",
            DestUf = "RJ",
            CProd = "001",
            XProd = "PRODUTO TESTE",
            Ncm = "84713000",
            Cfop = "5102",
            UCom = "UN",
            QCom = "10",
            VUnCom = "100.00",
            VProd = "1000.00",
            VBc = "1000.00",
            VIcms = "180.00",
            VProdTotal = "1000.00",
            VNf = "1000.00",
            TPag = "01",
            VPag = "1000.00",
            ModFrete = "0",
            TransCnpj = "",
            TransXNome = "",
            InfCpl = "",
            InfAdic = "",
        };
    }

    [Fact]
    public void RenderToFile_ShouldCreatePdf_WhenDataIsValid()
    {
        // Arrange
        var data = CreateSampleData();
        string tempPath = Path.GetTempFileName();

        try
        {
            // Act
            NFeRenderer.RenderToFile(data, tempPath);

            // Assert
            var fileInfo = new FileInfo(tempPath);
            Assert.True(fileInfo.Exists);
            Assert.True(fileInfo.Length > 0);

            // Verify it contains PDF header
            byte[] header = new byte[8];
            using (var fs = File.OpenRead(tempPath))
                fs.ReadExactly(header, 0, header.Length);
            Assert.Equal("%PDF-1.4"u8.ToArray(), header);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    public void RenderToFile_ShouldThrow_WhenDataIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => NFeRenderer.RenderToFile(null!, "output.pdf"));
    }

    [Fact]
    public void RenderToFile_ShouldThrow_WhenPathIsNull()
    {
        var data = CreateSampleData();
        Assert.Throws<ArgumentNullException>(
            () => NFeRenderer.RenderToFile(data, null!));
    }

    [Fact]
    public void RenderToFile_ShouldContainDANFETitle()
    {
        // Arrange
        var data = CreateSampleData();
        string tempPath = Path.GetTempFileName();

        try
        {
            // Act
            NFeRenderer.RenderToFile(data, tempPath);
            string content = File.ReadAllText(tempPath);

            // Assert - title appears in PDF content
            Assert.Contains("DANFE", content);
            Assert.Contains("EMPRESA TESTE LTDA", content);
            Assert.Contains("CLIENTE TESTE LTDA", content);
            Assert.Contains("VENDA", content);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    public void RenderToFile_ShouldContainPdfStructure()
    {
        // Arrange
        var data = CreateSampleData();
        string tempPath = Path.GetTempFileName();

        try
        {
            // Act
            NFeRenderer.RenderToFile(data, tempPath);
            string content = File.ReadAllText(tempPath);

            // Assert - PDF structure markers
            Assert.Contains("%PDF-1.4", content);
            Assert.Contains("xref", content);
            Assert.Contains("trailer", content);
            Assert.Contains("startxref", content);
            Assert.Contains("%%EOF", content);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }
}
