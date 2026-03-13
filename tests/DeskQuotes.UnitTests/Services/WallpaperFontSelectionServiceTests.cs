namespace DeskQuotes.UnitTests.Services;

public class WallpaperFontSelectionServiceTests
{
    private static readonly string[] ExpectedFonts =
    [
        "Segoe UI",
        "Georgia",
        "Palatino Linotype",
        "Trebuchet MS",
        "Constantia"
    ];

    [Fact]
    public void GetRandomFontFamilyName_WhenCalled_ReturnsOnlyCuratedFonts()
    {
        var sut = new WallpaperFontSelectionService(new Random(1234));

        var selectedFonts = Enumerable
            .Range(0, 25)
            .Select(_ => sut.GetRandomFontFamilyName())
            .ToArray();

        Assert.All(selectedFonts, font => Assert.Contains(font, ExpectedFonts));
    }

    [Fact]
    public void GetRandomFontFamilyName_WhenRandomSeedMatches_ProducesDeterministicSequence()
    {
        var first = new WallpaperFontSelectionService(new Random(42));
        var second = new WallpaperFontSelectionService(new Random(42));

        var firstSequence = Enumerable
            .Range(0, 10)
            .Select(_ => first.GetRandomFontFamilyName())
            .ToArray();
        var secondSequence = Enumerable
            .Range(0, 10)
            .Select(_ => second.GetRandomFontFamilyName())
            .ToArray();

        Assert.Equal(secondSequence, firstSequence);
    }

    [Fact]
    public void GetRandomFontFamilyName_WhenExcludedFontProvided_NeverReturnsThatFont()
    {
        var sut = new WallpaperFontSelectionService(new Random(1234));

        var selectedFonts = Enumerable
            .Range(0, 25)
            .Select(_ => sut.GetRandomFontFamilyName("Georgia"))
            .ToArray();

        Assert.All(selectedFonts, font =>
        {
            Assert.Contains(font, ExpectedFonts);
            Assert.NotEqual("Georgia", font);
        });
    }
}