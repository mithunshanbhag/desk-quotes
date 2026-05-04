namespace DeskQuotes.Services.Implementations;

public class MonitorResolutionService(ILogger<MonitorResolutionService>? logger = null)
{
    private static readonly Size FallbackResolution = new(1920, 1080);
    private readonly ILogger<MonitorResolutionService> _logger = logger ?? NullLogger<MonitorResolutionService>.Instance;

    public virtual Size InferWallpaperResolution()
    {
        var screens = Screen.AllScreens;
        if (screens.Length == 0)
        {
            _logger.LogWarning("No screens were detected. Falling back to {Width}x{Height}.", FallbackResolution.Width, FallbackResolution.Height);
            return FallbackResolution;
        }

        var bounds = Screen.PrimaryScreen?.Bounds ?? screens[0].Bounds;

        var width = bounds.Width > 0 ? bounds.Width : FallbackResolution.Width;
        var height = bounds.Height > 0 ? bounds.Height : FallbackResolution.Height;

        _logger.LogDebug("Inferred wallpaper resolution {Width}x{Height} using {ScreenCount} detected screen(s).", width, height, screens.Length);
        return new Size(width, height);
    }
}
