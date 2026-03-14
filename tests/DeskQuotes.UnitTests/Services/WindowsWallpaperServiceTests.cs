using WindowsWallpaperService = DeskQuotes.Services.Implementations.WindowsWallpaperService;

namespace DeskQuotes.UnitTests.Services;

public class WindowsWallpaperServiceTests
{
    private readonly WindowsWallpaperService _sut = new();

    private sealed class RejectingWallpaperPathValidator : AbstractValidator<WallpaperPathInput>
    {
        public int CallCount { get; private set; }
        public string? CapturedPath { get; private set; }

        public override ValidationResult Validate(ValidationContext<WallpaperPathInput> context)
        {
            CallCount++;
            CapturedPath = context.InstanceToValidate.WallpaperPath;
            return new ValidationResult([new ValidationFailure(nameof(WallpaperPathInput.WallpaperPath), "Invalid wallpaper path.")]);
        }
    }

    #region Negative cases

    [Fact]
    public void TryApplyWallpaper_WhenValidatorRejectsPath_ReturnsFalse()
    {
        var validator = new RejectingWallpaperPathValidator();
        var sut = new WindowsWallpaperService(validator);

        var result = sut.TryApplyWallpaper(@"C:\invalid-path\wallpaper.bmp");

        Assert.False(result);
        Assert.Equal(1, validator.CallCount);
        Assert.Equal(@"C:\invalid-path\wallpaper.bmp", validator.CapturedPath);
    }

    [Fact]
    public void TryApplyWallpaper_WhenPathIsNull_ReturnsFalse()
    {
        var result = _sut.TryApplyWallpaper(null!);

        Assert.False(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void TryApplyWallpaper_WhenPathIsBlank_ReturnsFalse(string wallpaperPath)
    {
        var result = _sut.TryApplyWallpaper(wallpaperPath);

        Assert.False(result);
    }

    [Fact]
    public void TryApplyWallpaper_WhenPathDoesNotExist_ReturnsFalse()
    {
        var result = _sut.TryApplyWallpaper(Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.bmp"));

        Assert.False(result);
    }

    #endregion
}