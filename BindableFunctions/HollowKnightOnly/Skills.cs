namespace DebugMod;

public static partial class BindableFunctions
{
    [BindableMethod(name = "SKILLS_GIVEALLSKILLS", category = "CATEGORY_SKILLS")]
    public static void GiveAllSkills()
    {
        PlayerData.instance.SetBool(nameof(PlayerData.hasDash), true);
        PlayerData.instance.SetBool(nameof(PlayerData.canDash), true);
        PlayerData.instance.SetBool(nameof(PlayerData.hasWalljump), true);
        PlayerData.instance.SetBool(nameof(PlayerData.canWallJump), true);
        PlayerData.instance.SetBool(nameof(PlayerData.hasSuperDash), true);
        PlayerData.instance.SetBool(nameof(PlayerData.canSuperDash), true);
        PlayerData.instance.SetBool(nameof(PlayerData.hasDoubleJump), true);
        PlayerData.instance.SetBool(nameof(PlayerData.hasAcidArmour), true);
        PlayerData.instance.SetBool(nameof(PlayerData.hasShadowDash), true);
        PlayerData.instance.SetBool(nameof(PlayerData.canShadowDash), true);

        PlayerData.instance.SetBool(nameof(PlayerData.hasDreamNail), true);
        PlayerData.instance.SetBool(nameof(PlayerData.hasDreamGate), true);
        PlayerData.instance.SetBool(nameof(PlayerData.dreamNailUpgraded), true);

        PlayerData.instance.SetBool(nameof(PlayerData.hasCyclone), true);
        PlayerData.instance.SetBool(nameof(PlayerData.hasUpwardSlash), true);
        PlayerData.instance.SetBool(nameof(PlayerData.hasDashSlash), true);
        UpdateNailArts();

        PlayerData.instance.SetInt(nameof(PlayerData.fireballLevel), 2);
        PlayerData.instance.SetInt(nameof(PlayerData.quakeLevel), 2);
        PlayerData.instance.SetInt(nameof(PlayerData.screamLevel), 2);
        PlayerData.instance.SetBool(nameof(PlayerData.hasSpell), true);

        PlayerData.instance.nailUpgrades = 4;
        DebugMod.extraNailDamage = 0;

        DebugMod.LogConsole("Giving player all skills and upgrades");
    }

    [BindableMethod(name = "HK_SKILLS_TOGGLEMOTHWINGCLOAK", category = "CATEGORY_SKILLS")]
    public static void ToggleMothwingCloak()
    {
        if (!PlayerData.instance.hasDash)
        {
            PlayerData.instance.SetBool(nameof(PlayerData.hasDash), true);
            PlayerData.instance.SetBool(nameof(PlayerData.canDash), true);
            DebugMod.LogConsole("Giving player Mothwing Cloak");
        }
        else
        {
            PlayerData.instance.SetBool(nameof(PlayerData.hasDash), false);
            PlayerData.instance.SetBool(nameof(PlayerData.canDash), false);
            DebugMod.LogConsole("Taking away Mothwing Cloak");
        }
    }

    [BindableMethod(name = "HK_SKILLS_TOGGLEMANTISCLAW", category = "CATEGORY_SKILLS")]
    public static void ToggleMantisClaw()
    {
        if (!PlayerData.instance.hasWalljump)
        {
            PlayerData.instance.SetBool(nameof(PlayerData.hasWalljump), true);
            PlayerData.instance.SetBool(nameof(PlayerData.canWallJump), true);
            DebugMod.LogConsole("Giving player Mantis Claw");
        }
        else
        {
            PlayerData.instance.SetBool(nameof(PlayerData.hasWalljump), false);
            PlayerData.instance.SetBool(nameof(PlayerData.canWallJump), false);
            DebugMod.LogConsole("Taking away Mantis Claw");
        }
    }

    [BindableMethod(name = "HK_SKILLS_TOGGLECRYSTALHEART", category = "CATEGORY_SKILLS")]
    public static void ToggleCrystalHeart()
    {
        if (!PlayerData.instance.hasSuperDash)
        {
            PlayerData.instance.SetBool(nameof(PlayerData.hasSuperDash), true);
            PlayerData.instance.SetBool(nameof(PlayerData.canSuperDash), true);
            DebugMod.LogConsole("Giving player Crystal Heart");
        }
        else
        {
            PlayerData.instance.SetBool(nameof(PlayerData.hasSuperDash), false);
            PlayerData.instance.SetBool(nameof(PlayerData.canSuperDash), false);
            DebugMod.LogConsole("Taking away Crystal Heart");
        }
    }

    [BindableMethod(name = "HK_SKILLS_TOGGLEMONARCHWINGS", category = "CATEGORY_SKILLS")]
    public static void ToggleMonarchWings()
    {
        if (!PlayerData.instance.hasDoubleJump)
        {
            PlayerData.instance.SetBool(nameof(PlayerData.hasDoubleJump), true);
            DebugMod.LogConsole("Giving player Monarch Wings");
        }
        else
        {
            PlayerData.instance.SetBool(nameof(PlayerData.hasDoubleJump), false);
            DebugMod.LogConsole("Taking away Monarch Wings");
        }
    }

