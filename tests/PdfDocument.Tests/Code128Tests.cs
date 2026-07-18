namespace PdfDocument.Tests;

public sealed class Code128Tests
{
    [Fact]
    public void Generate_ShouldCreateBars_ForValidEvenDigits()
    {
        // Arrange
        string digits = "1234567890"; // 10 digits (even)

        // Act
        var bars = Code128.Generate(digits);

        // Assert
        Assert.NotNull(bars);
        Assert.NotEmpty(bars);
    }

    [Fact]
    public void Generate_ShouldThrow_WhenNull()
    {
        Assert.Throws<ArgumentNullException>(() => Code128.Generate(null!));
    }

    [Fact]
    public void Generate_ShouldThrow_WhenEmptyString()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => Code128.Generate(""));
        Assert.Contains("even number of digits", ex.Message);
    }

    [Fact]
    public void Generate_ShouldThrow_WhenOddLength()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => Code128.Generate("123"));
        Assert.Contains("even number of digits", ex.Message);
    }

    [Fact]
    public void Generate_ShouldThrow_WhenOnlyNonDigits()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => Code128.Generate("abc!@#"));
        Assert.Contains("even number of digits", ex.Message);
    }

    [Fact]
    public void Generate_ShouldFilterNonDigits()
    {
        // Act
        var bars = Code128.Generate("12-34.56"); // non-digits filtered -> 123456 (even)

        // Assert
        Assert.NotNull(bars);
        Assert.NotEmpty(bars);
    }

    [Fact]
    public void Generate_ShouldStartWithStartCodeC()
    {
        // Arrange
        var bars = Code128.Generate("0000");

        // StartC = pattern index 105 = [1,1,1,4,1,4]
        // First 6 bars come from that pattern
        // Assert first bar is black (index 0 = black, width 1)
        Assert.True(bars[0].Black);
        Assert.Equal(1, bars[0].Width);
        // Second bar is white
        Assert.False(bars[1].Black);
        Assert.Equal(1, bars[1].Width);
    }

    [Fact]
    public void Generate_ShouldHaveCorrectBarPatternLength()
    {
        // 4 digits = 2 pairs → codewords: 1 (start) + 2 (pairs) + 1 (check) = 4
        // Each codeword = 6 bars = 24 bars
        // Stop pattern: 5 bars from loop + 1 extra = 6 bars
        // Total = 4*6 + 6 = 30
        var bars = Code128.Generate("1234");
        Assert.Equal(30, bars.Count);
    }

    [Fact]
    public void Generate_WithLargerInput_ShouldHaveExpectedBarCount()
    {
        // 10 digits = 5 pairs → codewords: 1 + 5 + 1 = 7
        // bars: 7*6 + 6 = 48
        var bars = Code128.Generate("1234567890");
        Assert.Equal(48, bars.Count);
    }

    [Fact]
    public void Generate_BarsShouldAlternateInCodewords()
    {
        // Arrange
        var bars = Code128.Generate("1234");

        // Assert: codeword bars must strictly alternate black/white
        // 4 codewords × 6 bars = 24 bars
        for (int i = 0; i < 24; i++)
            Assert.Equal(i % 2 == 0, bars[i].Black);
    }

    [Fact]
    public void Generate_LastBarShouldBeBlack()
    {
        // The stop pattern ends with a black bar of width 2
        var bars = Code128.Generate("1234");

        Assert.True(bars[^1].Black);
        Assert.Equal(2, bars[^1].Width);
    }

    [Fact]
    public void Generate_DifferentInputs_ShouldProduceDifferentBars()
    {
        // Arrange: use pairs with different first pattern widths
        // Pair "11" → pattern 11: [2,2,1,3,1,2] starts with 2
        // Pair "33" → pattern 33: [1,1,2,3,3,1] starts with 1
        var bars1 = Code128.Generate("1111");
        var bars2 = Code128.Generate("3333");

        // Compare the 7th bar (first data bar after StartC)
        Assert.NotEqual(bars1[6].Width, bars2[6].Width);
    }

    [Fact]
    public void Generate_CheckDigit_ShouldBeValid()
    {
        // For input "00": StartC=105, pair=0, sum = 105*1 + 0*2 = 105
        // Check digit = 105 % 103 = 2
        // Codewords: [105, 0, 2] → 3 codewords × 6 = 18 + stop 6 = 24
        var bars = Code128.Generate("00");
        Assert.Equal(24, bars.Count);
    }

    [Fact]
    public void Generate_WithAllZeros_ShouldNotThrow()
    {
        // Act
        var bars = Code128.Generate("0000");

        // Assert
        Assert.NotNull(bars);
        Assert.NotEmpty(bars);
    }
}
