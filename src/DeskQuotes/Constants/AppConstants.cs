namespace DeskQuotes.Constants;

public static class AppConstants
{
    public const string AppName = "DeskQuotes";
    public const string SingleInstanceMutexName = @"Local\DeskQuotes.SingleInstance";
    public const int WmHotkey = 0x0312;
    public const uint HotkeyModifierAlt = 0x0001;
    public const uint HotkeyModifierControl = 0x0002;
    public const uint HotkeyModifierNoRepeat = 0x4000;

    public const string RefreshWallpaperHotkeyDisplay = "(Ctrl + Alt + U)";
    public const string RefreshWallpaperMenuLabel = "Refresh Wallpaper";
    public const int RefreshWallpaperHotkeyId = 1;
    public const uint RefreshWallpaperHotkeyModifiers = HotkeyModifierAlt | HotkeyModifierControl | HotkeyModifierNoRepeat;
    public const uint RefreshWallpaperHotkeyVirtualKey = 0x55; // 'U' key
    public const string RunAtSignInMenuLabel = "Run at Logon";
    public const string RunAtSignInRegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    public const string RunAtSignInRegistryValueName = AppName;
    public const string SetMoodMenuLabel = "Set Mood";
    public const string AllQuotesMoodMenuLabel = "All Quotes";
    public const string SelectedMoodStateFileName = "selected-mood.txt";

    public const string EditSettingsHotkeyDisplay = "(Ctrl + Alt + E)";
    public const string EditSettingsMenuLabel = "Edit Settings";
    public const string ExitMenuLabel = "E&xit";
    public const int EditSettingsHotkeyId = 2;
    public const uint EditSettingsHotkeyModifiers = HotkeyModifierAlt | HotkeyModifierControl | HotkeyModifierNoRepeat;
    public const uint EditSettingsHotkeyVirtualKey = 0x45; // 'E' key

    public const string WallpaperBackgroundColorMenuLabel = "Wallpaper Background Color";

    public const string DarkenWallpaperBackgroundColorHotkeyDisplay = "(Ctrl + Alt + -)";
    public const string DarkenWallpaperBackgroundColorMenuLabel = "Darken Color";
    public const int DarkenWallpaperBackgroundColorHotkeyId = 3;
    public const uint DarkenWallpaperBackgroundColorHotkeyModifiers = HotkeyModifierAlt | HotkeyModifierControl | HotkeyModifierNoRepeat;
    public const uint DarkenWallpaperBackgroundColorHotkeyVirtualKey = 0xBD; // OEM Minus

    public const string LightenWallpaperBackgroundColorHotkeyDisplay = "(Ctrl + Alt + =)";
    public const string LightenWallpaperBackgroundColorMenuLabel = "Lighten Color";
    public const int LightenWallpaperBackgroundColorHotkeyId = 4;
    public const uint LightenWallpaperBackgroundColorHotkeyModifiers = HotkeyModifierAlt | HotkeyModifierControl | HotkeyModifierNoRepeat;
    public const uint LightenWallpaperBackgroundColorHotkeyVirtualKey = 0xBB; // OEM Plus / '=' key

    public const string RandomWallpaperBackgroundColorHotkeyDisplay = "(Ctrl + Alt + 0)";
    public const string RandomWallpaperBackgroundColorMenuLabel = "Random Color";
    public const int RandomWallpaperBackgroundColorHotkeyId = 5;
    public const uint RandomWallpaperBackgroundColorHotkeyModifiers = HotkeyModifierAlt | HotkeyModifierControl | HotkeyModifierNoRepeat;
    public const uint RandomWallpaperBackgroundColorHotkeyVirtualKey = 0x30; // '0' key

    public const string ChangeWallpaperFontMenuLabel = "Change Wallpaper Font";

    public const string RandomWallpaperFontHotkeyDisplay = "(Ctrl + Alt + F)";
    public const string RandomWallpaperFontMenuLabel = "Random Font";
    public const int RandomWallpaperFontHotkeyId = 6;
    public const uint RandomWallpaperFontHotkeyModifiers = HotkeyModifierAlt | HotkeyModifierControl | HotkeyModifierNoRepeat;
    public const uint RandomWallpaperFontHotkeyVirtualKey = 0x46; // 'F' key
}
