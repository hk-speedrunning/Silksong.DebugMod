## Changelog

### v0.3.1

- Fixed a crash on MacOS
- Fixed an error when another mod creates scenes at runtime
- Prevented double logging with BepInEx's Unity logger active

### v0.3.0

- UI overhauled:
  - Now supports Ultrawide resolutions.
  - Moved & resized all panels to be out of the way.
  - Merged the Keybinds and Top panels into one Main panel with rebind prompts next to each button.
    - Combined the Keybinds pages into a single scrollable list.
  - Expanded the Savestates UI into an interactive panel, including buttons to load, save & rename states in-game.
- Savestates can now be loaded on elevators, including after hitting Gurr's trap.
- Fixed various issues arising from loading a savestate immediately after loading a save file.
- Fixed a hardlock caused by loading a savestate at 1HP while `Load Quickslot on Death` is enabled.
- Made Savestates major-version specific & automatically move savestates from `Savestates Current Patch` to `Savestates 1.0`.

### v0.2.4

- Repackaged for Thunderstore releases, allowing installation by Gale/r2modman.
- Added alpha UI for renaming savestates in game.
- Enemy tracking refactored to improve reliability and usability in several ways.
- Added this changelog.
- Various smaller fixes and tweaks.