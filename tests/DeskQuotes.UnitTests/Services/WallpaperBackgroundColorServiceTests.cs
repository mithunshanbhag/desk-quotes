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

        firstColor.ToArgb().Should().NotBe(secondColor.ToArgb());
        firstColor.GetBrightness().Should().BeLessThan(0.2f);
        secondColor.GetBrightness().Should().BeLessThan(0.2f);
    }

    [Fact]
    public void GetRandomDarkColor_WhenCalled_ReturnsDarkColor()
    {
        var sut = new WallpaperBackgroundColorService(new Random(1234));

        var randomDarkColor = sut.GetRandomDarkColor();

        randomDarkColor.GetBrightness().Should().BeLessThan(0.2f);
    }

    [Fact]
    public void SetCurrentBackgroundColor_WhenColorIsProvided_GetCurrentBackgroundColorReturnsIt()
    {
        var sut = new WallpaperBackgroundColorService();
        var selectedColor = Color.FromArgb(42, 56, 78);

        sut.SetCurrentBackgroundColor(selectedColor);

        sut.GetCurrentBackgroundColor().ToArgb().Should().Be(selectedColor.ToArgb());
    }

    #endregion

    #region Boundary cases

    [Fact]
    public void Darken_WhenRedChannelIsNearZero_DoesNotUnderflowAndDarkensOtherChannels()
    {
        var sut = new WallpaperBackgroundColorService();
        var input = Color.FromArgb(1, 16, 255);

        var result = sut.Darken(input);

        result.R.Should().BeInRange(0, 1);
        result.G.Should().Be(14);
        result.B.Should().Be(219);
    }

    [Fact]
    public void Darken_WhenBlueChannelIsAlreadyZero_PreservesZeroWithoutUnderflow()
    {
        var sut = new WallpaperBackgroundColorService();
        var input = Color.FromArgb(40, 12, 0);

        var result = sut.Darken(input);

        result.R.Should().Be(34);
        result.G.Should().Be(10);
        result.B.Should().Be(0);
    }

    [Fact]
    public void Lighten_WhenGreenChannelIsNearMaximum_DoesNotOverflowAndLightensRemainingChannels()
    {
        var sut = new WallpaperBackgroundColorService();
        var input = Color.FromArgb(12, 254, 80);

        var result = sut.Lighten(input);

        result.R.Should().Be(46);
        result.G.Should().Be(254);
        result.B.Should().Be(105);
    }

    [Fact]
    public void Lighten_WhenBlueChannelIsAlreadyAtMaximum_LeavesItClampedAt255()
    {
        var sut = new WallpaperBackgroundColorService();
        var input = Color.FromArgb(250, 5, 255);

        var result = sut.Lighten(input);

        result.R.Should().Be(251);
        result.G.Should().Be(40);
        result.B.Should().Be(255);
    }

    #endregion
}