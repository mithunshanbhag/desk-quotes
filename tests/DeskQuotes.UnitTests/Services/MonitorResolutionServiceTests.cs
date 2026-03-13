using System.Windows.Forms;

namespace DeskQuotes.UnitTests.Services;

public class MonitorResolutionServiceTests
{
    #region Positive cases

    [Fact]
    public void InferWallpaperResolution_ReturnsPrimaryMonitorBounds()
    {
        var primaryScreen = Screen.PrimaryScreen;
        if (primaryScreen is null) return;

        var sut = new MonitorResolutionService();

        var resolution = sut.InferWallpaperResolution();

        Assert.Equal(new Size(primaryScreen.Bounds.Width, primaryScreen.Bounds.Height), resolution);
    }

    #endregion

    #region Boundary cases

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

        Assert.Equal(new Size(primaryBounds.Width, primaryBounds.Height), resolution);
        Assert.NotEqual(new Size(virtualDesktop.Width, virtualDesktop.Height), resolution);
    }

    #endregion
}