using DeskQuotes.Services.Validators;
using FluentValidation;
using FluentValidation.Results;
using WindowsWallpaperService = DeskQuotes.Services.Implementations.WindowsWallpaperService;

namespace DeskQuotes.UnitTests.Services;

public class WindowsWallpaperServiceTests
{
    private readonly WindowsWallpaperService _sut = new();

    #region Negative cases

    [Fact]
    public void TryApplyWallpaper_WhenValidatorRejectsPath_ReturnsFalse()
    {
        var validator = new RejectingWallpaperPathValidator();
        var sut = new WindowsWallpaperService(validator);

        var result = sut.TryApplyWallpaper(@"C:\invalid-path\wallpaper.bmp");

        result.Should().BeFalse();
        validator.CallCount.Should().Be(1);
        validator.CapturedPath.Should().Be(@"C:\invalid-path\wallpaper.bmp");
    }

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

    #endregion

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
}
