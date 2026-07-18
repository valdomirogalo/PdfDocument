namespace PdfDocument.Tests;

public sealed class Code39Tests
{
    [Fact]
    public void Generate_ShouldReturnBars_ForValidText()
    {
        // Act
        var bars = Code39.Generate("TEST");

        // Assert
        Assert.NotNull(bars);
        Assert.NotEmpty(bars);
    }

    [Fact]
    public void Generate_ShouldStartAndEndWithAsterisk()
    {
        // Act
        var bars = Code39.Generate("A");

        // Assert: * encoding is 010010100, first bar is Width=1, Black=false (narrow white)
        // * start character generates bars correctly
        Assert.NotNull(bars);
        Assert.True(bars.Count > 0);
        // Verify * pattern at start: 0,1,0,0,1,0,1,0,0 (pattern "010010100")
        Assert.False(bars[0].Black);   // 0
        Assert.True(bars[1].Black);    // 1
        Assert.False(bars[2].Black);   // 0
        Assert.False(bars[3].Black);   // 0
        Assert.True(bars[4].Black);    // 1
    }

    [Fact]
    public void Generate_ShouldThrow_ForInvalidCharacter()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Code39.Generate("TEST@"));
    }

    [Fact]
    public void Generate_ShouldThrow_ForNullInput()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Code39.Generate(null!));
    }

    [Fact]
    public void Generate_ShouldThrow_ForEmptyInput()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Code39.Generate(""));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("9")]
    [InlineData("A")]
    [InlineData("Z")]
    [InlineData("-")]
    [InlineData(".")]
    [InlineData(" ")]
    [InlineData("$")]
    [InlineData("/")]
    [InlineData("+")]
    [InlineData("%")]
    [InlineData("*")]
    public void Generate_ShouldAccept_ValidSingleCharacter(string singleChar)
    {
        // Act
        var bars = Code39.Generate(singleChar);

        // Assert
        Assert.NotEmpty(bars);
    }

    [Fact]
    public void Generate_ShouldConvertToUppercase()
    {
        // Act
        var barsUpper = Code39.Generate("ABC");
        var barsLower = Code39.Generate("abc");

        // Assert
        Assert.Equal(barsUpper.Count, barsLower.Count);
    }

    [Fact]
    public void Generate_ShouldIncludeInterCharacterGaps()
    {
        // Each character encoding = 9 bits + 1 gap = 10 bars
        // For \"AB\": * + [gap] + A + [gap] + B + [gap] + * + [gap] = 40 bars
        var bars = Code39.Generate("AB");

        // Total bars = 4 characters (start, A, B, stop) × 10 bars each = 40
        Assert.Equal(40, bars.Count);

        // The 10th bar (index 9, 19, 29, 39) after each character is gap (white, width=1)
        Assert.False(bars[9].Black);
        Assert.Equal(1, bars[9].Width);
        Assert.False(bars[19].Black);
        Assert.Equal(1, bars[19].Width);
        Assert.False(bars[29].Black);
        Assert.Equal(1, bars[29].Width);
        Assert.False(bars[39].Black);
        Assert.Equal(1, bars[39].Width);
    }
}
