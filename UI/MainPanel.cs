using DebugMod.Helpers;
using DebugMod.MonoBehaviours;
using DebugMod.UI.Canvas;
using GlobalSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DebugMod.UI;

public class MainPanel : CanvasPanel
{
    public static int TabButtonHeight => UICommon.ScaleHeight(20);
    public static int ScrollbarWidth => UICommon.ScaleWidth(11);
    public static int SectionEndPadding => UICommon.ScaleHeight(20);
    public static int SectionHeaderFontSize => UICommon.ScaleHeight(30);
    public static int SectionHeaderHeight => UICommon.ScaleHeight(30);
    public static int KeybindHeaderFontSize => UICommon.ScaleHeight(20);
    public static int ListingHeight => UICommon.ScaleHeight(16);

    public static MainPanel Instance { get; private set; }

    private static readonly List<string> keybindCategoryOrder =
    [
        "Cheats",
        "Savestates",
        "Mod UI",
        "Time",
        "Enemies",
        "Skills",
        "Upgrades",
        "Tools",
        "Consumables",
        "Masks & Spools",
        "Visual",
        "Misc",
    ];

    private readonly List<CanvasPanel> tabs = [];

    // Convenience fields for building
    private PanelBuilder currentTab;
    private CanvasPanel currentRow;
    private int rowCounter;
    private int[] rowPositions;
    private int[] rowWidths;
    private int rowIndex;
    private int lastRowCount;

    public static void BuildPanel()
    {
        Instance = new MainPanel();
        Instance.Build();
    }

