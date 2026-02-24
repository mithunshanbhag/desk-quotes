namespace DeskQuotes.UnitTests.Services;

public class WindowsWallpaperServiceTests
{
    private readonly WindowsWallpaperService _sut = new();

    [Fact]
    public void TryApplyWallpaper_WhenPathIsNull_ReturnsFalse()
    {
        var result = _sut.TryApplyWallpaper(null!);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void TryApplyWallpaper_WhenPathIsBlank_ReturnsFalse(string wallpaperPath)
    {
        var result = _sut.TryApplyWallpaper(wallpaperPath);

        result.Should().BeFalse();
    }

    [Fact]
    public void TryApplyWallpaper_WhenPathDoesNotExist_ReturnsFalse()
    {
        var result = _sut.TryApplyWallpaper(Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.bmp"));

        result.Should().BeFalse();
    }
}