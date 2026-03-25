namespace DeskQuotes;

internal static class Program
{
    /// <summary>
    ///     The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        using var singleInstanceGate = SingleInstanceGate.TryAcquire(AppConstants.SingleInstanceMutexName);

        if (singleInstanceGate is null)
        {
            return;
        }

        using var host = Host
            .CreateApplicationBuilder()
            .ConfigureApp()
            .ConfigureServices()
            .Build();

        var trayAppContext = host.Services.GetRequiredService<TrayAppContext>();

        Application.Run(trayAppContext);
    }
}
