namespace DebugMod
{
    public static partial class BindableFunctions
    {
        [BindableMethod(name = "Give All", category = "Skills")]
        public static void GiveAllSkills()
        {
            PlayerData.instance.silkRegenMax = 3;

            PlayerData.instance.hasDash = true;
            PlayerData.instance.hasBrolly = true;
            PlayerData.instance.hasWalljump = true;
            PlayerData.instance.hasHarpoonDash = true;
            PlayerData.instance.hasDoubleJump = true;
            PlayerData.instance.hasSuperJump = true;

            PlayerData.instance.hasNeedolin = true;
            PlayerData.instance.UnlockedFastTravelTeleport = true;
            PlayerData.instance.hasNeedolinMemoryPowerup = true;

            PlayerData.instance.hasChargeSlash = true;

            Console.AddLine("Giving player all skills");
        }

        [BindableMethod(name = "Give Silk Heart", category = "Skills")]
        public static void IncrementSilkHeart()
        {
            if (PlayerData.instance.silkRegenMax < 3)
            {
                PlayerData.instance.silkRegenMax++;
                Console.AddLine($"Giving player Silk Heart (now {PlayerData.instance.silkRegenMax})");
            }
            else
            {
                PlayerData.instance.silkRegenMax = 0;
                Console.AddLine("Taking away all Silk Hearts");
            }
        }

        [BindableMethod(name = "Give Swift Step", category = "Skills")]
        public static void ToggleSwiftStep()
        {
            if (!PlayerData.instance.hasDash)
            {
                PlayerData.instance.hasDash = true;
                Console.AddLine("Giving player Swift Step");
            }
            else
            {
                PlayerData.instance.hasDash = false;
                Console.AddLine("Taking away Swift Step");
            }
        }

        [BindableMethod(name = "Increment Cloak", category = "Skills")]
        public static void IncrementCloak()
        {
            if (!PlayerData.instance.hasBrolly && !PlayerData.instance.hasDoubleJump)
            {
                PlayerData.instance.hasBrolly = true;
                Console.AddLine("Giving player Drifter's Cloak");
            }
            else if (PlayerData.instance.hasBrolly && !PlayerData.instance.hasDoubleJump)
            {
                PlayerData.instance.hasDoubleJump = true;
                Console.AddLine("Giving player Faydown Cloak");
            }
            else
            {
                PlayerData.instance.hasBrolly = false;
                PlayerData.instance.hasDoubleJump = false;
                Console.AddLine("Taking away cloak upgrades");
            }
        }

        [BindableMethod(name = "Give Cling Grip", category = "Skills")]
        public static void ToggleClingGrip()
        {
            if (!PlayerData.instance.hasWalljump)
            {
                PlayerData.instance.hasWalljump = true;
                Console.AddLine("Giving player Cling Grip");
            }
            else
            {
                PlayerData.instance.hasWalljump = false;
                Console.AddLine("Taking away Cling Grip");
            }
        }

        [BindableMethod(name = "Give Needolin", category = "Skills")]
        public static void ToggleNeedolin()
        {
            if (!PlayerData.instance.hasNeedolin)
            {
                PlayerData.instance.hasNeedolin = true;
                Console.AddLine("Giving player Needolin");
            }
            else
            {
                PlayerData.instance.hasNeedolin = false;
                PlayerData.instance.UnlockedFastTravelTeleport = false;
                PlayerData.instance.hasNeedolinMemoryPowerup = false;
                Console.AddLine("Taking away Needolin and any upgrades");
            }
        }

        [BindableMethod(name = "Give Clawline", category = "Skills")]
        public static void ToggleClawline()
        {
            if (!PlayerData.instance.hasHarpoonDash)
            {
                PlayerData.instance.hasHarpoonDash = true;
                Console.AddLine("Giving player Clawline");
            }
            else
            {
                PlayerData.instance.hasHarpoonDash = false;
                Console.AddLine("Taking away Clawline");
            }
        }

        [BindableMethod(name = "Give Silk Soar", category = "Skills")]
        public static void ToggleSilkSoar()
        {
            if (!PlayerData.instance.hasSuperJump)
            {
                PlayerData.instance.hasSuperJump = true;
                Console.AddLine("Giving player Silk Soar");
            }
            else
            {
                PlayerData.instance.hasSuperJump = false;
                Console.AddLine("Taking away Silk Soar");
            }
        }

        [BindableMethod(name = "Give Beastling Call", category = "Skills")]
        public static void ToggleBeastlingCall()
        {
            if (!PlayerData.instance.hasNeedolin && !PlayerData.instance.UnlockedFastTravelTeleport)
            {
                PlayerData.instance.hasNeedolin = true;
                PlayerData.instance.UnlockedFastTravelTeleport = true;
                Console.AddLine("Giving player Needolin with Beastling Call");
            }
            else if (PlayerData.instance.hasNeedolin && !PlayerData.instance.UnlockedFastTravelTeleport)
            {
                PlayerData.instance.UnlockedFastTravelTeleport = true;
                Console.AddLine("Giving player Beastling Call");
            }
            else
            {
                PlayerData.instance.UnlockedFastTravelTeleport = false;
                Console.AddLine("Taking away Beastling Call");
            }
        }

        [BindableMethod(name = "Give Elegy of the Deep", category = "Skills")]
        public static void ToggleElegyOfTheDeep()
        {
            if (!PlayerData.instance.hasNeedolin && !PlayerData.instance.hasNeedolinMemoryPowerup)
            {
                PlayerData.instance.hasNeedolin = true;
                PlayerData.instance.hasNeedolinMemoryPowerup = true;
                Console.AddLine("Giving player Needolin with Elegy of the Deep");
            }
            else if (PlayerData.instance.hasNeedolin && !PlayerData.instance.hasNeedolinMemoryPowerup)
            {
                PlayerData.instance.hasNeedolinMemoryPowerup = true;
                Console.AddLine("Giving player Elegy of the Deep");
            }
            else
            {
                PlayerData.instance.hasNeedolinMemoryPowerup = false;
                Console.AddLine("Taking away Elegy of the Deep");
            }
        }

        [BindableMethod(name = "Give Needle Strike", category = "Skills")]
        public static void ToggleNeedleStrike()
        {
            if (!PlayerData.instance.hasChargeSlash)
            {
                PlayerData.instance.hasChargeSlash = true;
                Console.AddLine("Giving player Needle Strike");
            }
            else
            {
                PlayerData.instance.hasChargeSlash = false;
                Console.AddLine("Taking away Needle Strike");
            }
        }
    }
}