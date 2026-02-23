namespace DeskQuotes.Components;

public class TrayAppContext : ApplicationContext
{
    private readonly List<Quote> _quotes = [];
    private readonly ComponentResourceManager _resources = new(typeof(TrayAppContext));
    private readonly NotifyIcon _trayIcon;

    public TrayAppContext(IConfiguration configuration)
    {
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

        RefreshWallpaper(this, EventArgs.Empty);
    }

    private void RefreshWallpaper(object? sender, EventArgs e)
    {
        _trayIcon.ShowBalloonTip(1000, "Refreshing Wallpaper", "Your wallpaper is being refreshed.", ToolTipIcon.Info);
        // @TODO    
    }

    private void EditSettings(object? sender, EventArgs e)
    {
        _trayIcon.ShowBalloonTip(1000, "Opening Settings", "The settings window is opening.", ToolTipIcon.Info);
        // @TODO    
    }

    private void Exit(object? sender, EventArgs e)
    {
        _trayIcon.Visible = false;
        Application.Exit();
    }
}