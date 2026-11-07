using GlobalEnums;
using InControl;
using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace DebugMod.VersionInterop;

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal static class Extensions {
    extension(PlayerData playerData)
    {
#if SILKSONG
        public int manaMax
        {
            get => playerData.silkMax;
            set => playerData.silkMax = value;
        }
        public int mana
        {
            get => playerData.silk;
            set => playerData.silk = value;
        }
        public int CurrentManaMax => playerData.CurrentSilkMaxBasic;
        public int manaFragments
        {
            get => playerData.silkSpoolParts;
            set => playerData.silkSpoolParts = value;
        }
        public string corpseScene => playerData.HeroCorpseScene;
        public int corpseMoney => playerData.HeroCorpseMoneyPool;
#else
        public int manaMax
        {
            get => playerData.maxMP;
            set => playerData.maxMP = value;
        }
        public int mana
        {
            get => playerData.MPCharge;
            set => playerData.MPCharge = value;
        }
        public int CurrentManaMax => playerData.maxMP;
        public int manaFragments
        {
            get => playerData.vesselFragments;
            set => playerData.vesselFragments = value;
        }
        public string corpseScene => playerData.shadeScene;
        public int corpseMoney => playerData.geoPool;
        public int nailUpgrades
        {
            get => playerData.nailSmithUpgrades;
            set
            {
                playerData.nailSmithUpgrades = value;
                playerData.nailDamage = 5 + 4 * value;
                PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
            }
        }
#endif
    }

    extension(HeroController hero)
    {
#if SILKSONG
        public void AddToMaxMana(int amount) => hero.AddToMaxSilk(amount);
        public void TakeMana(int amount) => hero.TakeSilk(amount);
        public void AddMana(int amount, bool heroEffect) => hero.AddSilk(amount, heroEffect);
#else
        public void AddToMaxMana(int amount) => hero.AddToMaxMPReserve(amount);
        public void TakeMana(int amount) => hero.TakeMP(amount);
        public void AddMana(int amount, bool heroEffect) => hero.AddMPCharge(amount);
        public void AddInputBlocker(object blocker) => throw new NotImplementedException();
        public void RemoveInputBlocker(object blocker) => throw new NotImplementedException();
#endif
    }

#if !SILKSONG
    extension(HeroActions actions)
    {
        public PlayerAction Left => actions.left;
        public PlayerAction Right => actions.right;
        public PlayerAction Up => actions.up;
        public PlayerAction Down => actions.down;
        public PlayerAction Jump => actions.jump;
        public PlayerAction Dash => actions.dash;
        public PlayerAction Attack => actions.attack;
        public PlayerAction SuperDash => actions.superDash;
        public PlayerAction DreamNail => actions.dreamNail;
        public PlayerAction QuickCast => actions.quickCast;
        public PlayerAction Cast => actions.cast;
        public PlayerAction QuickMap => actions.quickMap;
        public PlayerAction OpenInventory => actions.openInventory;
        public PlayerAction Pause => actions.pause;
    }

    extension(GameManager gameManager)
    {
        public IEnumerator PauseGameToggle(bool playSound)
        {
            return gameManager.PauseGameToggle();
        }

        public GameState GameState => gameManager.gameState;
    }

    extension(GameCameras cameras)
    {
        public PlayMakerFSM hudCanvasSlideOut => cameras.hudCanvas.LocateMyFSM("Slide Out");
    }

    extension(NonSlider nonSlider)
    {
        public bool IsActive => true;
    }

    extension(HeroBox)
    {
        public static bool Inactive
        {
            get => HeroBox.inactive;
            set => HeroBox.inactive = value;
        }
    }
#endif
}
