namespace DeskQuotes.Services;

public class MonitorResolutionService
{
    private static readonly Size FallbackResolution = new(1920, 1080);

    public virtual Size InferWallpaperResolution()
    {
        var screens = Screen.AllScreens;
        if (screens.Length == 0) return FallbackResolution;

        var bounds = Screen.PrimaryScreen?.Bounds ?? screens[0].Bounds;

        var width = bounds.Width > 0 ? bounds.Width : FallbackResolution.Width;
        var height = bounds.Height > 0 ? bounds.Height : FallbackResolution.Height;

        return new Size(width, height);
    }
}
