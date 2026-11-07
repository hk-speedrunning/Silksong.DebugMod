namespace DebugMod;

public static partial class BindableFunctions
{
    [BindableMethod(name = "SKILLS_GIVEALLSKILLS", category = "CATEGORY_SKILLS")]
    public static void GiveAllSkills()
    {
        PlayerData.instance.hasDash = true;
        PlayerData.instance.canDash = true;
        PlayerData.instance.hasWalljump = true;
        PlayerData.instance.canWallJump = true;
        PlayerData.instance.hasSuperDash = true;
        PlayerData.instance.canSuperDash = true;
        PlayerData.instance.hasDoubleJump = true;
        PlayerData.instance.hasAcidArmour = true;
        PlayerData.instance.hasShadowDash = true;
        PlayerData.instance.canShadowDash = true;

        PlayerData.instance.hasDreamNail = true;
        PlayerData.instance.hasDreamGate = true;
        PlayerData.instance.dreamNailUpgraded = true;

        PlayerData.instance.hasCyclone = true;
        PlayerData.instance.hasUpwardSlash = true;
        PlayerData.instance.hasDashSlash = true;
        UpdateNailArts();

        PlayerData.instance.fireballLevel = 2;
        PlayerData.instance.quakeLevel = 2;
        PlayerData.instance.screamLevel = 2;
        PlayerData.instance.hasSpell = true;

        PlayerData.instance.nailUpgrades = 4;
        DebugMod.extraNailDamage = 0;

        DebugMod.LogConsole("Giving player all skills and upgrades");
    }

    [BindableMethod(name = "HK_SKILLS_TOGGLEMOTHWINGCLOAK", category = "CATEGORY_SKILLS")]
    public static void ToggleMothwingCloak()
    {
        if (!PlayerData.instance.hasDash)
        {
            PlayerData.instance.hasDash = true;
            PlayerData.instance.canDash = true;
            DebugMod.LogConsole("Giving player Mothwing Cloak");
        }
        else
        {
            PlayerData.instance.hasDash = false;
            PlayerData.instance.canDash = false;
            DebugMod.LogConsole("Taking away Mothwing Cloak");
        }
    }

    [BindableMethod(name = "HK_SKILLS_TOGGLEMANTISCLAW", category = "CATEGORY_SKILLS")]
    public static void ToggleMantisClaw()
    {
        if (!PlayerData.instance.hasWalljump)
        {
            PlayerData.instance.hasWalljump = true;
            PlayerData.instance.canWallJump = true;
            DebugMod.LogConsole("Giving player Mantis Claw");
        }
        else
        {
            PlayerData.instance.hasWalljump = false;
            PlayerData.instance.canWallJump = false;
            DebugMod.LogConsole("Taking away Mantis Claw");
        }
    }

    [BindableMethod(name = "HK_SKILLS_TOGGLECRYSTALHEART", category = "CATEGORY_SKILLS")]
    public static void ToggleCrystalHeart()
    {
        if (!PlayerData.instance.hasSuperDash)
        {
            PlayerData.instance.hasSuperDash = true;
            PlayerData.instance.canSuperDash = true;
            DebugMod.LogConsole("Giving player Crystal Heart");
        }
        else
        {
            PlayerData.instance.hasSuperDash = false;
            PlayerData.instance.canSuperDash = false;
            DebugMod.LogConsole("Taking away Crystal Heart");
        }
    }

    [BindableMethod(name = "HK_SKILLS_TOGGLEMONARCHWINGS", category = "CATEGORY_SKILLS")]
    public static void ToggleMonarchWings()
    {
        if (!PlayerData.instance.hasDoubleJump)
        {
            PlayerData.instance.hasDoubleJump = true;
            DebugMod.LogConsole("Giving player Monarch Wings");
        }
        else
        {
            PlayerData.instance.hasDoubleJump = false;
            DebugMod.LogConsole("Taking away Monarch Wings");
        }
    }

    [BindableMethod(name = "HK_SKILLS_TOGGLEISMASTEAR", category = "CATEGORY_SKILLS")]
    public static void ToggleIsmasTear()
    {
        if (!PlayerData.instance.hasAcidArmour)
        {
            PlayerData.instance.hasAcidArmour = true;
            DebugMod.LogConsole("Giving player Isma's Tear");
        }
        else
        {
            PlayerData.instance.hasAcidArmour = false;
            DebugMod.LogConsole("Taking away Isma's Tear");
        }
    }

