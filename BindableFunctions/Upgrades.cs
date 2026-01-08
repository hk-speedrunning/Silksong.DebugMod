namespace DebugMod;

public static partial class BindableFunctions
{
    [BindableMethod(name = "Increase Needle Damage", category = "Upgrades")]
    public static void IncreaseNeedleDamage()
    {
        if (PlayerData.instance.nailDamage == 0)
        {
            PlayerData.instance.nailUpgrades = 0;
            DebugMod.extraNailDamage = 0;
            DebugMod.LogConsole("Resetting needle damage to 5");
        }
        else if (PlayerData.instance.nailUpgrades == 4 || DebugMod.extraNailDamage < 0)
        {
            DebugMod.extraNailDamage += 4;
            DebugMod.LogConsole("Adding 4 extra needle damage");
        }
        else
        {
            PlayerData.instance.nailUpgrades++;
            DebugMod.LogConsole("Adding needle upgrade");
        }

        PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
    }

    [BindableMethod(name = "Decrease Needle Damage", category = "Upgrades")]
    public static void DecreaseNeedleDamage()
    {
        if (PlayerData.instance.nailUpgrades == 0 || DebugMod.extraNailDamage > 0)
        {
            DebugMod.extraNailDamage -= 4;
            if (DebugMod.extraNailDamage < -5)
            {
                DebugMod.extraNailDamage = -5;
                DebugMod.LogConsole("Setting needle damage to 0");
            }
            else
            {
                DebugMod.LogConsole("Reducing nail damage by 4");
            }
        }
        else
        {
            PlayerData.instance.nailUpgrades--;
            DebugMod.LogConsole("Removing needle upgrade");
        }

        PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
    }

    [BindableMethod(name = "Increment Tool Pouch", category = "Upgrades")]
    public static void IncrementPouches()
    {
        if (PlayerData.instance.ToolPouchUpgrades < 4)
        {
            PlayerData.instance.ToolPouchUpgrades++;
            DebugMod.LogConsole($"Increasing tool pouch level (now {PlayerData.instance.ToolPouchUpgrades})");
        }
        else
        {
            PlayerData.instance.ToolPouchUpgrades = 0;
            DebugMod.LogConsole("Resetting tool pouch level");
        }
    }

    [BindableMethod(name = "Increment Crafting Kit", category = "Upgrades")]
    public static void IncrementKits()
    {
        if (PlayerData.instance.ToolKitUpgrades < 4)
        {
            PlayerData.instance.ToolKitUpgrades++;
            DebugMod.LogConsole($"Increasing crafting kit level (now {PlayerData.instance.ToolKitUpgrades})");
        }
        else
        {
            PlayerData.instance.ToolKitUpgrades = 0;
            DebugMod.LogConsole("Resetting crafting kit level");
        }
    }

    [BindableMethod(name = "Give Silk Heart", category = "Upgrades")]
    public static void IncrementSilkHeart()
    {
        if (PlayerData.instance.silkRegenMax < 3)
        {
            PlayerData.instance.silkRegenMax++;
            DebugMod.LogConsole($"Giving player Silk Heart (now {PlayerData.instance.silkRegenMax})");
        }
        else
        {
            PlayerData.instance.silkRegenMax = 0;
            DebugMod.LogConsole("Taking away all Silk Hearts");
        }
    }
}