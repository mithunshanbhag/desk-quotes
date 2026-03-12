# DeskQuotes: A quote a day keeps the boredom away

This is the documentation for the DeskQuotes app, which publishes motivational quotes as rotating wallpapers on the user's desktop.

In its first iteration, the app will be a simple Windows system tray application that fetches a random quote (from a list of quotes) and sets it as the desktop wallpaper.

## App Settings

In its first iteration, the app will be completely configured through a `config.json` file located in the same directory as the executable. The file will contain the following settings:

```json
{
  "quotes": [
    {
      "text": "The best way to predict the future is to invent it.",
      "author": "Alan Kay"
    },
    {
      "text": "Life is 10% what happens to us and 90% how we react to it.",
      "author": "Charles R. Swindoll"
    },
    {
      "text": "The only way to do great work is to love what you do.",
      "author": "Unknown"
    }
  ]
}
```

## App Behavior

In its current iteration:

- **Wallpaper refresh frequency**: The app will refresh the wallpaper every hour, on the hour (in local time).

- **Multi-monitor support**: The app will determine the number of attached monitors, but will set the same wallpaper for all monitors.

- **Automatic wallpaper sizing**: The app will automatically infer the monitors' screen resolutions and adjust the wallpaper sizes accordingly.

- **Dark wallpaper color rotation**: Each wallpaper refresh will use a different dark background color so the desktop stays low-glare while still feeling fresh across refreshes.

- **Refresh on demand**: The app will have an option to refresh the wallpaper on demand through the tray context menu (`Refresh Wallpaper (Ctrl + Alt + U)`) and a fixed global hotkey (`Ctrl + Alt + U`).

- **Edit settings on demand**: The app will have an option to open the settings file for editing through the tray context menu (`Edit Settings (Ctrl + Alt + E)`) and a fixed global hotkey (`Ctrl + Alt + E`).

## User Interactions

See [ui.md](./ui.md).

## Open Questions

- What should be the behavior of the app when the machine is in different states, such as:
  - machine is sleeping
  - machine is locked
  - machine has hibernated
  - user has logged off

## Future Features

- **User interface**: A simple UI to allow users to add/remove quotes, change settings, and manually trigger a wallpaper update.

- **Integration with online quote APIs**: Fetch new quotes regularly from online sources.

- **Customizable wallpapers**: Allow users to choose from different wallpaper templates, styles, fonts.

- **Background helper service**: A helper background service will run in the background to manage wallpaper updates and other tasks. This will ensure that the wallpaper updates happen even if the main system tray application is not running.

- **Exclusion hours**: The app will have an option to exclude certain hours of the day from wallpaper updates (e.g., during work hours).

- **Run on startup/login**: The app will have an option to run automatically on system startup or user login. This can be implemented through an installer or by adding a shortcut to the startup folder.

- **Custom hotkeys**: Allow users to configure custom hotkeys for triggering wallpaper updates or opening the settings.