    [BindableMethod(name = "HK_SKILLS_TOGGLEISMASTEAR", category = "CATEGORY_SKILLS")]
    public static void ToggleIsmasTear()
    {
        if (!PlayerData.instance.hasAcidArmour)
        {
            PlayerData.instance.SetBool(nameof(PlayerData.hasAcidArmour), true);
            DebugMod.LogConsole("Giving player Isma's Tear");
        }
        else
        {
            PlayerData.instance.SetBool(nameof(PlayerData.hasAcidArmour), false);
            DebugMod.LogConsole("Taking away Isma's Tear");
        }
    }

    [BindableMethod(name = "HK_SKILLS_TOGGLESHADECLOAK", category = "CATEGORY_SKILLS")]
    public static void ToggleShadeCloak()
    {
        if (!PlayerData.instance.hasShadowDash)
        {
            PlayerData.instance.SetBool(nameof(PlayerData.hasShadowDash), true);
            PlayerData.instance.SetBool(nameof(PlayerData.canShadowDash), true);
            DebugMod.LogConsole("Giving player Shade Cloak");
        }
        else
        {
            PlayerData.instance.SetBool(nameof(PlayerData.hasShadowDash), false);
            PlayerData.instance.SetBool(nameof(PlayerData.canShadowDash), false);
            DebugMod.LogConsole("Taking away Shade Cloak");
        }
    }

    [BindableMethod(name = "HK_SKILLS_TOGGLEDREAMNAIL", category = "CATEGORY_SKILLS")]
    public static void ToggleDreamNail()
    {
        if (!PlayerData.instance.hasDreamNail)
        {
            PlayerData.instance.SetBool(nameof(PlayerData.hasDreamNail), true);
            DebugMod.LogConsole("Giving player Dream Nail");
        }
        else
        {
            PlayerData.instance.SetBool(nameof(PlayerData.hasDreamNail), false);
            DebugMod.LogConsole("Taking away Dream Nail");
        }
    }

    [BindableMethod(name = "HK_SKILLS_TOGGLEDREAMGATE", category = "CATEGORY_SKILLS")]
    public static void ToggleDreamGate()
    {
        if (!PlayerData.instance.hasDreamGate)
        {
            PlayerData.instance.SetBool(nameof(PlayerData.hasDreamGate), true);
            DebugMod.LogConsole("Giving player Dream Gate");
        }
        else
        {
            PlayerData.instance.SetBool(nameof(PlayerData.hasDreamGate), false);
            DebugMod.LogConsole("Taking away Dream Gate");
        }
    }

    [BindableMethod(name = "HK_SKILLS_TOGGLEAWOKENDREAMNAIL", category = "CATEGORY_SKILLS")]
    public static void ToggleAwokenDreamNail()
    {
        if (!PlayerData.instance.dreamNailUpgraded)
        {
            PlayerData.instance.SetBool(nameof(PlayerData.dreamNailUpgraded), true);
            DebugMod.LogConsole("Giving player Awoken Dream Nail");
        }
        else
        {
            PlayerData.instance.SetBool(nameof(PlayerData.dreamNailUpgraded), false);
            DebugMod.LogConsole("Taking away Awoken Dream Nail");
        }
    }

    [BindableMethod(name = "HK_SKILLS_TOGGLECYCLONESLASH", category = "CATEGORY_SKILLS")]
    public static void ToggleCycloneSlash()
    {
        if (!PlayerData.instance.hasCyclone)
        {
            PlayerData.instance.SetBool(nameof(PlayerData.hasCyclone), true);
            UpdateNailArts();
            DebugMod.LogConsole("Giving player Cyclone Slash");
        }
        else
        {
            PlayerData.instance.SetBool(nameof(PlayerData.hasCyclone), false);
            UpdateNailArts();
            DebugMod.LogConsole("Taking away Cyclone Slash");
        }
    }

    [BindableMethod(name = "HK_SKILLS_TOGGLEDASHSLASH", category = "CATEGORY_SKILLS")]
    public static void ToggleDashSlash()
    {
        if (!PlayerData.instance.hasUpwardSlash)
        {
            PlayerData.instance.SetBool(nameof(PlayerData.hasUpwardSlash), true);
            UpdateNailArts();
            DebugMod.LogConsole("Giving player Dash Slash");
        }
        else
        {
            PlayerData.instance.SetBool(nameof(PlayerData.hasUpwardSlash), false);
            UpdateNailArts();
            DebugMod.LogConsole("Taking away Dash Slash");
        }
    }

    [BindableMethod(name = "HK_SKILLS_TOGGLEGREATSLASH", category = "CATEGORY_SKILLS")]
    public static void ToggleGreatSlash()
    {
        if (!PlayerData.instance.hasDashSlash)
        {
            PlayerData.instance.SetBool(nameof(PlayerData.hasDashSlash), true);
            UpdateNailArts();
            DebugMod.LogConsole("Giving player Great Slash");
        }
        else
        {
            PlayerData.instance.SetBool(nameof(PlayerData.hasDashSlash), false);
            UpdateNailArts();
            DebugMod.LogConsole("Taking away Great Slash");
        }
    }

    private static void UpdateNailArts()
    {
        PlayerData pd = PlayerData.instance;
        pd.SetBool(nameof(PlayerData.hasNailArt), pd.hasCyclone || pd.hasUpwardSlash || pd.hasDashSlash);
        pd.SetBool(nameof(PlayerData.hasAllNailArts), pd.hasCyclone && pd.hasUpwardSlash && pd.hasDashSlash);
    }
}
