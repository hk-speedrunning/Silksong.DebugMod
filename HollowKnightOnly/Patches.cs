using HarmonyLib;

namespace DebugMod;

[HarmonyPatch]
public partial class DebugMod
{
    [HarmonyPatch(typeof(HeroController), nameof(HeroController.TakeDamage))]
    [HarmonyPrefix]
    private static void HeroController_TakeDamage()
    {
        if (playerInvincible)
        {
            PlayerData.instance.isInvincible = true;
        }
    }
}
