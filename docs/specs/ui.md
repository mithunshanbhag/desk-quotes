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

## Hotkey HUD overlays

When the user triggers one of the supported global keyboard hotkeys, the app should briefly show a compact HUD/overlay on screen so the action feels immediate and visible even though the app primarily lives in the system tray.

- The overlay should appear only for keyboard hotkey interactions, not for ordinary hourly refreshes.
- The overlay should appear only when the hotkey action is successfully handled. Error and warning cases can continue using tray balloon tips.
- The overlay should feel like a native desktop affordance: small, lightweight, and non-intrusive rather than a dialog or notification toast.
- The overlay should not take keyboard focus, should not appear in the taskbar, and should disappear automatically after a short moment.
- The overlay should use a dark translucent surface with rounded corners, a simple icon or visual cue, and concise text.
- The overlay should be positioned near the lower center of the primary display, slightly above the taskbar area, unless later testing shows another location feels more natural on Windows.
- The overlay should animate in and out quickly, with a subtle fade rather than an abrupt pop when feasible.

Each hotkey overlay should summarize the action that just ran. Examples:

- `Ctrl + Alt + U`: show a refresh-style icon and text such as `Wallpaper refreshed`.
- `Ctrl + Alt + -`: show a color/brightness-style cue and text such as `Background darker`.
- `Ctrl + Alt + =`: show a color/brightness-style cue and text such as `Background lighter`.
- `Ctrl + Alt + 0`: show a color swatch-style cue and text such as `Random background`.
- `Ctrl + Alt + F`: show a font/text-style cue and text that includes the selected font name when available.
- `Ctrl + Alt + E`: show a settings/edit-style cue and text such as `Opening settings`.

The overlay content should stay short enough to be understood at a glance. It should reinforce what changed, not explain the feature in detail.

>Note: In its first iteration, the app will not have a graphical user interface (GUI) for settings management, and all configurations will be done by manually editing the `settings.json` file.
