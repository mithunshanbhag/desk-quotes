using System.Security;
using Timer = System.Windows.Forms.Timer;

namespace DeskQuotes.Components;

public class TrayAppContext : ApplicationContext
{
    private readonly GlobalHotkeyService _globalHotkeyService;
    private readonly HotkeyHudOverlayService _hotkeyHudOverlayService;
    private readonly List<Quote> _quotes = [];
    private readonly SelectedMoodService _selectedMoodService;
    private readonly List<string> _tagCatalog = [];
    private readonly List<ToolStripMenuItem> _setMoodMenuItems = [];
    private readonly Timer _refreshTimer = new();
    private readonly StartupLaunchService _startupLaunchService;
    private readonly NotifyIcon _trayIcon;
    private readonly WallpaperBackgroundColorService _wallpaperBackgroundColorService;
    private readonly WallpaperRefreshSchedulerService _wallpaperRefreshSchedulerService;
    private readonly WallpaperUpdateService _wallpaperUpdateService;
    private bool _isRefreshing;

    public TrayAppContext(
        IConfiguration configuration,
        GlobalHotkeyService globalHotkeyService,
        HotkeyHudOverlayService hotkeyHudOverlayService,
        SelectedMoodService selectedMoodService,
        StartupLaunchService startupLaunchService,
        WallpaperBackgroundColorService wallpaperBackgroundColorService,
        WallpaperUpdateService wallpaperUpdateService,
        WallpaperRefreshSchedulerService wallpaperRefreshSchedulerService)
    {
        _globalHotkeyService = globalHotkeyService;
        _hotkeyHudOverlayService = hotkeyHudOverlayService;
        _selectedMoodService = selectedMoodService;
        _startupLaunchService = startupLaunchService;
        _wallpaperBackgroundColorService = wallpaperBackgroundColorService;
        _wallpaperUpdateService = wallpaperUpdateService;
        _wallpaperRefreshSchedulerService = wallpaperRefreshSchedulerService;

        BindConfiguration(configuration);
        NormalizeSelectedMood();

        _trayIcon = new NotifyIcon
        {
            Icon = LoadTrayIcon(),
            ContextMenuStrip = CreateContextMenuStrip(),
            Visible = true,
            Text = AppConstants.AppName
        };
        ShowSelectedMoodStartupWarningIfNeeded();
        _hotkeyHudOverlayService.WarmUp();

        _refreshTimer.Tick += RefreshWallpaperOnSchedule;

        RegisterHotkeys();

        Application.Idle += RefreshWallpaperOnStartup;
        ScheduleNextRefresh();
    }

    private void RefreshWallpaper(object? sender, EventArgs e)
    {
        _hotkeyHudOverlayService.ShowWallpaperRefreshed();
        _ = RefreshWallpaperCore();
    }

    private WallpaperUpdateResult RefreshWallpaperCore(Color? backgroundColor = null, bool showProgressBalloonTip = true)
    {
        var selectedMood = _selectedMoodService.GetSelectedMood();
        return RefreshWallpaperCore(() => _wallpaperUpdateService.UpdateWallpaper(_quotes, selectedMood, backgroundColor), selectedMood, showProgressBalloonTip);
    }

