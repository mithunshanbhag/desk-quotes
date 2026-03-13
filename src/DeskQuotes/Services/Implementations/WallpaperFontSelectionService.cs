namespace DeskQuotes.Services.Implementations;

public class WallpaperFontSelectionService(Random random)
{
    private static readonly string[] CuratedFontFamilyNames =
    [
        "Segoe UI",
        "Georgia",
        "Palatino Linotype",
        "Trebuchet MS",
        "Constantia"
    ];

    private readonly Random _random = random ?? throw new ArgumentNullException(nameof(random));

    public WallpaperFontSelectionService()
        : this(Random.Shared)
    {
    }

    public virtual string GetRandomFontFamilyName()
    {
        return CuratedFontFamilyNames[_random.Next(CuratedFontFamilyNames.Length)];
    }

    public virtual string GetRandomFontFamilyName(string? excludedFontFamilyName)
    {
        var availableFontFamilyNames = CuratedFontFamilyNames
            .Where(fontFamilyName => !string.Equals(fontFamilyName, excludedFontFamilyName, StringComparison.Ordinal))
            .ToArray();

        return availableFontFamilyNames.Length == 0
            ? GetRandomFontFamilyName()
            : availableFontFamilyNames[_random.Next(availableFontFamilyNames.Length)];
    }
}