    [BindableMethod(name = "HK_SKILLS_TOGGLESHADECLOAK", category = "CATEGORY_SKILLS")]
    public static void ToggleShadeCloak()
    {
        if (!PlayerData.instance.hasShadowDash)
        {
            PlayerData.instance.hasShadowDash = true;
            PlayerData.instance.canShadowDash = true;
            DebugMod.LogConsole("Giving player Shade Cloak");
        }
        else
        {
            PlayerData.instance.hasShadowDash = false;
            PlayerData.instance.canShadowDash = false;
            DebugMod.LogConsole("Taking away Shade Cloak");
        }
    }

    [BindableMethod(name = "HK_SKILLS_TOGGLEDREAMNAIL", category = "CATEGORY_SKILLS")]
    public static void ToggleDreamNail()
    {
        if (!PlayerData.instance.hasDreamNail)
        {
            PlayerData.instance.hasDreamNail = true;
            DebugMod.LogConsole("Giving player Dream Nail");
        }
        else
        {
            PlayerData.instance.hasDreamNail = false;
            DebugMod.LogConsole("Taking away Dream Nail");
        }
    }

    [BindableMethod(name = "HK_SKILLS_TOGGLEDREAMGATE", category = "CATEGORY_SKILLS")]
    public static void ToggleDreamGate()
    {
        if (!PlayerData.instance.hasDreamGate)
        {
            PlayerData.instance.hasDreamGate = true;
            DebugMod.LogConsole("Giving player Dream Gate");
        }
        else
        {
            PlayerData.instance.hasDreamGate = false;
            DebugMod.LogConsole("Taking away Dream Gate");
        }
    }

    [BindableMethod(name = "HK_SKILLS_TOGGLEAWOKENDREAMNAIL", category = "CATEGORY_SKILLS")]
    public static void ToggleAwokenDreamNail()
    {
        if (!PlayerData.instance.dreamNailUpgraded)
        {
            PlayerData.instance.dreamNailUpgraded = true;
            DebugMod.LogConsole("Giving player Awoken Dream Nail");
        }
        else
        {
            PlayerData.instance.dreamNailUpgraded = false;
            DebugMod.LogConsole("Taking away Awoken Dream Nail");
        }
    }

    [BindableMethod(name = "HK_SKILLS_TOGGLECYCLONESLASH", category = "CATEGORY_SKILLS")]
    public static void ToggleCycloneSlash()
    {
        if (!PlayerData.instance.hasCyclone)
        {
            PlayerData.instance.hasCyclone = true;
            UpdateNailArts();
            DebugMod.LogConsole("Giving player Cyclone Slash");
        }
        else
        {
            PlayerData.instance.hasCyclone = false;
            UpdateNailArts();
            DebugMod.LogConsole("Taking away Cyclone Slash");
        }
    }

    [BindableMethod(name = "HK_SKILLS_TOGGLEDASHSLASH", category = "CATEGORY_SKILLS")]
    public static void ToggleDashSlash()
    {
        if (!PlayerData.instance.hasUpwardSlash)
        {
            PlayerData.instance.hasUpwardSlash = true;
            UpdateNailArts();
            DebugMod.LogConsole("Giving player Dash Slash");
        }
        else
        {
            PlayerData.instance.hasUpwardSlash = false;
            UpdateNailArts();
            DebugMod.LogConsole("Taking away Dash Slash");
        }
    }

    [BindableMethod(name = "HK_SKILLS_TOGGLEGREATSLASH", category = "CATEGORY_SKILLS")]
    public static void ToggleGreatSlash()
    {
        if (!PlayerData.instance.hasDashSlash)
        {
            PlayerData.instance.hasDashSlash = true;
            UpdateNailArts();
            DebugMod.LogConsole("Giving player Great Slash");
        }
        else
        {
            PlayerData.instance.hasDashSlash = false;
            UpdateNailArts();
            DebugMod.LogConsole("Taking away Great Slash");
        }
    }

    private static void UpdateNailArts()
    {
        PlayerData pd = PlayerData.instance;
        pd.hasNailArt = pd.hasCyclone || pd.hasUpwardSlash || pd.hasDashSlash;
        pd.hasAllNailArts = pd.hasCyclone && pd.hasUpwardSlash && pd.hasDashSlash;
    }
}
