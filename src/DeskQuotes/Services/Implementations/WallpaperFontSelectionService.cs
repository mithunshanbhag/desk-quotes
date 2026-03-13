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
}