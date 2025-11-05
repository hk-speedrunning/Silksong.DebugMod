using DebugMod.UI;

namespace DebugMod;

public static partial class BindableFunctions
{

    [BindableMethod(name = "Toggle HP Bars", category = "Enemy Panel")]
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