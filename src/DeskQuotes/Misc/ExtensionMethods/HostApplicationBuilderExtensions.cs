using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging.ApplicationInsights;
using WindowsWallpaperService = DeskQuotes.Services.Implementations.WindowsWallpaperService;

namespace DeskQuotes.Misc.ExtensionMethods;

public static class HostApplicationBuilderExtensions
{
    extension(HostApplicationBuilder builder)
    {
        public HostApplicationBuilder ConfigureApp()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            builder.Configuration
                .AddJsonFile(AppConstants.SettingsFileName, false, true);

            return builder;
        }

        public HostApplicationBuilder ConfigureServices()
        {
            ConfigureLogging(builder);
            ConfigureTelemetry(builder);

            // automapper
            builder.Services.AddAutoMapper(cfg => { cfg.AddProfile<MapperProfile>(); });

            // mediatr
            builder.Services.AddMediatR(cfg => { cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()); });

            // fluent validation
            builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), ServiceLifetime.Singleton);

            // named http clients

            // services
            builder.Services
                .AddSingleton<QuoteSelectionService>()
                .AddSingleton<MonitorResolutionService>()
                .AddSingleton<SelectedMoodService>()
                .AddSingleton<WallpaperBackgroundColorService>()
                .AddSingleton<WallpaperFontSelectionService>()
                .AddSingleton<WallpaperRenderService>()
                .AddSingleton<WindowsWallpaperService>()
                .AddSingleton<GlobalHotkeyService>()
                .AddSingleton<HotkeyHudOverlayService>()
                .AddSingleton<StartupLaunchService>()
                .AddSingleton<WallpaperUpdateService>()
                .AddSingleton<WallpaperRefreshSchedulerService>();

            // winform components
            builder.Services
                .AddSingleton<TrayAppContext>();

            return builder;
        }
    }

    private static void ConfigureLogging(HostApplicationBuilder builder)
    {
        builder.Services.Configure<ApplicationInsightsConfiguration>(builder.Configuration.GetSection(ConfigKeys.ApplicationInsightsSectionName));

        builder.Logging.ClearProviders();
        builder.Logging.AddConfiguration(builder.Configuration.GetSection(ConfigKeys.LoggingSectionName));

        if (builder.Environment.IsDevelopment())
        {
            builder.Logging.SetMinimumLevel(LogLevel.Debug);
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();
        }
        else
        {
            builder.Logging.SetMinimumLevel(LogLevel.Information);
        }
    }

    private static void ConfigureTelemetry(HostApplicationBuilder builder)
    {
        var applicationInsightsConfiguration = GetApplicationInsightsConfiguration(builder);
        if (applicationInsightsConfiguration.ConnectionString is null)
            return;

        builder.Services.AddApplicationInsightsTelemetryWorkerService(options =>
        {
            options.ConnectionString = applicationInsightsConfiguration.ConnectionString;
        });

        builder.Logging.AddApplicationInsights(
            configureTelemetryConfiguration: telemetryConfiguration =>
            {
                telemetryConfiguration.ConnectionString = applicationInsightsConfiguration.ConnectionString;
            },
            configureApplicationInsightsLoggerOptions: options =>
            {
                options.IncludeScopes = true;
                options.TrackExceptionsAsExceptionTelemetry = true;
            });
        builder.Logging.AddFilter<ApplicationInsightsLoggerProvider>(string.Empty, LogLevel.Information);
    }

    private static ApplicationInsightsConfiguration GetApplicationInsightsConfiguration(HostApplicationBuilder builder)
    {
        var applicationInsightsConfiguration = new ApplicationInsightsConfiguration();

        builder.Configuration.GetSection(ConfigKeys.ApplicationInsightsSectionName).Bind(applicationInsightsConfiguration);

        applicationInsightsConfiguration.ConnectionString = NormalizeConnectionString(applicationInsightsConfiguration.ConnectionString);
        return applicationInsightsConfiguration;
    }

    private static string? NormalizeConnectionString(string? connectionString)
    {
        return string.IsNullOrWhiteSpace(connectionString)
            ? null
            : connectionString.Trim();
    }
}
