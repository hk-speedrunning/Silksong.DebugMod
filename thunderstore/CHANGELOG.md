## Changelog

### v1.0.4

- Added `Equip Anywhere` button to Items panel, enabling changing loadouts away from a bench.
- Fixed Red Memory savestates with Safe Savestate Loading disabled.
- Fixed visual glitch when loading states while held by a Wraith.
- Fixed unlocalised text when rebinding keys.
- Toggle All UI now reveals all even if individual panels are already shown.

### v1.0.3

- Fixed audio issue loading savestates during mist voidout on Release Patch.
- Fixed jagged movement after loading state while on an elevator.
- Improvements for glitched runners:
    - Show hitboxes while paused.
    - Load dupes with boss scenes reliably.
    - Remove SaveStateGlitchFixes setting; see GlitchDebug v0.2.0.
    - Override Savestate Lockout now bypasses all load protections, allowing loading during hardlocks.

### v1.0.2

- Fixed v1.0.2 causing crashes on main menu with I18N missing.
- Fixed loading savestates during Cross Stitch cutscene causing janky movement (due to rescaled FixedTime)
- Fixed loading savestates during certain cutscenes causing audio to be ducked after following transitions.
- Reverted change to savestate serializer; use other workarounds to fix fields broken by Unity's serializer.
- Reduced lag when loading file slots using Refresh File Slots & on startup

### v1.0.1

- BREAKING: changed the way savestates are serialized; memory savestates may need to be re-created to continue resetting tools correctly.
- Changed the Thunderstore package to reduce confusion; installers now auto-download I18N & ModMenu for ease of use. Manual installations remain unaffected.
- Fixed crest buttons not working on older patches.
- Update translation immediately when changing I18N.LanguageOverride.
- Fixed savestate panel moving offscreen when UI is rebuilt.
- Fixed HP bars displaying incorrectly.


### v1.0.0

- Added optional dependencies on I18N & ModMenu.
- Added Chinese localization (thanks @DUUScarlet & @yuniBiscuit!)
- New Items Panel:
  - Individual tool unlock buttons (including cycling between tool variants)
  - Individual crests, including Hunter evolution level & cursed/cloakless states.
  - Keys & items, including all unique keys & quill variants. if you wanted those.
  - Consumables (rosary strings, shard bundles, simple keys, silkeaters & more!).
  - Buttons to unlock all maps & fast travel
- Adding Lifeblood now correctly enters plasmified state.
- Zoom & Timescale now have reworked UI controls, with increment/decrement/reset as well as setting values directly.
- Zoom now correctly scales blurred background objects (thanks @FoxyRobo!)
- HP bars now have additional text for staggerable bosses, showing their stagger counter & whether stuns are blocked.
- Invincibility now prevents Hornet from being grabbed by barnacles.
- Savestates:
    - Loading out of a memory no longer resets tool amounts incorrectly.
    - Loading now correctly resets anklet & frost timers.
    - States now store Hornet's facing direction for all newly created savestates.
    - Fixed soft/hardlocks loading out of Silk Heart pickups, Cradle Elevator & other cutscenes.
    - Loading during hazard respawns should now be less disorienting.
    - Fixed inconsistent positioning when loading states in 4th Chorus' room.
    - Added specific state overrides for Cogwork Dancers & Karmelita's arena waves, and wrong-direction screams in HHA.
    - Added a Refresh File Slots function to load states without restarting the game.
- Hitbox viewer improvements:
    - The default "uncategorized" colour has been changed from orange to grey to reduce visual noise.
    - Pink breakable objects now show in reduced mode only if they are pogoable.
    - Multihit hazards & attacks were not being correctly detected; these are now correctly detected as Red.
    - Elevators now correctly update hitbox types when their doors become NonSlider.
    - New categories: Enemy Alert Regions (orange), Non Mantle regions (dark green)
- Hopefully better compatibility with future versions of GlitchedDebug; we're looking into further improvements we can make to avoid breaking compatibility every few updates :)
    - For now, glitched runners can reenable the slower glitch-safe savestate loading in Mod Settings as "Safe Savestate Loading".

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