# Learnings

- Runtime services are defined under `src\DeskQuotes\Services\Implementations`; tests should import that namespace rather than the removed legacy `DeskQuotes.Services` duplicates to avoid ambiguous type references.
- Use the root `run-local.ps1` script for local workflows: `app`, `tests`, `unit-tests`, and `e2e-tests`. If no E2E project exists yet, the `e2e-tests` target prints a skip message and exits successfully.
- The wallpaper refresh hotkey and edit settings hotkey are both implemented via `GlobalHotkeyService`, which combines Win32 `RegisterHotKey` with a WinForms `IMessageFilter` because `TrayAppContext` is an `ApplicationContext` rather than a form with its own window handle. The service supports multiple hotkeys via a `Dictionary<int, Action>` keyed by hotkey ID; the message filter is registered once and shared across all hotkeys.
- `WallpaperRenderService` is registered as a singleton and now rotates through a curated dark background palette, so consecutive wallpaper refreshes use different dark colors without changing the output file path.