    private WallpaperUpdateResult RefreshWallpaperCore(
        Func<WallpaperUpdateResult> refreshWallpaper,
        string? selectedMood = null,
        bool showProgressBalloonTip = true)
    {
        if (_isRefreshing)
            return WallpaperUpdateResult.Failed;

        _isRefreshing = true;
        if (showProgressBalloonTip)
            _trayIcon.ShowBalloonTip(1000, "Refreshing Wallpaper", "Your wallpaper is being refreshed.", ToolTipIcon.Info);

        try
        {
            var wallpaperUpdateResult = refreshWallpaper();

            if (wallpaperUpdateResult == WallpaperUpdateResult.NoMatchingQuotesForSelectedMood)
            {
                var selectedMoodLabel = selectedMood ?? "the selected mood";
                _trayIcon.ShowBalloonTip(
                    1000,
                    "No Quotes For Mood",
                    $"No quotes are tagged with \"{selectedMoodLabel}\". Keeping the current wallpaper.",
                    ToolTipIcon.Warning);
            }
            else if (wallpaperUpdateResult != WallpaperUpdateResult.Success)
            {
                _trayIcon.ShowBalloonTip(1000, "Refresh Failed", "Unable to refresh wallpaper from settings.", ToolTipIcon.Warning);
            }

            return wallpaperUpdateResult;
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    private void RefreshWallpaperOnSchedule(object? sender, EventArgs e)
    {
        _refreshTimer.Stop();
        _ = RefreshWallpaperCore();
        ScheduleNextRefresh();
    }

    private void ScheduleNextRefresh()
    {
        var delay = WallpaperRefreshSchedulerService.GetDelayUntilNextLocalTopOfHourRefresh(DateTimeOffset.Now);
        var interval = (int)Math.Clamp(Math.Ceiling(delay.TotalMilliseconds), 1, int.MaxValue);

        _refreshTimer.Stop();
        _refreshTimer.Interval = interval;
        _refreshTimer.Start();
    }

    private void RefreshWallpaperFromHotkey()
    {
        _hotkeyHudOverlayService.ShowWallpaperRefreshed();
        _ = RefreshWallpaperCore(showProgressBalloonTip: false);
    }

    private void EditSettingsFromHotkey()
    {
        _hotkeyHudOverlayService.ShowOpeningSettings();
        _ = EditSettingsCore(showProgressBalloonTip: false);
    }

    private void ToggleRunAtSignIn(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem runAtSignInMenuItem)
            return;

        try
        {
            if (runAtSignInMenuItem.Checked)
            {
                _startupLaunchService.Disable();
                runAtSignInMenuItem.Checked = false;
            }
            else
            {
                _startupLaunchService.Enable();
                runAtSignInMenuItem.Checked = true;
            }

            UpdateTopLevelMenuCheckMargin();
        }
        catch (IOException)
        {
            _trayIcon.ShowBalloonTip(1000, "Startup Preference Failed", "Unable to update the Run at Logon setting.", ToolTipIcon.Warning);
        }
        catch (UnauthorizedAccessException)
        {
            _trayIcon.ShowBalloonTip(1000, "Startup Preference Failed", "Unable to update the Run at Logon setting.", ToolTipIcon.Warning);
        }
        catch (SecurityException)
        {
            _trayIcon.ShowBalloonTip(1000, "Startup Preference Failed", "Unable to update the Run at Logon setting.", ToolTipIcon.Warning);
        }
    }

    private void RefreshWallpaperOnStartup(object? sender, EventArgs e)
    {
        Application.Idle -= RefreshWallpaperOnStartup;
        _ = RefreshWallpaperCore();
    }

    private void DarkenWallpaperBackgroundColor(object? sender, EventArgs e)
    {
        _hotkeyHudOverlayService.ShowBackgroundDarkened();
        _ = DarkenWallpaperBackgroundColorCore();
    }

    private WallpaperUpdateResult DarkenWallpaperBackgroundColorCore(bool showProgressBalloonTip = true)
    {
        var currentBackgroundColor = _wallpaperBackgroundColorService.GetCurrentBackgroundColor();
        return RefreshWallpaperCore(_wallpaperBackgroundColorService.Darken(currentBackgroundColor), showProgressBalloonTip);
    }

    private void LightenWallpaperBackgroundColor(object? sender, EventArgs e)
    {
        _hotkeyHudOverlayService.ShowBackgroundLightened();
        _ = LightenWallpaperBackgroundColorCore();
    }

    private WallpaperUpdateResult LightenWallpaperBackgroundColorCore(bool showProgressBalloonTip = true)
    {
        var currentBackgroundColor = _wallpaperBackgroundColorService.GetCurrentBackgroundColor();
        return RefreshWallpaperCore(_wallpaperBackgroundColorService.Lighten(currentBackgroundColor), showProgressBalloonTip);
    }

    private void RandomizeWallpaperBackgroundColor(object? sender, EventArgs e)
    {
        _hotkeyHudOverlayService.ShowRandomBackground();
        _ = RandomizeWallpaperBackgroundColorCore();
    }

    private WallpaperUpdateResult RandomizeWallpaperBackgroundColorCore(bool showProgressBalloonTip = true)
    {
        return RefreshWallpaperCore(_wallpaperBackgroundColorService.GetRandomDarkColor(), showProgressBalloonTip);
    }

    private void DarkenWallpaperBackgroundColorFromHotkey()
    {
        _hotkeyHudOverlayService.ShowBackgroundDarkened();
        _ = DarkenWallpaperBackgroundColorCore(showProgressBalloonTip: false);
    }

    private void LightenWallpaperBackgroundColorFromHotkey()
    {
        _hotkeyHudOverlayService.ShowBackgroundLightened();
        _ = LightenWallpaperBackgroundColorCore(showProgressBalloonTip: false);
    }

    private void RandomizeWallpaperBackgroundColorFromHotkey()
    {
        _hotkeyHudOverlayService.ShowRandomBackground();
        _ = RandomizeWallpaperBackgroundColorCore(showProgressBalloonTip: false);
    }

    private void RandomizeWallpaperFont(object? sender, EventArgs e)
    {
        _hotkeyHudOverlayService.ShowFontChanged(_wallpaperUpdateService.GetCurrentFontFamilyName());
        _ = RandomizeWallpaperFontCore();
    }

    private WallpaperUpdateResult RandomizeWallpaperFontCore(bool showProgressBalloonTip = true)
    {
        var selectedMood = _selectedMoodService.GetSelectedMood();
        return RefreshWallpaperCore(() => _wallpaperUpdateService.UpdateWallpaperWithRandomFont(_quotes, selectedMood), selectedMood, showProgressBalloonTip);
    }

    private void RandomizeWallpaperFontFromHotkey()
    {
        _hotkeyHudOverlayService.ShowFontChanged(_wallpaperUpdateService.GetCurrentFontFamilyName());
        _ = RandomizeWallpaperFontCore(showProgressBalloonTip: false);
    }

    private void EditSettings(object? sender, EventArgs e)
    {
        _hotkeyHudOverlayService.ShowOpeningSettings();
        _ = EditSettingsCore();
    }

    private bool EditSettingsCore(bool showProgressBalloonTip = true)
    {
        if (showProgressBalloonTip)
            _trayIcon.ShowBalloonTip(1000, "Opening Settings", "The settings window is opening.", ToolTipIcon.Info);

        var settingsPath = Path.Combine(AppContext.BaseDirectory, "settings.json");
        if (!File.Exists(settingsPath))
        {
            _trayIcon.ShowBalloonTip(1000, "Settings Missing", "Could not find settings.json.", ToolTipIcon.Error);
            return false;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = settingsPath,
                UseShellExecute = true,
                Verb = "open"
            });
            return true;
        }
        catch (InvalidOperationException)
        {
            _trayIcon.ShowBalloonTip(1000, "Open Failed", "Could not open settings.json in the default editor.", ToolTipIcon.Error);
        }
        catch (Win32Exception)
        {
            _trayIcon.ShowBalloonTip(1000, "Open Failed", "Could not open settings.json in the default editor.", ToolTipIcon.Error);
        }

