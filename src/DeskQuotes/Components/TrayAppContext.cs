using Timer = System.Windows.Forms.Timer;

namespace DeskQuotes.Components;

public class TrayAppContext : ApplicationContext
{
    private readonly List<Quote> _quotes = [];
    private readonly Timer _refreshTimer = new();
    private readonly ComponentResourceManager _resources = new(typeof(TrayAppContext));
    private readonly NotifyIcon _trayIcon;
    private readonly WallpaperRefreshSchedulerService _wallpaperRefreshSchedulerService;
    private readonly WallpaperUpdateService _wallpaperUpdateService;

    public TrayAppContext(
        IConfiguration configuration,
        WallpaperUpdateService wallpaperUpdateService,
        WallpaperRefreshSchedulerService wallpaperRefreshSchedulerService)
    {
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
                    new ToolStripMenuItem("&Refresh Wallpaper", null, RefreshWallpaper),
                    new ToolStripMenuItem("&Settings", null, EditSettings),
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
        RefreshWallpaper(this, EventArgs.Empty);
        ScheduleNextRefresh();
    }

    private void RefreshWallpaper(object? sender, EventArgs e)
    {
        _trayIcon.ShowBalloonTip(1000, "Refreshing Wallpaper", "Your wallpaper is being refreshed.", ToolTipIcon.Info);

        if (!_wallpaperUpdateService.TryUpdateWallpaper(_quotes))
            _trayIcon.ShowBalloonTip(1000, "Refresh Failed", "Unable to refresh wallpaper from settings.", ToolTipIcon.Warning);
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
        _refreshTimer.Stop();
        _refreshTimer.Dispose();
        _trayIcon.Visible = false;
        _trayIcon.ContextMenuStrip?.Dispose();
        _trayIcon.Dispose();
        Application.Exit();
    }
}