namespace DeskQuotes.UnitTests.Services;

public class WallpaperRenderServiceTests
{
    [Fact]
    public void RenderQuoteWallpaper_WhenQuoteIsNull_ThrowsArgumentNullException()
    {
        var sut = new WallpaperRenderService();

        var action = () => sut.RenderQuoteWallpaper(null!, new Size(1920, 1080));

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RenderQuoteWallpaper_WhenResolutionIsInvalid_UsesFallbackAndCreatesWallpaper()
    {
        var sut = new WallpaperRenderService();
        var quote = new Quote { Text = "", Author = "" };

        var outputPath = sut.RenderQuoteWallpaper(quote, new Size(0, 0));

        outputPath.Should().NotBeNullOrWhiteSpace();
        outputPath.Should().EndWith(".bmp");
        File.Exists(outputPath).Should().BeTrue();
    }
}