using DebugMod.UI;

namespace DebugMod;

public static partial class BindableFunctions
{

    [BindableMethod(name = "ACTION_TOGGLEHPBARS", category = "CATEGORY_ENEMIES")]
    public static void ToggleEnemyHPBars()
    {
        EnemiesPanel.hpBars = !EnemiesPanel.hpBars;

        if (EnemiesPanel.hpBars)
        {
            DebugMod.LogConsole("Enabled HP bars");
        }
        else
        {
            DebugMod.LogConsole("Disabled HP bars");
        }
    }
}