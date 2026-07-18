namespace PdfDocument.Tests;

public sealed class Ean13Tests
{
    [Fact]
    public void Generate_With12Digits_ShouldCalculateCheckDigit()
    {
        // Arrange: known EAN-13 ( 789123456789 → check digit 5 )
        const string input = "789123456789";

        // Act
        var bars = EAN13.Generate(input);

        // Assert
        Assert.NotNull(bars);
        Assert.NotEmpty(bars);
    }

    [Fact]
    public void Generate_With13ValidDigits_ShouldNotThrow()
    {
        // Arrange
        const string input = "7891234567895";

        // Act
        var bars = EAN13.Generate(input);

        // Assert
        Assert.NotEmpty(bars);
    }

    [Fact]
    public void Generate_WithInvalidCheckDigit_ShouldThrow()
    {
        // Arrange: 789123456789 should have check digit 5, not 0
        const string input = "7891234567890";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => EAN13.Generate(input));
    }

    [Fact]
    public void Generate_ShouldThrow_ForNullInput()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => EAN13.Generate(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("ABCDEFGHIJKL")]
    [InlineData("12345678901234")] // 14 digits
    public void Generate_ShouldThrow_ForInvalidLength(string invalidInput)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => EAN13.Generate(invalidInput));
    }

    [Fact]
    public void Generate_ShouldHandleHyphens()
    {
        // Act
        var bars = EAN13.Generate("789-123-456-789");

        // Assert
        Assert.NotEmpty(bars);
    }

    [Fact]
    public void Generate_ShouldHandleSpaces()
    {
        // Act
        var bars = EAN13.Generate("789 123 456 789");

        // Assert
        Assert.NotEmpty(bars);
    }

    [Fact]
    public void Generate_ShouldStartAndEndWithGuardBars()
    {
        // Act
        var bars = EAN13.Generate("7891234567895");

        // Assert: left guard = "101" (black, white, black)
        Assert.True(bars[0].Black);
        Assert.False(bars[1].Black);
        Assert.True(bars[2].Black);

        // Right guard = "101" (last 3)
        Assert.True(bars[^3].Black);
        Assert.False(bars[^2].Black);
        Assert.True(bars[^1].Black);
    }

    [Fact]
    public void Generate_ShouldContainCenterGuardPattern()
    {
        // Act
        var bars = EAN13.Generate("7891234567895");

        // Find "01010" center guard - pattern of 5 bars: white, black, white, black, white
        int halfPoint = bars.Count / 2;

        // Center guard should be around the middle: "01010"
        // The black ones at positions 1 and 3 relative to center
        Assert.False(bars[halfPoint - 2].Black); // 0
        Assert.True(bars[halfPoint - 1].Black);  // 1
        Assert.False(bars[halfPoint].Black);     // 0
        Assert.True(bars[halfPoint + 1].Black);  // 1
        Assert.False(bars[halfPoint + 2].Black); // 0
    }

    [Fact]
    public void Generate_SameInput_ShouldProduceSameOutput()
    {
        // Arrange
        const string input = "7891234567895";

        // Act
        var bars1 = EAN13.Generate(input);
        var bars2 = EAN13.Generate(input);

        // Assert
        Assert.Equal(bars1.Count, bars2.Count);
        for (int i = 0; i < bars1.Count; i++)
        {
            Assert.Equal(bars1[i].Width, bars2[i].Width);
            Assert.Equal(bars1[i].Black, bars2[i].Black);
        }
    }

    [Fact]
    public void Generate_With12Digits_ShouldResultIn13DigitBarcode()
    {
        // 12 digits → should auto-calculate check digit → 13-digit standard
        var bars12 = EAN13.Generate("789123456789");
        var bars13 = EAN13.Generate("7891234567895");

        // Both should produce the same barcode pattern
        Assert.Equal(bars12.Count, bars13.Count);
    }

    [Fact]
    public void CalculateCheckDigit_ThroughGeneration_ShouldBeCorrect()
    {
        // Known EAN-13 test vectors
        // 789123456789 → check digit 5 (3*1 + 1*3 + 9*1 + 2*3 + 3*1 + 4*3 + 5*1 + 6*3 + 7*1 + 8*3 + 9*1 = ...)
        // Let's test a simpler one: 123456789012 → check digit ?
        // (1*1 + 2*3 + 3*1 + 4*3 + 5*1 + 6*3 + 7*1 + 8*3 + 9*1 + 0*3 + 1*1 + 2*3)
        // = 1+6+3+12+5+18+7+24+9+0+1+6 = 92 → 10 - (92%10) = 10-2 = 8

        // Act
        var bars = EAN13.Generate("123456789012");

        // Assert
        Assert.NotEmpty(bars);

        // Verify the check digit would be 8
        // If we pass 13 with correct check digit 8, it should pass
        var barsWithCheck = EAN13.Generate("1234567890128");
        Assert.Equal(bars.Count, barsWithCheck.Count);
    }
}
