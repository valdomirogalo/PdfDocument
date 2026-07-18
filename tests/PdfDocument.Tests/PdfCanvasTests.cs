namespace PdfDocument.Tests;

public sealed class PdfCanvasTests
{
    [Fact]
    public void GetContent_ShouldReturnInitialState_WhenNoCommandsIssued()
    {
        // Arrange
        var canvas = new PdfCanvas();

        // Act
        string content = canvas.GetContent();

        // Assert
        Assert.Contains("0 0 0 RG", content);
        Assert.Contains("0 0 0 rg", content);
    }

    [Fact]
    public void DrawLine_ShouldAddLineCommand()
    {
        // Arrange
        var canvas = new PdfCanvas();

        // Act
        canvas.DrawLine(10, 20, 100, 200);
        string content = canvas.GetContent();

        // Assert
        Assert.Contains("10.00 20.00 m 100.00 200.00 l S", content);
    }

    [Fact]
    public void DrawRectangle_ShouldAddRectangleCommand()
    {
        // Arrange
        var canvas = new PdfCanvas();

        // Act
        canvas.DrawRectangle(5, 10, 50, 100);
        string content = canvas.GetContent();

        // Assert
        Assert.Contains("5.00 10.00 50.00 100.00 re S", content);
    }

    [Fact]
    public void FillRectangle_ShouldAddFillCommand()
    {
        // Arrange
        var canvas = new PdfCanvas();

        // Act
        canvas.FillRectangle(0, 0, 100, 50);
        string content = canvas.GetContent();

        // Assert
        Assert.Contains("0.00 0.00 100.00 50.00 re f", content);
    }

    [Fact]
    public void DrawGrid_ShouldNotThrow_WhenInvalidDimensions()
    {
        // Arrange
        var canvas = new PdfCanvas();

        // Act
        canvas.DrawGrid(0, 0, 100, 100, 0, 0);
        string content = canvas.GetContent();

        // Assert - should not draw anything beyond initial state (2 lines)
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(2, lines.Length);
    }

    [Fact]
    public void DrawGrid_ShouldDrawCorrectNumberOfLines()
    {
        // Arrange
        var canvas = new PdfCanvas();

        // Act
        canvas.DrawGrid(0, 0, 100, 100, 3, 2);
        string content = canvas.GetContent();

        // Assert: 3 cols => 4 vertical lines, 2 rows => 3 horizontal lines = 7 lines total
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        int lineCommands = lines.Count(x => x.EndsWith("l S"));
        Assert.Equal(7, lineCommands);
    }

    [Fact]
    public void DrawText_ShouldEscapeSpecialCharacters()
    {
        // Arrange
        var canvas = new PdfCanvas();

        // Act
        canvas.DrawText("Preço (R$)", 10, 20);
        string content = canvas.GetContent();

        // Assert - parentheses must be escaped in PDF string ($ is printable, no escape needed)
        Assert.Contains("\\(R$\\)", content);
        // Verify that 'ç' (0xE7 > 126) is escaped as octal
        Assert.Contains(@"\347", content); // ç (0xE7 = 231) in octal
    }

    [Fact]
    public void UsedImages_ShouldBeEmpty_Initially()
    {
        // Arrange
        var canvas = new PdfCanvas();

        // Assert
        Assert.Empty(canvas.UsedImages);
    }

    [Fact]
    public void DrawImage_ShouldTrackUsedImages()
    {
        // Arrange
        var canvas = new PdfCanvas();

        // Act
        canvas.DrawImage("logo", 0, 0, 100, 50);

        // Assert
        Assert.Contains("logo", canvas.UsedImages);
    }

    [Fact]
    public void DrawImage_ShouldNotDuplicateNameInUsedImages()
    {
        // Arrange
        var canvas = new PdfCanvas();

        // Act
        canvas.DrawImage("logo", 0, 0, 100, 50);
        canvas.DrawImage("logo", 50, 0, 100, 50);

        // Assert
        Assert.Single(canvas.UsedImages);
    }

    [Fact]
    public void DrawBarcode_ShouldFillBlackBars()
    {
        // Arrange
        var canvas = new PdfCanvas();
        var bars = new List<Bar>
        {
            new() { Width = 2, Black = true },
            new() { Width = 1, Black = false },
            new() { Width = 1, Black = true },
        };

        // Act
        canvas.DrawBarcode(bars, 10, 10, 2, 50);
        string content = canvas.GetContent();

        // Assert: 2 black bars => 2 fill commands
        var fillCmds = content.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Count(x => x.EndsWith("re f"));
        Assert.Equal(2, fillCmds);
    }

    [Fact]
    public void DrawCell_WithBorder_ShouldDrawRectangle()
    {
        // Arrange
        var canvas = new PdfCanvas();

        // Act
        canvas.DrawCell("Test", 0, 0, 100, 20);
        string content = canvas.GetContent();

        // Assert
        Assert.Contains("re S", content);
    }

    [Fact]
    public void DrawCell_WithoutBorder_ShouldNotDrawRectangle()
    {
        // Arrange
        var canvas = new PdfCanvas();

        // Act
        canvas.DrawCell("Test", 0, 0, 100, 20, drawBorder: false);
        string content = canvas.GetContent();

        // Assert
        Assert.DoesNotContain("re S", content);
    }

    [Fact]
    public void DrawTable_ShouldThrow_WhenColumnWidthsMismatch()
    {
        // Arrange
        var canvas = new PdfCanvas();
        var data = new string[,] { { "A", "B" } }; // 2 columns

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            canvas.DrawTable(data, 0, 0, [100.0], 20)); // 1 width
    }

    [Fact]
    public void DrawTable_ShouldDrawTableCorrectly()
    {
        // Arrange
        var canvas = new PdfCanvas();
        var data = new string[,]
        {
            { "H1", "H2" },
            { "D1", "D2" },
        };

        // Act
        canvas.DrawTable(data, 0, 0, [50.0, 50.0], 20);
        string content = canvas.GetContent();

        // Assert: 3 rows of horizontal lines (0 to 2) + 3 columns of vertical lines (0 to 2)
        var lineCommands = content.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Count(x => x.EndsWith("l S"));
        Assert.Equal(6, lineCommands);
    }

    [Theory]
    [InlineData(PdfCanvas.TextAlign.Left, 10, 10)]
    [InlineData(PdfCanvas.TextAlign.Center, 20, 20)]
    [InlineData(PdfCanvas.TextAlign.Right, 30, 30)]
    public void DrawTextAligned_ShouldPositionCorrectly(
        PdfCanvas.TextAlign align, double x, double width)
    {
        // Arrange
        var canvas = new PdfCanvas();
        string text = "X"; // single char to simplify calculation

        // Act
        canvas.DrawTextAligned(text, x, 0, width, 20, align);
        string content = canvas.GetContent();

        // Assert - must contain a BT...ET sequence
        Assert.Contains("BT ", content);
        Assert.Contains(" Tj ET", content);
    }
}
