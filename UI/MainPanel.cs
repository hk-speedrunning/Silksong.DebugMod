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
        AppendToggleControl("Toggle Hero Collider", () => DebugMod.heroColliderDisabled, BindableFunctions.ToggleHeroCollider);
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
        AppendToggleControl("Freeze Game", () => TimeScale.Frozen, BindableFunctions.PauseGameNoUI);
        AppendBasicControl("Advance Frame", BindableFunctions.AdvanceFrame);
        AppendRow(1, 1);
        AppendToggleControl("Force Pause", () =>
            DebugMod.forcePaused && GameManager.instance.isPaused, BindableFunctions.ForcePause);
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
        AppendRow(1, 1, 1);
        AppendToggleControl("Toggle HUD", HUDToggled, BindableFunctions.ToggleHUD);
        static bool HUDToggled()
        {
            PlayMakerFSM hud = GameCameras.instance.hudCanvasSlideOut;
            if (!hud) return false;
            return !hud.gameObject.activeInHierarchy;
        }
        AppendToggleControl("Toggle Vignette", () => VisualMaskHelper.vignetteDisabled, BindableFunctions.ToggleVignette);
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
        AppendToggleControl("Toggle Camera Shake", CameraShakeToggled, BindableFunctions.ToggleCameraShake);
        static bool CameraShakeToggled()
        {
            PlayMakerFSM cameraShake = GameCameras.instance.cameraShakeFSM;
            if (!cameraShake) return false;
            return !cameraShake.enabled;
        }
        AppendToggleControl("Deactivate Visual Masks", () => VisualMaskHelper.masksDisabled, BindableFunctions.DoDeactivateVisualMasks);
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
        AppendTileRow(5);
        AppendLabeledTile("Swift Step", () => PlayerData.instance.hasDash, BindableFunctions.ToggleSwiftStep, "Skill_SwiftStep");
        AppendLabeledTile("Cling Grip", () => PlayerData.instance.hasWalljump, BindableFunctions.ToggleClingGrip, "Skill_ClingGrip");
        AppendLabeledTile("Needolin", () => PlayerData.instance.hasNeedolin, BindableFunctions.ToggleNeedolin, "Skill_Needolin");
        AppendLabeledTile("Clawline", () => PlayerData.instance.hasHarpoonDash, BindableFunctions.ToggleClawline, "Skill_Clawline");
        AppendLabeledTile("Silk Soar", () => PlayerData.instance.hasSuperJump, BindableFunctions.ToggleSilkSoar, "Skill_SilkSoar");
        AppendTileRow(5);
        AppendLabeledTile("Drifter's Cloak", () => PlayerData.instance.hasBrolly, BindableFunctions.ToggleDriftersCloak, "Skill_DriftersCloak");
        AppendLabeledTile("Faydown Cloak", () => PlayerData.instance.hasDoubleJump, BindableFunctions.ToggleFaydownCloak, "Skill_FaydownCloak");
        AppendLabeledTile("Needle Strike", () => PlayerData.instance.hasChargeSlash, BindableFunctions.ToggleNeedleStrike, "Skill_NeedleStrike");
        AppendLabeledTile("Beastling Call", () => PlayerData.instance.UnlockedFastTravelTeleport, BindableFunctions.ToggleBeastlingCall, "Skill_BeastlingCall");
        AppendLabeledTile("Elegy of the Deep", () => PlayerData.instance.hasNeedolinMemoryPowerup, BindableFunctions.ToggleElegyOfTheDeep, "Skill_Elegy");

        AppendSectionHeader("Upgrades");
        AppendTileRow(2);
        AppendIncrementTile("Needle Damage", () => PlayerData.instance.nailDamage, BindableFunctions.IncreaseNeedleDamage, BindableFunctions.DecreaseNeedleDamage, "Inv_Needle");
        AppendIncrementTile("Silk Hearts", () => PlayerData.instance.silkRegenMax, BindableFunctions.IncrementSilkHeart, BindableFunctions.DecrementSilkHeart, "Inv_SilkHeart");
        AppendTileRow(2);
        AppendIncrementTile("Crafting Kit", () => PlayerData.instance.ToolKitUpgrades, BindableFunctions.IncrementKits, BindableFunctions.DecrementKits, "Inv_CraftingKit");
        AppendIncrementTile("Tool Pouch", () => PlayerData.instance.ToolPouchUpgrades, BindableFunctions.IncrementPouches, BindableFunctions.DecrementPouches, "Inv_ToolPouch");

        AppendSectionHeader("Tools");
        AppendRow(1, 1);
        AppendBasicControl("Unlock All Tools", BindableFunctions.UnlockAllTools);
        AppendBasicControl("Craft Tools", BindableFunctions.CraftTools);

        foreach (ToolItem tool in ToolItemManager.GetAllTools())
        {
            CanvasPanel tile = AppendLabeledTile(
                tool.name,
                () => tool.IsUnlockedNotHidden,
                () =>
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
            );

            tile.Get<CanvasText>("Label").Text = tool.GetPopupName();
            tile.Get<CanvasImage>("Icon").SetImage(tool.GetPopupIcon());
        }

        AppendSectionHeader("Crests");
        AppendRow(1);
        AppendBasicControl("Unlock All Crests", BindableFunctions.UnlockAllCrests);

        AppendSectionHeader("Consumables");
        AppendRow(1, 1);
        AppendBasicControl("Give Rosaries", BindableFunctions.GiveRosaries);
        AppendBasicControl("Give Shell Shards", BindableFunctions.GiveShellShards);

        /*
        // Probably useful
        static void SetCollectable(string name, int amount)
        {
            if (CollectableItemManager.IsInHiddenMode())
            {
                CollectableItemManager.Instance.AffectItemData(name, (ref CollectableItemsData.Data data) => data.AmountWhileHidden = amount);
            }
            else
            {
                CollectableItemManager.Instance.AffectItemData(name, (ref CollectableItemsData.Data data) => data.Amount = amount);
            }
        }
        */

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

        currentRow = row;
        rowCounter++;
        rowIndex = 0;
    }

    private void ResetRow()
    {
        currentRow = null;
        rowPositions = null;
        rowWidths = null;
        rowIndex = 0;
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
        text.Text = name;
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

        CanvasImage icon = builder.AppendSquare(new CanvasImage("Icon"));
        icon.SetImage(UICommon.images[image]);

        if (includeLabel)
        {
            CanvasText label = builder.AppendFixed(new CanvasText("Label"), ListingHeight * 2);
            label.Alignment = TextAnchor.MiddleCenter;
            label.Text = name;
        }

        builder.Build();

        button.Size = tile.Size;
        row.Size = new Vector2(row.Size.x, Mathf.Max(row.Size.y, tile.Size.y));

        return tile;
    }

    private CanvasPanel AppendIncrementTile(string name, Func<int> getter, Action add, Action remove, string image = "IconX")
    {
        CanvasPanel row = currentRow ?? AppendTileRow(2);

        // Image gets 1/3 of the tile and determines the tile height
        int imageWidth = rowWidths[0] / 3;

        CanvasPanel tile = AppendRowElement(name);
        tile.CollapseMode = CollapseMode.Deny;
        UICommon.AddBackground(tile);

        tile.Size = new Vector2(tile.Size.x, imageWidth + UICommon.Margin * 2);

        PanelBuilder builder = new(tile)
        {
            Horizontal = true,
            Padding = tile.ContentMargin(UICommon.Margin)
        };

        CanvasImage icon = builder.AppendSquare(new CanvasImage("Icon"));
        icon.SetImage(UICommon.images[image]);

        PanelBuilder containerBuilder = new(builder.AppendFlex(new CanvasPanel("Container")));

        containerBuilder.AppendFlexPadding();

        CanvasText label = containerBuilder.AppendFixed(new CanvasText("Label"), ListingHeight);
        label.Alignment = TextAnchor.MiddleCenter;
        label.Text = name;

        containerBuilder.AppendFlexPadding();

        CanvasPanel controlRow = containerBuilder.AppendFixed(new CanvasPanel("ControlRow"), UICommon.ControlHeight);
        PanelBuilder controlBuilder = new(controlRow) { Horizontal = true, InnerPadding = -UICommon.BORDER_THICKNESS };

        var decButton = controlBuilder.AppendSquare(new CanvasButton("Decrement"));
        decButton.SetImage(UICommon.images["IconMinusMin"]);
        decButton.RemoveText();
        decButton.OnClicked += remove;

        var valueButton = controlBuilder.AppendFlex(new CanvasButton("Value"));
        valueButton.SetImage(UICommon.clearBG);
        valueButton.OnUpdate += () => valueButton.Text.Text = getter().ToString();
        valueButton.Text.Alignment = TextAnchor.MiddleCenter;
        valueButton.Border.Sides &= ~BorderSides.LEFT;

        var incButton = controlBuilder.AppendSquare(new CanvasButton("Increment"));
        incButton.SetImage(UICommon.images["IconPlusMin"]);
        incButton.RemoveText();
        incButton.OnClicked += add;
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
            button.Text.Text = tab.Name;
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
