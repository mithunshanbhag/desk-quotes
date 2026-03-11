# DeskQuotes

[![desk-quotes-deploy](https://github.com/mithunshanbhag/desk-quotes/actions/workflows/deploy.yml/badge.svg)](https://github.com/mithunshanbhag/desk-quotes/actions/workflows/deploy.yml)
![License](https://img.shields.io/badge/license-MIT-blue)

![DeskQuotes Demo](./docs/assets/reference-screenshot.png)

DeskQuotes is a Windows tray app that rotates your desktop wallpaper using quotes from a local `settings.json` file.

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
   - **Settings** (opens `settings.json`)
   - **Exit**
3. Press `Ctrl + Alt + U` from anywhere, or use the tray menu, to trigger an immediate wallpaper refresh.
4. Edit quotes in `src\DeskQuotes\settings.json` and refresh from the tray menu.

## Build and run locally

```powershell
.\run-local.ps1 -target app
```

## Run tests

```powershell
.\run-local.ps1 -target unit-tests
```

Additional local targets:

```powershell
.\run-local.ps1 -target tests
.\run-local.ps1 -target e2e-tests
```
