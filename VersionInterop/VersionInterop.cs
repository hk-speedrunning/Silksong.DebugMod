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
        public static int MaxManaFragments => 1;
        public string corpseScene => playerData.HeroCorpseScene;
        public int corpseMoney => playerData.HeroCorpseMoneyPool;
        public int extraNailDamage
        {
            get => DebugMod.extraNailDamage;
            set => DebugMod.extraNailDamage = value;
        }
        public int spools => playerData.silkMax - 9;
        public static int MaxSpools => 9;
#else
        public int manaMax => playerData.maxMP;
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
        public static int MaxManaFragments => 2;
        public string corpseScene => playerData.shadeScene;
        public int corpseMoney => playerData.geoPool;
        public int spools => playerData.MPReserveMax / 33;
        public static int MaxSpools => 3;
        public int extraNailDamage
        {
            get => playerData.nailDamage - (5 + 4 * playerData.nailSmithUpgrades);
            set
            {
                playerData.nailDamage = 5 + 4 * playerData.nailSmithUpgrades + value;
                PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
            }
        }
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
        public void TakeFromMaxMana(int amount)
        {
            PlayerData.instance.silkMax -= amount;
            PlayerData.instance.silk = Math.Min(PlayerData.instance.silk, PlayerData.instance.silkMax);
        }
        public void TakeMana(int amount) => hero.TakeSilk(amount);
        public void AddMana(int amount, bool heroEffect) => hero.AddSilk(amount, heroEffect);
        public bool IsHeroPaused() => hero.IsPaused();
        public void ResetLockStates() => hero.SetLockStates(HeroLockStates.None);
        public void AddLifeblood()
        {
            bool wasInLifebloodState = hero.IsInLifebloodState;

            EventRegister.SendEvent("ADD BLUE HEALTH");

            if (!wasInLifebloodState && hero.IsInLifebloodState)
            {
                hero.HitMaxBlueHealthBurst();
            }
        }
#else
        public void AddToMaxMana(int amount) => PlayerData.instance.MPReserveMax += 33 * amount;
        public void TakeFromMaxMana(int amount)
        {
            PlayerData.instance.MPReserveMax -= 33 * amount;
            PlayerData.instance.MPReserve = Math.Min(PlayerData.instance.MPReserve, PlayerData.instance.MPReserveMax);
        }
        public void TakeMana(int amount) => hero.TakeMP(amount);
        public void AddMana(int amount, bool heroEffect) => hero.AddMPCharge(amount);
        public bool IsHeroPaused() => GameManager.instance.isPaused;
        // TODO(port): check if we need to do something here
        public void ResetLockStates() { }
        public void AddLifeblood() => EventRegister.SendEvent("ADD BLUE HEALTH");
        public void DamageSelf(int amount) => hero.TakeDamage(hero.gameObject, hero.cState.facingRight ? CollisionSide.right : CollisionSide.left, amount, 1);
        public IEnumerator Die(bool nonLethal, bool frostDeath) => hero.Die();
        // TODO(port): block hero input on HK. is this necessary?
        public void AddInputBlocker(object blocker) { }
        public void RemoveInputBlocker(object blocker) { }
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

    extension(InvulnerablePulse pulse)
    {
        public void StopInvulnerablePulse() => pulse.stopInvulnerablePulse();
    }

    extension(CameraController camera)
    {
        public void PositionToHeroInstant(bool forceDirect) => camera.PositionToHero(forceDirect);
    }

    extension(GameCameras cameras)
    {
        public PlayMakerFSM hudCanvasSlideOut => cameras.hudCanvas.LocateMyFSM("Slide Out");
    }

    extension(HeroControllerStates states)
    {
        public bool Invulnerable => states.invulnerable;
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
