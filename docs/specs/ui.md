# Common UI elements and layout

The system tray application will allow the user to have the following interactions through a context menu (accessible either by right-clicking the tray icon or left-clicking it):

- "Refresh Wallpaper (Ctrl + Alt + U)": Manually trigger a wallpaper update. Each standard refresh will rotate to a different dark background color and randomly pick one of the curated quote fonts. The same fixed global hotkey (`Ctrl + Alt + U`) will also be available for this action.
- "Set Mood": Opens a submenu that lets the user filter eligible quotes by mood:
  - "All Quotes": Clears the current mood filter so every configured quote is eligible again.
  - "Any configured mood from `tagCatalog`": Applies that mood immediately, keeps only matching tagged quotes eligible for refreshes, and marks the selected submenu item with a checkmark.
- "Wallpaper Background Color": Opens a submenu that immediately refreshes the wallpaper using one of these background actions:
  - "Darken Color (Ctrl + Alt + -)": Darkens the current wallpaper background color and refreshes immediately without changing the current quote font.
  - "Lighten Color (Ctrl + Alt + =)": Lightens the current wallpaper background color and refreshes immediately without changing the current quote font.
  - "Random Color (Ctrl + Alt + 0)": Picks a new dark random wallpaper background color and refreshes immediately without changing the current quote font.
- "Change Wallpaper Font": Opens a submenu that immediately refreshes the wallpaper using the current quote and current background color:
  - "Random Font (Ctrl + Alt + F)": Switches to a different font from the curated list of five Windows-safe fonts already used by the app.
- "Edit Settings (Ctrl + Alt + E)": Open the `settings.json` file in the default text editor for easy editing. The same fixed global hotkey (`Ctrl + Alt + E`) will also be available for this action.
- "Exit": Close the application.

The tray context menu should keep a native Windows appearance, but it should feel more deliberate than the default WinForms menu. Use slightly larger `Segoe UI` typography, a bit more vertical breathing room per item, and horizontal separators above and below `Edit Settings`. Avoid custom menu text colors unless a future design explicitly calls for a themed menu.

>Note: In its first iteration, the app will not have a graphical user interface (GUI) for settings management, and all configurations will be done by manually editing the `settings.json` file.
