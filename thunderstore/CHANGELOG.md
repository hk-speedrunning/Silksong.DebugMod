## Changelog

### v0.3.2

- Savestates:
	- Fixed incorrectly regenerating silk chunks after loading a savestate (& likely also where silk chunks get stuck in the centre of the screen)
	- Fixed loading savestates into a flame/lava hitbox causing flame effect to be applied every frame until taking damage.
	- Fixed some projectiles & effects incorrectly persisting across same-room loads using Load Quickslot on Death.
- Info panel has been reorganised to present more helpful information.
	- Last Scaling now shows all multipliers belonging to the last hit enemy, not just the one that was applied.
	- Added `ExtendedInfoPanel` setting to `<save files>/DebugModData/settings.json` to re-add fields removed in v0.3.0.
	- Optional new look with `AltInfoPanel` setting (see above) that may assist legibility.
- Timescale no longer resets on getting hit + other improvements (thanks Freya Holmér!)
- Fixed HP bars sometimes gaining infinite length or becoming detached from their enemies.

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