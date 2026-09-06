using HarmonyLib;

namespace DebugMod.CommandPalette;

[HarmonyPatch]
internal static class CommandPaletteInputBlocker
{
    [HarmonyPatch(typeof(InputHandler), "Update")]
    [HarmonyPrefix]
    private static bool InputHandler_Update()
    {
        return !CommandPaletteController.IsInputBlocked;
    }
}
