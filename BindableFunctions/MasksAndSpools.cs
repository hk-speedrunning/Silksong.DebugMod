using DebugMod.Helpers;
using System;

namespace DebugMod;

public static partial class BindableFunctions
{
    [BindableMethod(name = "MASKSANDSPOOLS_GIVEMASK", category = "CATEGORY_MASKSANDSPOOLS")]
    public static void GiveMask()
    {
        if (PlayerData.instance.maxHealthBase < 10)
        {
            HeroController.instance.MaxHealth();
            HeroController.instance.AddToMaxHealth(1);
            HudHelper.RefreshMasks();

            DebugMod.LogConsole("Added mask");
        }
        else
        {
            DebugMod.LogConsole("You have the maximum number of masks");
        }
    }

    [BindableMethod(name = "MASKSANDSPOOLS_GIVESPOOL", category = "CATEGORY_MASKSANDSPOOLS")]
    public static void GiveSpool()
    {
        if (PlayerData.instance.spools < PlayerData.MaxSpools)
        {
            HeroController.instance.AddToMaxMana(1);
            HudHelper.RefreshMana();

            DebugMod.LogConsole("Added spool");
        }
        else
        {
            DebugMod.LogConsole("You have the maximum number of spools");
        }

#if SILKSONG
        PlayerData.instance.IsSilkSpoolBroken = false;
        EventRegister.SendEvent("SPOOL UNBROKEN");
#endif
    }

    [BindableMethod(name = "MASKSANDSPOOLS_TAKEMASK", category = "CATEGORY_MASKSANDSPOOLS")]
    public static void TakeAwayMask()
    {
        if (PlayerData.instance.maxHealthBase > 1)
        {
            PlayerData.instance.maxHealth -= 1;
            PlayerData.instance.maxHealthBase -= 1;
            PlayerData.instance.health = Math.Min(PlayerData.instance.health, PlayerData.instance.maxHealth);
            HudHelper.RefreshMasks();

            DebugMod.LogConsole("Removed mask");
        }
        else
        {
            DebugMod.LogConsole("You have the minimum number of masks");
        }
    }

    [BindableMethod(name = "MASKSANDSPOOLS_TAKESPOOL", category = "CATEGORY_MASKSANDSPOOLS")]
    public static void TakeAwaySpool()
    {
        if (PlayerData.instance.spools > 0)
        {
            HeroController.instance.TakeFromMaxMana(1);
            HudHelper.RefreshMana();

            DebugMod.LogConsole("Removed spool");
        }
        else
        {
            DebugMod.LogConsole("You have the minimum number of spools");
        }
    }

    [BindableMethod(name = "MASKSANDSPOOLS_GIVEMASKSHARD", category = "CATEGORY_MASKSANDSPOOLS")]
    public static void GiveMaskShard()
    {
        if (PlayerData.instance.heartPieces < 3)
        {
            PlayerData.instance.heartPieces++;
            DebugMod.LogConsole($"Added mask shard (now {PlayerData.instance.heartPieces})");
        }
        else
        {
            PlayerData.instance.heartPieces = 0;
            GiveMask();
        }
    }

    [BindableMethod(name = "MASKSANDSPOOLS_TAKEMASKSHARD", category = "CATEGORY_MASKSANDSPOOLS")]
    public static void TakeMaskShard()
    {
        if (PlayerData.instance.heartPieces > 0)
        {
            PlayerData.instance.heartPieces--;
            DebugMod.LogConsole($"Removed mask shard (now {PlayerData.instance.heartPieces})");
        }
        else
        {
            PlayerData.instance.heartPieces = 3;
            TakeAwayMask();
        }
    }

    [BindableMethod(name = "MASKSANDSPOOLS_GIVESPOOLFRAGMENT", category = "CATEGORY_MASKSANDSPOOLS")]
    public static void GiveSpoolFragment()
    {
        if (PlayerData.instance.manaFragments < PlayerData.MaxManaFragments)
        {
            PlayerData.instance.manaFragments++;
            DebugMod.LogConsole("Added spool fragment");
        }
        else
        {
            PlayerData.instance.manaFragments = 0;
            GiveSpool();
        }
    }

    [BindableMethod(name = "MASKSANDSPOOLS_TAKESPOOLFRAGMENT", category = "CATEGORY_MASKSANDSPOOLS")]
    public static void TakeSpoolFragment()
    {
        if (PlayerData.instance.manaFragments > 0)
        {
            PlayerData.instance.manaFragments--;
            DebugMod.LogConsole("Removed spool fragment");
        }
        else
        {
            PlayerData.instance.manaFragments = PlayerData.MaxManaFragments;
            TakeAwaySpool();
        }
    }

    private static bool CanModifyHealth(int health)
    {
        if (health <= 0)
        {
            DebugMod.LogConsole("Cannot add/take health: health <= 0");
            return false;
        }

        if (HeroController.instance.cState.dead)
        {
            DebugMod.LogConsole("Cannot add/take health: player is dead");
            return false;
        }

        if (!GameManager.instance.IsGameplayScene())
        {
            DebugMod.LogConsole("Cannot add/take health: not a gameplay scene");
            return false;
        }

        return true;
    }

    [BindableMethod(name = "MASKSANDSPOOLS_ADDHEALTH", category = "CATEGORY_MASKSANDSPOOLS")]
    public static void AddHealth()
    {
        if (CanModifyHealth(PlayerData.instance.health + 1))
        {
            HeroController.instance.AddHealth(1);
            HudHelper.RefreshMasks();

            DebugMod.LogConsole("Added health");
        }
    }

    [BindableMethod(name = "MASKSANDSPOOLS_TAKEHEALTH", category = "CATEGORY_MASKSANDSPOOLS")]
    public static void TakeHealth()
    {
        if (CanModifyHealth(PlayerData.instance.health - 1))
        {
            HeroController.instance.TakeHealth(1);
            HudHelper.RefreshMasks();

            DebugMod.LogConsole("Took health");
        }
    }

    [BindableMethod(name = "MASKSANDSPOOLS_ADDSILK", category = "CATEGORY_MASKSANDSPOOLS")]
    public static void AddSilk()
    {
        HeroController.instance.AddMana(1, true);

        DebugMod.LogConsole("Added silk");
    }

    [BindableMethod(name = "MASKSANDSPOOLS_TAKESILK", category = "CATEGORY_MASKSANDSPOOLS")]
    public static void TakeSilk()
    {
        HeroController.instance.TakeMana(1);

        DebugMod.LogConsole("Attempting to take silk");
    }

    [BindableMethod(name = "MASKSANDSPOOLS_ADDLIFEBLOOD", category = "CATEGORY_MASKSANDSPOOLS")]
    public static void Lifeblood()
    {
        HeroController.instance.AddLifeblood();
        DebugMod.LogConsole("Attempting to add lifeblood");
    }
}