    public MainPanel() : base(nameof(MainPanel))
    {
        LocalPosition = new Vector2(Screen.width - UICommon.ScreenMargin - UICommon.RightSideWidth, UICommon.ScreenMargin);
        Size = new Vector2(UICommon.RightSideWidth, UICommon.MainPanelHeight);
        OnUpdate += DoUpdate;

        AddTab("MAINPANEL_TAB_GAMEPLAY");

        AppendSectionHeader("GAMEPLAY_SECTION_CHEATS");
        AppendRow(1, 1, 1);
        AppendToggleControl("GAMEPLAY_CHEATS_NOCLIP", () => DebugMod.noclip, BindableFunctions.ToggleNoclip);
        AppendToggleControl("GAMEPLAY_CHEATS_INVINCIBILITY", () => DebugMod.playerInvincible, BindableFunctions.ToggleInvincibility);
        AppendToggleControl("GAMEPLAY_CHEATS_INFINITEJUMP", () => PlayerData.instance.infiniteAirJump, BindableFunctions.ToggleInfiniteJump);
        AppendRow(1, 1, 1);
        AppendToggleControl("GAMEPLAY_CHEATS_INFINITEHP", () => DebugMod.infiniteHP, BindableFunctions.ToggleInfiniteHP);
        AppendToggleControl("GAMEPLAY_CHEATS_INFINITESILK", () => DebugMod.infiniteSilk, BindableFunctions.ToggleInfiniteSilk);
        AppendToggleControl("GAMEPLAY_CHEATS_INFINITETOOLS", () => DebugMod.infiniteTools, BindableFunctions.ToggleInfiniteTools);
        AppendRow(2, 1);
        AppendToggleControl("GAMEPLAY_CHEATS_TOGGLEHEROCOLLIDER", () => DebugMod.heroColliderDisabled, BindableFunctions.ToggleHeroCollider);
        AppendBasicControl("GAMEPLAY_CHEATS_KILLALL", BindableFunctions.KillAll);

        AppendSectionHeader("GAMEPLAY_SECTION_MODUI");
        AppendRow(1, 1);
        AppendToggleControl("GAMEPLAY_MODUI_TOGGLEALLUI", () => true, BindableFunctions.ToggleAllPanels);
        AppendToggleControl("GAMEPLAY_MODUI_TOGGLEMAINPANEL", () => DebugMod.settings.MainPanelVisible, BindableFunctions.ToggleMainPanel);
        AppendRow(1, 1);
        AppendToggleControl("GAMEPLAY_MODUI_TOGGLEENEMIESPANEL", () => DebugMod.settings.EnemiesPanelVisible, BindableFunctions.ToggleEnemiesPanel);
        AppendToggleControl("GAMEPLAY_MODUI_TOGGLECONSOLEPANEL", () => DebugMod.settings.ConsoleVisible, BindableFunctions.ToggleConsolePanel);
        AppendRow(1, 1);
        AppendToggleControl("GAMEPLAY_MODUI_TOGGLESAVESTATESPANEL", () => DebugMod.settings.SaveStatePanelVisible, BindableFunctions.ToggleSaveStatePanel);
        AppendToggleControl("GAMEPLAY_MODUI_EXPANDCOLLAPSESAVESTATES", () => DebugMod.settings.SaveStatePanelExpanded, BindableFunctions.ToggleExpandedSaveStatePanel);
        AppendRow(1, 1);
        AppendToggleControl("GAMEPLAY_MODUI_TOGGLEINFOPANEL", () => DebugMod.settings.InfoPanelVisible, BindableFunctions.ToggleInfoPanel);
        AppendToggleControl("GAMEPLAY_MODUI_ALWAYSSHOWCURSOR", () => DebugMod.settings.ShowCursorWhileUnpaused, BindableFunctions.ToggleAlwaysShowCursor);

        AppendSectionHeader("GAMEPLAY_SECTION_TIME");
        AppendRow(1, 1);
        AppendBasicControl("GAMEPLAY_TIME_INCREASETIMESCALE", BindableFunctions.TimescaleUp);
        AppendBasicControl("GAMEPLAY_TIME_DECREASETIMESCALE", BindableFunctions.TimescaleDown);
        AppendRow(1, 1);
        AppendToggleControl("GAMEPLAY_TIME_FREEZEGAME", () => TimeScale.Frozen, BindableFunctions.PauseGameNoUI);
        AppendBasicControl("GAMEPLAY_TIME_ADVANCEFRAME", BindableFunctions.AdvanceFrame);
        AppendRow(1, 1);
        AppendToggleControl("GAMEPLAY_TIME_FORCEPAUSE", () =>
            DebugMod.forcePaused && GameManager.instance.isPaused, BindableFunctions.ForcePause);
        AppendBasicControl("GAMEPLAY_TIME_RESETFRAMECOUNTER", BindableFunctions.ResetFrameCounter);

        AppendSectionHeader("GAMEPLAY_SECTION_VISUAL");
        AppendRow(1, 1);
        AppendToggleControl("GAMEPLAY_VISUAL_TOGGLEHITBOXES", () => DebugMod.settings.ShowHitBoxes != 0, BindableFunctions.ShowHitboxes);
        AppendToggleControl("GAMEPLAY_VISUAL_FORCECAMERAFOLLOW", () => DebugMod.cameraFollow, BindableFunctions.ForceCameraFollow);
        AppendRow(1, 1);
        AppendToggleControl("GAMEPLAY_VISUAL_PREVIEWCOCOONPOSITION", CocoonPreviewToggled, BindableFunctions.PreviewCocoonPosition);
        static bool CocoonPreviewToggled()
        {
            CocoonPreviewer previewer = GameManager.instance.GetComponent<CocoonPreviewer>();
            if (!previewer) return false;
            return previewer.previewEnabled;
        }

        AppendToggleControl("GAMEPLAY_VISUAL_TOGGLEHEROLIGHT", HeroLightToggled, BindableFunctions.ToggleHeroLight);
        static bool HeroLightToggled()
        {
            // Null propagation doesn't work on Unity objects
            GameObject hero = DebugMod.RefKnight;
            if (!hero) return false;
            Transform heroLight = hero.transform.Find("HeroLight");
            if (!heroLight) return false;
            SpriteRenderer renderer = heroLight.GetComponent<SpriteRenderer>();
            if (!renderer) return false;
            return Math.Abs(renderer.color.a) == 0;
        }
        AppendRow(1, 1, 1);
        AppendToggleControl("GAMEPLAY_VISUAL_TOGGLEHUD", HUDToggled, BindableFunctions.ToggleHUD);
        static bool HUDToggled()
        {
            PlayMakerFSM hud = GameCameras.instance.hudCanvasSlideOut;
            if (!hud) return false;
            return !hud.gameObject.activeInHierarchy;
        }
        AppendToggleControl("GAMEPLAY_VISUAL_TOGGLEVIGNETTE", () => VisualMaskHelper.vignetteDisabled, BindableFunctions.ToggleVignette);
        AppendToggleControl("GAMEPLAY_VISUAL_HIDEHERO", HideHeroToggled, BindableFunctions.HideHero);
        static bool HideHeroToggled()
        {
            GameObject hero = DebugMod.RefKnight;
            if (!hero) return false;
            tk2dSprite sprite = hero.GetComponent<tk2dSprite>();
            if (!sprite) return false;
            return Math.Abs(sprite.color.a) == 0;
        }
        AppendRow(1, 1);
        AppendToggleControl("GAMEPLAY_VISUAL_TOGGLECAMERASHAKE", CameraShakeToggled, BindableFunctions.ToggleCameraShake);
        static bool CameraShakeToggled()
        {
            PlayMakerFSM cameraShake = GameCameras.instance.cameraShakeFSM;
            if (!cameraShake) return false;
            return !cameraShake.enabled;
        }
        AppendToggleControl("GAMEPLAY_VISUAL_DEACTIVATEVISUALMASKS", () => VisualMaskHelper.masksDisabled, BindableFunctions.DoDeactivateVisualMasks);
        AppendRow(1, 1, 1);
        AppendBasicControl("GAMEPLAY_VISUAL_ZOOMIN", BindableFunctions.ZoomIn);
        AppendBasicControl("GAMEPLAY_VISUAL_ZOOMOUT", BindableFunctions.ZoomOut);
        AppendBasicControl("GAMEPLAY_VISUAL_RESETZOOM", BindableFunctions.ResetZoom);

        AppendSectionHeader("GAMEPLAY_SECTION_MISC");
        AppendRow(1, 1);
        AppendBasicControl("GAMEPLAY_MISC_SETHAZARDRESPAWN", BindableFunctions.SetHazardRespawn);
        AppendBasicControl("GAMEPLAY_MISC_HAZARDRESPAWN", BindableFunctions.Respawn);
        AppendRow(1, 1, 1);
        AppendBasicControl("GAMEPLAY_MISC_DAMAGESELF", BindableFunctions.SelfDamage);
        AppendBasicControl("GAMEPLAY_MISC_KILLSELF", BindableFunctions.KillSelf);
        AppendBasicControl("GAMEPLAY_MISC_BREAKCOCOON", BindableFunctions.BreakCocoon);
        AppendRow(1, 1);
        AppendBasicControl("GAMEPLAY_MISC_RESETCURRENTSCENEDATA", BindableFunctions.ResetCurrentScene);
        AppendBasicControl("GAMEPLAY_MISC_BLOCKSCENEDATACHANGES", BindableFunctions.BlockCurrentSceneChanges);
        AppendRow(1, 1, 1);
        AppendToggleControl("GAMEPLAY_MISC_TOGGLEACT3", () => PlayerData.instance.blackThreadWorld, BindableFunctions.ToggleAct3);
        AppendToggleControl("GAMEPLAY_MISC_LOCKKEYBINDS", () => DebugMod.KeyBindLock, BindableFunctions.ToggleLockKeyBinds);
        AppendBasicControl("GAMEPLAY_MISC_RESETALL", BindableFunctions.Reset);

        AddTab("MAINPANEL_TAB_ITEMS");

        AppendSectionHeader("ITEMS_SECTION_SKILLS");
        AppendRow(1);
        AppendBasicControl("ITEMS_SKILLS_ALLSKILLS", BindableFunctions.GiveAllSkills);
        AppendTileRow(5);
        AppendLabeledTile("ITEMS_SKILLS_SWIFTSTEP", () => PlayerData.instance.hasDash, BindableFunctions.ToggleSwiftStep, "Skill_SwiftStep");
        AppendLabeledTile("ITEMS_SKILLS_CLINGGRIP", () => PlayerData.instance.hasWalljump, BindableFunctions.ToggleClingGrip, "Skill_ClingGrip");
        AppendLabeledTile("ITEMS_SKILLS_NEEDOLIN", () => PlayerData.instance.hasNeedolin, BindableFunctions.ToggleNeedolin, "Skill_Needolin");
        AppendLabeledTile("ITEMS_SKILLS_CLAWLINE", () => PlayerData.instance.hasHarpoonDash, BindableFunctions.ToggleClawline, "Skill_Clawline");
        AppendLabeledTile("ITEMS_SKILLS_SILKSOAR", () => PlayerData.instance.hasSuperJump, BindableFunctions.ToggleSilkSoar, "Skill_SilkSoar");
        AppendTileRow(5);
        AppendLabeledTile("ITEMS_SKILLS_DRIFTERSCLOAK", () => PlayerData.instance.hasBrolly, BindableFunctions.ToggleDriftersCloak, "Skill_DriftersCloak");
        AppendLabeledTile("ITEMS_SKILLS_FAYDOWNCLOAK", () => PlayerData.instance.hasDoubleJump, BindableFunctions.ToggleFaydownCloak, "Skill_FaydownCloak");
        AppendLabeledTile("ITEMS_SKILLS_NEEDLESTRIKE", () => PlayerData.instance.hasChargeSlash, BindableFunctions.ToggleNeedleStrike, "Skill_NeedleStrike");
        AppendLabeledTile("ITEMS_SKILLS_BEASTLINGCALL", () => PlayerData.instance.UnlockedFastTravelTeleport, BindableFunctions.ToggleBeastlingCall, "Skill_BeastlingCall");
        AppendLabeledTile("ITEMS_SKILLS_ELEGYOFTHEDEEP", () => PlayerData.instance.hasNeedolinMemoryPowerup, BindableFunctions.ToggleElegyOfTheDeep, "Skill_Elegy");

        AppendSectionHeader("ITEMS_SECTION_UPGRADES");
        AppendTileRow(2);
        AppendIncrementTile("ITEMS_UPGRADES_NEEDLEDAMAGE", () => PlayerData.instance.nailDamage, SetNailDamage, "Inv_Needle",
            customAdd: BindableFunctions.IncreaseNeedleDamage, customRemove: BindableFunctions.DecreaseNeedleDamage);
        static void SetNailDamage(int value)
        {
            int nailUpgrades = Math.Clamp((value - 5) / 4, 0, 4);
            PlayerData.instance.nailUpgrades = nailUpgrades;
            DebugMod.extraNailDamage = value - (nailUpgrades * 4 + 5);
        }
        AppendIncrementTile("ITEMS_UPGRADES_SILKHEARTS", () => PlayerData.instance.silkRegenMax,
            value => PlayerData.instance.silkRegenMax = value, "Inv_SilkHeart", max: 3, wrap: true);
        AppendTileRow(2);
        AppendIncrementTile("ITEMS_UPGRADES_CRAFTINGKIT", () => PlayerData.instance.ToolKitUpgrades,
            value => PlayerData.instance.ToolKitUpgrades = value, "Inv_CraftingKit", max: 4, wrap: true);
        AppendIncrementTile("ITEMS_UPGRADES_TOOLPOUCH", () => PlayerData.instance.ToolPouchUpgrades,
            value => PlayerData.instance.ToolPouchUpgrades = value, "Inv_ToolPouch", max: 4, wrap: true);
        AppendRow(1, 1);
        AppendBasicControl("ITEMS_UPGRADES_ALLMAPS", BindableFunctions.UnlockAllMaps);
        AppendBasicControl("ITEMS_UPGRADES_ALLFASTTRAVEL", BindableFunctions.UnlockAllFastTravel);

        AppendSectionHeader("ITEMS_SECTION_TOOLS");
        AppendRow(1, 1);
        AppendBasicControl("ITEMS_TOOLS_UNLOCKALLTOOLS", BindableFunctions.UnlockAllTools);
        AppendBasicControl("ITEMS_TOOLS_CRAFTTOOLS", BindableFunctions.CraftTools);

        Dictionary<string, List<ToolItem>> tools = [];

        foreach (ToolItem tool in ToolItemManager.GetAllTools())
        {
            string key = tool.name;

            if (key == "Curve Claws Upgraded")
            {
                key = "Curve Claws";
            }

            if (key == "WebShot Architect" || key == "WebShot Weaver")
            {
                key = "WebShot Forge";
            }

            if (key == "Mosscreep Tool 2")
            {
                key = "Mosscreep Tool 1";
            }

            if (key == "Dazzle Bind Upgraded")
            {
                key = "Dazzle Bind";
            }

            if (key == "Shell Satchel")
            {
                key = "Dead Mans Purse";
            }

            tools.TryAdd(key, []);
            tools[key].Add(tool);
        }

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

        foreach (KeyValuePair<string, List<ToolItem>> pair in tools)
        {
            currentRow ??= AppendTileRow(6);
            if (pair.Value.Count == 1)
            {
                ToolItem tool = pair.Value[0];

                CanvasPanel tile = AppendLabeledTile(
                    tool.name,
                    () => tool.IsUnlockedNotHidden,
                    () => ToggleTool(tool),
                    includeLabel: false
                );

                tile.Get<CanvasImage>("Icon").SetImage(tool.GetPopupIcon());
            }
            else
            {
                ToolItem firstTool = pair.Value[0];

                CanvasPanel tile = null;
                tile = AppendLabeledTile(
                    firstTool.name,
                    () =>
                    {
                        foreach (ToolItem tool in pair.Value)
                        {
                            if (tool.IsUnlockedNotHidden)
                            {
                                tile.Get<CanvasImage>("Icon").SetImage(tool.GetPopupIcon());
                                return true;
                            }
                        }

                        tile.Get<CanvasImage>("Icon").SetImage(firstTool.GetPopupIcon());
                        return false;
                    },
                    () =>
                    {
                        for (int i = 0; i < pair.Value.Count; i++)
                        {
                            ToolItem current = pair.Value[i];
                            if (current.IsUnlockedNotHidden)
                            {
                                ToggleTool(current);
                                if (i < pair.Value.Count - 1)
                                {
                                    ToggleTool(pair.Value[i + 1]);
                                }
                                return;
                            }
                        }

                        ToggleTool(firstTool);
                    },
                    includeLabel: false
                );
            }
        }

        AppendSectionHeader("ITEMS_SECTION_CRESTS");
        AppendRow(1);
        AppendBasicControl("ITEMS_CRESTS_UNLOCKALLCRESTS", BindableFunctions.UnlockAllCrests);

        static void ToggleCrest(ToolCrest crest)
        {
            if (crest.IsUnlocked)
            {
                ToolCrestsData.Data data = crest.SaveData;
                data.IsUnlocked = false;
                crest.SaveData = data;
            }
            else
            {
                crest.Unlock();
            }
        }

        AppendRow(1, 1);
        AppendToggleControl("ITEMS_CRESTS_TOGGLECURSED", () => Gameplay.CursedCrest.IsEquipped, BindableFunctions.ToggleCursed);
        AppendToggleControl("ITEMS_CRESTS_TOGGLECLOAKLESS", () => Gameplay.CloaklessCrest.IsEquipped, BindableFunctions.ToggleCloakless);

        List<string> hunterTiers = ["Hunter", "Hunter_v2", "Hunter_v3"];
        ToolCrest hunterCrest = ToolItemManager.GetCrestByName(hunterTiers[0]);

        CanvasPanel hunterTile = null;
        hunterTile = AppendLabeledTile(
            "ITEMS_CRESTS_HUNTER",
            () =>
            {
                for (int i = hunterTiers.Count - 1; i >= 0; i--)
                {
                    ToolCrest crest = ToolItemManager.GetCrestByName(hunterTiers[i]);
                    if (crest.IsUnlocked)
                    {
                        hunterTile.Get<CanvasImage>("Icon").SetImage(crest.CrestSprite);
                        return true;
                    }
                }

                hunterTile.Get<CanvasImage>("Icon").SetImage(hunterCrest.CrestSprite);
                return false;
            },
            () =>
            {
                foreach (string tier in hunterTiers)
                {
                    ToolCrest crest = ToolItemManager.GetCrestByName(tier);
                    if (!crest.IsUnlocked)
                    {
                        ToggleCrest(crest);
                        return;
                    }
                }

                // Cycle back to base hunter crest

                foreach (string tier in hunterTiers)
                {
                    ToolCrest crest = ToolItemManager.GetCrestByName(tier);
                    ToggleCrest(crest);
                }

                ToggleCrest(hunterCrest);
            }
        );

        List<(string, string)> regularCrests = [
            ("Reaper", "ITEMS_CRESTS_REAPER"),
            ("Wanderer", "ITEMS_CRESTS_WANDERER"),
            ("Warrior", "ITEMS_CRESTS_BEAST"),
            ("Witch", "ITEMS_CRESTS_WITCH"),
            ("Toolmaster", "ITEMS_CRESTS_ARCHITECT"),
            ("Spell", "ITEMS_CRESTS_SHAMAN")
        ];

        foreach ((string name, string displayName) in regularCrests)
        {
            ToolCrest crest = ToolItemManager.GetCrestByName(name);

            CanvasPanel tile = AppendLabeledTile(
                displayName,
                () => crest.IsUnlocked,
                () => ToggleCrest(crest)
            );

            tile.Get<CanvasImage>("Icon").SetImage(crest.CrestSprite);
        }

        AppendLabeledTile("ITEMS_CRESTS_VESTICREST",
            () => PlayerData.instance.UnlockedExtraYellowSlot,
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
            }
        );

