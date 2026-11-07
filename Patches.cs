using GlobalEnums;
using HarmonyLib;
using UnityEngine;

namespace DebugMod;

[HarmonyPatch]
public partial class DebugMod
{
    // Bounce off lava when invincible so the player doesn't just fall through the map
    [HarmonyPatch(typeof(HeroController), nameof(HeroController.TakeDamage))]
    [HarmonyPrefix]
    private static bool HeroController_TakeDamage(GameObject go, HazardType hazardType)
    {
        if (playerInvincible)
        {
            PlayerData.instance.isInvincible = true;
        }

        if (playerInvincible && !noclip && hazardType == HazardType.LAVA && go.name.Contains("Lava Box"))
        {
            HeroController.instance.ShroomBounce();
            return false;
        }

        return true;
    }
}
