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

        var logger = host.Services.GetRequiredService<ILogger<ProgramEntryPoint>>();

        ThreadExceptionEventHandler threadExceptionHandler = (_, args) =>
            logger.LogError(args.Exception, "DeskQuotes encountered an unhandled UI thread exception.");
        UnhandledExceptionEventHandler unhandledExceptionHandler = (_, args) =>
        {
            if (args.ExceptionObject is Exception exception)
            {
                logger.LogCritical(exception, "DeskQuotes encountered an unhandled exception. IsTerminating: {IsTerminating}.", args.IsTerminating);
                return;
            }

            logger.LogCritical("DeskQuotes encountered an unhandled non-Exception termination. IsTerminating: {IsTerminating}.", args.IsTerminating);
        };

        Application.ThreadException += threadExceptionHandler;
        AppDomain.CurrentDomain.UnhandledException += unhandledExceptionHandler;

        try
        {
            logger.LogInformation("DeskQuotes acquired the single-instance gate and built the host.");

            var trayAppContext = host.Services.GetRequiredService<TrayAppContext>();

            logger.LogInformation("DeskQuotes entering the application message loop.");
            Application.Run(trayAppContext);
            logger.LogInformation("DeskQuotes exited the application message loop normally.");
        }
        catch (Exception exception)
        {
            logger.LogCritical(exception, "DeskQuotes terminated unexpectedly during startup or message processing.");
            throw;
        }
        finally
        {
            Application.ThreadException -= threadExceptionHandler;
            AppDomain.CurrentDomain.UnhandledException -= unhandledExceptionHandler;
            logger.LogInformation("DeskQuotes is shutting down.");
        }
    }

    private sealed class ProgramEntryPoint;
}
