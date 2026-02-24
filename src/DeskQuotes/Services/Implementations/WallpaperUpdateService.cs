namespace DeskQuotes.Services.Implementations;

public class WallpaperUpdateService(
    QuoteSelectionService quoteSelectionService,
    MonitorResolutionService monitorResolutionService,
    WallpaperRenderService wallpaperRenderService,
    WindowsWallpaperService windowsWallpaperService)
{
    private readonly MonitorResolutionService _monitorResolutionService = monitorResolutionService ?? throw new ArgumentNullException(nameof(monitorResolutionService));
    private readonly QuoteSelectionService _quoteSelectionService = quoteSelectionService ?? throw new ArgumentNullException(nameof(quoteSelectionService));
    private readonly WallpaperRenderService _wallpaperRenderService = wallpaperRenderService ?? throw new ArgumentNullException(nameof(wallpaperRenderService));
    private readonly WindowsWallpaperService _windowsWallpaperService = windowsWallpaperService ?? throw new ArgumentNullException(nameof(windowsWallpaperService));

    public bool TryUpdateWallpaper(IEnumerable<Quote>? configuredQuotes)
    {
        if (!_quoteSelectionService.TrySelectQuote(configuredQuotes, out var selectedQuote) || selectedQuote is null) return false;

        try
        {
            var resolution = _monitorResolutionService.InferWallpaperResolution();
            var wallpaperPath = _wallpaperRenderService.RenderQuoteWallpaper(selectedQuote, resolution);
            return _windowsWallpaperService.TryApplyWallpaper(wallpaperPath);
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (ExternalException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }
}