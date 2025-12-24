namespace DebugMod;

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

        DebugMod.LogConsole("Giving player all skills");
    }

    [BindableMethod(name = "Give Silk Heart", category = "Skills")]
    public static void IncrementSilkHeart()
    {
        if (PlayerData.instance.silkRegenMax < 3)
        {
            PlayerData.instance.silkRegenMax++;
            DebugMod.LogConsole($"Giving player Silk Heart (now {PlayerData.instance.silkRegenMax})");
        }
        else
        {
            PlayerData.instance.silkRegenMax = 0;
            DebugMod.LogConsole("Taking away all Silk Hearts");
        }
    }

    [BindableMethod(name = "Give Swift Step", category = "Skills")]
    public static void ToggleSwiftStep()
    {
        if (!PlayerData.instance.hasDash)
        {
            PlayerData.instance.hasDash = true;
            DebugMod.LogConsole("Giving player Swift Step");
        }
        else
        {
            PlayerData.instance.hasDash = false;
            DebugMod.LogConsole("Taking away Swift Step");
        }
    }

    [BindableMethod(name = "Increment Cloak", category = "Skills")]
    public static void IncrementCloak()
    {
        if (!PlayerData.instance.hasBrolly && !PlayerData.instance.hasDoubleJump)
        {
            PlayerData.instance.hasBrolly = true;
            DebugMod.LogConsole("Giving player Drifter's Cloak");
        }
        else if (PlayerData.instance.hasBrolly && !PlayerData.instance.hasDoubleJump)
        {
            PlayerData.instance.hasDoubleJump = true;
            DebugMod.LogConsole("Giving player Faydown Cloak");
        }
        else
        {
            PlayerData.instance.hasBrolly = false;
            PlayerData.instance.hasDoubleJump = false;
            DebugMod.LogConsole("Taking away cloak upgrades");
        }
    }

    [BindableMethod(name = "Give Cling Grip", category = "Skills")]
    public static void ToggleClingGrip()
    {
        if (!PlayerData.instance.hasWalljump)
        {
            PlayerData.instance.hasWalljump = true;
            DebugMod.LogConsole("Giving player Cling Grip");
        }
        else
        {
            PlayerData.instance.hasWalljump = false;
            DebugMod.LogConsole("Taking away Cling Grip");
        }
    }

    [BindableMethod(name = "Give Needolin", category = "Skills")]
    public static void ToggleNeedolin()
    {
        if (!PlayerData.instance.hasNeedolin)
        {
            PlayerData.instance.hasNeedolin = true;
            DebugMod.LogConsole("Giving player Needolin");
        }
        else
        {
            PlayerData.instance.hasNeedolin = false;
            PlayerData.instance.UnlockedFastTravelTeleport = false;
            PlayerData.instance.hasNeedolinMemoryPowerup = false;
            DebugMod.LogConsole("Taking away Needolin and any upgrades");
        }
    }

    [BindableMethod(name = "Give Clawline", category = "Skills")]
    public static void ToggleClawline()
    {
        if (!PlayerData.instance.hasHarpoonDash)
        {
            PlayerData.instance.hasHarpoonDash = true;
            DebugMod.LogConsole("Giving player Clawline");
        }
        else
        {
            PlayerData.instance.hasHarpoonDash = false;
            DebugMod.LogConsole("Taking away Clawline");
        }
    }

    [BindableMethod(name = "Give Silk Soar", category = "Skills")]
    public static void ToggleSilkSoar()
    {
        if (!PlayerData.instance.hasSuperJump)
        {
            PlayerData.instance.hasSuperJump = true;
            DebugMod.LogConsole("Giving player Silk Soar");
        }
        else
        {
            PlayerData.instance.hasSuperJump = false;
            DebugMod.LogConsole("Taking away Silk Soar");
        }
    }

    [BindableMethod(name = "Give Beastling Call", category = "Skills")]
    public static void ToggleBeastlingCall()
    {
        if (!PlayerData.instance.hasNeedolin && !PlayerData.instance.UnlockedFastTravelTeleport)
        {
            PlayerData.instance.hasNeedolin = true;
            PlayerData.instance.UnlockedFastTravelTeleport = true;
            DebugMod.LogConsole("Giving player Needolin with Beastling Call");
        }
        else if (PlayerData.instance.hasNeedolin && !PlayerData.instance.UnlockedFastTravelTeleport)
        {
            PlayerData.instance.UnlockedFastTravelTeleport = true;
            DebugMod.LogConsole("Giving player Beastling Call");
        }
        else
        {
            PlayerData.instance.UnlockedFastTravelTeleport = false;
            DebugMod.LogConsole("Taking away Beastling Call");
        }
    }

    [BindableMethod(name = "Give Elegy of the Deep", category = "Skills")]
    public static void ToggleElegyOfTheDeep()
    {
        if (!PlayerData.instance.hasNeedolin && !PlayerData.instance.hasNeedolinMemoryPowerup)
        {
            PlayerData.instance.hasNeedolin = true;
            PlayerData.instance.hasNeedolinMemoryPowerup = true;
            DebugMod.LogConsole("Giving player Needolin with Elegy of the Deep");
        }
        else if (PlayerData.instance.hasNeedolin && !PlayerData.instance.hasNeedolinMemoryPowerup)
        {
            PlayerData.instance.hasNeedolinMemoryPowerup = true;
            DebugMod.LogConsole("Giving player Elegy of the Deep");
        }
        else
        {
            PlayerData.instance.hasNeedolinMemoryPowerup = false;
            DebugMod.LogConsole("Taking away Elegy of the Deep");
        }
    }

    [BindableMethod(name = "Give Needle Strike", category = "Skills")]
    public static void ToggleNeedleStrike()
    {
        if (!PlayerData.instance.hasChargeSlash)
        {
            PlayerData.instance.hasChargeSlash = true;
            DebugMod.LogConsole("Giving player Needle Strike");
        }
        else
        {
            PlayerData.instance.hasChargeSlash = false;
            DebugMod.LogConsole("Taking away Needle Strike");
        }
    }
}