using System.Linq;

namespace DebugMod;

public static partial class BindableFunctions
{
    [BindableMethod(name = "Unlock All Tools", category = "Tools")]
    public static void UnlockAllTools()
    {
        ToolItemManager.UnlockAllTools();
        Console.AddLine("Unlocked all tools");
    }

    [BindableMethod(name = "Unlock All Crests", category = "Tools")]
    public static void UnlockAllCrests()
    {
        ToolItemManager.UnlockAllCrests();

        if (ToolItemManager.Instance && ToolItemManager.Instance.crestList)
        {
            foreach (ToolCrest crest in ToolItemManager.Instance.crestList)
            {
                crest.slots = crest.slots.Select(slotInfo => slotInfo with { IsLocked = false }).ToArray();

                ToolCrestsData.Data crestData = crest.SaveData;
                if (crestData.Slots != null)
                {
                    crestData.Slots = crestData.Slots.Select(slot => slot with { IsUnlocked = true }).ToList();
                }
                crest.SaveData = crestData;
            }
        }

        Console.AddLine("Unlocked all crests");
    }

    [BindableMethod(name = "Increment Tool Pouch", category = "Tools")]
    public static void IncrementPouches()
    {
        if (PlayerData.instance.ToolPouchUpgrades < 4)
        {
            PlayerData.instance.ToolPouchUpgrades++;
            Console.AddLine($"Increasing tool pouch level (now {PlayerData.instance.ToolPouchUpgrades})");
        }
        else
        {
            PlayerData.instance.ToolPouchUpgrades = 0;
            Console.AddLine("Resetting tool pouch level");
        }
    }

    [BindableMethod(name = "Increment Crafting Kit", category = "Tools")]
    public static void IncrementKits()
    {
        if (PlayerData.instance.ToolKitUpgrades < 4)
        {
            PlayerData.instance.ToolKitUpgrades++;
            Console.AddLine($"Increasing crafting kit level (now {PlayerData.instance.ToolKitUpgrades})");
        }
        else
        {
            PlayerData.instance.ToolKitUpgrades = 0;
            Console.AddLine("Resetting crafting kit level");
        }
    }

    [BindableMethod(name = "Infinite Tools", category = "Tools")]
    public static void ToggleInfiniteTools()
    {
        DebugMod.infiniteTools = !DebugMod.infiniteTools;
        Console.AddLine("Infinite Tools set to " + DebugMod.infiniteTools.ToString().ToUpper());
    }

    [BindableMethod(name = "Craft Tools", category = "Tools")]
    public static void CraftTools()
    {
        ToolItemManager.TryReplenishTools(true, ToolItemManager.ReplenishMethod.Bench);
        Console.AddLine("Crafted new tools");
    }
}