namespace DeskQuotes.Services.Implementations;

public partial class WindowsWallpaperService(IValidator<WallpaperPathInput>? wallpaperPathValidator = null)
{
    private const uint SpiSetDeskWallpaper = 0x0014;
    private const uint SpifUpdateIniFile = 0x0001;
    private const uint SpifSendWinIniChange = 0x0002;
    private readonly IValidator<WallpaperPathInput> _wallpaperPathValidator = wallpaperPathValidator ?? new WindowsWallpaperPathInputValidator();

    public virtual bool TryApplyWallpaper(string wallpaperPath)
    {
        var validationResult = _wallpaperPathValidator.Validate(new WallpaperPathInput(wallpaperPath));
        if (!validationResult.IsValid) return false;

        return SystemParametersInfo(
            SpiSetDeskWallpaper,
            0,
            wallpaperPath,
            SpifUpdateIniFile | SpifSendWinIniChange);
    }

    [LibraryImport("user32.dll", EntryPoint = "SystemParametersInfoW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SystemParametersInfo(uint uiAction, uint uiParam, string pvParam, uint fWinIni);
}