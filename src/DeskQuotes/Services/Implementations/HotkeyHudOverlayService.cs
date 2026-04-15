namespace DeskQuotes.Services.Implementations;

public class HotkeyHudOverlayService : IDisposable
{
    private HotkeyHudOverlayForm? _overlayForm;
    private bool _isDisposed;

    public virtual void WarmUp()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        _ = EnsureOverlayForm();
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        if (_overlayForm is not null)
        {
            DisposeOverlayForm(_overlayForm);
            _overlayForm = null;
        }

        _isDisposed = true;
        GC.SuppressFinalize(this);
    }

    public virtual void Show(HotkeyHudOverlayContent content)
    {
        ArgumentNullException.ThrowIfNull(content);
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        PresentOverlay(content);
    }

    public void ShowBackgroundDarkened()
    {
        Show(new HotkeyHudOverlayContent
        {
            Kind = HotkeyHudOverlayKind.BackgroundDarker,
            Message = "Background darker"
        });
    }

    public void ShowBackgroundLightened()
    {
        Show(new HotkeyHudOverlayContent
        {
            Kind = HotkeyHudOverlayKind.BackgroundLighter,
            Message = "Background lighter"
        });
    }

    public void ShowFontChanged(string? fontFamilyName)
    {
        var normalizedFontFamilyName = string.IsNullOrWhiteSpace(fontFamilyName)
            ? null
            : fontFamilyName.Trim();

        Show(new HotkeyHudOverlayContent
        {
            Kind = HotkeyHudOverlayKind.Font,
            Message = normalizedFontFamilyName is null
                ? "Font changed"
                : $"Font: {normalizedFontFamilyName}"
        });
    }

    public void ShowOpeningSettings()
    {
        Show(new HotkeyHudOverlayContent
        {
            Kind = HotkeyHudOverlayKind.Settings,
            Message = "Opening settings"
        });
    }

    public void ShowRandomBackground()
    {
        Show(new HotkeyHudOverlayContent
        {
            Kind = HotkeyHudOverlayKind.RandomBackground,
            Message = "Random background"
        });
    }

    public void ShowWallpaperRefreshed()
    {
        Show(new HotkeyHudOverlayContent
        {
            Kind = HotkeyHudOverlayKind.Refresh,
            Message = "Wallpaper refreshed"
        });
    }

    protected virtual HotkeyHudOverlayForm CreateOverlayForm()
    {
        return new HotkeyHudOverlayForm();
    }

    protected virtual void DisposeOverlayForm(HotkeyHudOverlayForm overlayForm)
    {
        overlayForm.Dispose();
    }

    protected virtual void PresentOverlay(HotkeyHudOverlayContent content)
    {
        var overlayForm = EnsureOverlayForm();
        overlayForm.ShowOverlay(content);
        RenderOverlayImmediately(overlayForm);
    }

    private HotkeyHudOverlayForm EnsureOverlayForm()
    {
        _overlayForm ??= CreateOverlayForm();
        _ = _overlayForm.Handle;
        return _overlayForm;
    }

    protected virtual void RenderOverlayImmediately(HotkeyHudOverlayForm overlayForm)
    {
        overlayForm.Update();
        overlayForm.Refresh();
        Application.DoEvents();
    }
}
