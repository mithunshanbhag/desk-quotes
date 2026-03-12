namespace DeskQuotes.Constants;

public static class AppConstants
{
    public const string AppName = "DeskQuotes";
    public const int WmHotkey = 0x0312;
    public const uint HotkeyModifierAlt = 0x0001;
    public const uint HotkeyModifierControl = 0x0002;
    public const uint HotkeyModifierNoRepeat = 0x4000;

    public const string RefreshWallpaperHotkeyDisplay = "Ctrl + Alt + U";
    public const string RefreshWallpaperMenuLabel = "Refresh Wallpaper (" + RefreshWallpaperHotkeyDisplay + ")";
    public const int RefreshWallpaperHotkeyId = 1;
    public const uint RefreshWallpaperHotkeyModifiers = HotkeyModifierAlt | HotkeyModifierControl | HotkeyModifierNoRepeat;
    public const uint RefreshWallpaperHotkeyVirtualKey = 0x55; // 'U' key

    public const string EditSettingsHotkeyDisplay = "Ctrl + Alt + E";
    public const string EditSettingsMenuLabel = "Edit Settings (" + EditSettingsHotkeyDisplay + ")";
    public const int EditSettingsHotkeyId = 2;
    public const uint EditSettingsHotkeyModifiers = HotkeyModifierAlt | HotkeyModifierControl | HotkeyModifierNoRepeat;
    public const uint EditSettingsHotkeyVirtualKey = 0x45; // 'E' key
}