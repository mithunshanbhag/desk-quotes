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
                .AddJsonFile("settings.json", false, true);

            return builder;
        }

        public HostApplicationBuilder ConfigureServices()
        {
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
                .AddSingleton<StartupLaunchService>()
                .AddSingleton<WallpaperUpdateService>()
                .AddSingleton<WallpaperRefreshSchedulerService>();

            // winform components
            builder.Services
                .AddSingleton<TrayAppContext>();

            return builder;
        }
    }
}
