namespace DeskQuotes;

internal static class Program
{
    /// <summary>
    ///     The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        var host = Host
            .CreateApplicationBuilder()
            .ConfigureApp()
            .ConfigureServices()
            .Build();

        var trayAppContext = host.Services.GetRequiredService<TrayAppContext>();

        Application.Run(trayAppContext);
    }
}