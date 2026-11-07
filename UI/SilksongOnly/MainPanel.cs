using DebugMod.Helpers;
using DebugMod.UI.Canvas;
using GlobalSettings;
using System;
using System.Collections.Generic;

namespace DebugMod.UI;

public record struct ToolDef(string IconKey, ToolItem Item)
{
    public string IconKey = IconKey;
    public ToolItem Item = Item;
}

public partial class MainPanel
{
    partial void AppendSkillsAndUpgrades()
    {
        AppendSectionHeader("CATEGORY_SKILLS");
        AppendRow(1);
        AppendBasicControl("SKILLS_ALLSKILLS", BindableFunctions.GiveAllSkills);
        AppendTileRow(5);
        AppendLabeledTile("SKILLS_SWIFTSTEP", () => PlayerData.instance.hasDash, BindableFunctions.ToggleSwiftStep, "Skill_SwiftStep");
        AppendLabeledTile("SKILLS_CLINGGRIP", () => PlayerData.instance.hasWalljump, BindableFunctions.ToggleClingGrip, "Skill_ClingGrip");
        AppendLabeledTile("SKILLS_NEEDOLIN", () => PlayerData.instance.hasNeedolin, BindableFunctions.ToggleNeedolin, "Skill_Needolin");
        AppendLabeledTile("SKILLS_CLAWLINE", () => PlayerData.instance.hasHarpoonDash, BindableFunctions.ToggleClawline, "Skill_Clawline");
        AppendLabeledTile("SKILLS_SILKSOAR", () => PlayerData.instance.hasSuperJump, BindableFunctions.ToggleSilkSoar, "Skill_SilkSoar");
        AppendTileRow(5);
        AppendLabeledTile("SKILLS_DRIFTERSCLOAK", () => PlayerData.instance.hasBrolly, BindableFunctions.ToggleDriftersCloak, "Skill_DriftersCloak");
        AppendLabeledTile("SKILLS_FAYDOWNCLOAK", () => PlayerData.instance.hasDoubleJump, BindableFunctions.ToggleFaydownCloak, "Skill_FaydownCloak");
        AppendLabeledTile("SKILLS_NEEDLESTRIKE", () => PlayerData.instance.hasChargeSlash, BindableFunctions.ToggleNeedleStrike, "Skill_NeedleStrike");
        AppendLabeledTile("SKILLS_BEASTLINGCALL", () => PlayerData.instance.UnlockedFastTravelTeleport, BindableFunctions.ToggleBeastlingCall, "Skill_BeastlingCall");
        AppendLabeledTile("SKILLS_ELEGYOFTHEDEEP", () => PlayerData.instance.hasNeedolinMemoryPowerup, BindableFunctions.ToggleElegyOfTheDeep, "Skill_Elegy");

        AppendSectionHeader("CATEGORY_UPGRADES");
        AppendTileRow(2);
        AppendIncrementTile("UPGRADES_NEEDLEDAMAGE", () => PlayerData.instance.nailDamage, SetNailDamage, "Inv_Needle",
            customAdd: BindableFunctions.IncreaseNeedleDamage, customRemove: BindableFunctions.DecreaseNeedleDamage);
        static void SetNailDamage(int value)
        {
            int nailUpgrades = Math.Clamp((value - 5) / 4, 0, 4);
            PlayerData.instance.nailUpgrades = nailUpgrades;
            DebugMod.extraNailDamage = value - (nailUpgrades * 4 + 5);
        }
        AppendIncrementTile("UPGRADES_SILKHEARTS", () => PlayerData.instance.silkRegenMax,
            value => PlayerData.instance.silkRegenMax = value, "Inv_SilkHeart");
        AppendTileRow(2);
        AppendIncrementTile("UPGRADES_CRAFTINGKIT", () => PlayerData.instance.ToolKitUpgrades,
            value => PlayerData.instance.ToolKitUpgrades = value, "Inv_CraftingKit", max: 4, wrap: true);
        AppendIncrementTile("UPGRADES_TOOLPOUCH", () => PlayerData.instance.ToolPouchUpgrades,
            value => PlayerData.instance.ToolPouchUpgrades = value, "Inv_ToolPouch", max: 4, wrap: true);
    }

