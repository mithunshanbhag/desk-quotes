using DeskQuotes.Services.Validators;

namespace DeskQuotes.UnitTests.Services;

public class WallpaperRenderServiceTests
{
    #region Negative cases

    [Fact]
    public void RenderQuoteWallpaper_WhenQuoteIsNull_ThrowsArgumentNullException()
    {
        var sut = new WallpaperRenderService();

        var action = () => sut.RenderQuoteWallpaper(null!, new Size(1920, 1080));

        action.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Boundary cases

    [Fact]
    public void RenderQuoteWallpaper_WhenResolutionIsInvalid_UsesFallbackAndCreatesWallpaper()
    {
        var sut = new WallpaperRenderService(new WallpaperResolutionValidator());
        var quote = new Quote { Text = "", Author = "" };

        var outputPath = sut.RenderQuoteWallpaper(quote, new Size(0, 0));
        using var renderedImage = Image.FromFile(outputPath);

        outputPath.Should().NotBeNullOrWhiteSpace();
        outputPath.Should().EndWith(".bmp");
        File.Exists(outputPath).Should().BeTrue();
        renderedImage.Width.Should().Be(1920);
        renderedImage.Height.Should().Be(1080);
    }

    [Fact]
    public void RenderQuoteWallpaper_WhenOnlyWidthIsInvalid_UsesFallbackWidthAndProvidedHeight()
    {
        var sut = new WallpaperRenderService(new WallpaperResolutionValidator());
        var quote = new Quote { Text = "Quote", Author = "Author" };

        var outputPath = sut.RenderQuoteWallpaper(quote, new Size(0, 720));
        using var renderedImage = Image.FromFile(outputPath);

        renderedImage.Width.Should().Be(1920);
        renderedImage.Height.Should().Be(720);
    }

    #endregion
}
