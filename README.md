# DebugMod for Silksong

HK-Speedrunning's primary practice tool, with fully-featured savestates, cheats, and many more features. 
Based on [Hollow Knight's DebugMod](https://github.com/TheMulhima/HollowKnight.DebugMod/), with an all-new intuitive UI & further improvements to savestates.

Press `F2` to reveal the UI.

For any questions or bug reports, please join the [Modding Discord](https://discord.gg/F6Y5TeFQ8j) or [Speedrunning Discord](https://discord.gg/3JtHPsBjHD).

---
- [DebugMod for Silksong](#debugmod-for-silksong)
  - [Features](#features)
  - [Upcoming Features](#upcoming-features)
  - [Installation](#installation)
    - [Manual Installation](#manual-installation)
  - [Credits](#credits)
    - [Hollow Knight DebugMod](#hollow-knight-debugmod)
    - [Silksong DebugMod](#silksong-debugmod)
---

## Features

- All-new modernized UI to use all the mod's features in-game, without obscuring gameplay.
- All functions bindable to hotkeys via the in-game UI, or usable with the mouse.
- View debug information in the overlay.
- ### Savestates:
    - Set & load complete savestates, reverting all changes to Hornet's inventory, 
      wish & journal progress, world state & much more.
  - Automatically load savestate on death for instant retries.
  - Store savestates to file, and use third-party savestate packs to practice easily.
  > [!WARNING]
  > Loading savestates **_will overwrite your savefile_** with no undo option!
- ### Gameplay:
  - Infinite health/silk, Invincibility, noclip & others.
  - Change & freeze timescale, advance frame-by-frame.
  - Built-in hitbox visualiser, color coded by hitbox type.
  - Preview where the cocoon will spawn when the player dies.
  - Damage or kill Hornet.
  - Set & warp to the last hazard respawn.
  - Zoom the camera in & out.
  - Visual changes like hiding Hornet, the HUD, disabling the lighting around her & more.
  - Toggle Act 3 world state (takes effect on next scene load).
  - Block changes to scene data (rosary strings, breakable walls etc) & reset scene data to default
- ### Items:
  - Give/remove player skills.
  - Set player health, silk, masks, spools, needle damage.
  - Give unlimited tools, rosaries & shell shards.
  - Refill tools.
  - Unlock all tools, crests, Hunter crest upgrades & crest slots .
- ### Enemies:
  - View all enemy health.
  - Delete, clone, or give any enemy 9999 HP.
  - Add HP bars above enemies.
  - Kill all enemies.

## Upcoming Features

- Reorganising the Items tab with more granular control over tools & many additional items.
- Hollow Knight DebugMod parity: as of right now this needs;
  - Respawn Boss (+ Arena). _This may already be covered by scene data but it's possible bosses have varying implementations reliant on PlayerData._
  - Give specific keys/items & bellway / ventrica access
  - Grant melodies & hearts
- Expose some way for extensions to present their own UI in Main Panel. Currently they are only appended to the Keybinds list.

## Installation

DebugMod is distributed on [Thunderstore](https://thunderstore.io/c/hollow-knight-silksong/p/hk_speedrunning/DebugMod/) so any compatible installer can install it.
We recommend [Gale](https://github.com/Kesomannen/gale/releases/tag/1.11.1), where you can search for DebugMod and install it directly to a profile & launch.

Other compatible installers include [r2modman]() & [Thunderstore](https://get.thunderstore.io) (Overwolf).

### Manual Installation 

Required on MacOS as installers don't appear to support it. Instructions are for a Windows install using Steam.

1) Download BepInEx from here: https://thunderstore.io/c/hollow-knight-silksong/p/BepInEx/BepInExPack_Silksong/
1) Right click Silksong in Steam -> Properties -> Installed Files -> Browse...
1) Copy the contents of the BepInEx zip into this folder (overwrite files if asked)
1) Open and close the game once so BepInEx can generate its initial files
1) Download `DebugMod_MANUAL.zip` from here: https://github.com/hk-speedrunning/Silksong.DebugMod/releases/latest
1) Copy the DebugMod and Silksong.ModList folders into `BepInEx/plugins` in the game folder (overwrite files if asked)
1) This mod should not affect saves negatively, but it is a good idea to back them up anyway.
   Saves are located at `%AppData%/../LocalLow/Team Cherry/Hollow Knight Silksong`

> [!IMPORTANT]
> For moderation reasons, this mod requires [Silksong.ModList](https://github.com/silksong-modding/Silksong.ModList) to be installed. This mod is included in the [release download](<https://github.com/hk-speedrunning/Silksong.DebugMod/releases/latest>).
If Silksong.ModList is not installed, DebugMod will silently fail to load.
   
## Credits

### Hollow Knight DebugMod
- Coding - Serena
- SaveStates/Old Current Patch - 56
- UI design and graphics - The Embraced One
- Assistance with canvas - KDT
- 1.5 and A lot of Changes - Mulhima
- Multiple SaveStates/Minimal info panel- Cerpin
- Improve hitbox viewer - DemoJameson
- Multiple SaveState Pages - Magnetic Pizza (and jhearom for porting to lp)
- Additional Glitched functionality - pseudorandomhk
- Additional Bindable Functions, and Fix Stuff - Flib
- Buttons to directly run bindable actions - flukebull
- Add frame by frame advance code - SFGrenade

### Silksong DebugMod
- Initial Port, Core development & UI Overhaul - spacemonkeyy
- Minor bugfixing, nitpicking & maintenance - Jamie
- Additional fixes - Flib, cometcake575, Freya Holmér
