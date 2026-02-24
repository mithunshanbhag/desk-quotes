namespace DeskQuotes.Services.Validators;

public sealed class WallpaperResolutionValidator : AbstractValidator<Size>
{
    public WallpaperResolutionValidator()
    {
        RuleFor(size => size.Width).GreaterThan(0);
        RuleFor(size => size.Height).GreaterThan(0);
    }
}