namespace DebugMod
{
    public static partial class BindableFunctions
    {
        [BindableMethod(name = "Give Mask", category = "Mask & Vessels")]
        public static void GiveMask()
        {
            if (PlayerData.instance.maxHealthBase < 9)
            {
                HeroController.instance.MaxHealth();
                HeroController.instance.AddToMaxHealth(1);
                PlayMakerFSM.BroadcastEvent("MAX HP UP");
                Console.AddLine("Added Mask");
            }
            else
            {
                Console.AddLine("You have the maximum number of masks");
            }
        }
        
        [BindableMethod(name = "Give Vessel", category = "Mask & Vessels")]
        public static void GiveVessel()
        {
            if (PlayerData.instance.silkMax < 18)
            {
                HeroController.instance.AddToMaxSilk(1);
                PlayMakerFSM.BroadcastEvent("NEW SOUL ORB");
                Console.AddLine("Added Vessel");
            }
            else
            {
                Console.AddLine("You have the maximum number of vessels");
            }
        }
        
        [BindableMethod(name = "Take Away Mask", category = "Mask & Vessels")]
        public static void TakeAwayMask()
        {
            if (PlayerData.instance.maxHealthBase > 1)
            {
                PlayerData.instance.maxHealth -= 1;
                PlayerData.instance.maxHealthBase -= 1;
                // if (!GameCameras.instance.hudCanvas.gameObject.activeInHierarchy)
                //     GameCameras.instance.hudCanvas.gameObject.SetActive(true);
                // else
                // {
                //     GameCameras.instance.hudCanvas.gameObject.SetActive(false);
                //     GameCameras.instance.hudCanvas.gameObject.SetActive(true);
                // }
                Console.AddLine("Took Away Mask");
            }
            else
            {
                Console.AddLine("You have the minimum number of masks");
            }
        }
        
        [BindableMethod(name = "Take Away Vessel", category = "Mask & Vessels")]
        public static void TakeAwayVessel()
        {
            if (PlayerData.instance.silkMax > 9)
            {
                PlayerData.instance.silkMax--;
                // if (!GameCameras.instance.hudCanvas.gameObject.activeInHierarchy)
                //     GameCameras.instance.hudCanvas.gameObject.SetActive(true);
                // else
                // {
                //     GameCameras.instance.hudCanvas.gameObject.SetActive(false);
                //     GameCameras.instance.hudCanvas.gameObject.SetActive(true);
                // }
                Console.AddLine("Removed Vessel");
            }
            else
            {
                Console.AddLine("You have the minimum number of vessels");
            }
        }

        
        [BindableMethod(name = "Add Health", category = "Mask & Vessels")]
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
        [BindableMethod(name = "Take Health", category = "Mask & Vessels")]
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
        
        [BindableMethod(name = "Add Soul", category = "Mask & Vessels")]
        public static void AddSoul()
        {
            HeroController.instance.AddSilk(1, true);

            Console.AddLine("Added Soul");
        }
        [BindableMethod(name = "Take Soul", category = "Mask & Vessels")]
        public static void TakeSoul()
        {
            HeroController.instance.TakeSilk(1);

            Console.AddLine("Attempting to take soul");
        }
        [BindableMethod(name = "Add Lifeblood", category = "Mask & Vessels")]
        public static void Lifeblood()
        {
            EventRegister.SendEvent("ADD BLUE HEALTH");

            Console.AddLine("Attempting to add lifeblood");
        }
    }
}
