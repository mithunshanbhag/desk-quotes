namespace DeskQuotes.Components;

internal sealed class TrayContextMenuActions
{
    public required EventHandler RefreshWallpaper { get; init; }
    public required EventHandler SetMood { get; init; }
    public required EventHandler DarkenWallpaperBackgroundColor { get; init; }
    public required EventHandler LightenWallpaperBackgroundColor { get; init; }
    public required EventHandler RandomizeWallpaperBackgroundColor { get; init; }
    public required EventHandler RandomizeWallpaperFont { get; init; }
    public required EventHandler ToggleRunAtSignIn { get; init; }
    public required EventHandler EditSettings { get; init; }
    public required EventHandler Exit { get; init; }
}
