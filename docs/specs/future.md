# DeskQuotes Feature-Gap Roadmap for Mass-Market Windows Adoption

## Summary

DeskQuotes is currently a narrow Windows tray utility: it loads quotes from a local `settings.json`, rotates a rendered wallpaper hourly, exposes a few fixed hotkeys, and has unit coverage around that core pipeline. The current shape is visible in [README.md](/C:/source/github-mithunshanbhag/desk-quotes/README.md), [TrayAppContext.cs](/C:/source/github-mithunshanbhag/desk-quotes/src/DeskQuotes/Components/TrayAppContext.cs), and [docs/specs/README.md](/C:/source/github-mithunshanbhag/desk-quotes/docs/specs/README.md).  
If the goal is a famous, broadly downloaded Windows app, the biggest gaps are not just “more quote features”; they are onboarding, settings UX, distribution, personalization depth, reliability, and retention loops.

## Key Product Gaps

- **P0: First-run and usability**
  - Add a real settings window; editing raw JSON is a hard blocker for mainstream users.
  - Add first-run onboarding with wallpaper preview, permission guidance, sample packs, and “start with Windows” setup.
  - Add basic empty/error states for malformed settings, missing quotes, hotkey conflicts, render/apply failures, and unsupported environments.
  - Add installer + auto-update + uninstall flow; current repo publishes binaries but does not expose consumer-grade packaging.
  - Add system startup/login behavior as a user setting, not a manual workaround.

- **P0: Core settings and personalization**
  - Replace or wrap `settings.json` with persisted user settings for refresh cadence, theme, fonts, background behavior, quote density, and monitor behavior.
  - Add refresh schedule controls: interval, active hours, work/personal modes, pause/resume, battery-aware behavior.
  - Add quote management UI: create/edit/delete, bulk import, dedupe, search, filter, sort, and validation.
  - Make tags actually useful: filter by mood/tag, rotation rules, include/exclude sets, time-of-day selection.
  - Add per-monitor options: same wallpaper everywhere, different quote per monitor, span mode, monitor-specific layout.

- **P1: Content and retention**
  - Add bundled quote packs by theme/category so the app feels valuable immediately after install.
  - Add online content sync: optional quote feeds, curated packs, seasonal packs, creator packs, backup/restore.
  - Add favorites, hide/dislike, “show less like this,” viewed history, and “never repeat too soon.”
  - Add templates/styles beyond plain centered text: minimal, bold, serif, photo overlay, gradient, calendar/date, focus mode.
  - Add widget-like overlays or lock-screen/export/shareable image generation to increase repeat use.

- **P1: Mainstream polish**
  - Add wallpaper preview before apply and a “revert last wallpaper” action.
  - Add richer font controls: size, weight, alignment, quote/author layout, safe contrast checks.
  - Add accessibility features: high-contrast themes, dyslexia-friendly fonts, screen-reader-friendly settings UI, keyboard-only navigation.
  - Add localization/internationalization; current app is effectively English-only.
  - Add notification strategy that is useful but non-annoying: onboarding tips, pack updates, failures, disabled hotkeys.

- **P1: Trust, reliability, supportability**
  - Add crash reporting and structured logs with an in-app “export diagnostics” flow.
  - Add telemetry/analytics for activation, retention, hotkey usage, settings adoption, failure rates, and wallpaper apply success.
  - Add explicit recovery behavior for sleep/resume, lock/unlock, monitor changes, DPI changes, and settings edits while running.
  - Add secure update and dependency hygiene; current test run reports a high-severity AutoMapper vulnerability in the dependency tree.
  - Add privacy controls and a user-facing privacy policy if any telemetry/cloud features are introduced.

- **P2: Growth and discoverability**
  - Add Windows Store/MSIX readiness, screenshots, onboarding copy, and polished branding assets.
  - Add import from CSV/Markdown/Notion/Obsidian/plain text to tap existing user quote collections.
  - Add sharing hooks: export current wallpaper, copy quote card, social-share-friendly images.
  - Add community or marketplace mechanics only after core product quality exists: downloadable packs, curated authors, creator submissions.
  - Add differentiation hooks: habit/focus integrations, workday modes, Pomodoro tie-ins, desktop aesthetic themes.

## Technical and Delivery Gaps

- **Architecture**
  - Introduce an internal app settings model and persistence layer; today most product configuration is implicit in raw JSON.
  - Separate wallpaper engine, settings/domain logic, content source layer, and UI layer so a future settings app or sync service is not coupled to the tray context.
  - Add background-worker resilience instead of depending entirely on the tray process lifecycle.

- **Quality**
  - Keep the existing unit tests, but add integration tests for settings load/save, tray actions, render/apply flow, startup behavior, and monitor-state changes.
  - Add true E2E or UI automation; the repo currently has an `e2e-tests` target but no actual E2E project.
  - Add installer/update smoke tests and compatibility checks across supported Windows versions and multi-monitor setups.
  - Add performance and memory regression checks for repeated wallpaper renders over long-running sessions.

- **Docs and ops**
  - Add a real user manual, FAQ, troubleshooting guide, release notes, and supported-version policy; `docs/user-manual` is empty.
  - Add product analytics dashboards, crash triage, and support workflows before chasing scale.
  - Add release channels such as stable/beta and rollback capability.

## Public Interfaces To Add or Change

- Replace the current bare `settings.json` contract with either:
  - an internal persisted settings store plus import/export of quotes, or
  - a richer schema including refresh schedule, startup behavior, monitor mode, style/template config, source providers, tag filters, favorites/history, and sync preferences.
- Add import/export interfaces for quote libraries and backups.
- Add a plugin/provider boundary for local packs vs online quote sources.
- Add analytics/crash-reporting interfaces behind opt-in privacy settings.

## Test Plan

- First-run onboarding works on a clean machine and produces a valid initial wallpaper without manual file edits.
- Settings UI changes persist across restart and survive malformed import attempts.
- Refresh scheduling behaves correctly across sleep/resume, midnight, timezone changes, and monitor hot-plug events.
- Multi-monitor modes render/apply correctly for single, mirrored, extended, and mixed-resolution setups.
- Quote filtering, favorites, hidden items, and repeat-avoidance rules behave deterministically.
- Installer, auto-start, update, rollback, and uninstall flows leave the system in a clean state.
- Telemetry/privacy toggles actually gate data collection.
- Failure paths surface actionable messages instead of silent tray-only degradation.

## Assumptions and Defaults

- Optimized for **mass-market Windows** success, not a cross-platform rewrite.
- Recommendations stay grounded in the current wallpaper-quote utility rather than pivoting into a different product category.
- “Comprehensive” here includes product, UX, packaging, reliability, analytics, and support gaps, because those are mandatory for high-download consumer software.
- Highest-value near-term sequence is:
  1. installer + startup + onboarding + settings UI,
  2. quote library management + scheduling + personalization,
  3. telemetry/crash reporting + update pipeline + E2E coverage,
  4. online packs/sync + marketplace/share features.