    partial void AppendEquippableItems()
    {
        AppendSectionHeader("CATEGORY_CRESTS");
        AppendRow(1, 1);
        AppendBasicControl("CRESTS_UNLOCKALLCRESTS", BindableFunctions.UnlockAllCrests);
        AppendToggleControl("CRESTS_FREEEQUIP", () => DebugMod.freeEquip, BindableFunctions.FreeEquip);

        static void ToggleCrest(ToolCrest crest)
        {
            if (crest.IsUnlocked)
            {
                ToolCrestsData.Data data = crest.SaveData;
                data.IsUnlocked = false;
                crest.SaveData = data;

                if (PlayerData.instance.CurrentCrestID == crest.name)
                {
                    // Choose an unlocked crest and equip it
                    Utils.AutoEquipCrest(null, false);
                }
            }
            else
            {
                crest.Unlock();
            }
        }

        AppendRow(1, 1);
        AppendToggleControl("CRESTS_TOGGLECURSED", () => Gameplay.CursedCrest.IsEquipped, BindableFunctions.ToggleCursed);
        AppendToggleControl("CRESTS_TOGGLECLOAKLESS", () => Gameplay.CloaklessCrest.IsEquipped, BindableFunctions.ToggleCloakless);

        List<string> hunterTiers = ["Hunter", "Hunter_v2", "Hunter_v3"];
        ToolCrest hunterCrest = ToolItemManager.GetCrestByName(hunterTiers[0]);

        CanvasPanel hunterTile = null;
        hunterTile = AppendLabeledTile(
            "CRESTS_HUNTER",
            () =>
            {
                for (int i = hunterTiers.Count - 1; i >= 0; i--)
                {
                    ToolCrest crest = ToolItemManager.GetCrestByName(hunterTiers[i]);
                    if (crest.IsUnlocked)
                    {
                        hunterTile.Get<CanvasImage>("Icon").SetImage(UICommon.images[$"Crest_{hunterTiers[i]}"]);
                        return true;
                    }
                }

                hunterTile.Get<CanvasImage>("Icon").SetImage(UICommon.images["Crest_Hunter"]);
                return false;
            },
            () =>
            {
                ToolCrest next = null;
                bool cycleToFirst = true;

                foreach (string tier in hunterTiers)
                {
                    ToolCrest crest = ToolItemManager.GetCrestByName(tier);
                    if (!crest.IsUnlocked)
                    {
                        ToggleCrest(crest);
                        next = crest;
                        cycleToFirst = false;
                        break;
                    }
                }

                if (cycleToFirst)
                {
                    // Cycle back to base hunter crest

                    foreach (string tier in hunterTiers)
                    {
                        ToolCrest crest = ToolItemManager.GetCrestByName(tier);
                        ToggleCrest(crest);
                    }

                    ToggleCrest(hunterCrest);
                    next = hunterCrest;
                }

                if (PlayerData.instance.CurrentCrestID.StartsWith("Hunter"))
                {
                    Utils.AutoEquipCrest(next, false);
                }
            },
            includeLabel: false,
            defaultRowWidth: 4
        );

        List<(string, string)> regularCrests = [
            ("Reaper", "CRESTS_REAPER"),
            ("Wanderer", "CRESTS_WANDERER"),
            ("Warrior", "CRESTS_BEAST"),
            ("Witch", "CRESTS_WITCH"),
            ("Toolmaster", "CRESTS_ARCHITECT"),
            ("Spell", "CRESTS_SHAMAN")
        ];

        foreach ((string name, string displayName) in regularCrests)
        {
            ToolCrest crest = ToolItemManager.GetCrestByName(name);

            CanvasPanel tile = AppendLabeledTile(
                displayName,
                () => crest.IsUnlocked,
                () => ToggleCrest(crest),
                includeLabel: false,
                defaultRowWidth: 4
            );

            tile.Get<CanvasImage>("Icon").SetImage(UICommon.images[$"Crest_{name}"]);
        }

        CanvasPanel vesticrestTile = null;
        vesticrestTile = AppendLabeledTile("CRESTS_VESTICREST",
            () =>
            {
                vesticrestTile.Get<CanvasImage>("Icon")?.SetImage(PlayerData.instance.UnlockedExtraBlueSlot
                    ? UICommon.images["Inv_Vesticrest2"]
                    : UICommon.images["Inv_Vesticrest"]);
                return PlayerData.instance.UnlockedExtraYellowSlot;
            },
            () =>
            {
                if (!PlayerData.instance.UnlockedExtraYellowSlot)
                {
                    PlayerData.instance.UnlockedExtraYellowSlot = true;
                }
                else if (!PlayerData.instance.UnlockedExtraBlueSlot)
                {
                    PlayerData.instance.UnlockedExtraBlueSlot = true;
                }
                else
                {
                    PlayerData.instance.UnlockedExtraYellowSlot = false;
                    PlayerData.instance.UnlockedExtraBlueSlot = false;
                }
            },
            includeLabel: false,
            defaultRowWidth: 4,
            image: "Inv_Vesticrest"
        );

        AppendSectionHeader("CATEGORY_TOOLS");
        AppendRow(1, 1);
        AppendBasicControl("TOOLS_UNLOCKALLTOOLS", BindableFunctions.UnlockAllTools);
        AppendBasicControl("TOOLS_CRAFTTOOLS", BindableFunctions.CraftTools);

        Dictionary<string, List<ToolDef>> tools = new()
        {
            // Spells
            {"Silk Spear", [new ToolDef("Spell_SilkSpear", ToolItemManager.GetToolByName("Silk Spear"))]},
            {"Thread Sphere", [new ToolDef("Spell_ThreadStorm", ToolItemManager.GetToolByName("Thread Sphere"))]},
            {"Parry", [new ToolDef("Spell_CrossStitch", ToolItemManager.GetToolByName("Parry"))]},
            {"Silk Charge", [new ToolDef("Spell_Sharpdart", ToolItemManager.GetToolByName("Silk Charge"))]},
            {"Silk Bomb", [new ToolDef("Spell_RuneRage", ToolItemManager.GetToolByName("Silk Bomb"))]},
            {"Silk Boss Needle", [new ToolDef("Spell_PaleNails", ToolItemManager.GetToolByName("Silk Boss Needle"))]},
            // Red
            {"Straight Pin", [new ToolDef("Tool_StraightPin", ToolItemManager.GetToolByName("Straight Pin"))]},
            {"Tri Pin", [new ToolDef("Tool_ThreefoldPin", ToolItemManager.GetToolByName("Tri Pin"))]},
            {"Sting Shard", [new ToolDef("Tool_StingShard", ToolItemManager.GetToolByName("Sting Shard"))]},
            {"Tack", [new ToolDef("Tool_Tacks", ToolItemManager.GetToolByName("Tack"))]},
            {"Harpoon", [new ToolDef("Tool_Longpin", ToolItemManager.GetToolByName("Harpoon"))]},
            {"Curve Claws", [
                new ToolDef("Tool_Curveclaw", ToolItemManager.GetToolByName("Curve Claws")),
                new ToolDef("Tool_Curveclaw2", ToolItemManager.GetToolByName("Curve Claws Upgraded"))
            ]},
            {"Shakra Ring", [new ToolDef("Tool_ThrowingRing", ToolItemManager.GetToolByName("Shakra Ring"))]},
            {"Pimpilo", [new ToolDef("Tool_Pimpillo", ToolItemManager.GetToolByName("Pimpilo"))]},
            {"Conch Drill", [new ToolDef("Tool_Conchcutter", ToolItemManager.GetToolByName("Conch Drill"))]},
            {"WebShot", [
                new ToolDef("Tool_Silkshot_Weaver", ToolItemManager.GetToolByName("WebShot Weaver")),
                new ToolDef("Tool_Silkshot_Forge", ToolItemManager.GetToolByName("WebShot Forge")),
                new ToolDef("Tool_Silkshot_Architect", ToolItemManager.GetToolByName("WebShot Architect")),
            ]},
            {"Screw Attack", [new ToolDef("Tool_DelversDrill", ToolItemManager.GetToolByName("Screw Attack"))]},
            {"Cogwork Saw", [new ToolDef("Tool_CogworkWheel", ToolItemManager.GetToolByName("Cogwork Saw"))]},
            {"Cogwork Flier", [new ToolDef("Tool_Cogfly", ToolItemManager.GetToolByName("Cogwork Flier"))]},
            {"Rosary Cannon", [new ToolDef("Tool_RosaryCannon", ToolItemManager.GetToolByName("Rosary Cannon"))]},
            {"Lightning Rod", [new ToolDef("Tool_Voltvessels", ToolItemManager.GetToolByName("Lightning Rod"))]},
            {"Flintstone", [new ToolDef("Tool_Flintslate", ToolItemManager.GetToolByName("Flintstone"))]},
            {"Silk Snare", [new ToolDef("Tool_SnareSetter", ToolItemManager.GetToolByName("Silk Snare"))]},
            {"Flea Brew", [new ToolDef("Tool_FleaBrew", ToolItemManager.GetToolByName("Flea Brew"))]},
            {"Lifeblood Syringe", [new ToolDef("Tool_PlasmiumPhial", ToolItemManager.GetToolByName("Lifeblood Syringe"))]},
            {"Extractor", [new ToolDef("Tool_NeedlePhial", ToolItemManager.GetToolByName("Extractor"))]},
            // Blue
            {"Mosscreep Tool", [
                new ToolDef("Tool_DruidsEye", ToolItemManager.GetToolByName("Mosscreep Tool 1")),
                new ToolDef("Tool_DruidsEye2", ToolItemManager.GetToolByName("Mosscreep Tool 2"))
            ]},
            {"Lava Charm", [new ToolDef("Tool_MagmaBell", ToolItemManager.GetToolByName("Lava Charm"))]},
            {"Bell Bind", [new ToolDef("Tool_WardingBell", ToolItemManager.GetToolByName("Bell Bind"))]},
            {"Poison Pouch", [new ToolDef("Tool_PollipPouch", ToolItemManager.GetToolByName("Poison Pouch"))]},
            {"Fractured Mask", [new ToolDef("Tool_FracturedMask", ToolItemManager.GetToolByName("Fractured Mask"))]},
            {"Multibind", [new ToolDef("Tool_Multibinder", ToolItemManager.GetToolByName("Multibind"))]},
            {"White Ring", [new ToolDef("Tool_Weavelight", ToolItemManager.GetToolByName("White Ring"))]},
            {"Brolly Spike", [new ToolDef("Tool_SawtoothCirclet", ToolItemManager.GetToolByName("Brolly Spike"))]},
            {"Quickbind", [new ToolDef("Tool_InjectorBand", ToolItemManager.GetToolByName("Quickbind"))]},
            {"Spool Extender", [new ToolDef("Tool_SpoolExtender", ToolItemManager.GetToolByName("Spool Extender"))]},
            {"Reserve Bind", [new ToolDef("Tool_ReserveBind", ToolItemManager.GetToolByName("Reserve Bind"))]},
            {"Dazzle Bind", [
                new ToolDef("Tool_ClawMirror", ToolItemManager.GetToolByName("Dazzle Bind")),
                new ToolDef("Tool_ClawMirror2", ToolItemManager.GetToolByName("Dazzle Bind Upgraded"))
            ] },
            {"Revenge Crystal", [new ToolDef("Tool_MemoryCrystal", ToolItemManager.GetToolByName("Revenge Crystal"))]},
            {"Thief Claw", [new ToolDef("Tool_SnitchPick", ToolItemManager.GetToolByName("Thief Claw"))]},
            {"Zap Imbuement", [new ToolDef("Tool_VoltFilament", ToolItemManager.GetToolByName("Zap Imbuement"))]},
            {"Quick Sling", [new ToolDef("Tool_QuickSling", ToolItemManager.GetToolByName("Quick Sling"))]},
            {"Maggot Charm", [new ToolDef("Tool_WreathOfPurity", ToolItemManager.GetToolByName("Maggot Charm"))]},
            {"Longneedle", [new ToolDef("Tool_Longclaw", ToolItemManager.GetToolByName("Longneedle"))]},
            {"Wisp Lantern", [new ToolDef("Tool_WispfireLantern", ToolItemManager.GetToolByName("Wisp Lantern"))]},
            {"Flea Charm", [new ToolDef("Tool_EggOfFlealia", ToolItemManager.GetToolByName("Flea Charm"))]},
            {"Pinstress Tool", [new ToolDef("Tool_PinBadge", ToolItemManager.GetToolByName("Pinstress Tool"))]},
            // Yellow
            {"Compass", [new ToolDef("Tool_Compass", ToolItemManager.GetToolByName("Compass"))]},
            {"Bone Necklace", [new ToolDef("Tool_ShardPendant", ToolItemManager.GetToolByName("Bone Necklace"))]},
            {"Rosary Magnet", [new ToolDef("Tool_MagnetiteBrooch", ToolItemManager.GetToolByName("Rosary Magnet"))]},
            {"Weighted Anklet", [new ToolDef("Tool_WeightedBelt", ToolItemManager.GetToolByName("Weighted Anklet"))]},
            {"Barbed Wire", [new ToolDef("Tool_BarbedBracelet", ToolItemManager.GetToolByName("Barbed Wire"))]},
            {"Dead Mans Purse", [
                new ToolDef("Tool_DeadBugsPurse", ToolItemManager.GetToolByName("Dead Mans Purse")),
                new ToolDef("Tool_ShellSatchel", ToolItemManager.GetToolByName("Shell Satchel"))
            ]},
            {"Magnetite Dice", [new ToolDef("Tool_MagnetiteDice", ToolItemManager.GetToolByName("Magnetite Dice"))]},
            {"Scuttlebrace", [new ToolDef("Tool_Scuttlebrace", ToolItemManager.GetToolByName("Scuttlebrace"))]},
            {"Wallcling", [new ToolDef("Tool_AscendantsGrip", ToolItemManager.GetToolByName("Wallcling"))]},
            {"Musician Charm", [new ToolDef("Tool_SpiderStrings", ToolItemManager.GetToolByName("Musician Charm"))]},
            {"Sprintmaster", [new ToolDef("Tool_SilkspeedAnklets", ToolItemManager.GetToolByName("Sprintmaster"))]},
            {"Thief Charm", [new ToolDef("Tool_ThiefsMark", ToolItemManager.GetToolByName("Thief Charm"))]},
        };

        static void ToggleTool(ToolItem tool)
        {
            if (tool.IsUnlockedNotHidden)
            {
                tool.Lock();
            }
            else
            {
                tool.Unlock(null, ToolItem.PopupFlags.None);
            }
        }

        void AppendToolTile(string key, List<ToolDef> toolDefs, int width)
        {
            if (toolDefs.Count == 1)
            {
                ToolDef tool = toolDefs[0];

                AppendLabeledTile(
                    key,
                    () => tool.Item.IsUnlockedNotHidden,
                    () => ToggleTool(tool.Item),
                    image: tool.IconKey,
                    includeLabel: false,
                    defaultRowWidth: width
                );
            }
            else
            {
                ToolDef firstTool = toolDefs[0];

                CanvasPanel tile = null;
                tile = AppendLabeledTile(
                    key,
                    () =>
                    {
                        foreach (ToolDef tool in toolDefs)
                        {
                            if (tool.Item.IsUnlockedNotHidden)
                            {
                                tile.Get<CanvasImage>("Icon").SetImage(UICommon.images[tool.IconKey]);
                                return true;
                            }
                        }

                        tile.Get<CanvasImage>("Icon").SetImage(UICommon.images[firstTool.IconKey]);
                        return false;
                    },
                    () =>
                    {
                        for (int i = 0; i < toolDefs.Count; i++)
                        {
                            ToolItem current = toolDefs[i].Item;
                            if (current.IsUnlockedNotHidden)
                            {
                                ToggleTool(current);
                                if (i < toolDefs.Count - 1)
                                {
                                    ToggleTool(toolDefs[i + 1].Item);
                                }
                                return;
                            }
                        }

                        ToggleTool(firstTool.Item);
                    },
                    includeLabel: false,
                    defaultRowWidth: width
                );
            }
        }

        foreach (var (ToolKey, ToolDefs) in tools)
        {
            AppendToolTile(ToolKey, ToolDefs, 6);
        }
    }
}
