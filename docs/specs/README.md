# DeskQuotes: A quote a day keeps the boredom away

This is the documentation for the DeskQuotes app, which publishes motivational quotes as rotating wallpapers on the user's desktop.

In its first iteration, the app will be a simple Windows system tray application that fetches a random quote (from a list of quotes) and sets it as the desktop wallpaper.

## App Settings

In its first iteration, the app will be completely configured through a `settings.json` file located in the same directory as the executable. The file will contain the following settings:

```json
{
  "applicationInsights": {
    "connectionString": "InstrumentationKey=...;IngestionEndpoint=https://..."
  },
  "logging": {
    "logLevel": {
      "default": "Information",
      "DeskQuotes": "Information",
      "Microsoft": "Warning"
    }
  },
  "tagCatalog": [
    "focus",
    "action",
    "discipline",
    "resilience",
    "courage",
    "growth",
    "relationships",
    "leadership",
    "wellbeing",
    "purpose"
  ],
  "quotes": [
    {
      "text": "The best way to predict the future is to invent it.",
      "author": "Alan Kay",
      "tags": ["action", "purpose"]
    },
    {
      "text": "Life is 10% what happens to us and 90% how we react to it.",
      "author": "Charles R. Swindoll",
      "tags": ["resilience", "wellbeing"]
    },
    {
      "text": "The only way to do great work is to love what you do.",
      "author": "Unknown",
      "tags": ["purpose", "discipline"]
    }
  ]
}
```

Notes on diagnostics configuration:

- `applicationInsights.connectionString` is optional. When present, DeskQuotes forwards application logs and traces to the configured Azure Application Insights instance.
- `logging.logLevel` controls the minimum verbosity for application and framework categories.

Notes on tags:

- `tags` drive the `Set Mood` tray submenu. When a mood is selected, only quotes whose `tags` contain that value remain eligible for refreshes, background-color changes, and random-font updates.
- Tags should describe mood/scenario applicability rather than broad subject matter.
- Use only values from `tagCatalog`.
- Each quote should typically have 1-3 tags.

## App Behavior

In its current iteration:

- **Wallpaper refresh frequency**: The app will refresh the wallpaper every hour, on the hour (in local time).

- **Multi-monitor support**: The app will determine the number of attached monitors, but will set the same wallpaper for all monitors.

- **Automatic wallpaper sizing**: The app will automatically infer the monitors' screen resolutions and adjust the wallpaper sizes accordingly.

- **Dark wallpaper color rotation**: Each wallpaper refresh will use a different dark background color so the desktop stays low-glare while still feeling fresh across refreshes.

- **Quote font rotation**: Standard wallpaper refreshes (startup, hourly refresh, tray refresh, and `Ctrl + Alt + U`) will randomly select one of five curated fonts for the quote: `Segoe UI`, `Georgia`, `Palatino Linotype`, `Trebuchet MS`, or `Constantia`.

- **Refresh on demand**: The app will have an option to refresh the wallpaper on demand through the tray context menu (`Refresh Wallpaper (Ctrl + Alt + U)`) and a fixed global hotkey (`Ctrl + Alt + U`).

- **Wallpaper background controls**: The tray context menu will include a `Wallpaper Background Color` submenu with `Darken Color (Ctrl + Alt + -)`, `Lighten Color (Ctrl + Alt + =)`, and `Random Color (Ctrl + Alt + 0)`. Each action should immediately refresh the wallpaper using the selected background adjustment, while preserving the current quote and font. Later normal refreshes continue auto-picking a dark color and rotating the font.

- **Wallpaper font controls**: The tray context menu will include a `Change Wallpaper Font` submenu with `Random Font (Ctrl + Alt + F)`. This action should immediately refresh the wallpaper using the current quote and current background color while switching to a different curated font from the one currently displayed.

- **Edit settings on demand**: The app will have an option to open the settings file for editing through the tray context menu (`Edit Settings (Ctrl + Alt + E)`) and a fixed global hotkey (`Ctrl + Alt + E`).

- **Hotkey HUD overlays**: Whenever a supported global hotkey is pressed and handled successfully, the app should briefly show a compact on-screen HUD/overlay so the user gets immediate visual feedback without needing to look at the tray icon. The overlay should summarize the action that just happened, such as wallpaper refreshed, background darkened/lightened/randomized, font changed, or settings opening.

- **Tray context menu presentation**: The menu should keep a native Windows look while using slightly larger `Segoe UI` typography, roomier item spacing, and horizontal separators above and below `Edit Settings` so the settings action is visually grouped apart from wallpaper actions and `Exit`.

## User Interactions

See [ui.md](./ui.md).

## Local run and MSI installer

For the current Windows desktop implementation:

- **Quick local run**: Start the tray app directly with:

```powershell
dotnet run --project .\src\DeskQuotes\DeskQuotes.csproj
```

- **Convenience script**: The repository also exposes the equivalent shortcut:

```powershell
.\run-local.ps1 -target app
```

- **Build the MSI installer**:

```powershell
dotnet build .\src\DeskQuotes.MSI\DeskQuotes.MSI.wixproj -c Release --nologo
```

- **Installer output**: The built package is written to:

```text
src\DeskQuotes.MSI\bin\Release\DeskQuotes.msi
```

- **Install the MSI**: Install it by double-clicking the generated MSI in File Explorer or by running:

```powershell
msiexec /i .\src\DeskQuotes.MSI\bin\Release\DeskQuotes.msi
```

- **Install behavior**: The MSI performs a per-user install under `%LocalAppData%\DeskQuotes`, does not require administrator rights, and should preserve user-edited `settings.json` content across upgrades.

## Open Questions

- What should be the behavior of the app when the machine is in different states, such as:
  - machine is sleeping
  - machine is locked
  - machine has hibernated
  - user has logged off

## Future Features

- **User interface**: A simple UI to allow users to add/remove quotes, change settings, and manually trigger a wallpaper update.

- **Integration with online quote APIs**: Fetch new quotes regularly from online sources.

- **Customizable wallpapers**: Allow users to choose from different wallpaper templates, styles, fonts, and font lists.

- **Background helper service**: A helper background service will run in the background to manage wallpaper updates and other tasks. This will ensure that the wallpaper updates happen even if the main system tray application is not running.

- **Exclusion hours**: The app will have an option to exclude certain hours of the day from wallpaper updates (e.g., during work hours).

- **Run on startup/login**: The app will have an option to run automatically on system startup or user login. This can be implemented through an installer or by adding a shortcut to the startup folder.

- **Custom hotkeys**: Allow users to configure custom hotkeys for triggering wallpaper updates or opening the settings.
