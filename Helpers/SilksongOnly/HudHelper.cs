using UnityEngine;

namespace DebugMod.Helpers;

public static partial class HudHelper
{
    public static partial void RefreshMana()
    {
        SilkSpool spool = GameCameras.instance.silkSpool;
        spool.usingSilk.RemoveAll(x => x != SilkSpool.SilkUsingFlags.Normal);
        spool.DrawSpool();

        Transform parent = spool.activeParent.transform.parent;
        GameObject fleaEgg = null;
        for (int i = 0; i < parent.childCount; i++)
        {
            GameObject child = parent.GetChild(i).gameObject;
            if (child.name == "Tool Flea Egg HUD")
            {
                fleaEgg = child;
                break;
            }
        }

        // Loading save states breaks this FSM for some reason
        fleaEgg.GetComponent<SpriteRenderer>().enabled = ToolItemManager.GetToolByName("Flea Charm").IsEquippedHud;

        HeroController.instance.hasSilkSpoolAppeared = true;
    }
}
