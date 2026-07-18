namespace PdfDocument.Tests;

public sealed class PdfConstantsTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsValidPdfName_ShouldReturnFalse_ForNullOrEmpty(string? name)
    {
        // Act
        bool result = PdfConstants.IsValidPdfName(name!);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("logo")]
    [InlineData("img123")]
    [InlineData("my-image")]
    [InlineData("my_image")]
    [InlineData("a")]
    [InlineData("A")]
    [InlineData("123")]
    [InlineData("a1-B_c")]
    public void IsValidPdfName_ShouldReturnTrue_ForValidNames(string name)
    {
        // Act
        bool result = PdfConstants.IsValidPdfName(name);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("bad<name>")]
    [InlineData("bad/name")]
    [InlineData("bad name")]
    [InlineData("bad.name")]
    [InlineData("bad(name)")]
    [InlineData("bad&name")]
    [InlineData("bad$name")]
    [InlineData("bad@name")]
    public void IsValidPdfName_ShouldReturnFalse_ForInvalidChars(string name)
    {
        // Act
        bool result = PdfConstants.IsValidPdfName(name);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidPdfName_ShouldReturnFalse_ForNameWithSpaces()
    {
        // Act
        bool result = PdfConstants.IsValidPdfName("my image");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidPdfName_ShouldReturnFalse_ForNameStartingWithSpecial()
    {
        // Act
        bool result = PdfConstants.IsValidPdfName("/name");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void MaxImageSizeBytes_ShouldBe512K()
    {
        Assert.Equal(524288, PdfConstants.MaxImageSizeBytes);
    }

    [Fact]
    public void DefaultPageSize_ShouldBeLetter()
    {
        Assert.Equal(612.0, PdfConstants.DefaultPageWidth);
        Assert.Equal(792.0, PdfConstants.DefaultPageHeight);
    }
}
