using GlobalEnums;
using UnityEngine;

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

        [BindableMethod(name = "Enemy Scan", category = "Enemy Panel")]
        public static void EnemyScan()
        {
            EnemiesPanel.EnemyUpdate();
            Console.AddLine("Scanning for enemies...");
        }

        [BindableMethod(name = "Self Damage", category = "Enemy Panel")]
        public static void SelfDamage()
        {
            if (PlayerData.instance.health <= 0 || HeroController.instance.cState.dead || !GameManager.instance.IsGameplayScene() || GameManager.instance.IsGamePaused() || HeroController.instance.cState.recoiling || HeroController.instance.cState.invulnerable)
            {
                Console.AddLine("Unacceptable conditions for selfDamage(" + PlayerData.instance.health + "," + DebugMod.HC.cState.dead + "," + DebugMod.GM.IsGameplayScene() + "," + DebugMod.HC.cState.recoiling + "," + DebugMod.GM.IsGamePaused() + "," + DebugMod.HC.cState.invulnerable + ")." + " Pressed too many times at once?");
                return;
            }
            //GameManager.instance.gameObject.AddComponent<SelfDamage>();
            HeroController.instance.TakeDamage(new GameObject(),CollisionSide.left,1,(int)HazardType.NON_HAZARD);

            Console.AddLine("Attempting self damage");
        }
    }
}