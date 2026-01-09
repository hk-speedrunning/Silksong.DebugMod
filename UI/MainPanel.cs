using DebugMod.Helpers;
using DebugMod.MonoBehaviours;
using DebugMod.UI.Canvas;
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
    public static int KeybindListingHeight => UICommon.ScaleHeight(16);

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
    private PanelBuilder currentRow;
    private int rowCounter;
    private int[] rowWidths;
    private int rowIndex;

    public static void BuildPanel()
    {
        Instance = new MainPanel();
        Instance.Build();
    }

    public MainPanel() : base(nameof(MainPanel))
    {
        LocalPosition = new Vector2(Screen.width - UICommon.ScreenMargin - UICommon.RightSideWidth, UICommon.ScreenMargin);
        Size = new Vector2(UICommon.RightSideWidth, UICommon.MainPanelHeight);
        OnUpdate += Update;

        AddTab("Gameplay");

        AppendSectionHeader("Cheats");
        AppendRow(1, 1, 1);
        AppendToggleControl("Noclip", () => DebugMod.noclip, BindableFunctions.ToggleNoclip);
        AppendToggleControl("Invincibility", () => DebugMod.playerInvincible, BindableFunctions.ToggleInvincibility);
        AppendToggleControl("Infinite Jump", () => PlayerData.instance.infiniteAirJump, BindableFunctions.ToggleInfiniteJump);
        AppendRow(1, 1, 1);
        AppendToggleControl("Infinite HP", () => DebugMod.infiniteHP, BindableFunctions.ToggleInfiniteHP);
        AppendToggleControl("Infinite Silk", () => DebugMod.infiniteSilk, BindableFunctions.ToggleInfiniteSilk);
        AppendToggleControl("Infinite Tools", () => DebugMod.infiniteTools, BindableFunctions.ToggleInfiniteTools);
        AppendRow(2, 1);
        AppendToggleControl("Toggle Hero Collider", HeroColliderToggled, BindableFunctions.ToggleHeroCollider);
        static bool HeroColliderToggled()
        {
            Collider2D heroCollider = DebugMod.RefHeroCollider;
            if (!heroCollider) return false;
            return !heroCollider.enabled;
        }
        AppendBasicControl("Kill All", BindableFunctions.KillAll);

        AppendSectionHeader("Mod UI");
        AppendRow(1, 1);
        AppendToggleControl("Toggle All UI", () => true, BindableFunctions.ToggleAllPanels);
        AppendToggleControl("Toggle Main Panel", () => DebugMod.settings.MainPanelVisible, BindableFunctions.ToggleMainPanel);
        AppendRow(1, 1);
        AppendToggleControl("Toggle Enemies Panel", () => DebugMod.settings.EnemiesPanelVisible, BindableFunctions.ToggleEnemiesPanel);
        AppendToggleControl("Toggle Console Panel", () => DebugMod.settings.ConsoleVisible, BindableFunctions.ToggleConsolePanel);
        AppendRow(1, 1);
        AppendToggleControl("Toggle Savestates Panel", () => DebugMod.settings.SaveStatePanelVisible, BindableFunctions.ToggleSaveStatePanel);
        AppendToggleControl("Expand/Collapse Savestates", () => DebugMod.settings.SaveStatePanelExpanded, BindableFunctions.ToggleExpandedSaveStatePanel);
        AppendRow(1, 1);
        AppendToggleControl("Toggle Info Panel", () => DebugMod.settings.InfoPanelVisible, BindableFunctions.ToggleInfoPanel);
        AppendToggleControl("Always Show Cursor", () => DebugMod.settings.ShowCursorWhileUnpaused, BindableFunctions.ToggleAlwaysShowCursor);

        AppendSectionHeader("Time");
        AppendRow(1, 1);
        AppendBasicControl("Increase Timescale", BindableFunctions.TimescaleUp);
        AppendBasicControl("Decrease Timescale", BindableFunctions.TimescaleDown);
        AppendRow(1, 1);
        AppendToggleControl("Freeze Game", () => DebugMod.PauseGameNoUIActive, BindableFunctions.PauseGameNoUI);
        AppendToggleControl("Force Pause", () =>
            DebugMod.forcePaused && GameManager.instance.isPaused, BindableFunctions.ForcePause);
        AppendRow(1, 1);
        AppendToggleControl("Toggle Frame Advance", () =>
            DebugMod.frameAdvanceActive && (Time.timeScale == 0 || DebugMod.advancingFrame), BindableFunctions.ToggleFrameAdvance);
        AppendBasicControl("Advance Frame", BindableFunctions.AdvanceFrame);
        AppendRow(1);
        AppendBasicControl("Reset Frame Counter", BindableFunctions.ResetFrameCounter);

        AppendSectionHeader("Visual");
        AppendRow(1, 1);
        AppendToggleControl("Toggle Hitboxes", () => DebugMod.settings.ShowHitBoxes != 0, BindableFunctions.ShowHitboxes);
        AppendToggleControl("Force Camera Follow", () => DebugMod.cameraFollow, BindableFunctions.ForceCameraFollow);
        AppendRow(1, 1);
        AppendToggleControl("Preview Cocoon Position", CocoonPreviewToggled, BindableFunctions.PreviewCocoonPosition);
        static bool CocoonPreviewToggled()
        {
            CocoonPreviewer previewer = GameManager.instance.GetComponent<CocoonPreviewer>();
            if (!previewer) return false;
            return previewer.previewEnabled;
        }
        AppendToggleControl("Hide Hero", HideHeroToggled, BindableFunctions.HideHero);
        static bool HideHeroToggled()
        {
            GameObject hero = DebugMod.RefKnight;
            if (!hero) return false;
            tk2dSprite sprite = hero.GetComponent<tk2dSprite>();
            if (!sprite) return false;
            return Math.Abs(sprite.color.a) == 0;
        }
        AppendRow(1, 1);
        AppendToggleControl("Toggle HUD", HUDToggled, BindableFunctions.ToggleHUD);
        static bool HUDToggled()
        {
            PlayMakerFSM hud = GameCameras.instance.hudCanvasSlideOut;
            if (!hud) return false;
            return !hud.gameObject.activeInHierarchy;
        }
        AppendToggleControl("Toggle Vignette", () => VisualMaskHelper.VignetteDisabled, BindableFunctions.ToggleVignette);
        AppendRow(1, 1);
        AppendToggleControl("Toggle Hero Light", HeroLightToggled, BindableFunctions.ToggleHeroLight);
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
        AppendToggleControl("Toggle Camera Shake", CameraShakeToggled, BindableFunctions.ToggleCameraShake);
        static bool CameraShakeToggled()
        {
            PlayMakerFSM cameraShake = GameCameras.instance.cameraShakeFSM;
            if (!cameraShake) return false;
            return !cameraShake.enabled;
        }
        AppendRow(1, 1);
        AppendToggleControl("Deactivate Visual Masks", () => VisualMaskHelper.MasksDisabled, BindableFunctions.DoDeactivateVisualMasks);
        AppendBasicControl("Clear White Screen", BindableFunctions.ClearWhiteScreen);
        AppendRow(1, 1, 1);
        AppendBasicControl("Zoom In", BindableFunctions.ZoomIn);
        AppendBasicControl("Zoom Out", BindableFunctions.ZoomOut);
        AppendBasicControl("Reset Zoom", BindableFunctions.ResetZoom);

        AppendSectionHeader("Misc");
        AppendRow(1, 1);
        AppendBasicControl("Set Hazard Respawn", BindableFunctions.SetHazardRespawn);
        AppendBasicControl("Hazard Respawn", BindableFunctions.Respawn);
        AppendRow(1, 1, 1);
        AppendBasicControl("Damage Self", BindableFunctions.SelfDamage);
        AppendBasicControl("Kill Self", BindableFunctions.KillSelf);
        AppendBasicControl("Break Cocoon", BindableFunctions.BreakCocoon);
        AppendRow(1, 1);
        AppendBasicControl("Reset Current Scene Data", BindableFunctions.ResetCurrentScene);
        AppendBasicControl("Block Scene Data Changes", BindableFunctions.BlockCurrentSceneChanges);
        AppendRow(1, 1, 1);
        AppendToggleControl("Toggle Act 3", () => PlayerData.instance.blackThreadWorld, BindableFunctions.ToggleAct3);
        AppendToggleControl("Lock Keybinds", () => DebugMod.KeyBindLock, BindableFunctions.ToggleLockKeyBinds);
        AppendBasicControl("Reset All", BindableFunctions.Reset);

        AddTab("Items");

        AppendSectionHeader("Skills");
        AppendRow(1);
        AppendBasicControl("All Skills", BindableFunctions.GiveAllSkills);
        AppendRow(1, 1);
        AppendToggleControl("Swift Step", () => PlayerData.instance.hasDash, BindableFunctions.ToggleSwiftStep);
        AppendToggleControl("Silk Soar", () => PlayerData.instance.hasSuperJump, BindableFunctions.ToggleSilkSoar);
        AppendRow(1, 1);
        AppendToggleControl("Drifter's Cloak", () => PlayerData.instance.hasBrolly, BindableFunctions.ToggleDriftersCloak);
        AppendToggleControl("Needle Strike", () => PlayerData.instance.hasChargeSlash, BindableFunctions.ToggleNeedleStrike);
        AppendRow(1, 1);
        AppendToggleControl("Cling Grip", () => PlayerData.instance.hasWalljump, BindableFunctions.ToggleClingGrip);
        AppendToggleControl("Needolin", () => PlayerData.instance.hasNeedolin, BindableFunctions.ToggleNeedolin);
        AppendRow(1, 1);
        AppendToggleControl("Clawline", () => PlayerData.instance.hasHarpoonDash, BindableFunctions.ToggleClawline);
        AppendToggleControl("Beastling Call", () => PlayerData.instance.UnlockedFastTravelTeleport, BindableFunctions.ToggleBeastlingCall);
        AppendRow(1, 1);
        AppendToggleControl("Faydown Cloak", () => PlayerData.instance.hasDoubleJump, BindableFunctions.ToggleFaydownCloak);
        AppendToggleControl("Elegy of the Deep", () => PlayerData.instance.hasNeedolinMemoryPowerup, BindableFunctions.ToggleElegyOfTheDeep);

        AppendSectionHeader("Upgrades");
        AppendRow(1, 1);
        AppendBasicControl("Increase Needle Damage", BindableFunctions.IncreaseNeedleDamage);
        AppendBasicControl("Decrease Needle Damage", BindableFunctions.DecreaseNeedleDamage);
        AppendRow(1, 1);
        AppendIncrementControl("Tool Pouches", () => PlayerData.instance.ToolPouchUpgrades, BindableFunctions.IncrementPouches);
        AppendIncrementControl("Crafting Kits", () => PlayerData.instance.ToolKitUpgrades, BindableFunctions.IncrementKits);
        AppendRow(1);
        AppendIncrementControl("Silk Hearts", () => PlayerData.instance.silkRegenMax, BindableFunctions.IncrementSilkHeart);

        AppendSectionHeader("Tools");
        AppendRow(1, 1);
        AppendBasicControl("Unlock All Tools", BindableFunctions.UnlockAllTools);
        AppendBasicControl("Unlock All Crests", BindableFunctions.UnlockAllCrests);
        AppendRow(1);
        AppendBasicControl("Craft Tools", BindableFunctions.CraftTools);

        AppendSectionHeader("Consumables");
        AppendRow(1, 1);
        AppendBasicControl("Give Rosaries", BindableFunctions.GiveRosaries);
        AppendBasicControl("Give Shell Shards", BindableFunctions.GiveShellShards);
        AppendRow(1, 1);
        AppendBasicControl("Give All Memory Lockets", BindableFunctions.GiveMemoryLockets);
        AppendBasicControl("Give All Craftmetal", BindableFunctions.GiveCraftmetal);
        AppendRow(1);
        AppendBasicControl("Give Silkeater", BindableFunctions.GiveSilkeater);

        AppendSectionHeader("Masks and Spools");
        AppendRow(1, 1);
        AppendBasicControl("Give Mask", BindableFunctions.GiveMask);
        AppendBasicControl("Take Mask", BindableFunctions.TakeAwayMask);
        AppendRow(1, 1);
        AppendBasicControl("Give Spool", BindableFunctions.GiveSpool);
        AppendBasicControl("Take Spool", BindableFunctions.TakeAwaySpool);
        AppendRow(1, 1);
        AppendBasicControl("Give Health", BindableFunctions.AddHealth);
        AppendBasicControl("Take Health", BindableFunctions.TakeHealth);
        AppendRow(1, 1);
        AppendBasicControl("Give Silk", BindableFunctions.AddSilk);
        AppendBasicControl("Take Silk", BindableFunctions.TakeSilk);
        AppendRow(1);
        AppendBasicControl("Add Lifeblood", BindableFunctions.Lifeblood);

        AddTab("Keybinds");

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
                using PanelBuilder builder = new(currentTab.AppendFixed(new CanvasPanel(action.Name), KeybindListingHeight));
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
        tab.LocalPosition = new Vector2(0, TabButtonHeight);
        tab.Size = new Vector2(UICommon.RightSideWidth, UICommon.MainPanelHeight - TabButtonHeight);
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

    private PanelBuilder AppendRow(params int[] widths)
    {
        CanvasPanel row = currentTab.AppendFixed(new CanvasPanel(rowCounter.ToString()), UICommon.ControlHeight);
        row.CollapseMode = CollapseMode.AllowNoRenaming;

        PanelBuilder builder = new(row);
        builder.Horizontal = true;

        int totalWidth = (int)row.Size.x;
        int widthUnits = widths.Sum();
        int singleWidth = (totalWidth - UICommon.Margin * (widthUnits - 1)) / widthUnits;

        rowWidths = new int[widths.Length];
        for (int i = 0; i < widths.Length; i++)
        {
            rowWidths[i] = singleWidth * widths[i] + UICommon.Margin * (widths[i] - 1);
        }

        currentRow?.Build();
        currentRow = builder;
        rowCounter++;
        rowIndex = 0;

        return builder;
    }

    private CanvasText AppendSectionHeader(string name)
    {
        currentTab.AppendPadding(SectionEndPadding);

        CanvasText text = currentTab.AppendFixed(new CanvasText(name), SectionHeaderHeight);
        text.Text = name;
        text.Font = UICommon.trajanBold;
        text.FontSize = SectionHeaderFontSize;
        text.Alignment = TextAnchor.MiddleCenter;

        return text;
    }

    private PanelBuilder AppendButtonControl(string name, Action effect, Action<CanvasButton> update)
    {
        PanelBuilder row = currentRow ?? AppendRow(1);

        int width = rowWidths[rowIndex];
        if (rowIndex == rowWidths.Length - 1)
        {
            width = (int)(row.Length() - row.GetCurrentLength());
        }

        if (DebugMod.bindsByMethod.ContainsKey(effect.Method))
        {
            width -= UICommon.ControlHeight - UICommon.BORDER_THICKNESS;
        }

        CanvasButton button = row.AppendFixed(new CanvasButton(name), width);
        button.Text.Text = name;

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
            button.OnUpdate += () =>
            {
                try
                {
                    update(button);
                }
                catch (Exception e)
                {
                    DebugMod.LogError($"Error updating button {button.GetQualifiedName()}: {e}");
                }
            };
        }

        if (DebugMod.bindsByMethod.TryGetValue(effect.Method, out BindAction action))
        {
            UICommon.AppendKeybindButton(row, action);
        }

        rowIndex++;
        if (rowIndex == rowWidths.Length)
        {
            currentRow.Build();
            currentRow = null;
            rowWidths = null;
            rowIndex = 0;
        }
        else
        {
            row.AppendPadding(UICommon.Margin);
        }

        return row;
    }

    private PanelBuilder AppendBasicControl(string name, Action effect) => AppendButtonControl(name, effect, null);

    // TODO: replace this with checkbox
    private PanelBuilder AppendToggleControl(string name, Func<bool> getter, Action effect)
    {
        return AppendButtonControl(name, effect, button =>
        {
            button.Toggled = getter();
        });
    }

    // TODO: replace this with a slider or increment/decrement buttons
    private PanelBuilder AppendIncrementControl(string name, Func<int> getter, Action effect)
    {
        return AppendButtonControl(name, effect, button =>
        {
            button.Text.Text = $"{name}: {getter()}";
        });
    }

    public override void Build()
    {
        currentRow?.Build();
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
            button.Border.Sides &= ~BorderSides.BOTTOM;
            button.Text.Text = tab.Name;
            button.OnClicked += () => DebugMod.settings.MainPanelCurrentTab = tab.Name;
            button.OnUpdate += () => button.Toggled = DebugMod.settings.MainPanelCurrentTab == tab.Name;

            tabX += tabButtonWidth + UICommon.Margin;
        }

        base.Build();
    }

    private void Update()
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
