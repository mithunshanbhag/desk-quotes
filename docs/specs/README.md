# DeskQuotes: A quote a day keep the boredom away

This is the documentation for the DeskQuotes app, which publishes motivational quotes as rotating wallpapers on the user's desktop.

In it's first iteration, the app will be a simple Windows system tray application that fetches a random quote (from a list of quotes) and sets it as the desktop wallpaper.

## Settings

In its first iteration, the app will be completely configured through a `config.json` file located in the same directory as the executable. The file will contain the following settings:

```jsonc
{
  "quoteList": [
    "The only way to do great work is to love what you do. - Steve Jobs",
    "Success is not the key to happiness. Happiness is the key to success. - Albert Schweitzer",
    "Don't watch the clock; do what it does. Keep going. - Sam Levenson"
  ],
  "updateCRON": "0 0 * * *", // Every day at midnight
  "imageSize": {
    "width": 1920,
    "height": 1080
  },
  "font": {
    "family": "Arial",
    "size": 24,
    "color": "#FFFFFF"
  }
}
```

## User Interaction

The system tray application will allow the user to have the following interactions through a context menu (accessible either by right-clicking the tray icon or left-clicking it):

- "Update Wallpaper Now": Manually trigger a wallpaper update.
- "Settings": Open the `config.json` file in the default text editor for easy editing.
- "Exit": Close the application.

>Note: In its first iteration, the app will not have a graphical user interface (GUI) for settings management, and all configurations will be done by manually editing the `config.json` file.

## Future Features

1. The app should automatically infer the user's screen resolution and adjust the wallpaper size accordingly.
2. Multi-monitor support: The app should be able to set different wallpapers for each monitor.
3. User interface: A simple UI to allow users to add/remove quotes, change settings, and manually trigger a wallpaper update.
4. Integration with online quote APIs to fetch new quotes regularly.
