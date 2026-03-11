using DeskQuotes.Services.Validators;

namespace DeskQuotes.UnitTests.Services;

public class WallpaperRenderServiceTests
{
    private static readonly Color WallpaperBackgroundColor = Color.FromArgb(24, 27, 36);

    #region Negative cases

    [Fact]
    public void RenderQuoteWallpaper_WhenQuoteIsNull_ThrowsArgumentNullException()
    {
        var sut = new WallpaperRenderService();

        var action = () => sut.RenderQuoteWallpaper(null!, new Size(1920, 1080));

        action.Should().Throw<ArgumentNullException>();
    }

    #endregion

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

        var outputPath = sut.RenderQuoteWallpaper(quote, new Size(1920, 1080));
        using var renderedImage = new Bitmap(outputPath);
        var textClusters = GetMergedTextClusters(renderedImage);

        textClusters.Should().HaveCount(2);

        var gapBetweenQuoteAndAuthor = textClusters[1].Start - textClusters[0].End - 1;
        gapBetweenQuoteAndAuthor.Should().BeLessThan(140);
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

    private static List<(int Start, int End)> GetMergedTextClusters(Bitmap renderedImage)
    {
        var rowsWithText = Enumerable
            .Range(0, renderedImage.Height)
            .Where(row => RowContainsText(renderedImage, row))
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

    private static bool RowContainsText(Bitmap renderedImage, int row)
    {
        for (var column = 0; column < renderedImage.Width; column++)
        {
            if (renderedImage.GetPixel(column, row).ToArgb() != WallpaperBackgroundColor.ToArgb())
                return true;
        }

        return false;
    }
}
