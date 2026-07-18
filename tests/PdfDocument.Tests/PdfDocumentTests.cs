namespace PdfDocument.Tests;

public sealed class PdfDocumentTests
{
    [Fact]
    public void AddPage_ShouldReturnPage_WithDefaultDimensions()
    {
        // Arrange
        using var pdf = new PdfBuilder();

        // Act
        var page = pdf.AddPage();

        // Assert
        Assert.NotNull(page);
        Assert.Equal(612.0, page.Width);
        Assert.Equal(792.0, page.Height);
    }

    [Fact]
    public void AddPage_ShouldReturnPage_WithCustomDimensions()
    {
        // Arrange
        using var pdf = new PdfBuilder();

        // Act
        var page = pdf.AddPage(400, 600);

        // Assert
        Assert.Equal(400.0, page.Width);
        Assert.Equal(600.0, page.Height);
    }

    [Fact]
    public void AddImage_ShouldThrow_WhenNameIsNull()
    {
        // Arrange
        using var pdf = new PdfBuilder();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => pdf.AddImage(null!, []));
    }

    [Fact]
    public void AddImage_ShouldThrow_WhenDataIsNull()
    {
        // Arrange
        using var pdf = new PdfBuilder();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => pdf.AddImage("test", (byte[])null!));
    }

    [Fact]
    public void AddImage_ShouldThrow_WhenDataExceedsMaxSize()
    {
        // Arrange
        using var pdf = new PdfBuilder();
        var largeData = new byte[513 * 1024]; // > 512 KB

        // Act & Assert
        Assert.Throws<ArgumentException>(() => pdf.AddImage("large", largeData));
    }

    [Fact]
    public void AddImage_ShouldThrow_WhenNameAlreadyExists()
    {
        // Arrange
        using var pdf = new PdfBuilder();
        // Minimal valid JPEG (SOF0 marker at position 2)
        byte[] jpeg = CreateMinimalJpeg();

        // Act
        pdf.AddImage("img1", jpeg);

        // Assert
        Assert.Throws<ArgumentException>(() => pdf.AddImage("img1", jpeg));
    }

    [Fact]
    public void Save_ShouldCreateValidPdfFile()
    {
        // Arrange
        using var pdf = new PdfBuilder();
        pdf.AddPage();
        string tempPath = Path.GetTempFileName();

        try
        {
            // Act
            pdf.Save(tempPath);

            // Assert
            var fileInfo = new FileInfo(tempPath);
            Assert.True(fileInfo.Exists);
            Assert.True(fileInfo.Length > 0);

            // Check PDF header
            byte[] header = new byte[8];
            using (var fs = File.OpenRead(tempPath))
            {
                fs.ReadExactly(header, 0, header.Length);
            }

            Assert.Equal("%PDF-1.4"u8.ToArray(), header);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    public void Save_ShouldContainExpectedPdfSections()
    {
        // Arrange
        using var pdf = new PdfBuilder();
        pdf.AddPage();
        string tempPath = Path.GetTempFileName();

        try
        {
            // Act
            pdf.Save(tempPath);
            string content = File.ReadAllText(tempPath);

            // Assert
            Assert.Contains("%PDF-1.4", content);
            Assert.Contains("xref", content);
            Assert.Contains("trailer", content);
            Assert.Contains("startxref", content);
            Assert.Contains("%%EOF", content);
            Assert.Contains("/Type /Catalog", content);
            Assert.Contains("/Type /Pages", content);
            Assert.Contains("/Type /Page", content);
            Assert.Contains("/Type /Font", content);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    public void Save_WithMultiplePages_ShouldListAllPages()
    {
        // Arrange
        using var pdf = new PdfBuilder();
        pdf.AddPage();
        pdf.AddPage();
        pdf.AddPage();
        string tempPath = Path.GetTempFileName();

        try
        {
            // Act
            pdf.Save(tempPath);
            string content = File.ReadAllText(tempPath);

            // Assert: /Count 3 should appear in the pages dictionary
            Assert.Contains("/Count 3", content);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    public void Save_WithText_ShouldContainTextOperations()
    {
        // Arrange
        using var pdf = new PdfBuilder();
        var page = pdf.AddPage();
        page.Canvas.DrawText("Hello World", 10, 10);
        string tempPath = Path.GetTempFileName();

        try
        {
            // Act
            pdf.Save(tempPath);
            string content = File.ReadAllText(tempPath);

            // Assert
            Assert.Contains("BT", content);
            Assert.Contains("ET", content);
            Assert.Contains("Tf", content);
            Assert.Contains("Td", content);
            Assert.Contains("Tj", content);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    public void Dispose_ShouldNotThrow()
    {
        // Act & Assert
        var pdf = new PdfBuilder();
        pdf.Dispose(); // Should not throw
    }

    /// <summary>
    /// Creates a minimal valid JPEG file with SOF0 marker.
    /// </summary>
    private static byte[] CreateMinimalJpeg()
    {
        // Minimal JPEG: SOI, APP0, DQT, SOF0, SOS, EOI
        return
        [
            0xFF, 0xD8,       // SOI
            0xFF, 0xE0,       // APP0
            0x00, 0x10,       // Length
            0x4A, 0x46, 0x49, 0x46, 0x00, // JFIF\0
            0x01, 0x01,       // Version
            0x00,             // Units
            0x00, 0x01,       // X density
            0x00, 0x01,       // Y density
            0x00, 0x00,       // Thumbnail
            0xFF, 0xDB,       // DQT
            0x00, 0x43,       // Length
            0x00,             // Precision & table ID
            0x08, 0x06, 0x06, 0x07, 0x06, 0x05, 0x08, 0x07,
            0x07, 0x07, 0x09, 0x09, 0x08, 0x0A, 0x0C, 0x14,
            0x0D, 0x0C, 0x0B, 0x0B, 0x0C, 0x19, 0x12, 0x13,
            0x0F, 0x14, 0x1D, 0x1A, 0x1F, 0x1E, 0x1D, 0x1A,
            0x1C, 0x1C, 0x20, 0x24, 0x2E, 0x27, 0x20, 0x22,
            0x2C, 0x23, 0x1C, 0x1C, 0x28, 0x37, 0x29, 0x2C,
            0x30, 0x31, 0x34, 0x34, 0x34, 0x1F, 0x27, 0x39,
            0x3D, 0x38, 0x32, 0x3C, 0x2E, 0x33, 0x34, 0x32,
            0xFF, 0xC0,       // SOF0
            0x00, 0x0B,       // Length
            0x08,             // Precision
            0x00, 0x02,       // Height = 2
            0x00, 0x03,       // Width = 3
            0x03,             // Number of components
            0x01, 0x11, 0x00, // Component 1
            0x02, 0x11, 0x01, // Component 2
            0x03, 0x11, 0x01, // Component 3
            0xFF, 0xDA,       // SOS
            0x00, 0x08,       // Length
            0x01, 0x01, 0x00, // Component
            0x00, 0x3F, 0x00, // Spectral selection
            0xFF, 0xD9,       // EOI
        ];
    }
}
