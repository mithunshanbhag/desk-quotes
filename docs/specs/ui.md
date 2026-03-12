# Common UI elements and layout

The system tray application will allow the user to have the following interactions through a context menu (accessible either by right-clicking the tray icon or left-clicking it):

- "Refresh Wallpaper (Ctrl + Alt + U)": Manually trigger a wallpaper update. Each refresh will also rotate the wallpaper to a different dark background color. The same fixed global hotkey (`Ctrl + Alt + U`) will also be available for this action.
- "Edit Settings (Ctrl + Alt + E)": Open the `settings.json` file in the default text editor for easy editing. The same fixed global hotkey (`Ctrl + Alt + E`) will also be available for this action.
- "Exit": Close the application.

>Note: In its first iteration, the app will not have a graphical user interface (GUI) for settings management, and all configurations will be done by manually editing the `settings.json` file.