        AppendSectionHeader("ITEMS_SECTION_ITEMS");

        static void ToggleItem(CollectableItem item)
        {
            if (item.CollectedAmount == 0)
            {
                item.Collect(1, false);
            }
            else
            {
                item.Take(1, false);
            }
        }

        static void SetCollectableAmount(string name, Func<int, int> affector)
        {
            if (CollectableItemManager.IsInHiddenMode())
            {
                CollectableItemManager.Instance.AffectItemData(name, (ref data) => data.AmountWhileHidden = affector(data.AmountWhileHidden));
            }
            else
            {
                CollectableItemManager.Instance.AffectItemData(name, (ref data) => data.Amount = affector(data.Amount));
            }
        }

        AppendLabeledTile("ITEMS_ITEMS_ARCHITECTSMELODY", () => PlayerData.instance.HasMelodyArchitect,
            () => PlayerData.instance.HasMelodyArchitect = !PlayerData.instance.HasMelodyArchitect);
        AppendLabeledTile("ITEMS_ITEMS_VAULTKEEPERSMELODY", () => PlayerData.instance.HasMelodyLibrarian,
            () => PlayerData.instance.HasMelodyLibrarian = !PlayerData.instance.HasMelodyLibrarian);
        AppendLabeledTile("ITEMS_ITEMS_CONDUCTORSMELODY", () => PlayerData.instance.HasMelodyConductor,
            () => PlayerData.instance.HasMelodyConductor = !PlayerData.instance.HasMelodyConductor);

