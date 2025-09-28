namespace DebugMod;

public static class InventoryHandler
{
    public static void UnlockAllTools()
    {
        ToolItemManager.UnlockAllTools();
    }

    public static void UnlockAllCrests()
    {
        ToolItemManager.UnlockAllCrests();
    }

    public static void IncrementPouches()
    {
        var i = ++PlayerData.instance.ToolPouchUpgrades;
        if (i > 4) PlayerData.instance.ToolPouchUpgrades = 0;
    }

    public static void IncrementKits()
    {
        var i = ++PlayerData.instance.ToolKitUpgrades;
        if (i > 4) PlayerData.instance.ToolKitUpgrades = 0;
    }
}