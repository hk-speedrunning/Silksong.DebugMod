## Changelog

### v0.3.6

- Fixed a regression in v0.3.5 causing savestate loads to persist scene data.
- Added `Queue Walljump Interrupt` bind. 
- Recoloured NonSlider terrain hitboxes to teal. 

### v0.3.5

- Added `Q` flag to Wall States for tracking `queuedWalljumpInterrupt`, one of the flags responsible for Walljump Storage.
- Further speedups to savestate loading & more accurate timing.
- Improved handling of Windowed mode - UI should now rescale to an appropriate size shortly after the window is resized.
  - Note that excessive rebuilds may cause performance issues - if you experience this, try restarting the game after resizing the window to your desired size.

### v0.3.4

- Fixed an issue causing poor performance on main menu (thanks @dplochcoder)
- Massively improved performance with Hitboxes shown via culling (thanks @olvior)
- Minor fix for Red Memory savestates created before v0.3.3
- Fixed some instances of the hero collider incorrectly being disabled on savestate load

### v0.3.3

- Savestates now save semi-persistent states, meaning eg. you can now set a state after killing some bench-respawning enemies & they correctly do not respawn when loading.
- Various minor UI adjustments; HP bars are now wider & translucent, Infinite Silk no longer causes UI flashing & Last Scaling will now overlap the second info column less often.
- Fixed "Deactivate Visual Masks" function & adjusted several hitbox categories for use in Silksong.
- Added support for other mods to add fields to the Info panel (#85)
- Savestate fixes:
  - Tools no longer remain disabled when savestating out of memories.
  - Fixed savestates in Memory scenes - now you can practice silk heart, needolin & red memory parkour all you wish :)
  - Fixed Hunter's crest meter not updating after savestate load.
  - Savestates now correctly store maggot & hunter's crest evo states.
  - Loading state while falling out of the Mist no longer hardlocks the game on Release Patch.

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