        List<(string, string)> items =
        [
            ("Coral Heart", "ITEMS_ITEMS_HEARTOFMIGHT"),
            ("Flower Heart", "ITEMS_ITEMS_HEARTOFTHEWOODS"),
            ("Hunter Heart", "ITEMS_ITEMS_HEARTOFTHEWILD"),
            ("Clover Heart", "ITEMS_ITEMS_CONJOINEDHEART"),
            ("White Flower", "ITEMS_ITEMS_EVERBLOOM"),
            ("Ward Key", "ITEMS_ITEMS_WHITEKEY"),
            ("Ward Boss Key", "ITEMS_ITEMS_SURGEONSKEY")
        ];

        foreach ((string name, string displayName) in items)
        {
            CollectableItem item = CollectableItemManager.GetItemByName(name);
            CanvasPanel tile = AppendLabeledTile(
                displayName,
                () => item.IsVisible,
                () => ToggleItem(item)
            );
            tile.Get<CanvasImage>("Icon").SetImage(item.GetPopupIcon());
        }

        AppendLabeledTile("ITEMS_ITEMS_KEYOFINDOLENT", () => PlayerData.instance.HasSlabKeyA, () =>
        {
            PlayerData.instance.HasSlabKeyA = !PlayerData.instance.HasSlabKeyA;
            CollectableItemManager.IncrementVersion();
        });
        AppendLabeledTile("ITEMS_ITEMS_KEYOFHERETIC", () => PlayerData.instance.HasSlabKeyB, () =>
        {
            PlayerData.instance.HasSlabKeyB = !PlayerData.instance.HasSlabKeyB;
            CollectableItemManager.IncrementVersion();
        });
        AppendLabeledTile("ITEMS_ITEMS_KEYOFAPOSTATE", () => PlayerData.instance.HasSlabKeyC, () =>
        {
            PlayerData.instance.HasSlabKeyC = !PlayerData.instance.HasSlabKeyC;
            CollectableItemManager.IncrementVersion();
        });

