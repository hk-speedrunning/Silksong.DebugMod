using DebugMod.Helpers;
using DebugMod.MonoBehaviours;
using DebugMod.SaveStates;
using DebugMod.UI.Canvas;
using DebugMod.UI.CommandPalette;
using DebugMod.UI.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace DebugMod.UI;

public partial class MainPanel : CanvasPanel
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

    partial void AppendSkillsAndUpgrades();
    partial void AppendEquippableItems();

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
        AppendToggleControl("MODUI_TOGGLECOMMANDPALETTE", () => CommandPaletteController.IsOpen, BindableFunctions.ToggleCommandPalette);
        AppendRow(1, 1);
        AppendToggleControl("MODUI_TOGGLEMAINPANEL", () => DebugMod.settings.MainPanelVisible, BindableFunctions.ToggleMainPanel);
        AppendToggleControl("MODUI_TOGGLEENEMIESPANEL", () => DebugMod.settings.EnemiesPanelVisible, BindableFunctions.ToggleEnemiesPanel);
        AppendRow(1, 1);
        AppendToggleControl("MODUI_TOGGLECONSOLEPANEL", () => DebugMod.settings.ConsoleVisible, BindableFunctions.ToggleConsolePanel);
        AppendToggleControl("MODUI_TOGGLESAVESTATESPANEL", () => DebugMod.settings.SaveStatePanelVisible, BindableFunctions.ToggleSaveStatePanel);
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

        AppendSectionHeader("CATEGORY_SAVESTATES");

        AppendRow(1, 1);
        CanvasPanel importPack = null;
        importPack = AppendBasicControl("SAVESTATES_IMPORTPACK", () => ImportPackDialog.Instance.Toggle(importPack));
        CanvasPanel exportPack = null;
        exportPack = AppendBasicControl("SAVESTATES_EXPORTPACK", () => ExportPackDialog.Instance.Toggle(exportPack));

        AppendRow(1, 1);
        AppendToggleControl("SAVESTATES_SAVESTATEONDEATH", () => DebugMod.stateOnDeath, BindableFunctions.LoadStateOnDeath);
        AppendBasicControl("SAVESTATES_OPENSAVESTATESFOLDER", () => OpenFolder(SaveStateManager.saveStatesBaseDirectory));

        AppendRow(1, 1);
        AppendToggleControl("SAVESTATES_OVERRIDELOADLOCKOUT", () => DebugMod.overrideLoadLockout, BindableFunctions.OverrideLoadLockout);
        AppendBasicControl("SAVESTATES_OPENPACKSFOLDER", () => OpenFolder(SaveStateManager.packsBaseDirectory));

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

        AppendSkillsAndUpgrades();

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
        AppendIncrementTile("MASKSANDSPOOLS_SPOOLS", () => PlayerData.instance.manaMax, SetMaxSilk, image: "Inv_Spool", min: 9, max: 18);
        static void SetMaxSilk(int value)
        {
            PlayerData.instance.manaMax = value;
            PlayerData.instance.mana = Math.Min(PlayerData.instance.mana, PlayerData.instance.manaMax);
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
        AppendIncrementTile("MASKSANDSPOOLS_SPOOLFRAGMENTS", () => PlayerData.instance.manaFragments,
            value => PlayerData.instance.manaFragments = value, "Inv_SpoolFragment", max: 1,
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
        AppendIncrementTile("MASKSANDSPOOLS_SILK", () => PlayerData.instance.mana, SetSilk, image: "Inv_Silk");
        static void SetSilk(int value)
        {
            if (value > PlayerData.instance.mana)
            {
                HeroController.instance.AddMana(value - PlayerData.instance.mana, true);
            }
            else if (value < PlayerData.instance.mana)
            {
                HeroController.instance.TakeMana(PlayerData.instance.mana - value);
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

        AppendEquippableItems();

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

        AppendSectionHeader("CATEGORY_WORLD");
        AppendRow(1, 1);
        AppendBasicControl("WORLD_ALLMAPS", BindableFunctions.UnlockAllMaps);
        AppendBasicControl("WORLD_ALLFASTTRAVEL", BindableFunctions.UnlockAllFastTravel);
        AppendRow(1, 1);
        AppendToggleControl("WORLD_DEEPDOCKSBELLWAY", () => PlayerData.instance.UnlockedDocksStation,
            () => PlayerData.instance.UnlockedDocksStation = !PlayerData.instance.UnlockedDocksStation);
        AppendToggleControl("WORLD_FARFIELDSBELLWAY", () => PlayerData.instance.UnlockedBoneforestEastStation,
            () => PlayerData.instance.UnlockedBoneforestEastStation = !PlayerData.instance.UnlockedBoneforestEastStation);
        AppendRow(1, 1);
        AppendToggleControl("WORLD_GREYMOORBELLWAY", () => PlayerData.instance.UnlockedGreymoorStation,
            () => PlayerData.instance.UnlockedGreymoorStation = !PlayerData.instance.UnlockedGreymoorStation);
        AppendToggleControl("WORLD_BELLHARTBELLWAY", () => PlayerData.instance.UnlockedBelltownStation,
            () => PlayerData.instance.UnlockedBelltownStation = !PlayerData.instance.UnlockedBelltownStation);
        AppendRow(1, 1);
        AppendToggleControl("WORLD_SHELLWOODBELLWAY", () => PlayerData.instance.UnlockedShellwoodStation,
            () => PlayerData.instance.UnlockedShellwoodStation = !PlayerData.instance.UnlockedShellwoodStation);
        AppendToggleControl("WORLD_BLASTEDSTEPSBELLWAY", () => PlayerData.instance.UnlockedCoralTowerStation,
            () => PlayerData.instance.UnlockedCoralTowerStation = !PlayerData.instance.UnlockedCoralTowerStation);
        AppendRow(1, 1);
        AppendToggleControl("WORLD_THESLABBELLWAY", () => PlayerData.instance.UnlockedPeakStation,
            () => PlayerData.instance.UnlockedPeakStation = !PlayerData.instance.UnlockedPeakStation);
        AppendToggleControl("WORLD_GRANDBELLWAY", () => PlayerData.instance.UnlockedCityStation,
            () => PlayerData.instance.UnlockedCityStation = !PlayerData.instance.UnlockedCityStation);
        AppendRow(1, 1);
        AppendToggleControl("WORLD_BILEWATERBELLWAY", () => PlayerData.instance.UnlockedShadowStation,
            () => PlayerData.instance.UnlockedShadowStation = !PlayerData.instance.UnlockedShadowStation);
        AppendToggleControl("WORLD_PUTRIFIEDDUCTSBELLWAY", () => PlayerData.instance.UnlockedAqueductStation,
            () => PlayerData.instance.UnlockedAqueductStation = !PlayerData.instance.UnlockedAqueductStation);
        AppendRow(1, 1);
        AppendToggleControl("WORLD_MEMORIUMVENTRICA", () => PlayerData.instance.UnlockedArboriumTube,
            () => PlayerData.instance.UnlockedArboriumTube = !PlayerData.instance.UnlockedArboriumTube);
        AppendToggleControl("WORLD_HIGHHALLSVENTRICA", () => PlayerData.instance.UnlockedHangTube,
            () => PlayerData.instance.UnlockedHangTube = !PlayerData.instance.UnlockedHangTube);
        AppendRow(1, 1);
        AppendToggleControl("WORLD_FIRSTSHRINEVENTRICA", () => PlayerData.instance.UnlockedEnclaveTube,
            () => PlayerData.instance.UnlockedEnclaveTube = !PlayerData.instance.UnlockedEnclaveTube);
        AppendToggleControl("WORLD_CHORALCHAMBERSVENTRICA", () => PlayerData.instance.UnlockedSongTube,
            () => PlayerData.instance.UnlockedSongTube = !PlayerData.instance.UnlockedSongTube);
        AppendRow(1, 1);
        AppendToggleControl("WORLD_GRANDBELLWAYVENTRICA", () => PlayerData.instance.UnlockedCityBellwayTube,
            () => PlayerData.instance.UnlockedCityBellwayTube = !PlayerData.instance.UnlockedCityBellwayTube);
        AppendToggleControl("WORLD_UNDERWORKSVENTRICA", () => PlayerData.instance.UnlockedUnderTube,
            () => PlayerData.instance.UnlockedUnderTube = !PlayerData.instance.UnlockedUnderTube);

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
                DebugMod.bindUpdated += (name, _) => UpdateKeycodeText(name);
                GUIController.RebindTargetChanged += UpdateKeycodeText;

                CanvasButton edit = builder.AppendSquare(new CanvasButton("Edit"));
                edit.ImageOnly(UICommon.images["IconDotCircled"]);
                edit.OnClicked += () => GUIController.StartRebind(action.Name);

                builder.AppendPadding(UICommon.Margin);

                CanvasButton clear = builder.AppendSquare(new CanvasButton("Clear"));
                clear.ImageOnly(UICommon.images["IconX"]);
                clear.OnClicked += () => DebugMod.UpdateBind(action.Name, null);

                builder.AppendPadding(UICommon.Margin);

                CanvasButton run = builder.AppendSquare(new CanvasButton("Run"));
                run.ImageOnly(UICommon.images["IconRun"]);
                run.OnClicked += action.Action;
                continue;

                void UpdateKeycodeText(string name)
                {
                    if (name == action.Name)
                    {
                        keycode.Text = KeybindDialog.GetKeycodeText(action.Name);
                    }
                }
            }
        }
    }

    private static void OpenFolder(string path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            ProcessStartInfo info = new("xdg-open");
            info.ArgumentList.Add(path);
            info.UseShellExecute = false; // required if setting environment variables
            info.EnvironmentVariables.Remove("LD_PRELOAD");
            Process.Start(info);
            return;
        }

        Process.Start(path);
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
        lastRowCount = 0;

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