        return false;
    }

    private void Exit(object? sender, EventArgs e)
    {
        Application.Idle -= RefreshWallpaperOnStartup;
        _globalHotkeyService.Dispose();
        _hotkeyHudOverlayService.Dispose();
        _refreshTimer.Stop();
        _refreshTimer.Dispose();
        _trayIcon.Visible = false;
        _trayIcon.ContextMenuStrip?.Dispose();
        _trayIcon.Dispose();
        Application.Exit();
    }

    private ContextMenuStrip CreateContextMenuStrip()
    {
        _setMoodMenuItems.Clear();

        return new TrayContextMenuFactory(
            _tagCatalog,
            _selectedMoodService.GetSelectedMood(),
            _startupLaunchService.IsEnabled(),
            _setMoodMenuItems,
            new TrayContextMenuActions
            {
                RefreshWallpaper = RefreshWallpaper,
                SetMood = SetMood,
                DarkenWallpaperBackgroundColor = DarkenWallpaperBackgroundColor,
                LightenWallpaperBackgroundColor = LightenWallpaperBackgroundColor,
                RandomizeWallpaperBackgroundColor = RandomizeWallpaperBackgroundColor,
                RandomizeWallpaperFont = RandomizeWallpaperFont,
                ToggleRunAtSignIn = ToggleRunAtSignIn,
                EditSettings = EditSettings,
                Exit = Exit
            }).Build();
    }

    private void UpdateTopLevelMenuCheckMargin()
    {
        if (_trayIcon.ContextMenuStrip is not { } contextMenuStrip)
            return;

        contextMenuStrip.ShowCheckMargin = contextMenuStrip.Items
            .OfType<ToolStripMenuItem>()
            .Any(item => item.Checked);
    }

    private void BindConfiguration(IConfiguration configuration)
    {
        try
        {
            configuration.GetSection("quotes").Bind(_quotes);
            configuration.GetSection("tagCatalog").Bind(_tagCatalog);
        }
        catch (Exception e)
        {
            throw new Exception(
                "Failed to read quotes and tagCatalog from configuration. Ensure that the settings.json file is properly formatted.",
                e);
        }

        var configuredMoods = _tagCatalog
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Select(tag => tag.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        _tagCatalog.Clear();
        _tagCatalog.AddRange(configuredMoods);
    }

    private static bool IsSameMood(string? left, string? right)
    {
        return string.IsNullOrWhiteSpace(left)
            ? string.IsNullOrWhiteSpace(right)
            : string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
    }

    private void NormalizeSelectedMood()
    {
        var selectedMood = _selectedMoodService.GetSelectedMood();
        if (selectedMood is null || _tagCatalog.Any(mood => IsSameMood(mood, selectedMood)))
            return;

        _selectedMoodService.SetSelectedMood(null);
    }

    private void SetMood(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem setMoodMenuItem)
            return;

        var selectedMood = setMoodMenuItem.Tag as string;
        try
        {
            _selectedMoodService.SetSelectedMood(selectedMood);
        }
        catch (IOException)
        {
            _trayIcon.ShowBalloonTip(1000, "Set Mood Failed", "Unable to save the selected mood.", ToolTipIcon.Warning);
            return;
        }
        catch (UnauthorizedAccessException)
        {
            _trayIcon.ShowBalloonTip(1000, "Set Mood Failed", "Unable to save the selected mood.", ToolTipIcon.Warning);
            return;
        }

        UpdateSetMoodChecks(selectedMood);
        _ = RefreshWallpaperCore();
    }

    private void UpdateSetMoodChecks(string? selectedMood)
    {
        foreach (var setMoodMenuItem in _setMoodMenuItems)
            setMoodMenuItem.Checked = IsSameMood(setMoodMenuItem.Tag as string, selectedMood);
    }

    private void ShowSelectedMoodStartupWarningIfNeeded()
    {
        var startupWarningMessage = _selectedMoodService.GetStartupWarningMessage();
        if (string.IsNullOrWhiteSpace(startupWarningMessage))
            return;

        _trayIcon.ShowBalloonTip(1000, "Mood State Unavailable", startupWarningMessage, ToolTipIcon.Warning);
    }

    private void RegisterHotkeys()
    {
        TryRegisterHotkey(AppConstants.RefreshWallpaperHotkeyId, AppConstants.RefreshWallpaperHotkeyModifiers, AppConstants.RefreshWallpaperHotkeyVirtualKey,
            RefreshWallpaperFromHotkey, AppConstants.RefreshWallpaperHotkeyDisplay);
        TryRegisterHotkey(AppConstants.EditSettingsHotkeyId, AppConstants.EditSettingsHotkeyModifiers, AppConstants.EditSettingsHotkeyVirtualKey,
            EditSettingsFromHotkey, AppConstants.EditSettingsHotkeyDisplay);
        TryRegisterHotkey(AppConstants.DarkenWallpaperBackgroundColorHotkeyId, AppConstants.DarkenWallpaperBackgroundColorHotkeyModifiers,
            AppConstants.DarkenWallpaperBackgroundColorHotkeyVirtualKey, DarkenWallpaperBackgroundColorFromHotkey,
            AppConstants.DarkenWallpaperBackgroundColorHotkeyDisplay);
        TryRegisterHotkey(AppConstants.LightenWallpaperBackgroundColorHotkeyId, AppConstants.LightenWallpaperBackgroundColorHotkeyModifiers,
            AppConstants.LightenWallpaperBackgroundColorHotkeyVirtualKey, LightenWallpaperBackgroundColorFromHotkey,
            AppConstants.LightenWallpaperBackgroundColorHotkeyDisplay);
        TryRegisterHotkey(AppConstants.RandomWallpaperBackgroundColorHotkeyId, AppConstants.RandomWallpaperBackgroundColorHotkeyModifiers,
            AppConstants.RandomWallpaperBackgroundColorHotkeyVirtualKey, RandomizeWallpaperBackgroundColorFromHotkey,
            AppConstants.RandomWallpaperBackgroundColorHotkeyDisplay);
        TryRegisterHotkey(AppConstants.RandomWallpaperFontHotkeyId, AppConstants.RandomWallpaperFontHotkeyModifiers,
            AppConstants.RandomWallpaperFontHotkeyVirtualKey, RandomizeWallpaperFontFromHotkey,
            AppConstants.RandomWallpaperFontHotkeyDisplay);
    }

    private void TryRegisterHotkey(int id, uint modifiers, uint virtualKey, Action hotkeyPressedHandler, string hotkeyDisplay)
    {
        if (!_globalHotkeyService.TryRegisterHotkey(id, modifiers, virtualKey, hotkeyPressedHandler))
            _trayIcon.ShowBalloonTip(1000, "Hotkey Unavailable", $"Unable to register {hotkeyDisplay}. The app will keep running without the hotkey.",
                ToolTipIcon.Warning);
    }

    private static Icon LoadTrayIcon()
    {
        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico");

        try
        {
            if (File.Exists(iconPath))
                return new Icon(iconPath);
        }
        catch (ArgumentException)
        {
        }
        catch (IOException)
        {
        }

        try
        {
            using var extractedIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            if (extractedIcon is not null)
                return (Icon)extractedIcon.Clone();
        }
        catch (ArgumentException)
        {
        }
        catch (FileNotFoundException)
        {
        }
        catch (Win32Exception)
        {
        }

        return (Icon)SystemIcons.Application.Clone();
    }
}
