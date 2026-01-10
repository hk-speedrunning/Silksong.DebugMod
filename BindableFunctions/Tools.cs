using System.Linq;

namespace DebugMod;

public static partial class BindableFunctions
{
    [BindableMethod(name = "Unlock All Tools", category = "Tools")]
    public static void UnlockAllTools()
    {
        ToolItemManager.UnlockAllTools();
        DebugMod.LogConsole("Unlocked all tools");
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

        DebugMod.LogConsole("Unlocked all crests");
    }

    [BindableMethod(name = "Craft Tools", category = "Tools")]
    public static void CraftTools()
    {
        ToolItemManager.TryReplenishTools(true, ToolItemManager.ReplenishMethod.Bench);
        DebugMod.LogConsole("Crafted new tools");
    }
}