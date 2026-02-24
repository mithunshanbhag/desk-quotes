using System.Windows.Forms;

namespace DeskQuotes.UnitTests.Services;

public class MonitorResolutionServiceTests
{
    [Fact]
    public void InferWallpaperResolution_ReturnsPrimaryMonitorBounds()
    {
        var primaryScreen = Screen.PrimaryScreen;
        if (primaryScreen is null) return;

        var sut = new MonitorResolutionService();

        var resolution = sut.InferWallpaperResolution();

        resolution.Should().Be(new Size(primaryScreen.Bounds.Width, primaryScreen.Bounds.Height));
    }

    [Fact]
    public void InferWallpaperResolution_WhenVirtualDesktopIsLargerThanPrimaryMonitor_DoesNotUseVirtualDesktopUnion()
    {
        var primaryScreen = Screen.PrimaryScreen;
        if (primaryScreen is null) return;

        var virtualDesktop = SystemInformation.VirtualScreen;
        var primaryBounds = primaryScreen.Bounds;
        if (virtualDesktop.Width <= primaryBounds.Width && virtualDesktop.Height <= primaryBounds.Height) return;

        var sut = new MonitorResolutionService();

        var resolution = sut.InferWallpaperResolution();

        resolution.Should().Be(new Size(primaryBounds.Width, primaryBounds.Height));
        resolution.Should().NotBe(new Size(virtualDesktop.Width, virtualDesktop.Height));
    }
}