        items =
        [
            ("Architect Key", "ITEMS_ITEMS_ARCHITECTSKEY"),
            ("Belltown House Key", "ITEMS_ITEMS_BELLHOMEKEY"),
            ("Dock Key", "ITEMS_ITEMS_DIVINGBELLKEY"),
            ("Craw Summons", "ITEMS_ITEMS_CRAWSUMMONS")
        ];

        foreach ((string name, string displayName) in items)
        {
            CollectableItem item = CollectableItemManager.GetItemByName(name);
            CanvasPanel tile = AppendLabeledTile(
                displayName,
                () => item.IsVisible,
                () => ToggleItem(item)
            );
            tile.Get<CanvasImage>("Icon").SetImage(item.GetPopupIcon());
        }

        AppendLabeledTile("ITEMS_ITEMS_FARSIGHT", () => PlayerData.instance.ConstructedFarsight,
            () => PlayerData.instance.ConstructedFarsight = !PlayerData.instance.ConstructedFarsight);

        AppendSectionHeader("ITEMS_SECTION_CONSUMABLES");
        AppendTileRow(2);
        AppendIncrementTile(
            "Rosaries",
            () => PlayerData.instance.geo,
            value => HeroController.instance.AddGeo(value - PlayerData.instance.geo),
            step: 100
        );
        AppendIncrementTile(
            "Shell Shards",
            () => PlayerData.instance.ShellShards,
            value => HeroController.instance.AddShards(value - PlayerData.instance.ShellShards),
            step: 100
        );

        List<(string, string)> consumables =
        [
            ("Rosary_Set_Frayed", "ITEMS_CONSUMABLES_FRAYEDROSARYSTRING"),
            ("Fixer Idol", "ITEMS_CONSUMABLES_HORNETSTATUETTE"),
            ("Rosary_Set_Small", "ITEMS_CONSUMABLES_ROSARYSTRING"),
            ("Shard Pouch", "ITEMS_CONSUMABLES_SHARDBUNDLE"),
            ("Rosary_Set_Medium", "ITEMS_CONSUMABLES_ROSARYNECKLACE"),
            ("Great Shard", "ITEMS_CONSUMABLES_BEASTSHARD"),
            ("Rosary_Set_Large", "ITEMS_CONSUMABLES_HEAVYROSARYNECKLACE"),
            ("Pristine Core", "ITEMS_CONSUMABLES_PRISTINECORE"),
            ("Rosary_Set_Huge_White", "ITEMS_CONSUMABLES_PALEROSARYNECKLACE"),
            ("Silk Grub", "ITEMS_CONSUMABLES_SILKEATER"),
            ("Simple Key", "ITEMS_CONSUMABLES_SIMPLEKEY"),
            ("Crest Socket Unlocker", "ITEMS_CONSUMABLES_MEMORYLOCKET"),
            ("Tool Metal", "ITEMS_CONSUMABLES_CRAFTMETAL"),
            ("Pale_Oil", "ITEMS_CONSUMABLES_PALEOIL")
        ];

        foreach ((string name, string displayName) in consumables)
        {
            CollectableItem item = CollectableItemManager.GetItemByName(name);
            CanvasPanel tile = AppendIncrementTile(
                displayName,
                () => item.CollectedAmount,
                value => SetCollectableAmount(name, _ => value)
            );
            tile.Get<CanvasImage>("Icon").SetImage(item.GetPopupIcon());
        }

        AppendRow(1);
        AppendBasicControl("ITEMS_CONSUMABLES_GIVEQUESTITEMS", BindableFunctions.GiveQuestItems);

