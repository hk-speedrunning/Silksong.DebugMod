using System;
using DebugMod.Helpers;

namespace DebugMod;

public static partial class BindableFunctions
{
    [BindableMethod(name = "Give Mask", category = "Masks & Spools")]
    public static void GiveMask()
    {
        if (PlayerData.instance.maxHealthBase < 10)
        {
            HeroController.instance.MaxHealth();
            HeroController.instance.AddToMaxHealth(1);
            HudHelper.RefreshMasks();

            DebugMod.LogConsole("Added Mask");
        }
        else
        {
            DebugMod.LogConsole("You have the maximum number of masks");
        }
    }
    
    [BindableMethod(name = "Give Spool", category = "Masks & Spools")]
    public static void GiveSpool()
    {
        if (PlayerData.instance.silkMax < 18)
        {
            HeroController.instance.AddToMaxSilk(1);
            HudHelper.RefreshSpool();

            DebugMod.LogConsole("Added Spool");
        }
        else
        {
            DebugMod.LogConsole("You have the maximum number of spools");
        }

        PlayerData.instance.IsSilkSpoolBroken = false;
        EventRegister.SendEvent("SPOOL UNBROKEN");
    }
    
    [BindableMethod(name = "Take Away Mask", category = "Masks & Spools")]
    public static void TakeAwayMask()
    {
        if (PlayerData.instance.maxHealthBase > 1)
        {
            PlayerData.instance.maxHealth -= 1;
            PlayerData.instance.maxHealthBase -= 1;
            PlayerData.instance.health = Math.Min(PlayerData.instance.health, PlayerData.instance.maxHealth);
            HudHelper.RefreshMasks();

            DebugMod.LogConsole("Took Away Mask");
        }
        else
        {
            DebugMod.LogConsole("You have the minimum number of masks");
        }
    }

    [BindableMethod(name = "Take Away Spool", category = "Masks & Spools")]
    public static void TakeAwaySpool()
    {
        if (PlayerData.instance.silkMax > 9)
        {
            PlayerData.instance.silkMax--;
            PlayerData.instance.silk = Math.Min(PlayerData.instance.silk, PlayerData.instance.silkMax);
            HudHelper.RefreshSpool();

            DebugMod.LogConsole("Removed Spool");
        }
        else
        {
            DebugMod.LogConsole("You have the minimum number of spools");
        }
    }

    private static bool CanModifyHealth(int health)
    {
        if (health <= 0)
        {
            DebugMod.LogConsole("Cannot add/take health: health is too low");
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

    [BindableMethod(name = "Add Health", category = "Masks & Spools")]
    public static void AddHealth()
    {
        if (CanModifyHealth(PlayerData.instance.health + 1))
        {
            HeroController.instance.AddHealth(1);
            HudHelper.RefreshMasks();

            DebugMod.LogConsole("Added Health");
        }
    }

    [BindableMethod(name = "Take Health", category = "Masks & Spools")]
    public static void TakeHealth()
    {
        if (CanModifyHealth(PlayerData.instance.health - 1))
        {
            HeroController.instance.TakeHealth(1);
            HudHelper.RefreshMasks();

            DebugMod.LogConsole("Took health");
        }
    }
    
    [BindableMethod(name = "Add Silk", category = "Masks & Spools")]
    public static void AddSilk()
    {
        HeroController.instance.AddSilk(1, true);

        DebugMod.LogConsole("Added Silk");
    }

    [BindableMethod(name = "Take Silk", category = "Masks & Spools")]
    public static void TakeSilk()
    {
        HeroController.instance.TakeSilk(1);

        DebugMod.LogConsole("Attempting to take silk");
    }

    [BindableMethod(name = "Add Lifeblood", category = "Masks & Spools")]
    public static void Lifeblood()
    {
        EventRegister.SendEvent("ADD BLUE HEALTH");

        DebugMod.LogConsole("Attempting to add lifeblood");
    }
}