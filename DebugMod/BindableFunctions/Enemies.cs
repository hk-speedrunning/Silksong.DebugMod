using DebugMod.UI;

namespace DebugMod
{
    public static partial class BindableFunctions
    {

        [BindableMethod(name = "Toggle HP Bars", category = "Enemy Panel")]
        public static void ToggleEnemyHPBars()
        {
            EnemiesPanel.hpBars = !EnemiesPanel.hpBars;

            if (EnemiesPanel.hpBars)
            {
                Console.AddLine("Enabled HP bars");
            }
            else
            {
                Console.AddLine("Disabled HP bars");
            }
        }

        [BindableMethod(name = "Self Damage", category = "Enemy Panel")]
        public static void SelfDamage()
        {
            if (PlayerData.instance.health <= 0)
            {
                Console.AddLine("Cannot damage self: health <= 0");
            }
            else if (HeroController.instance.cState.dead)
            {
                Console.AddLine("Cannot damage self: player is dead");
            }
            else if (!GameManager.instance.IsGameplayScene())
            {
                Console.AddLine("Cannot damage self: not a gameplay scene");
            }
            else if (HeroController.instance.cState.recoiling)
            {
                Console.AddLine("Cannot damage self: player is recoiling");
            }
            else if (HeroController.instance.cState.invulnerable)
            {
                Console.AddLine("Cannot damage self: player is invulnerable");
            }
            else
            {
                HeroController.instance.DamageSelf(1);
                Console.AddLine("Attempting self damage");
            }
        }
    }
}