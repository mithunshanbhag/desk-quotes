namespace DeskQuotes.Services.Implementations;

public class WallpaperBackgroundColorService(Random random)
{
    private const float DarkenAmount = 0.14f;
    private const float LightenAmount = 0.14f;
    private const float MaximumDarkBrightness = 0.2f;

    private static readonly Color[] AutomaticBackgroundPalette =
    [
        Color.FromArgb(24, 27, 36),
        Color.FromArgb(18, 33, 46),
        Color.FromArgb(33, 27, 44),
        Color.FromArgb(29, 38, 30),
        Color.FromArgb(38, 28, 28)
    ];

    private readonly Random _random = random ?? throw new ArgumentNullException(nameof(random));
    private Color? _currentBackgroundColor;
    private int _lastAutomaticBackgroundColorIndex = -1;

    public WallpaperBackgroundColorService()
        : this(Random.Shared)
    {
    }

    public virtual Color Darken(Color input)
    {
        return Color.FromArgb(
            input.A,
            DarkenChannel(input.R),
            DarkenChannel(input.G),
            DarkenChannel(input.B));
    }

    public virtual Color GetCurrentBackgroundColor()
    {
        return _currentBackgroundColor ?? AutomaticBackgroundPalette[0];
    }

    public virtual Color GetNextAutomaticBackgroundColor()
    {
        _lastAutomaticBackgroundColorIndex = (_lastAutomaticBackgroundColorIndex + 1) % AutomaticBackgroundPalette.Length;
        return AutomaticBackgroundPalette[_lastAutomaticBackgroundColorIndex];
    }

    public virtual Color GetRandomDarkColor()
    {
        Color candidate;

        do
        {
            candidate = Color.FromArgb(
                _random.Next(0, 96),
                _random.Next(0, 96),
                _random.Next(0, 96));
        } while (candidate.GetBrightness() >= MaximumDarkBrightness);

        return candidate;
    }

    public virtual Color Lighten(Color input)
    {
        return Color.FromArgb(
            input.A,
            LightenChannel(input.R),
            LightenChannel(input.G),
            LightenChannel(input.B));
    }

    public virtual void SetCurrentBackgroundColor(Color backgroundColor)
    {
        _currentBackgroundColor = backgroundColor;
    }

    private static int DarkenChannel(int channel)
    {
        return Math.Clamp((int)Math.Round(channel * (1f - DarkenAmount), MidpointRounding.AwayFromZero), 0, 255);
    }

    private static int LightenChannel(int channel)
    {
        return Math.Clamp((int)Math.Round(channel + (255 - channel) * LightenAmount, MidpointRounding.AwayFromZero), 0, 255);
    }
}