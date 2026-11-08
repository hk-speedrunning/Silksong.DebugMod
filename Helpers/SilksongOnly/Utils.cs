using System.Collections.Generic;
using System.Linq;

namespace DebugMod.Helpers;

internal static partial class Utils
{
    // Rewritten ToolItemManager.AutoEquip() that works on any game version
    internal static void AutoEquipCrest(ToolCrest crest, bool removeTools)
    {
        if (!crest) crest = ToolItemManager.GetCrestByName(PlayerData.instance.PreviousCrestID);
        if (!crest) crest = ToolItemManager.GetAllCrests().FirstOrDefault(c => c.IsVisible);
        if (!crest) return;

        if (crest.name != PlayerData.instance.CurrentCrestID)
        {
            PlayerData.instance.PreviousCrestID = PlayerData.instance.CurrentCrestID;
        }

        ToolItemManager.SetEquippedCrest(crest.name);
        PlayerData.instance.IsCurrentCrestTemp = false;

        if (removeTools)
        {
            List<string> equips = [];
            for (int i = 0; i < crest.Slots.Length; i++)
            {
                equips.Add("");
            }

            ToolItemManager.SetEquippedTools(crest.name, equips);
        }

        ToolItemManager.SendEquippedChangedEvent();
    }
}
