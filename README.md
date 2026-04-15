# DeskQuotes

[![desk-quotes-deploy](https://github.com/mithunshanbhag/desk-quotes/actions/workflows/deploy.yml/badge.svg)](https://github.com/mithunshanbhag/desk-quotes/actions/workflows/deploy.yml)
![License](https://img.shields.io/badge/license-MIT-blue)

![DeskQuotes Demo](./docs/assets/reference-screenshot.png)

DeskQuotes is a Windows tray app that rotates your desktop wallpaper using quotes from a local `settings.json` file and a curated set of readable quote fonts.

## Installation

1. Install the .NET 10 SDK on Windows.
2. Clone this repository.
3. Restore dependencies:

```powershell
dotnet restore .\DeskQuotes.slnx
```

## Usage

1. Run the app (see the next section).
2. Use the tray icon menu to:
    - **Refresh Wallpaper (Ctrl + Alt + U)**
    - **Set Mood**
      - **All Quotes**
      - **Any configured mood from `tagCatalog`**
    - **Wallpaper Background Color**
      - **Darken Color (Ctrl + Alt + -)**
      - **Lighten Color (Ctrl + Alt + =)**
      - **Random Color (Ctrl + Alt + 0)**
    - **Change Wallpaper Font**
      - **Random Font (Ctrl + Alt + F)**
    - **Edit Settings (Ctrl + Alt + E)** (opens `settings.json`)
    - **Exit**
3. Press `Ctrl + Alt + U` from anywhere, or use the tray menu, to trigger an immediate wallpaper refresh.
4. Standard wallpaper refreshes rotate the quote font among `Segoe UI`, `Georgia`, `Palatino Linotype`, `Trebuchet MS`, and `Constantia`.
5. Use the background color submenu or its hotkeys to immediately darken, lighten, or randomize the wallpaper background color while keeping the current quote font unchanged.
6. Use `Ctrl + Alt + F`, or the tray menu, to switch to a different curated font while keeping the current quote and background.
7. Successful wallpaper, background-color, font, and settings actions triggered from either the tray menu or the global hotkeys show a compact on-screen HUD overlay near the bottom-center of the primary display so the action is visible immediately without opening a full notification.
8. Use **Set Mood** to persist a mood selection across restarts. With **All Quotes** selected, every configured quote remains eligible. When a mood is selected, only quotes whose `tags` contain that mood are eligible for refreshes, background-color changes, and random-font updates. If no configured quote matches the selected mood, DeskQuotes keeps the current wallpaper and shows a warning instead of falling back to all quotes.
9. Edit quotes in `src\DeskQuotes\settings.json` and refresh from the tray menu.

## Build and run locally

```powershell
.\run-local.ps1 -target app
```

## Build the MSI installer

Build the installer on Windows with the .NET CLI:

```powershell
dotnet build .\src\DeskQuotes.MSI\DeskQuotes.MSI.wixproj -c Release
```

The generated MSI will be written under:

```text
src\DeskQuotes.MSI\bin\Release\
```

The installer performs a per-user install under `%LocalAppData%\DeskQuotes` so the bundled `settings.json` remains editable without requiring administrator rights.

## Run tests

```powershell
.\run-local.ps1 -target unit-tests
```

Additional local targets:

```powershell
.\run-local.ps1 -target tests
.\run-local.ps1 -target e2e-tests
```
