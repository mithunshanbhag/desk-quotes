namespace DeskQuotes.Services.Validators;

public sealed record WallpaperPathInput(string? WallpaperPath);

public sealed class WindowsWallpaperPathInputValidator : AbstractValidator<WallpaperPathInput>
{
    public WindowsWallpaperPathInputValidator()
    {
        RuleFor(x => x.WallpaperPath)
            .Must(wallpaperPath => !string.IsNullOrWhiteSpace(wallpaperPath) && File.Exists(wallpaperPath));
    }
}