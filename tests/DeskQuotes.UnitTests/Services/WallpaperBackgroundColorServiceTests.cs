namespace DeskQuotes.UnitTests.Services;

public class WallpaperBackgroundColorServiceTests
{
    #region Positive cases

    [Fact]
    public void GetNextAutomaticBackgroundColor_WhenCalledSequentially_ReturnsDifferentDarkColors()
    {
        var sut = new WallpaperBackgroundColorService();

        var firstColor = sut.GetNextAutomaticBackgroundColor();
        var secondColor = sut.GetNextAutomaticBackgroundColor();

        Assert.NotEqual(secondColor.ToArgb(), firstColor.ToArgb());
        Assert.True(firstColor.GetBrightness() < 0.2f);
        Assert.True(secondColor.GetBrightness() < 0.2f);
    }

    [Fact]
    public void GetRandomDarkColor_WhenCalled_ReturnsDarkColor()
    {
        var sut = new WallpaperBackgroundColorService(new Random(1234));

        var randomDarkColor = sut.GetRandomDarkColor();

        Assert.True(randomDarkColor.GetBrightness() < 0.2f);
    }

    [Fact]
    public void SetCurrentBackgroundColor_WhenColorIsProvided_GetCurrentBackgroundColorReturnsIt()
    {
        var sut = new WallpaperBackgroundColorService();
        var selectedColor = Color.FromArgb(42, 56, 78);

        sut.SetCurrentBackgroundColor(selectedColor);

        Assert.Equal(selectedColor.ToArgb(), sut.GetCurrentBackgroundColor().ToArgb());
    }

    #endregion

    #region Boundary cases

    [Fact]
    public void Darken_WhenRedChannelIsNearZero_DoesNotUnderflowAndDarkensOtherChannels()
    {
        var sut = new WallpaperBackgroundColorService();
        var input = Color.FromArgb(1, 16, 255);

        var result = sut.Darken(input);

        Assert.InRange(result.R, 0, 1);
        Assert.Equal(14, result.G);
        Assert.Equal(219, result.B);
    }

    [Fact]
    public void Darken_WhenBlueChannelIsAlreadyZero_PreservesZeroWithoutUnderflow()
    {
        var sut = new WallpaperBackgroundColorService();
        var input = Color.FromArgb(40, 12, 0);

        var result = sut.Darken(input);

        Assert.Equal(34, result.R);
        Assert.Equal(10, result.G);
        Assert.Equal(0, result.B);
    }

    [Fact]
    public void Lighten_WhenGreenChannelIsNearMaximum_DoesNotOverflowAndLightensRemainingChannels()
    {
        var sut = new WallpaperBackgroundColorService();
        var input = Color.FromArgb(12, 254, 80);

        var result = sut.Lighten(input);

        Assert.Equal(46, result.R);
        Assert.Equal(254, result.G);
        Assert.Equal(105, result.B);
    }

    [Fact]
    public void Lighten_WhenBlueChannelIsAlreadyAtMaximum_LeavesItClampedAt255()
    {
        var sut = new WallpaperBackgroundColorService();
        var input = Color.FromArgb(250, 5, 255);

        var result = sut.Lighten(input);

        Assert.Equal(251, result.R);
        Assert.Equal(40, result.G);
        Assert.Equal(255, result.B);
    }

    #endregion
}