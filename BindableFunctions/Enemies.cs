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

    [BindableMethod(name = "Self Damage", category = "Enemy Panel")]
    public static void SelfDamage()
    {
        if (PlayerData.instance.health <= 0)
        {
            DebugMod.LogConsole("Cannot damage self: health <= 0");
        }
        else if (HeroController.instance.cState.dead)
        {
            DebugMod.LogConsole("Cannot damage self: player is dead");
        }
        else if (!GameManager.instance.IsGameplayScene())
        {
            DebugMod.LogConsole("Cannot damage self: not a gameplay scene");
        }
        else if (HeroController.instance.cState.recoiling)
        {
            DebugMod.LogConsole("Cannot damage self: player is recoiling");
        }
        else if (HeroController.instance.cState.invulnerable)
        {
            DebugMod.LogConsole("Cannot damage self: player is invulnerable");
        }
        else
        {
            HeroController.instance.DamageSelf(1);
            DebugMod.LogConsole("Attempting self damage");
        }
    }
}