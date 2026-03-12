using Timer = System.Windows.Forms.Timer;

namespace DeskQuotes.Components;

public class TrayAppContext : ApplicationContext
{
    private readonly GlobalHotkeyService _globalHotkeyService;
    private readonly List<Quote> _quotes = [];
    private readonly Timer _refreshTimer = new();
    private readonly ComponentResourceManager _resources = new(typeof(TrayAppContext));
    private readonly NotifyIcon _trayIcon;
    private readonly WallpaperRefreshSchedulerService _wallpaperRefreshSchedulerService;
    private readonly WallpaperUpdateService _wallpaperUpdateService;
    private bool _isRefreshing;

    public TrayAppContext(
        IConfiguration configuration,
        GlobalHotkeyService globalHotkeyService,
        WallpaperUpdateService wallpaperUpdateService,
        WallpaperRefreshSchedulerService wallpaperRefreshSchedulerService)
    {
        _globalHotkeyService = globalHotkeyService;
        _wallpaperUpdateService = wallpaperUpdateService;
        _wallpaperRefreshSchedulerService = wallpaperRefreshSchedulerService;

        _trayIcon = new NotifyIcon
        {
            //Icon = (Icon?)_resources.GetObject("notifyIcon.Icon"),
            Icon = SystemIcons.Application,
            ContextMenuStrip = new ContextMenuStrip
            {
                Items =
                {
                    new ToolStripMenuItem(AppConstants.RefreshWallpaperMenuLabel, null, RefreshWallpaper),
                    new ToolStripMenuItem(AppConstants.EditSettingsMenuLabel, null, EditSettings),
                    new ToolStripMenuItem("E&xit", null, Exit)
                }
            },
            Visible = true,
            Text = AppConstants.AppName
        };

        // Read the settings.json file (via configuration), and extract the quotes
        try
        {
            configuration.GetSection("quotes").Bind(_quotes);
        }
        catch (Exception e)
        {
            throw new Exception("Failed to read quotes from configuration. Ensure that the settings.json file is properly formatted and contains a 'quotes' section.", e);
        }

        _refreshTimer.Tick += RefreshWallpaperOnSchedule;

        if (!_globalHotkeyService.TryRegisterHotkey(AppConstants.RefreshWallpaperHotkeyId, AppConstants.RefreshWallpaperHotkeyModifiers, AppConstants.RefreshWallpaperHotkeyVirtualKey, RefreshWallpaperFromHotkey))
            _trayIcon.ShowBalloonTip(1000, "Hotkey Unavailable", $"Unable to register {AppConstants.RefreshWallpaperHotkeyDisplay}. The app will keep running without the hotkey.",
                ToolTipIcon.Warning);

        if (!_globalHotkeyService.TryRegisterHotkey(AppConstants.EditSettingsHotkeyId, AppConstants.EditSettingsHotkeyModifiers, AppConstants.EditSettingsHotkeyVirtualKey, EditSettingsFromHotkey))
            _trayIcon.ShowBalloonTip(1000, "Hotkey Unavailable", $"Unable to register {AppConstants.EditSettingsHotkeyDisplay}. The app will keep running without the hotkey.",
                ToolTipIcon.Warning);

        Application.Idle += RefreshWallpaperOnStartup;
        ScheduleNextRefresh();
    }

    private void RefreshWallpaper(object? sender, EventArgs e)
    {
        if (_isRefreshing)
            return;

        _isRefreshing = true;
        _trayIcon.ShowBalloonTip(1000, "Refreshing Wallpaper", "Your wallpaper is being refreshed.", ToolTipIcon.Info);

        try
        {
            var wallpaperUpdated = _wallpaperUpdateService.TryUpdateWallpaper(_quotes);

            if (!wallpaperUpdated)
                _trayIcon.ShowBalloonTip(1000, "Refresh Failed", "Unable to refresh wallpaper from settings.", ToolTipIcon.Warning);
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
        RefreshWallpaper(this, EventArgs.Empty);
    }

    private void EditSettingsFromHotkey()
    {
        EditSettings(this, EventArgs.Empty);
    }

    private void RefreshWallpaperOnStartup(object? sender, EventArgs e)
    {
        Application.Idle -= RefreshWallpaperOnStartup;
        RefreshWallpaper(this, EventArgs.Empty);
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
}