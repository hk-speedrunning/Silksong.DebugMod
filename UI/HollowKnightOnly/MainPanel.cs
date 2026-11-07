using DebugMod.UI.Canvas;
using System;

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
        AppendSpellTile("HK_SKILLS_FIREBALL", "Spell_Fireball", () => PlayerData.instance.fireballLevel,
            value => PlayerData.instance.fireballLevel = value);
        AppendSpellTile("HK_SKILLS_QUAKE", "Spell_Quake", () => PlayerData.instance.quakeLevel,
            value => PlayerData.instance.quakeLevel = value);
        AppendSpellTile("HK_SKILLS_SCREAM", "Spell_Scream", () => PlayerData.instance.screamLevel,
            value => PlayerData.instance.screamLevel = value);
        void AppendSpellTile(string name, string image, Func<int> getter, Action<int> setter)
        {
            CanvasPanel tile = AppendIncrementTile(name, getter, value => { setter(value); UpdateHasSpell(); },
                $"{image}_1", max: 2);
            tile.OnUpdate += () => tile.Get<CanvasImage>("Icon").SetImage(UICommon.images[$"{image}_{Math.Max(getter(), 1)}"]);
        }
        static void UpdateHasSpell()
        {
            PlayerData pd = PlayerData.instance;
            pd.hasSpell = pd.fireballLevel > 0 || pd.quakeLevel > 0 || pd.screamLevel > 0;
        }

        AppendSectionHeader("CATEGORY_UPGRADES");
        AppendTileRow(2);
        CanvasPanel nailTile = AppendIncrementTile("UPGRADES_NEEDLEDAMAGE", () => PlayerData.instance.nailDamage, SetNailDamage, "Nail_0",
            customAdd: BindableFunctions.IncreaseNeedleDamage, customRemove: BindableFunctions.DecreaseNeedleDamage);
        nailTile.OnUpdate += () => nailTile.Get<CanvasImage>("Icon").SetImage(UICommon.images[$"Nail_{PlayerData.instance.nailUpgrades}"]);
        static void SetNailDamage(int value)
        {
            int nailUpgrades = Math.Clamp((value - 5) / 4, 0, 4);
            PlayerData.instance.nailUpgrades = nailUpgrades;
            PlayerData.instance.extraNailDamage = value - (nailUpgrades * 4 + 5);
        }
        AppendIncrementTile("HK_UPGRADES_NOTCHES", () => PlayerData.instance.charmSlots,
            value => PlayerData.instance.charmSlots = value, "Charm_Notch", min: 3, max: 11);
    }

    partial void AppendEquippableItems()
    {
        AppendSectionHeader("HK_CATEGORY_CHARMS");
        AppendRow(1);
        AppendBasicControl("HK_CHARMS_GIVEALLCHARMS", BindableFunctions.GiveAllCharms);

        // Inventory grid order, from InvCharmBackboard.charmNum
        int[] charmOrder = [
            2, 1, 4, 20, 19, 21, 31, 37, 3, 35,
            23, 24, 25, 33, 14, 15, 32, 18, 13, 6,
            12, 5, 11, 10, 22, 7, 34, 8, 9, 27,
            29, 17, 16, 28, 26, 39, 30, 38, 40, 36,
        ];
        foreach (int charm in charmOrder)
        {
            // Indexed by BindableFunctions.GetCharmState
            string[] stateImages = charm switch
            {
                23 or 24 or 25 => [$"Charm_{charm}", $"Charm_{charm}_Broken", $"Charm_{charm}_Unbreakable"],
                36 => ["Charm_36", "Charm_36_1", "Charm_36_2", "Charm_36_3", "Charm_36_4"],
                40 => ["Charm_40", "Charm_40_2", "Charm_40_3", "Charm_40_4", "Charm_40_5"],
                _ => [$"Charm_{charm}"],
            };

            if (stateImages.Length == 1)
            {
                AppendLabeledTile($"HK_CHARMS_{charm}", () => PlayerData.instance.GetBool($"gotCharm_{charm}"),
                    () => BindableFunctions.ToggleCharm(charm), image: $"Charm_{charm}", includeLabel: false, defaultRowWidth: 10);
            }
            else
            {
                CanvasPanel tile = null;
                tile = AppendLabeledTile($"HK_CHARMS_{charm}",
                    () =>
                    {
                        tile.Get<CanvasImage>("Icon").SetImage(UICommon.images[stateImages[BindableFunctions.GetCharmState(charm)]]);
                        return PlayerData.instance.GetBool($"gotCharm_{charm}");
                    },
                    () => BindableFunctions.CycleCharm(charm, stateImages.Length), image: $"Charm_{charm}", includeLabel: false, defaultRowWidth: 10);
            }
        }
    }
}
