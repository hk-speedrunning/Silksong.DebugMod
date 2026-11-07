namespace DebugMod.UI;

public partial class MainPanel
{
    partial void AppendSkillsAndUpgrades()
    {
        AppendSectionHeader("CATEGORY_SKILLS");
        AppendRow(1);
        AppendBasicControl("SKILLS_ALLSKILLS", BindableFunctions.GiveAllSkills);
        AppendTileRow(5);
        AppendLabeledTile("HK_SKILLS_MOTHWINGCLOAK", () => PlayerData.instance.hasDash, BindableFunctions.ToggleMothwingCloak, "MothwingCloak");
        AppendLabeledTile("HK_SKILLS_MANTISCLAW", () => PlayerData.instance.hasWalljump, BindableFunctions.ToggleMantisClaw, "MantisClaw");
        AppendLabeledTile("HK_SKILLS_CRYSTALHEART", () => PlayerData.instance.hasSuperDash, BindableFunctions.ToggleCrystalHeart, "CrystalHeart");
        AppendLabeledTile("HK_SKILLS_MONARCHWINGS", () => PlayerData.instance.hasDoubleJump, BindableFunctions.ToggleMonarchWings, "MonarchWings");
        AppendLabeledTile("HK_SKILLS_ISMASTEAR", () => PlayerData.instance.hasAcidArmour, BindableFunctions.ToggleIsmasTear, "IsmasTear");
        AppendTileRow(5);
        AppendLabeledTile("HK_SKILLS_SHADECLOAK", () => PlayerData.instance.hasShadowDash, BindableFunctions.ToggleShadeCloak, "ShadeCloak");
        AppendLabeledTile("HK_SKILLS_DREAMNAIL", () => PlayerData.instance.hasDreamNail, BindableFunctions.ToggleDreamNail, "DreamNail1");
        AppendLabeledTile("HK_SKILLS_DREAMGATE", () => PlayerData.instance.hasDreamGate, BindableFunctions.ToggleDreamGate, "DreamGate");
        AppendLabeledTile("HK_SKILLS_AWOKENDREAMNAIL", () => PlayerData.instance.dreamNailUpgraded, BindableFunctions.ToggleAwokenDreamNail, "DreamNail2");
        AppendLabeledTile("HK_SKILLS_CYCLONESLASH", () => PlayerData.instance.hasCyclone, BindableFunctions.ToggleCycloneSlash, "NailArt_CycloneSlash");
        AppendTileRow(5);
        AppendLabeledTile("HK_SKILLS_DASHSLASH", () => PlayerData.instance.hasUpwardSlash, BindableFunctions.ToggleDashSlash, "NailArt_DashSlash");
        AppendLabeledTile("HK_SKILLS_GREATSLASH", () => PlayerData.instance.hasDashSlash, BindableFunctions.ToggleGreatSlash, "NailArt_GreatSlash");
        AppendTileRow(3);
        AppendIncrementTile("HK_SKILLS_FIREBALL", () => PlayerData.instance.fireballLevel,
            value => { PlayerData.instance.fireballLevel = value; UpdateHasSpell(); }, max: 2);
        AppendIncrementTile("HK_SKILLS_QUAKE", () => PlayerData.instance.quakeLevel,
            value => { PlayerData.instance.quakeLevel = value; UpdateHasSpell(); }, max: 2);
        AppendIncrementTile("HK_SKILLS_SCREAM", () => PlayerData.instance.screamLevel,
            value => { PlayerData.instance.screamLevel = value; UpdateHasSpell(); }, max: 2);
        static void UpdateHasSpell()
        {
            PlayerData pd = PlayerData.instance;
            pd.hasSpell = pd.fireballLevel > 0 || pd.quakeLevel > 0 || pd.screamLevel > 0;
        }
    }
}
