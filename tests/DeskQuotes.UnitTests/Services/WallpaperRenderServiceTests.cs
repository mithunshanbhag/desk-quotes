using DeskQuotes.Services.Validators;

namespace DeskQuotes.UnitTests.Services;

public class WallpaperRenderServiceTests
{
    #region Negative cases

    [Fact]
    public void RenderQuoteWallpaper_WhenQuoteIsNull_ThrowsArgumentNullException()
    {
        var sut = new WallpaperRenderService();

        var action = () => sut.RenderQuoteWallpaper(null!, new Size(1920, 1080), Color.Black, "Segoe UI");

        action.Should().Throw<ArgumentNullException>();
    }

    #endregion

    private static Color GetBackgroundColor(Bitmap renderedImage)
    {
        return renderedImage.GetPixel(0, 0);
    }

    private static List<(int Start, int End)> GetMergedTextClusters(Bitmap renderedImage, Color backgroundColor)
    {
        var rowsWithText = Enumerable
            .Range(0, renderedImage.Height)
            .Where(row => RowContainsText(renderedImage, row, backgroundColor))
            .ToList();

        rowsWithText.Should().NotBeEmpty();

        var clusters = new List<(int Start, int End)>();
        var clusterStart = rowsWithText[0];
        var previousRow = rowsWithText[0];
        var allowedGap = Math.Max(12, renderedImage.Height / 36);

        foreach (var row in rowsWithText.Skip(1))
        {
            if (row - previousRow > allowedGap)
            {
                clusters.Add((clusterStart, previousRow));
                clusterStart = row;
            }

            previousRow = row;
        }

        clusters.Add((clusterStart, previousRow));
        return clusters;
    }

    private static bool RowContainsText(Bitmap renderedImage, int row, Color backgroundColor)
    {
        for (var column = 0; column < renderedImage.Width; column++)
            if (renderedImage.GetPixel(column, row).ToArgb() != backgroundColor.ToArgb())
                return true;

        return false;
    }

    #region Positive cases

    [Fact]
    public void RenderQuoteWallpaper_WhenQuoteHasAuthor_RendersAuthorCloseToQuoteText()
    {
        var sut = new WallpaperRenderService(new WallpaperResolutionValidator());
        var quote = new Quote
        {
            Text = "Never give up. Today is hard, tomorrow will be worse, but the day after tomorrow will be sunshine",
            Author = "Jack Ma"
        };

        var outputPath = sut.RenderQuoteWallpaper(quote, new Size(1920, 1080), Color.FromArgb(24, 27, 36), "Georgia");
        using var renderedImage = new Bitmap(outputPath);
        var backgroundColor = GetBackgroundColor(renderedImage);
        var textClusters = GetMergedTextClusters(renderedImage, backgroundColor);

        textClusters.Should().HaveCount(2);

        var gapBetweenQuoteAndAuthor = textClusters[1].Start - textClusters[0].End - 1;
        gapBetweenQuoteAndAuthor.Should().BeLessThan(140);
    }

    [Fact]
    public void RenderQuoteWallpaper_WhenBackgroundColorProvided_UsesThatBackgroundColor()
    {
        var sut = new WallpaperRenderService(new WallpaperResolutionValidator());
        var quote = new Quote { Text = "Stay curious.", Author = "Unknown" };
        var backgroundColor = Color.FromArgb(28, 44, 60);

        var outputPath = sut.RenderQuoteWallpaper(quote, new Size(1920, 1080), backgroundColor, "Trebuchet MS");
        using var renderedImage = new Bitmap(outputPath);

        GetBackgroundColor(renderedImage).ToArgb().Should().Be(backgroundColor.ToArgb());
    }

    [Fact]
    public void RenderQuoteWallpaper_WhenCuratedFontProvided_RendersWallpaper()
    {
        var sut = new WallpaperRenderService(new WallpaperResolutionValidator());
        var quote = new Quote { Text = "Stay steady.", Author = "Unknown" };

        var outputPath = sut.RenderQuoteWallpaper(quote, new Size(1920, 1080), Color.FromArgb(24, 27, 36), "Constantia");
        using var renderedImage = new Bitmap(outputPath);

        File.Exists(outputPath).Should().BeTrue();
        renderedImage.Width.Should().Be(1920);
        renderedImage.Height.Should().Be(1080);
    }

    [Fact]
    public void RenderQuoteWallpaper_WhenFontFamilyIsUnavailable_FallsBackAndStillRendersWallpaper()
    {
        var sut = new WallpaperRenderService(new WallpaperResolutionValidator());
        var quote = new Quote { Text = "Stay curious.", Author = "Unknown" };
        var backgroundColor = Color.FromArgb(24, 27, 36);

        var outputPath = sut.RenderQuoteWallpaper(quote, new Size(1920, 1080), backgroundColor, "Definitely Missing Font");
        using var renderedImage = new Bitmap(outputPath);

        File.Exists(outputPath).Should().BeTrue();
        GetBackgroundColor(renderedImage).ToArgb().Should().Be(backgroundColor.ToArgb());
    }

    #endregion

    #region Boundary cases

    [Fact]
    public void RenderQuoteWallpaper_WhenResolutionIsInvalid_UsesFallbackAndCreatesWallpaper()
    {
        var sut = new WallpaperRenderService(new WallpaperResolutionValidator());
        var quote = new Quote { Text = "", Author = "" };

        var outputPath = sut.RenderQuoteWallpaper(quote, new Size(0, 0), Color.Black, "Segoe UI");
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

        var outputPath = sut.RenderQuoteWallpaper(quote, new Size(0, 720), Color.Black, "Segoe UI");
        using var renderedImage = Image.FromFile(outputPath);

        renderedImage.Width.Should().Be(1920);
        renderedImage.Height.Should().Be(720);
    }

    #endregion
}