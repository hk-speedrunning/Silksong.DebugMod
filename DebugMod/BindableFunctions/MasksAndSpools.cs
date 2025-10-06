using System;
using DebugMod.MethodHelpers;

namespace DebugMod
{
    public static partial class BindableFunctions
    {
        [BindableMethod(name = "Give Mask", category = "Masks & Spools")]
        public static void GiveMask()
        {
            if (PlayerData.instance.maxHealthBase < 10)
            {
                HeroController.instance.MaxHealth();
                HeroController.instance.AddToMaxHealth(1);
                HudHelper.RefreshMasks();

                Console.AddLine("Added Mask");
            }
            else
            {
                Console.AddLine("You have the maximum number of masks");
            }
        }
        
        [BindableMethod(name = "Give Spool", category = "Masks & Spools")]
        public static void GiveSpool()
        {
            if (PlayerData.instance.silkMax < 18)
            {
                HeroController.instance.AddToMaxSilk(1);
                HudHelper.RefreshSpool();

                Console.AddLine("Added Spool");
            }
            else
            {
                Console.AddLine("You have the maximum number of spools");
            }

            PlayerData.instance.IsSilkSpoolBroken = false;
            EventRegister.SendEvent("SPOOL UNBROKEN");
        }
        
        [BindableMethod(name = "Take Away Mask", category = "Masks & Spools")]
        public static void TakeAwayMask()
        {
            if (PlayerData.instance.maxHealthBase > 1)
            {
                PlayerData.instance.maxHealth -= 1;
                PlayerData.instance.maxHealthBase -= 1;
                PlayerData.instance.health = Math.Min(PlayerData.instance.health, PlayerData.instance.maxHealth);
                HudHelper.RefreshMasks();

                Console.AddLine("Took Away Mask");
            }
            else
            {
                Console.AddLine("You have the minimum number of masks");
            }
        }

        [BindableMethod(name = "Take Away Spool", category = "Masks & Spools")]
        public static void TakeAwaySpool()
        {
            if (PlayerData.instance.silkMax > 9)
            {
                PlayerData.instance.silkMax--;
                PlayerData.instance.silk = Math.Min(PlayerData.instance.silk, PlayerData.instance.silkMax);
                HudHelper.RefreshSpool();

                Console.AddLine("Removed Spool");
            }
            else
            {
                Console.AddLine("You have the minimum number of spools");
            }
        }

        [BindableMethod(name = "Add Health", category = "Masks & Spools")]
        public static void AddHealth()
        {
            if (PlayerData.instance.health <= 0 || HeroController.instance.cState.dead || !GameManager.instance.IsGameplayScene())
            {
                Console.AddLine("Unacceptable conditions for adding health" + PlayerData.instance.health + "," + DebugMod.HC.cState.dead + "," + DebugMod.GM.IsGameplayScene() + "," + DebugMod.HC.cState.recoiling + "," + DebugMod.GM.IsGamePaused() + "," + DebugMod.HC.cState.invulnerable + ")." + " Pressed too many times at once?");
                return;
            }
            HeroController.instance.AddHealth(1);

            Console.AddLine("Added Health");
        }

        [BindableMethod(name = "Take Health", category = "Masks & Spools")]
        public static void TakeHealth()
        {
            if (PlayerData.instance.health <= 0 || HeroController.instance.cState.dead || !GameManager.instance.IsGameplayScene())
            {
                Console.AddLine("Unacceptable conditions for taking health" + PlayerData.instance.health + "," + DebugMod.HC.cState.dead + "," + DebugMod.GM.IsGameplayScene() + "," + DebugMod.HC.cState.recoiling + "," + DebugMod.GM.IsGamePaused() + "," + DebugMod.HC.cState.invulnerable + ")." + " Pressed too many times at once?");
                return;
            }
            HeroController.instance.TakeHealth(1);

            Console.AddLine("Attempting to take health");
        }
        
        [BindableMethod(name = "Add Silk", category = "Masks & Spools")]
        public static void AddSilk()
        {
            HeroController.instance.AddSilk(1, true);

            Console.AddLine("Added Silk");
        }

        [BindableMethod(name = "Take Silk", category = "Masks & Spools")]
        public static void TakeSilk()
        {
            HeroController.instance.TakeSilk(1);

            Console.AddLine("Attempting to take silk");
        }

        [BindableMethod(name = "Add Lifeblood", category = "Masks & Spools")]
        public static void Lifeblood()
        {
            EventRegister.SendEvent("ADD BLUE HEALTH");

            Console.AddLine("Attempting to add lifeblood");
        }
    }
}
