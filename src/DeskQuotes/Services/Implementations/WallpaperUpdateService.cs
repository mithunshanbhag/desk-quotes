namespace DeskQuotes.Services.Implementations;

public class WallpaperUpdateService(
    QuoteSelectionService quoteSelectionService,
    MonitorResolutionService monitorResolutionService,
    WallpaperBackgroundColorService wallpaperBackgroundColorService,
    WallpaperRenderService wallpaperRenderService,
    WindowsWallpaperService windowsWallpaperService)
{
    private Quote? _currentQuote;

    private readonly MonitorResolutionService _monitorResolutionService = monitorResolutionService ?? throw new ArgumentNullException(nameof(monitorResolutionService));
    private readonly QuoteSelectionService _quoteSelectionService = quoteSelectionService ?? throw new ArgumentNullException(nameof(quoteSelectionService));

    private readonly WallpaperBackgroundColorService _wallpaperBackgroundColorService =
        wallpaperBackgroundColorService ?? throw new ArgumentNullException(nameof(wallpaperBackgroundColorService));

    private readonly WallpaperRenderService _wallpaperRenderService = wallpaperRenderService ?? throw new ArgumentNullException(nameof(wallpaperRenderService));
    private readonly WindowsWallpaperService _windowsWallpaperService = windowsWallpaperService ?? throw new ArgumentNullException(nameof(windowsWallpaperService));

    public bool TryUpdateWallpaper(IEnumerable<Quote>? configuredQuotes, Color? backgroundColor = null)
    {
        var selectedQuote = backgroundColor.HasValue
            ? _currentQuote
            : null;

        if (selectedQuote is null &&
            (!_quoteSelectionService.TrySelectQuote(configuredQuotes, out selectedQuote) || selectedQuote is null))
            return false;

        try
        {
            var resolution = _monitorResolutionService.InferWallpaperResolution();
            var selectedBackgroundColor = backgroundColor ?? _wallpaperBackgroundColorService.GetNextAutomaticBackgroundColor();
            var wallpaperPath = _wallpaperRenderService.RenderQuoteWallpaper(selectedQuote, resolution, selectedBackgroundColor);
            var wallpaperApplied = _windowsWallpaperService.TryApplyWallpaper(wallpaperPath);

            if (wallpaperApplied)
            {
                _currentQuote = selectedQuote;
                _wallpaperBackgroundColorService.SetCurrentBackgroundColor(selectedBackgroundColor);
            }

            return wallpaperApplied;
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