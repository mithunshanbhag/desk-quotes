using Microsoft.ApplicationInsights.WorkerService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;

namespace DeskQuotes.UnitTests;

public class TelemetryConfigurationTests
{
    #region Positive cases

    [Fact]
    public void Build_WhenApplicationInsightsWorkerServiceAndLoggerAreConfigured_DoesNotThrow()
    {
        const string connectionString =
            "InstrumentationKey=abc123;IngestionEndpoint=https://westeurope-0.in.applicationinsights.azure.com/";
        var builder = Host.CreateApplicationBuilder();

        builder.Services.AddApplicationInsightsTelemetryWorkerService(
            options =>
            {
                options.ConnectionString = connectionString;
            });
        builder.Logging.AddApplicationInsights(
            configureTelemetryConfiguration: telemetryConfiguration =>
            {
                telemetryConfiguration.ConnectionString = connectionString;
            },
            configureApplicationInsightsLoggerOptions: options =>
            {
                options.IncludeScopes = true;
                options.TrackExceptionsAsExceptionTelemetry = true;
            });

        using var host = builder.Build();
        var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(nameof(TelemetryConfigurationTests));

        logger.LogInformation("Application Insights telemetry configuration smoke test.");
    }

    #endregion
}