        AppendSectionHeader("ITEMS_SECTION_MASKSANDSPOOLS");
        AppendTileRow(2);
        AppendIncrementTile("ITEMS_MASKSANDSPOOLS_MASKS", () => PlayerData.instance.maxHealth, SetMaxHealth, min: 1, max: 10);
        static void SetMaxHealth(int value)
        {
            bool increase = value > PlayerData.instance.maxHealth;
            PlayerData.instance.maxHealth = value;
            PlayerData.instance.maxHealthBase = value;
            if (increase)
            {
                HeroController.instance.MaxHealth();
            }
            else
            {
                PlayerData.instance.health = Math.Min(PlayerData.instance.health, PlayerData.instance.maxHealth);
            }
            HudHelper.RefreshMasks();
        }
        AppendIncrementTile("ITEMS_MASKSANDSPOOLS_SPOOLS", () => PlayerData.instance.silkMax, SetMaxSilk, min: 9, max: 18);
        static void SetMaxSilk(int value)
        {
            PlayerData.instance.silkMax = value;
            PlayerData.instance.silk = Math.Min(PlayerData.instance.silk, PlayerData.instance.silkMax);
            HudHelper.RefreshSpool();
            if (PlayerData.instance.IsSilkSpoolBroken && value > 9)
            {
                PlayerData.instance.IsSilkSpoolBroken = false;
                EventRegister.SendEvent("SPOOL UNBROKEN");
            }
        }
        AppendTileRow(2);
        AppendIncrementTile("ITEMS_MASKSANDSPOOLS_HEALTH", () => PlayerData.instance.health, SetHealth, min: 1, max: 10);
        static void SetHealth(int value)
        {
            if (!HeroController.instance.cState.dead && GameManager.instance.IsGameplayScene())
            {
                HeroController.instance.AddHealth(value - PlayerData.instance.health);
                HudHelper.RefreshMasks();
            }
        }
        AppendIncrementTile("ITEMS_MASKSANDSPOOLS_SILK", () => PlayerData.instance.silk, SetSilk);
        static void SetSilk(int value)
        {
            if (value > PlayerData.instance.silk)
            {
                HeroController.instance.AddSilk(value - PlayerData.instance.silk, true);
            }
            else if (value < PlayerData.instance.silk)
            {
                HeroController.instance.TakeSilk(PlayerData.instance.silk - value);
            }
        }
        AppendRow(1);
        AppendBasicControl("ITEMS_MASKSANDSPOOLS_ADDLIFEBLOOD", BindableFunctions.Lifeblood);

        AddTab("MAINPANEL_TAB_KEYBINDS");

        Dictionary<string, List<BindAction>> keybindData = [];
        foreach (string category in keybindCategoryOrder)
        {
            keybindData.Add(category, []);
        }

        foreach (BindAction action in DebugMod.bindActions.Values)
        {
            if (!keybindData.ContainsKey(action.Category))
            {
                keybindCategoryOrder.Add(action.Category);
                keybindData.Add(action.Category, []);
            }
            keybindData[action.Category].Add(action);
        }

