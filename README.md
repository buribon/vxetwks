# VXE — Fortnite / Windows Gaming Optimizer

Anime-inspired dark/neon WinForms prototype with a searchable catalogue of 156 tweak guides across 26 categories.

## Download the Windows build (no local coding required)
1. Open the repository's **Actions** tab: https://github.com/buribon/vxetwks/actions
2. Select the latest successful **Build VXE for Windows** run.
3. In **Artifacts**, download **VXE-Windows-x64** and extract the ZIP.
4. Run `VXE.exe`. It is a self-contained x64 Windows build; .NET does not need to be installed separately.

The workflow runs on every push to `main), and can also be started from Actions > Build VXE for Windows > Run workflow.

## Current features
- Search and filter the 156-entry tweak catalogue.
- Per-tweak explanations and cautious, non-guaranteed FPS estimates.
- Opens Windows Storage, Startup and advanced display settings where appropriate.
- System snapshot and a repeatable manual benchmark protocol.
- Anime-inspired dark/neon console UI.

## Important limitations
This is a starter/prototype, not a guaranteed FPS booster. It does not yet read Fortnite's live FPS, GPU temperature, ping or frametimes; nor does it automatically apply 156 system changes. Advanced registry, service, BIOS, driver, overclock and undervolt items provide guidance instead of silently changing system state.

## Safety
- Do not disable Windows security or anti-cheat to chase FPS.
- Review files before deleting them; downloads and shader caches may be useful.
- Change one setting at a time and compare the same Fortnite scene.
- GitHub Actions builds an unsigned executable. Windows Smart App Control may still block an unsigned app; do not disable security to run it. A public build is not the same as a Microsoft-signed/trusted release.
