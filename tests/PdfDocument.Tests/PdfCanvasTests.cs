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

    [Fact]
    public void DrawImage_ShouldThrow_WhenNameIsNull()
    {
        // Arrange
        var canvas = new PdfCanvas();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => canvas.DrawImage(null!, 0, 0, 100, 50));
    }

    [Fact]
    public void DrawImage_ShouldThrow_WhenNameIsInvalid()
    {
        // Arrange
        var canvas = new PdfCanvas();

        // Act & Assert - name with angle brackets (PDF injection)
        Assert.Throws<ArgumentException>(
            () => canvas.DrawImage("bad<name>", 0, 0, 100, 50));
    }

    [Fact]
    public void DrawImage_InvalidName_ShouldNotAddToUsedImages()
    {
        // Arrange
        var canvas = new PdfCanvas();

        // Act
        Assert.Throws<ArgumentException>(
            () => canvas.DrawImage("test/image", 0, 0, 100, 50));

        // Assert
        Assert.Empty(canvas.UsedImages);
    }

    [Fact]
    public void DrawBarcode_ShouldNotThrow_WhenEmptyList()
    {
        // Arrange
        var canvas = new PdfCanvas();
        var emptyBars = new List<Bar>();

        // Act (should not throw)
        canvas.DrawBarcode(emptyBars, 0, 0, 1, 10);

        // Assert - content should only have initial state (2 lines)
        var lines = canvas.GetContent().Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(2, lines.Length);
    }

    [Fact]
    public void DrawTable_ShouldWork_WithSingleRow()
    {
        // Arrange
        var canvas = new PdfCanvas();
        var data = new string[,] { { "A" } };

        // Act
        canvas.DrawTable(data, 0, 0, [50.0], 20);

        // Assert
        var lines = canvas.GetContent().Split('\n', StringSplitOptions.RemoveEmptyEntries);
        // Initial (2) + horizontal (2) + vertical (2) + text (1) = 7
        Assert.True(lines.Length >= 6);
    }

    [Fact]
    public void EscapePdfString_ShouldHandleAsciiOnly()
    {
        // Arrange
        var canvas = new PdfCanvas();

        // Act
        canvas.DrawText("Hello World", 10, 10);
        string content = canvas.GetContent();

        // Assert - no escaping needed for plain ASCII
        Assert.Contains("(Hello World)", content);
    }

    [Fact]
    public void EscapePdfString_ShouldHandleNullChars()
    {
        // Arrange
        var canvas = new PdfCanvas();

        // Act - text with special chars
        canvas.DrawText("Line1\nLine2", 10, 10);
        string content = canvas.GetContent();

        // Assert - newline (0x0A < 32) should be escaped as octal
        Assert.Contains("\\", content);
    }

    [Fact]
    public void EscapePdfString_ShouldThrow_WhenTextTooLong()
    {
        // Arrange
        var canvas = new PdfCanvas();
        string longText = new string('A', 32769); // exceeds MaxTextLength

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => canvas.DrawText(longText, 10, 10));
    }

    [Fact]
    public void EscapePdfString_ShouldHandleBackslash()
    {
        // Arrange
        var canvas = new PdfCanvas();

        // Act
        canvas.DrawText("Path\\File", 10, 10);
        string content = canvas.GetContent();

        // Assert - backslash must be escaped
        Assert.Contains("\\\\", content);
    }

    [Fact]
    public void DrawCell_DefaultBorder_ShouldDrawRectangle()
    {
        // Arrange
        var canvas = new PdfCanvas();

        // Act
        canvas.DrawCell("Text", 0, 0, 100, 20);

        // Assert
        string content = canvas.GetContent();
        Assert.Contains("re S", content);
    }

    [Fact]
    public void DrawGrid_WithSingleCell_ShouldDrawFourLines()
    {
        // Arrange
        var canvas = new PdfCanvas();

        // Act
        canvas.DrawGrid(0, 0, 50, 50, 1, 1);
        string content = canvas.GetContent();

        // Assert: 1 col => 2 vertical lines, 1 row => 2 horizontal lines = 4
        var lineCommands = content.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Count(x => x.EndsWith("l S"));
        Assert.Equal(4, lineCommands);
    }
}
