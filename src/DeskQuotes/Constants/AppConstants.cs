namespace DeskQuotes.Constants;

public static class AppConstants
{
    public const string AppName = "DeskQuotes";
    public const string RefreshWallpaperHotkeyDisplay = "Ctrl + Alt + U";
    public const string RefreshWallpaperMenuLabel = "Refresh Wallpaper (" + RefreshWallpaperHotkeyDisplay + ")";
    public const int RefreshWallpaperHotkeyId = 1;
    public const int WmHotkey = 0x0312;
    public const uint HotkeyModifierAlt = 0x0001;
    public const uint HotkeyModifierControl = 0x0002;
    public const uint HotkeyModifierNoRepeat = 0x4000;
    public const uint RefreshWallpaperHotkeyModifiers = HotkeyModifierAlt | HotkeyModifierControl | HotkeyModifierNoRepeat;
    public const uint RefreshWallpaperHotkeyVirtualKey = 0x55;
}