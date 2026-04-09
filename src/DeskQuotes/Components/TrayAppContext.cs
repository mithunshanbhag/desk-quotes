using System.Security;
using Timer = System.Windows.Forms.Timer;

namespace DeskQuotes.Components;

public class TrayAppContext : ApplicationContext
{
    private readonly GlobalHotkeyService _globalHotkeyService;
    private readonly List<Quote> _quotes = [];
    private readonly SelectedMoodService _selectedMoodService;
    private readonly List<string> _tagCatalog = [];
    private readonly List<ToolStripMenuItem> _setMoodMenuItems = [];
    private readonly Timer _refreshTimer = new();
    private readonly ComponentResourceManager _resources = new(typeof(TrayAppContext));
    private readonly StartupLaunchService _startupLaunchService;
    private readonly NotifyIcon _trayIcon;
    private readonly WallpaperBackgroundColorService _wallpaperBackgroundColorService;
    private readonly WallpaperRefreshSchedulerService _wallpaperRefreshSchedulerService;
    private readonly WallpaperUpdateService _wallpaperUpdateService;
    private bool _isRefreshing;

    public TrayAppContext(
        IConfiguration configuration,
        GlobalHotkeyService globalHotkeyService,
        SelectedMoodService selectedMoodService,
        StartupLaunchService startupLaunchService,
        WallpaperBackgroundColorService wallpaperBackgroundColorService,
        WallpaperUpdateService wallpaperUpdateService,
        WallpaperRefreshSchedulerService wallpaperRefreshSchedulerService)
    {
        _globalHotkeyService = globalHotkeyService;
        _selectedMoodService = selectedMoodService;
        _startupLaunchService = startupLaunchService;
        _wallpaperBackgroundColorService = wallpaperBackgroundColorService;
        _wallpaperUpdateService = wallpaperUpdateService;
        _wallpaperRefreshSchedulerService = wallpaperRefreshSchedulerService;

        BindConfiguration(configuration);
        NormalizeSelectedMood();

        _trayIcon = new NotifyIcon
        {
            //Icon = (Icon?)_resources.GetObject("notifyIcon.Icon"),
            Icon = SystemIcons.Application,
            ContextMenuStrip = CreateContextMenuStrip(),
            Visible = true,
            Text = AppConstants.AppName
        };
        ShowSelectedMoodStartupWarningIfNeeded();

        _refreshTimer.Tick += RefreshWallpaperOnSchedule;

        RegisterHotkeys();

        Application.Idle += RefreshWallpaperOnStartup;
        ScheduleNextRefresh();
    }

    private void RefreshWallpaper(object? sender, EventArgs e)
    {
        RefreshWallpaperCore();
    }

    private void RefreshWallpaperCore(Color? backgroundColor = null)
    {
        var selectedMood = _selectedMoodService.GetSelectedMood();
        RefreshWallpaperCore(() => _wallpaperUpdateService.UpdateWallpaper(_quotes, selectedMood, backgroundColor), selectedMood);
    }

    private void RefreshWallpaperCore(Func<WallpaperUpdateResult> refreshWallpaper, string? selectedMood = null)
    {
        if (_isRefreshing)
            return;

        _isRefreshing = true;
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
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    private void RefreshWallpaperOnSchedule(object? sender, EventArgs e)
    {
        _refreshTimer.Stop();
        RefreshWallpaper(sender, e);
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
        RefreshWallpaperCore();
    }

    private void EditSettingsFromHotkey()
    {
        EditSettings(this, EventArgs.Empty);
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
        RefreshWallpaperCore();
    }

    private void DarkenWallpaperBackgroundColor(object? sender, EventArgs e)
    {
        var currentBackgroundColor = _wallpaperBackgroundColorService.GetCurrentBackgroundColor();
        RefreshWallpaperCore(_wallpaperBackgroundColorService.Darken(currentBackgroundColor));
    }

    private void LightenWallpaperBackgroundColor(object? sender, EventArgs e)
    {
        var currentBackgroundColor = _wallpaperBackgroundColorService.GetCurrentBackgroundColor();
        RefreshWallpaperCore(_wallpaperBackgroundColorService.Lighten(currentBackgroundColor));
    }

    private void RandomizeWallpaperBackgroundColor(object? sender, EventArgs e)
    {
        RefreshWallpaperCore(_wallpaperBackgroundColorService.GetRandomDarkColor());
    }

    private void DarkenWallpaperBackgroundColorFromHotkey()
    {
        DarkenWallpaperBackgroundColor(this, EventArgs.Empty);
    }

    private void LightenWallpaperBackgroundColorFromHotkey()
    {
        LightenWallpaperBackgroundColor(this, EventArgs.Empty);
    }

    private void RandomizeWallpaperBackgroundColorFromHotkey()
    {
        RandomizeWallpaperBackgroundColor(this, EventArgs.Empty);
    }

    private void RandomizeWallpaperFont(object? sender, EventArgs e)
    {
        var selectedMood = _selectedMoodService.GetSelectedMood();
        RefreshWallpaperCore(() => _wallpaperUpdateService.UpdateWallpaperWithRandomFont(_quotes, selectedMood), selectedMood);
    }

    private void RandomizeWallpaperFontFromHotkey()
    {
        RandomizeWallpaperFont(this, EventArgs.Empty);
    }

    private void EditSettings(object? sender, EventArgs e)
    {
        _trayIcon.ShowBalloonTip(1000, "Opening Settings", "The settings window is opening.", ToolTipIcon.Info);

        var settingsPath = Path.Combine(AppContext.BaseDirectory, "settings.json");
        if (!File.Exists(settingsPath))
        {
            _trayIcon.ShowBalloonTip(1000, "Settings Missing", "Could not find settings.json.", ToolTipIcon.Error);
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = settingsPath,
                UseShellExecute = true,
                Verb = "open"
            });
        }
        catch (InvalidOperationException)
        {
            _trayIcon.ShowBalloonTip(1000, "Open Failed", "Could not open settings.json in the default editor.", ToolTipIcon.Error);
        }
        catch (Win32Exception)
        {
            _trayIcon.ShowBalloonTip(1000, "Open Failed", "Could not open settings.json in the default editor.", ToolTipIcon.Error);
        }
    }

    private void Exit(object? sender, EventArgs e)
    {
        Application.Idle -= RefreshWallpaperOnStartup;
        _globalHotkeyService.Dispose();
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
        RefreshWallpaperCore();
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
}