        foreach (string category in keybindCategoryOrder)
        {
            CanvasText header = AppendSectionHeader(category);
            header.FontSize = KeybindHeaderFontSize;
            header.Alignment = TextAnchor.MiddleLeft;

            foreach (BindAction action in keybindData[category])
            {
                using PanelBuilder builder = new(currentTab.AppendFixed(new CanvasPanel(action.Name), ListingHeight));
                builder.Horizontal = true;

                CanvasText keybindName = builder.AppendFlex(new CanvasText("KeybindName"));
                keybindName.Text = action.Name;
                keybindName.Alignment = TextAnchor.MiddleLeft;

                CanvasText keycode = builder.AppendFlex(new CanvasText("Keycode"));
                keycode.Alignment = TextAnchor.MiddleLeft;
                keycode.OnUpdate += () => keycode.Text = KeybindDialog.GetKeycodeText(action.Name);

                CanvasButton edit = builder.AppendSquare(new CanvasButton("Edit"));
                edit.ImageOnly(UICommon.images["IconDotCircled"]);
                edit.OnClicked += () => DebugMod.UpdateBind(action.Name, KeyCode.None);

                builder.AppendPadding(UICommon.Margin);

                CanvasButton clear = builder.AppendSquare(new CanvasButton("Clear"));
                clear.ImageOnly(UICommon.images["IconX"]);
                clear.OnClicked += () => DebugMod.UpdateBind(action.Name, null);

                builder.AppendPadding(UICommon.Margin);

                CanvasButton run = builder.AppendSquare(new CanvasButton("Run"));
                run.ImageOnly(UICommon.images["IconRun"]);
                run.OnClicked += action.Action;
            }
        }
    }

    private PanelBuilder AddTab(string name)
    {
        CanvasPanel tab = Add(new CanvasPanel(name));
        tab.LocalPosition = new Vector2(0, TabButtonHeight - UICommon.BORDER_THICKNESS);
        tab.Size = new Vector2(UICommon.RightSideWidth, UICommon.MainPanelHeight - tab.LocalPosition.y);
        tab.CollapseMode = CollapseMode.Deny;
        UICommon.AddBackground(tab);
        tabs.Add(tab);

        CanvasScrollView scrollView = tab.Add(new CanvasScrollView("ScrollView"));
        scrollView.LocalPosition = new Vector2(tab.ContentMargin(), tab.ContentMargin());
        scrollView.Size = new Vector2(tab.Size.x - tab.ContentMargin() * 2 - UICommon.Margin - ScrollbarWidth,
            tab.Size.y - tab.ContentMargin() * 2);

        CanvasScrollbar scrollbar = tab.Add(new CanvasScrollbar("Scrollbar"));
        scrollbar.LocalPosition = new Vector2(tab.ContentMargin() + scrollView.Size.x, tab.ContentMargin(UICommon.Margin));
        scrollbar.Size = new Vector2(ScrollbarWidth, tab.Size.y - tab.ContentMargin(UICommon.Margin) * 2);
        scrollbar.ScrollView = scrollView;

        CanvasPanel panel = scrollView.SetContent(new CanvasPanel("Panel"));
        panel.Size = scrollView.Size;

        PanelBuilder builder = new(panel);
        builder.DynamicLength = true;
        builder.Padding = UICommon.Margin;

        currentTab?.Build();
        currentTab = builder;
        rowCounter = 0;
        return builder;
    }

    private void SetupRow(CanvasPanel row, params int[] widths)
    {
        int totalWidth = (int)row.Size.x;
        int widthUnits = widths.Sum();
        int singleWidth = (totalWidth - UICommon.Margin * (widthUnits - 1)) / widthUnits;

        if (widthUnits != lastRowCount || rowIndex != widths.Length)
        {
            int[] lastPositions = rowPositions;
            int[] lastWidths = rowWidths;

            int x = 0;
            rowPositions = new int[widths.Length];
            rowWidths = new int[widths.Length];
            for (int i = 0; i < widths.Length; i++)
            {
                rowPositions[i] = x;
                rowWidths[i] = singleWidth * widths[i] + UICommon.Margin * (widths[i] - 1);
                x += rowWidths[i] + UICommon.Margin;
            }

            rowWidths[widths.Length - 1] = totalWidth - rowPositions[widths.Length - 1];

            if (lastRowCount > 1 && widthUnits > lastRowCount && widthUnits % lastRowCount == 0)
            {
                // Make sure this row's margins line up properly with the previous row's margins
                // (This is a horrible way to do this lmao)

                int factor = widthUnits / lastRowCount;

                for (int i = 0; i < lastRowCount; i++)
                {
                    int j = (i + 1) * factor - 1;
                    int lastX = lastPositions[i] + lastWidths[i];
                    int curX = rowPositions[j] + rowWidths[j];

                    if (lastX != curX)
                    {
                        rowWidths[j] += lastX - curX;
                        for (int k = j + 1; k < widths.Length; k++)
                        {
                            rowPositions[k] += lastX - curX;
                        }
                    }
                }

                rowWidths[widths.Length - 1] = totalWidth - rowPositions[widths.Length - 1];
            }
        }

        currentRow = row;
        rowCounter++;
        rowIndex = 0;
        lastRowCount = widthUnits;
    }

    private void ResetRow()
    {
        currentRow = null;
    }

    private CanvasPanel AppendRow(params int[] widths)
    {
        CanvasPanel row = currentTab.AppendFixed(new CanvasPanel(rowCounter.ToString()), UICommon.ControlHeight);
        row.CollapseMode = CollapseMode.AllowNoRenaming;

        SetupRow(row, widths);

        return row;
    }

    private CanvasPanel AppendTileRow(int count)
    {
        CanvasPanel row = currentTab.AppendLazy(new CanvasPanel(rowCounter.ToString()));
        row.CollapseMode = CollapseMode.AllowNoRenaming;

        SetupRow(row, Enumerable.Repeat(1, count).ToArray());

        return row;
    }

    private CanvasText AppendSectionHeader(string name)
    {
        currentTab.AppendPadding(SectionEndPadding);

        CanvasText text = currentTab.AppendFixed(new CanvasText(name), SectionHeaderHeight);
        text.Text = Utils.Localize(name);
        text.Font = UICommon.trajanBold;
        text.FontSize = SectionHeaderFontSize;
        text.Alignment = TextAnchor.MiddleCenter;

        ResetRow();

        return text;
    }

    private CanvasPanel AppendRowElement(string name)
    {
        CanvasPanel panel = currentRow.Add(new CanvasPanel(name));
        panel.LocalPosition = new Vector2(rowPositions[rowIndex], 0);
        panel.Size = new Vector2(rowWidths[rowIndex], currentRow.Size.y);

        rowIndex++;
        if (rowIndex == rowPositions.Length)
        {
            ResetRow();
        }

        return panel;
    }

    private CanvasPanel AppendButtonControl(string name, Action effect, Action<CanvasButton> update)
    {
        currentRow ??= AppendRow(1);

        CanvasPanel controlPanel = AppendRowElement(name);

        PanelBuilder control = new(controlPanel);
        control.Horizontal = true;

        CanvasButton button = control.AppendFlex(new CanvasButton("Button"));
        button.Text.Text = Utils.Localize(name);

        button.OnClicked += () =>
        {
            try
            {
                effect();
            }
            catch (Exception e)
            {
                DebugMod.LogError($"Error clicking button {button.GetQualifiedName()}: {e}");
            }
        };

        if (update != null)
        {
            button.OnUpdate += () => update(button);
        }

        if (DebugMod.bindsByMethod.TryGetValue(effect.Method, out BindAction action))
        {
            UICommon.AppendKeybindButton(control, action);
        }

        control.Build();

        return controlPanel;
    }

    private CanvasPanel AppendBasicControl(string name, Action effect) => AppendButtonControl(name, effect, null);

    // TODO: replace this with checkbox
    private CanvasPanel AppendToggleControl(string name, Func<bool> getter, Action effect)
    {
        return AppendButtonControl(name, effect, button =>
        {
            button.Toggled = getter();
        });
    }

    // TODO: replace this with a slider or increment/decrement buttons
    private CanvasPanel AppendIncrementControl(string name, Func<int> getter, Action effect)
    {
        return AppendButtonControl(name, effect, button =>
        {
            button.Text.Text = $"{name}: {getter()}";
        });
    }

    private CanvasPanel AppendLabeledTile(string name, Func<bool> getter, Action effect, string image = "IconX", bool includeLabel = true)
    {
        CanvasPanel row = currentRow ?? AppendTileRow(5);

        CanvasPanel tile = AppendRowElement(name);
        tile.CollapseMode = CollapseMode.Deny;

        CanvasButton button = tile.Add(new CanvasButton("Button"));
        button.OnUpdate += () => button.Toggled = getter();
        button.OnClicked += effect;

        PanelBuilder builder = new(tile);
        // Adding the divide by 2 magically makes the text line up properly
        builder.Padding = UICommon.Margin / 2f;
        builder.DynamicLength = true;

        // Effectively AppendSquare() but ensures the last tile in a row isn't longer than the others
        CanvasImage icon = builder.AppendFixed(new CanvasImage("Icon"), rowWidths[0] - builder.OuterPadding * 2);
        icon.SetImage(UICommon.images[image]);

        if (includeLabel)
        {
            CanvasText label = builder.AppendFixed(new CanvasText("Label"), ListingHeight * 2);
            label.Alignment = TextAnchor.MiddleCenter;
            label.Text = Utils.Localize(name);
        }

        builder.Build();

        button.Size = tile.Size;
        row.Size = new Vector2(row.Size.x, Mathf.Max(row.Size.y, tile.Size.y));

        return tile;
    }

    private CanvasPanel AppendIncrementTile(string name, Func<int> getter, Action<int> setter, string image = "IconX",
        Action customAdd = null, Action customRemove = null, int step = 1, int min = 0, int max = int.MaxValue, bool wrap = false)
    {
        CanvasPanel row = currentRow ?? AppendTileRow(2);

        // Image gets 1/3 of the tile and determines the tile height
        int imageWidth = rowWidths[0] / 3;

        CanvasPanel tile = AppendRowElement(name);
        tile.CollapseMode = CollapseMode.Deny;
        UICommon.AddBackground(tile);

        tile.Size = new Vector2(tile.Size.x, imageWidth + UICommon.Margin * 2);

        PanelBuilder builder = new(tile);
        builder.Horizontal = true;
        builder.InnerPadding = UICommon.Margin;
        builder.OuterPadding = tile.ContentMargin(UICommon.Margin);

        CanvasImage icon = builder.AppendSquare(new CanvasImage("Icon"));
        icon.SetImage(UICommon.images[image]);

        PanelBuilder containerBuilder = new(builder.AppendFlex(new CanvasPanel("Container")));

        containerBuilder.AppendFlexPadding();

        CanvasText label = containerBuilder.AppendFixed(new CanvasText("Label"), ListingHeight * 2);
        label.Alignment = TextAnchor.MiddleCenter;
        label.Text = Utils.Localize(name);

        containerBuilder.AppendFlexPadding();
        containerBuilder.AppendPadding(UICommon.Margin); // Evens spacing between tile border and control row border

        CanvasPanel controlRow = containerBuilder.AppendFixed(new CanvasPanel("ControlRow"), UICommon.ControlHeight);
        PanelBuilder controlBuilder = new(controlRow) { Horizontal = true, InnerPadding = -UICommon.BORDER_THICKNESS };

        void ValueUpdate(int value)
        {
            if (value < min)
            {
                value = wrap ? max : min;
            }

            if (value > max)
            {
                value = wrap ? min : max;
            }

            setter(value);
        }

        var decButton = controlBuilder.AppendSquare(new CanvasButton("Decrement"));
        decButton.SetImage(UICommon.images["IconMinusMin"]);
        decButton.RemoveText();
        decButton.OnClicked += customRemove ?? (() => ValueUpdate(getter() - step));

        var valueButton = controlBuilder.AppendFlex(new CanvasButton("Value"));
        valueButton.SetImage(UICommon.clearBG);
        valueButton.Border.Sides &= ~BorderSides.LEFT;

        var textField = valueButton.SetTextField();
        textField.OnUpdate += () => textField.UpdateDefaultText(getter().ToString());
        textField.OnSubmit += text =>
        {
            if (!int.TryParse(text, out int value))
            {
                return;
            }

            // Always clamp text input instead of wrapping
            if (value < min) value = min;
            if (value > max) value = max;

            setter(value);
        };

        var incButton = controlBuilder.AppendSquare(new CanvasButton("Increment"));
        incButton.SetImage(UICommon.images["IconPlusMin"]);
        incButton.RemoveText();
        incButton.OnClicked += customAdd ?? (() => ValueUpdate(getter() + step));
        incButton.Border.Sides &= ~BorderSides.LEFT;

        builder.Build();
        containerBuilder.Build();
        controlBuilder.Build();

        row.Size = new Vector2(row.Size.x, Mathf.Max(row.Size.y, tile.Size.y));

        return tile;
    }

    public override void Build()
    {
        currentTab?.Build();

        float tabButtonWidth = (Size.x - UICommon.Margin * (tabs.Count - 1)) / tabs.Count;
        float tabX = 0;

        foreach (CanvasPanel tab in tabs)
        {
            // Created after the tabs themselves so they get input priority over offscreen controls
            CanvasButton button = Add(new CanvasButton($"{tab.Name}TabButton"));
            button.LocalPosition = new Vector2(tabX, 0);
            button.Size = new Vector2(tabButtonWidth, TabButtonHeight);
            button.SetImage(UICommon.panelBG);
            button.Text.Text = Utils.Localize(tab.Name);
            button.OnClicked += () => DebugMod.settings.MainPanelCurrentTab = tab.Name;
            button.OnUpdate += () => button.Toggled = DebugMod.settings.MainPanelCurrentTab == tab.Name;

            tabX += tabButtonWidth + UICommon.Margin;
        }

        base.Build();
    }

    private void DoUpdate()
    {
        bool needsReset = true;

        foreach (CanvasPanel tab in tabs)
        {
            if (DebugMod.settings.MainPanelCurrentTab == tab.Name)
            {
                tab.ActiveSelf = true;
                needsReset = false;
            }
            else
            {
                tab.ActiveSelf = false;
            }
        }

        if (needsReset)
        {
            DebugMod.settings.MainPanelCurrentTab = tabs[0].Name;
            tabs[0].ActiveSelf = true;
        }
    }
}
