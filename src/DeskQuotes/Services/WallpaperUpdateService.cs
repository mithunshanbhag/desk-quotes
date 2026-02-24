namespace DeskQuotes.Services;

public class WallpaperUpdateService(
    QuoteSelectionService quoteSelectionService,
    MonitorResolutionService monitorResolutionService,
    WallpaperRenderService wallpaperRenderService,
    WindowsWallpaperService windowsWallpaperService)
{
    public bool TryUpdateWallpaper(IEnumerable<Quote>? configuredQuotes)
    {
        if (!QuoteSelectionService.TrySelectRandomQuote(configuredQuotes, out var selectedQuote) || selectedQuote is null) return false;

        try
        {
            var resolution = monitorResolutionService.InferWallpaperResolution();
            var wallpaperPath = wallpaperRenderService.RenderQuoteWallpaper(selectedQuote, resolution);
            return windowsWallpaperService.TryApplyWallpaper(wallpaperPath);
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