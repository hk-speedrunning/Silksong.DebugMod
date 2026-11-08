using HarmonyLib;

namespace DebugMod;

public static partial class BindableFunctions
{
    // Don't try to spawn new silk chunks if the HUD is disabled
    [HarmonyPatch(typeof(SilkSpool), nameof(SilkSpool.ChangeSilk))]
    [HarmonyPrefix]
    private static bool SilkSpool_ChangeSilk()
    {
        return GameCameras.instance.hudCanvasSlideOut.gameObject.activeInHierarchy;
    }
}
