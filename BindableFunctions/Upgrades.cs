namespace DebugMod;

public static partial class BindableFunctions
{
    [BindableMethod(name = "UPGRADES_INCREASENEEDLEDAMAGE", category = "CATEGORY_UPGRADES")]
    public static void IncreaseNeedleDamage()
    {
        if (PlayerData.instance.nailDamage == 0)
        {
            PlayerData.instance.nailUpgrades = 0;
            PlayerData.instance.extraNailDamage = 0;
            DebugMod.LogConsole("Resetting needle damage to 5");
        }
        else if (PlayerData.instance.nailUpgrades == 4 || PlayerData.instance.extraNailDamage < 0)
        {
            PlayerData.instance.extraNailDamage += 4;
            DebugMod.LogConsole("Adding 4 extra needle damage");
        }
        else
        {
            PlayerData.instance.nailUpgrades++;
            DebugMod.LogConsole("Adding needle upgrade");
        }

        PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
    }

    [BindableMethod(name = "UPGRADES_DECREASENEEDLEDAMAGE", category = "CATEGORY_UPGRADES")]
    public static void DecreaseNeedleDamage()
    {
        if (PlayerData.instance.nailUpgrades == 0 || PlayerData.instance.extraNailDamage > 0)
        {
            PlayerData.instance.extraNailDamage -= 4;
            if (PlayerData.instance.extraNailDamage < -5)
            {
                PlayerData.instance.extraNailDamage = -5;
                DebugMod.LogConsole("Setting needle damage to 0");
            }
            else
            {
                DebugMod.LogConsole("Reducing needle damage by 4");
            }
        }
        else
        {
            PlayerData.instance.nailUpgrades--;
            DebugMod.LogConsole("Removing needle upgrade");
        }

        PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
    }
}
