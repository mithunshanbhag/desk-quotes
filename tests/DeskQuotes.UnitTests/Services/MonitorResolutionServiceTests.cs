namespace DeskQuotes.UnitTests.Services;

public class MonitorResolutionServiceTests
{
    [Fact]
    public void InferWallpaperResolution_ReturnsPositiveDimensions()
    {
        var sut = new MonitorResolutionService();

        var resolution = sut.InferWallpaperResolution();

        resolution.Width.Should().BeGreaterThan(0);
        resolution.Height.Should().BeGreaterThan(0);
    }
}