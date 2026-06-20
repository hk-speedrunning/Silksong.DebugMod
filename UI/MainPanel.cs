using DebugMod.Helpers;
using DebugMod.MonoBehaviours;
using DebugMod.UI.Canvas;
using GlobalSettings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace DebugMod.UI;

public record struct ToolDef(string IconKey, ToolItem Item)
{
    public string IconKey = IconKey;
    public ToolItem Item = Item;
}

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
        "CATEGORY_CHEATS",
        "CATEGORY_SAVESTATES",
        "CATEGORY_MODUI",
        "CATEGORY_TIME",
        "CATEGORY_ENEMIES",
        "CATEGORY_SKILLS",
        "CATEGORY_UPGRADES",
        "CATEGORY_TOOLS",
        "CATEGORY_CONSUMABLES",
        "CATEGORY_MASKSANDSPOOLS",
        "CATEGORY_VISUAL",
        "CATEGORY_MISC"
    ];

    private readonly List<CanvasPanel> tabs = [];
    private readonly List<CanvasButton> tabButtons = [];

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

        AppendSectionHeader("CATEGORY_CHEATS");
        AppendRow(1, 1, 1);
        AppendToggleControl("CHEATS_NOCLIP", () => DebugMod.noclip, BindableFunctions.ToggleNoclip);
        AppendToggleControl("CHEATS_INVINCIBILITY", () => DebugMod.playerInvincible, BindableFunctions.ToggleInvincibility);
        AppendToggleControl("CHEATS_INFINITEJUMP", () => PlayerData.instance.infiniteAirJump, BindableFunctions.ToggleInfiniteJump);
        AppendRow(1, 1, 1);
        AppendToggleControl("CHEATS_INFINITEHP", () => DebugMod.infiniteHP, BindableFunctions.ToggleInfiniteHP);
        AppendToggleControl("CHEATS_INFINITESILK", () => DebugMod.infiniteSilk, BindableFunctions.ToggleInfiniteSilk);
        AppendToggleControl("CHEATS_INFINITETOOLS", () => DebugMod.infiniteTools, BindableFunctions.ToggleInfiniteTools);
        AppendRow(2, 1);
        AppendToggleControl("CHEATS_TOGGLEHEROCOLLIDER", () => DebugMod.heroColliderDisabled, BindableFunctions.ToggleHeroCollider);
        AppendBasicControl("CHEATS_KILLALL", BindableFunctions.KillAll);

        AppendSectionHeader("CATEGORY_MODUI");
        AppendRow(1, 1);
        AppendToggleControl("MODUI_TOGGLEALLUI", () => true, BindableFunctions.ToggleAllPanels);
        AppendToggleControl("MODUI_TOGGLEMAINPANEL", () => DebugMod.settings.MainPanelVisible, BindableFunctions.ToggleMainPanel);
        AppendRow(1, 1);
        AppendToggleControl("MODUI_TOGGLEENEMIESPANEL", () => DebugMod.settings.EnemiesPanelVisible, BindableFunctions.ToggleEnemiesPanel);
        AppendToggleControl("MODUI_TOGGLECONSOLEPANEL", () => DebugMod.settings.ConsoleVisible, BindableFunctions.ToggleConsolePanel);
        AppendRow(1, 1);
        AppendToggleControl("MODUI_TOGGLESAVESTATESPANEL", () => DebugMod.settings.SaveStatePanelVisible, BindableFunctions.ToggleSaveStatePanel);
        AppendToggleControl("MODUI_EXPANDCOLLAPSESAVESTATES", () => DebugMod.settings.SaveStatePanelExpanded, BindableFunctions.ToggleExpandedSaveStatePanel);
        AppendRow(1, 1);
        AppendToggleControl("MODUI_TOGGLEINFOPANEL", () => DebugMod.settings.InfoPanelVisible, BindableFunctions.ToggleInfoPanel);
        AppendToggleControl("MODUI_ALWAYSSHOWCURSOR", () => DebugMod.settings.ShowCursorWhileUnpaused, BindableFunctions.ToggleAlwaysShowCursor);

        AppendSectionHeader("CATEGORY_TIME");
        AppendRow(1);
        AppendNumericControl(
            "TIME_TIMESCALE",
            () => TimeScale.CustomTimeScale,
            1f,
            f =>
            {
                if (f >= 0f)
                {
                    TimeScale.CustomTimeScale = f;
                }
            },
            BindableFunctions.TimescaleUp,
            BindableFunctions.TimescaleDown,
            BindableFunctions.TimescaleReset
        );
        AppendRow(1, 1);
        AppendToggleControl("TIME_FREEZEGAME", () => TimeScale.Frozen, BindableFunctions.PauseGameNoUI);
        AppendBasicControl("TIME_ADVANCEFRAME", BindableFunctions.AdvanceFrame);
        AppendRow(1, 1);
        AppendToggleControl("TIME_FORCEPAUSE", () =>
            DebugMod.forcePaused && GameManager.instance.isPaused, BindableFunctions.ForcePause);
        AppendBasicControl("TIME_RESETFRAMECOUNTER", BindableFunctions.ResetFrameCounter);

        AppendSectionHeader("CATEGORY_VISUAL");
        AppendRow(1, 1);
        AppendToggleControl("VISUAL_TOGGLEHITBOXES", () => DebugMod.settings.ShowHitBoxes != 0, BindableFunctions.ShowHitboxes);
        AppendToggleControl("VISUAL_FORCECAMERAFOLLOW", () => DebugMod.cameraFollow, BindableFunctions.ForceCameraFollow);
        AppendRow(1, 1);
        AppendToggleControl("VISUAL_PREVIEWCOCOONPOSITION", CocoonPreviewToggled, BindableFunctions.PreviewCocoonPosition);
        static bool CocoonPreviewToggled()
        {
            CocoonPreviewer previewer = GameManager.instance.GetComponent<CocoonPreviewer>();
            if (!previewer) return false;
            return previewer.previewEnabled;
        }

        AppendToggleControl("VISUAL_TOGGLEHEROLIGHT", HeroLightToggled, BindableFunctions.ToggleHeroLight);
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
        AppendToggleControl("VISUAL_TOGGLEHUD", HUDToggled, BindableFunctions.ToggleHUD);
        static bool HUDToggled()
        {
            PlayMakerFSM hud = GameCameras.instance.hudCanvasSlideOut;
            if (!hud) return false;
            return !hud.gameObject.activeInHierarchy;
        }
        AppendToggleControl("VISUAL_TOGGLEVIGNETTE", () => VisualMaskHelper.vignetteDisabled, BindableFunctions.ToggleVignette);
        AppendToggleControl("VISUAL_HIDEHERO", HideHeroToggled, BindableFunctions.HideHero);
        static bool HideHeroToggled()
        {
            GameObject hero = DebugMod.RefKnight;
            if (!hero) return false;
            tk2dSprite sprite = hero.GetComponent<tk2dSprite>();
            if (!sprite) return false;
            return Math.Abs(sprite.color.a) == 0;
        }
        AppendRow(1, 1);
        AppendToggleControl("VISUAL_TOGGLECAMERASHAKE", CameraShakeToggled, BindableFunctions.ToggleCameraShake);
        static bool CameraShakeToggled()
        {
            PlayMakerFSM cameraShake = GameCameras.instance.cameraShakeFSM;
            if (!cameraShake) return false;
            return !cameraShake.enabled;
        }
        AppendToggleControl("VISUAL_DEACTIVATEVISUALMASKS", () => VisualMaskHelper.masksDisabled, BindableFunctions.DoDeactivateVisualMasks);
        AppendRow(1);
        AppendNumericControl(
            "VISUAL_ZOOM",
            () => GameCameras.instance.tk2dCam.zoomFactor,
            1f,
            f =>
            {
                if (f > 0f)
                {
                    GameCameras.instance.tk2dCam.zoomFactor = f;
                    ZoomHelper.UpdateCameraFOV();
                }
            },
            BindableFunctions.ZoomIn,
            BindableFunctions.ZoomOut,
            BindableFunctions.ResetZoom
        );

        AppendSectionHeader("CATEGORY_MISC");
        AppendRow(1, 1);
        AppendBasicControl("MISC_SETHAZARDRESPAWN", BindableFunctions.SetHazardRespawn);
        AppendBasicControl("MISC_HAZARDRESPAWN", BindableFunctions.Respawn);
        AppendRow(1, 1, 1);
        AppendBasicControl("MISC_DAMAGESELF", BindableFunctions.SelfDamage);
        AppendBasicControl("MISC_KILLSELF", BindableFunctions.KillSelf);
        AppendBasicControl("MISC_BREAKCOCOON", BindableFunctions.BreakCocoon);
        AppendRow(1, 1);
        AppendBasicControl("MISC_RESETCURRENTSCENEDATA", BindableFunctions.ResetCurrentScene);
        AppendBasicControl("MISC_BLOCKSCENEDATACHANGES", BindableFunctions.BlockCurrentSceneChanges);
        AppendRow(1, 1, 1);
        AppendToggleControl("MISC_TOGGLEACT3", () => PlayerData.instance.blackThreadWorld, BindableFunctions.ToggleAct3);
        AppendToggleControl("MISC_LOCKKEYBINDS", () => DebugMod.KeyBindLock, BindableFunctions.ToggleLockKeyBinds);
        AppendBasicControl("MISC_RESETALL", BindableFunctions.Reset);

        AddTab("MAINPANEL_TAB_ITEMS");

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
            value => PlayerData.instance.silkRegenMax = value, "Inv_SilkHeart", max: 3, wrap: true);
        AppendTileRow(2);
        AppendIncrementTile("UPGRADES_CRAFTINGKIT", () => PlayerData.instance.ToolKitUpgrades,
            value => PlayerData.instance.ToolKitUpgrades = value, "Inv_CraftingKit", max: 4, wrap: true);
        AppendIncrementTile("UPGRADES_TOOLPOUCH", () => PlayerData.instance.ToolPouchUpgrades,
            value => PlayerData.instance.ToolPouchUpgrades = value, "Inv_ToolPouch", max: 4, wrap: true);
        AppendRow(1, 1);
        AppendBasicControl("UPGRADES_ALLMAPS", BindableFunctions.UnlockAllMaps);
        AppendBasicControl("UPGRADES_ALLFASTTRAVEL", BindableFunctions.UnlockAllFastTravel);

        AppendSectionHeader("CATEGORY_MASKSANDSPOOLS");
        AppendTileRow(2);
        AppendIncrementTile("MASKSANDSPOOLS_MASKS", () => PlayerData.instance.maxHealth, SetMaxHealth, image: "Inv_Mask", min: 1, max: 10);
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
        AppendIncrementTile("MASKSANDSPOOLS_SPOOLS", () => PlayerData.instance.silkMax, SetMaxSilk, image: "Inv_Spool", min: 9, max: 18);
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
        AppendIncrementTile("MASKSANDSPOOLS_MASKSHARDS", () => PlayerData.instance.heartPieces,
            value => PlayerData.instance.heartPieces = value, "Inv_MaskShard", max: 3,
            customAdd: BindableFunctions.GiveMaskShard, customRemove: BindableFunctions.TakeMaskShard);
        AppendIncrementTile("MASKSANDSPOOLS_SPOOLFRAGMENTS", () => PlayerData.instance.silkSpoolParts,
            value => PlayerData.instance.silkSpoolParts = value, "Inv_SpoolFragment", max: 1,
            customAdd: BindableFunctions.GiveSpoolFragment, customRemove: BindableFunctions.TakeSpoolFragment);
        AppendTileRow(3);
        AppendIncrementTile("MASKSANDSPOOLS_HEALTH", () => PlayerData.instance.health, SetHealth, image: "Inv_Health", min: 1, max: 10);
        static void SetHealth(int value)
        {
            if (!HeroController.instance.cState.dead && GameManager.instance.IsGameplayScene())
            {
                HeroController.instance.AddHealth(value - PlayerData.instance.health);
                HudHelper.RefreshMasks();
            }
        }
        AppendIncrementTile("MASKSANDSPOOLS_SILK", () => PlayerData.instance.silk, SetSilk, image: "Inv_Silk");
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
        AppendWideTile("MASKSANDSPOOLS_LIFEBLOOD", BuildLifebloodTile, image: "Inv_Lifeblood");
        static void BuildLifebloodTile(CanvasPanel controlRow)
        {
            CanvasButton button = controlRow.Add(new CanvasButton("Button"));
            button.Size = controlRow.Size;
            button.Text.Text = Localization.Get("ITEMS_ADD");
            button.OnClicked += BindableFunctions.Lifeblood;
        }

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

        AppendSectionHeader("CATEGORY_ITEMS");

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

        void AddItemsLabeledTiles(List<(string, string, string)> items)
        {
            foreach ((string name, string displayName, string iconName) in items)
            {
                CollectableItem item = CollectableItemManager.GetItemByName(name);
                AppendLabeledTile(
                    displayName,
                    () => item.IsVisible,
                    () => ToggleItem(item),
                    image: iconName
                );
            }
        }

        AppendLabeledTile("ITEMS_ARCHITECTSMELODY", () => PlayerData.instance.HasMelodyArchitect,
            () => PlayerData.instance.HasMelodyArchitect = !PlayerData.instance.HasMelodyArchitect, image: "Inv_MelodyArchitect");
        AppendLabeledTile("ITEMS_VAULTKEEPERSMELODY", () => PlayerData.instance.HasMelodyLibrarian,
            () => PlayerData.instance.HasMelodyLibrarian = !PlayerData.instance.HasMelodyLibrarian, image: "Inv_MelodyVaultkeeper");
        AppendLabeledTile("ITEMS_CONDUCTORSMELODY", () => PlayerData.instance.HasMelodyConductor,
            () => PlayerData.instance.HasMelodyConductor = !PlayerData.instance.HasMelodyConductor, image: "Inv_MelodyConductor");

        CanvasPanel quillTile = AppendLabeledTile(
            "ITEMS_QUILL",
            () => PlayerData.instance.hasQuill,
            () =>
            {
                if (!PlayerData.instance.hasQuill)
                {
                    PlayerData.instance.hasQuill = true;
                    PlayerData.instance.QuillState = 1;
                }
                else if (PlayerData.instance.QuillState >= 3)
                {
                    PlayerData.instance.hasQuill = false;
                }
                else
                {
                    PlayerData.instance.QuillState++;
                }

                CollectableItemManager.IncrementVersion();
            }
        );
        quillTile.OnUpdate += () =>
        {
            string image;

            if (PlayerData.instance.hasQuill && PlayerData.instance.QuillState > 1)
            {
                if (PlayerData.instance.QuillState == 2)
                {
                    image = "Inv_RedQuill";
                }
                else
                {
                    image = "Inv_PurpleQuill";
                }
            }
            else
            {
                image = "Inv_Quill";
            }

            quillTile.Get<CanvasImage>("Icon").SetImage(UICommon.images[image]);
        };

        AppendLabeledTile("ITEMS_FARSIGHT", () => PlayerData.instance.ConstructedFarsight,
            () => PlayerData.instance.ConstructedFarsight = !PlayerData.instance.ConstructedFarsight,
            image: "Inv_Farsight");

        AddItemsLabeledTiles([
            ("Coral Heart", "ITEMS_HEARTOFMIGHT", "Inv_HeartCoral"),
            ("Flower Heart", "ITEMS_HEARTOFTHEWOODS", "Inv_HeartFlower"),
            ("Hunter Heart", "ITEMS_HEARTOFTHEWILD", "Inv_HeartAnt"),
            ("Clover Heart", "ITEMS_CONJOINEDHEART", "Inv_HeartJoined"),
            ("White Flower", "ITEMS_EVERBLOOM", "Inv_Everbloom")
        ]);

        AppendLabeledTile("ITEMS_KEYOFINDOLENT", () => PlayerData.instance.HasSlabKeyA, () =>
        {
            PlayerData.instance.HasSlabKeyA = !PlayerData.instance.HasSlabKeyA;
            CollectableItemManager.IncrementVersion();
        }, image: "Inv_KeyIndolent");
        AppendLabeledTile("ITEMS_KEYOFHERETIC", () => PlayerData.instance.HasSlabKeyB, () =>
        {
            PlayerData.instance.HasSlabKeyB = !PlayerData.instance.HasSlabKeyB;
            CollectableItemManager.IncrementVersion();
        }, image: "Inv_KeyHeretic");
        AppendLabeledTile("ITEMS_KEYOFAPOSTATE", () => PlayerData.instance.HasSlabKeyC, () =>
        {
            PlayerData.instance.HasSlabKeyC = !PlayerData.instance.HasSlabKeyC;
            CollectableItemManager.IncrementVersion();
        }, image: "Inv_KeyApostate");

        AddItemsLabeledTiles([
            ("Ward Key", "ITEMS_WHITEKEY", "Inv_WhiteKey"),
            ("Ward Boss Key", "ITEMS_SURGEONSKEY", "Inv_SurgeonsKey"),
            ("Architect Key", "ITEMS_ARCHITECTSKEY", "Inv_ArchitectKey"),
            ("Dock Key", "ITEMS_DIVINGBELLKEY", "Inv_DockKey"),
            ("Belltown House Key", "ITEMS_BELLHOMEKEY", "Inv_BellhomeKey"),
            ("Craw Summons", "ITEMS_CRAWSUMMONS", "Inv_CrawSummons")
        ]);

        AppendSectionHeader("CATEGORY_CONSUMABLES");
        AppendRow(1);
        AppendBasicControl("CONSUMABLES_GIVEQUESTITEMS", BindableFunctions.GiveQuestItems);
        AppendTileRow(2);
        AppendIncrementTile(
            "CONSUMABLES_ROSARIES",
            () => PlayerData.instance.geo,
            value => HeroController.instance.AddGeo(value - PlayerData.instance.geo),
            image: "Inv_Rosaries",
            step: 100
        );
        AppendIncrementTile(
            "CONSUMABLES_SHELLSHARDS",
            () => PlayerData.instance.ShellShards,
            value => HeroController.instance.AddShards(value - PlayerData.instance.ShellShards),
            image: "Inv_ShellShards",
            step: 100
        );


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

        List<(string, string, string)> consumables =
        [
            ("Simple Key", "CONSUMABLES_SIMPLEKEY", "Inv_SimpleKey"),
            ("Crest Socket Unlocker", "CONSUMABLES_MEMORYLOCKET", "Inv_MemoryLocket"),
            ("Tool Metal", "CONSUMABLES_CRAFTMETAL", "Inv_Craftmetal"),
            ("Pale_Oil", "CONSUMABLES_PALEOIL", "Inv_PaleOil"),
            ("Rosary_Set_Frayed", "CONSUMABLES_FRAYEDROSARYSTRING", "Inv_RosaryString1"),
            ("Silk Grub", "CONSUMABLES_SILKEATER", "Inv_Silkeater"),
            ("Rosary_Set_Small", "CONSUMABLES_ROSARYSTRING", "Inv_RosaryString2"),
            ("Fixer Idol", "CONSUMABLES_HORNETSTATUETTE", "Inv_ShardStatue"),
            ("Rosary_Set_Medium", "CONSUMABLES_ROSARYNECKLACE", "Inv_RosaryString3"),
            ("Shard Pouch", "CONSUMABLES_SHARDBUNDLE", "Inv_ShardBundle"),
            ("Rosary_Set_Large", "CONSUMABLES_HEAVYROSARYNECKLACE", "Inv_RosaryString4"),
            ("Great Shard", "CONSUMABLES_BEASTSHARD", "Inv_ShardBeast"),
            ("Rosary_Set_Huge_White", "CONSUMABLES_PALEROSARYNECKLACE", "Inv_RosaryString5"),
            ("Pristine Core", "CONSUMABLES_PRISTINECORE", "Inv_ShardCore"),
        ];

        foreach ((string name, string displayName, string iconName) in consumables)
        {
            CollectableItem item = CollectableItemManager.GetItemByName(name);
            AppendIncrementTile(
                displayName,
                () => item.CollectedAmount,
                value => SetCollectableAmount(name, _ => value),
                image: iconName
            );
        }

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
                keybindName.Text = Localization.Get(action.Name);
                keybindName.Alignment = TextAnchor.MiddleLeft;

                CanvasText keycode = builder.AppendFlex(new CanvasText("Keycode"));
                keycode.Alignment = TextAnchor.MiddleLeft;
                keycode.Text = KeybindDialog.GetKeycodeText(action.Name);
                DebugMod.bindUpdated += (name, _) =>
                {
                    if (name == action.Name)
                    {
                        keycode.Text = KeybindDialog.GetKeycodeText(action.Name);
                    }
                };

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
        text.Text = Localization.Get(name);
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
        button.Text.Text = Localization.Get(name);

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

    private CanvasPanel AppendNumericControl(string name, Func<float> getter, float defaultValue, Action<float> setter, Action increase,
        Action decrease, Action reset)
    {
        currentRow ??= AppendRow(1);

        CanvasPanel controlPanel = AppendRowElement(name);

        PanelBuilder control = new(controlPanel);
        control.Horizontal = true;
        control.InnerPadding = -UICommon.BORDER_THICKNESS;

        CanvasButton label = control.AppendFlex(new CanvasButton("Label"));
        label.Text.Text = Localization.Get(name);
        label.RemoveHoverBorder();
        label.OnUpdate += () => label.SetImage(Mathf.Approximately(getter(), defaultValue) ? UICommon.panelBG : UICommon.panelStrongBG);

        List<BindAction> actions = [];
        BindAction action;

        if (DebugMod.bindsByMethod.TryGetValue(increase.Method, out action))
        {
            actions.Add(action);
        }

        if (DebugMod.bindsByMethod.TryGetValue(decrease.Method, out action))
        {
            actions.Add(action);
        }

        if (DebugMod.bindsByMethod.TryGetValue(reset.Method, out action))
        {
            actions.Add(action);
        }

        if (actions.Count > 0)
        {
            control.AppendPadding(-control.InnerPadding * 2);
            UICommon.AppendKeybindButton(control, actions.ToArray());
        }

        int connecterLength = UICommon.ScaleWidth(70);

        control.AppendPadding(-control.InnerPadding * 2);
        CanvasPanel connectorPanel = control.AppendFixed(new CanvasPanel("Connector1"), connecterLength);
        connectorPanel.CollapseMode = CollapseMode.Deny;
        CanvasBorder connector = connectorPanel.Add(new CanvasBorder("Border"));
        connector.LocalPosition = new Vector2(0f, (int)(control.ChildBreadth() / 2f));
        connector.Sides = BorderSides.TOP;

        CanvasButton decButton = control.AppendSquare(new CanvasButton("Decrease"));
        decButton.SetImage(UICommon.images["IconMinusMin"]);
        decButton.RemoveText();
        decButton.OnClicked += decrease;

        CanvasButton button = control.AppendFlex(new CanvasButton("Button"));
        button.SetImage(UICommon.clearBG);
        button.Border.Sides &= ~BorderSides.LEFT;

        CanvasTextField textField = button.SetTextField();
        textField.OnUpdate += () => textField.UpdateDefaultText(getter().ToString(CultureInfo.CurrentCulture));
        textField.OnSubmit += text =>
        {
            if (float.TryParse(text, out float f))
            {
                setter(f);
            }
        };

        CanvasButton incButton = control.AppendSquare(new CanvasButton("Increase"));
        incButton.SetImage(UICommon.images["IconPlusMin"]);
        incButton.RemoveText();
        incButton.OnClicked += increase;
        incButton.Border.Sides &= ~BorderSides.LEFT;

        control.AppendPadding(-control.InnerPadding * 2);
        connectorPanel = control.AppendFixed(new CanvasPanel("Connector2"), connecterLength);
        connectorPanel.CollapseMode = CollapseMode.Deny;
        connector = connectorPanel.Add(new CanvasBorder("Border"));
        connector.LocalPosition = new Vector2(0f, (int)(control.ChildBreadth() / 2f));
        connector.Sides = BorderSides.TOP;

        CanvasPanel resetContainer = control.AppendSquare(new CanvasPanel("Reset"));
        resetContainer.CollapseMode = CollapseMode.Deny;
        UICommon.AddBackground(resetContainer);

        CanvasButton resetButton = resetContainer.Add(new CanvasButton("Button"));
        resetButton.Size = resetContainer.Size;
        resetButton.SetImage(UICommon.images["IconReset"]);
        resetButton.RemoveText();
        resetButton.OnClicked += reset;

        control.Build();
        return controlPanel;
    }

    private CanvasPanel AppendLabeledTile(string name, Func<bool> getter, Action effect, string image = "IconX", bool includeLabel = true, int defaultRowWidth = 5)
    {
        CanvasPanel row = currentRow ?? AppendTileRow(defaultRowWidth);

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
            label.Text = Localization.Get(name);
        }

        builder.Build();

        button.Size = tile.Size;
        row.Size = new Vector2(row.Size.x, Mathf.Max(row.Size.y, tile.Size.y));

        return tile;
    }

    private CanvasPanel AppendWideTile(string name, Action<CanvasPanel> controlBuilder, string image)
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

        Texture2D texture = UICommon.images.GetValueOrDefault(image);
        if (!texture) texture = UICommon.images["IconX"];
        CanvasImage icon = builder.AppendSquare(new CanvasImage("Icon"));
        icon.SetImage(texture);

        PanelBuilder containerBuilder = new(builder.AppendFlex(new CanvasPanel("Container")));

        containerBuilder.AppendFlexPadding();

        var labelHeight = rowWidths.Length > 2 ? ListingHeight : ListingHeight * 2;

        CanvasText label = containerBuilder.AppendFixed(new CanvasText("Label"), labelHeight);
        label.Alignment = TextAnchor.MiddleCenter;
        label.Text = Localization.Get(name);

        containerBuilder.AppendFlexPadding();
        containerBuilder.AppendPadding(UICommon.Margin); // Evens spacing between tile border and control row border

        var controlHeight = rowWidths.Length > 2 ? UICommon.ScaleHeight(20) : UICommon.ControlHeight;

        CanvasPanel controlRow = containerBuilder.AppendFixed(new CanvasPanel("ControlRow"), controlHeight);

        builder.Build();
        containerBuilder.Build();
        controlBuilder(controlRow);

        row.Size = new Vector2(row.Size.x, Mathf.Max(row.Size.y, tile.Size.y));

        return tile;
    }

    private CanvasPanel AppendIncrementTile(string name, Func<int> getter, Action<int> setter, string image = "IconX",
        Action customAdd = null, Action customRemove = null, int step = 1, int min = 0, int max = int.MaxValue, bool wrap = false)
    {
        void Builder(CanvasPanel controlRow)
        {
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

            controlBuilder.Build();
        }

        return AppendWideTile(name, Builder, image);
    }

    public override void Build()
    {
        currentTab?.Build();

        float tabButtonWidth = (Size.x - UICommon.Margin * (tabs.Count - 1)) / tabs.Count;

        foreach (CanvasPanel tab in tabs)
        {
            // Created after the tabs themselves so they get input priority over offscreen controls
            CanvasButton button = Add(new CanvasButton($"{tab.Name}TabButton"));
            button.Size = new Vector2(tabButtonWidth, TabButtonHeight);
            button.SetImage(UICommon.panelBG);
            button.Text.Text = Localization.Get(tab.Name);
            button.OnClicked += () => DebugMod.settings.MainPanelCurrentTab = tab.Name;
            button.OnUpdate += () => button.Toggled = DebugMod.settings.MainPanelCurrentTab == tab.Name;
            tabButtons.Add(button);
        }

        LayoutTabsNormal();

        base.Build();
    }

    internal void LayoutTabsNormal()
    {
        float tabX = 0;

        foreach (CanvasButton button in tabButtons)
        {
            button.LocalPosition = new Vector2(tabX, 0);
            tabX += button.Size.x + UICommon.Margin;
        }
    }

    internal void LayoutTabsSide(float offset)
    {
        float tabY = offset;

        foreach (CanvasButton button in tabButtons)
        {
            button.LocalPosition = new Vector2(-button.Size.x + UICommon.BORDER_THICKNESS, tabY);
            tabY += button.Size.y + UICommon.Margin;
        }
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
