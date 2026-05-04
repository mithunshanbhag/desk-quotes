namespace DeskQuotes.Services.Implementations;

public partial class WindowsWallpaperService(
    IValidator<WallpaperPathInput>? wallpaperPathValidator = null,
    ILogger<WindowsWallpaperService>? logger = null)
{
    private const uint SpiSetDeskWallpaper = 0x0014;
    private const uint SpifUpdateIniFile = 0x0001;
    private const uint SpifSendWinIniChange = 0x0002;
    private readonly ILogger<WindowsWallpaperService> _logger = logger ?? NullLogger<WindowsWallpaperService>.Instance;
    private readonly IValidator<WallpaperPathInput> _wallpaperPathValidator = wallpaperPathValidator ?? new WindowsWallpaperPathInputValidator();

    public virtual bool TryApplyWallpaper(string wallpaperPath)
    {
        var validationResult = _wallpaperPathValidator.Validate(new WallpaperPathInput(wallpaperPath));
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Wallpaper path validation failed for {WallpaperPath}.", wallpaperPath);
            return false;
        }

        var wallpaperApplied = SystemParametersInfo(
            SpiSetDeskWallpaper,
            0,
            wallpaperPath,
            SpifUpdateIniFile | SpifSendWinIniChange);

        if (wallpaperApplied)
        {
            _logger.LogInformation("Applied wallpaper from {WallpaperPath}.", wallpaperPath);
        }
        else
        {
            _logger.LogWarning("Windows rejected wallpaper path {WallpaperPath}. Win32Error: {Win32Error}.", wallpaperPath, Marshal.GetLastWin32Error());
        }

        return wallpaperApplied;
    }

    [LibraryImport("user32.dll", EntryPoint = "SystemParametersInfoW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SystemParametersInfo(uint uiAction, uint uiParam, string pvParam, uint fWinIni);
}
