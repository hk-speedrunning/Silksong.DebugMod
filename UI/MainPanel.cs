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
        "Player",
        "Time",
        "Enemy Panel",
        "Skills",
        "Tools",
        "Items",
        "Consumables",
        "Masks & Spools",
        "Mod UI",
        "Visual",
        "Misc",
    ];

    private readonly List<CanvasPanel> tabs = [];

    // Convenience fields for building
    private PanelBuilder currentTab;
    private PanelBuilder currentRow;
    private int rowCounter;
    private int[] relativeWidths;
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

        AddTab("Gameplay");

        AppendSectionHeader("Cheats");
        AppendRow(1, 1, 1);
        AppendToggleControl("Noclip", () => DebugMod.noclip, BindableFunctions.ToggleNoclip);
        AppendToggleControl("Invincibility", () => DebugMod.playerInvincible, BindableFunctions.ToggleInvincibility);
        AppendToggleControl("Infinite HP", () => DebugMod.infiniteHP, BindableFunctions.ToggleInfiniteHP);
        AppendRow(1, 1, 1);
        AppendToggleControl("Infinite Silk", () => DebugMod.infiniteSilk, BindableFunctions.ToggleInfiniteSilk);
        AppendToggleControl("Infinite Tools", () => DebugMod.infiniteTools, BindableFunctions.ToggleInfiniteTools);
        AppendToggleControl("Infinite Jump", () => PlayerData.instance.infiniteAirJump, BindableFunctions.ToggleInfiniteJump);
        AppendRow(2, 1);
        AppendBasicControl("Toggle Hero Collider", BindableFunctions.ToggleHeroCollider);
        AppendBasicControl("Kill All", BindableFunctions.KillAll);

        AppendSectionHeader("Player");
        AppendRow(1, 1);
        AppendBasicControl("Increase Needle Damage", BindableFunctions.IncreaseNeedleDamage);
        AppendBasicControl("Decrease Needle Damage", BindableFunctions.DecreaseNeedleDamage);
        AppendRow(1, 1);
        AppendBasicControl("Set Hazard Respawn", BindableFunctions.SetHazardRespawn);
        AppendBasicControl("Hazard Respawn", BindableFunctions.Respawn);
        AppendRow(1, 1, 1);
        AppendBasicControl("Damage Self", BindableFunctions.SelfDamage);
        AppendBasicControl("Kill Self", BindableFunctions.KillSelf);
        AppendBasicControl("Break Cocoon", BindableFunctions.BreakCocoon);

        AppendSectionHeader("Time");
        AppendRow(1, 1);
        AppendBasicControl("Increase Timescale", BindableFunctions.TimescaleUp);
        AppendBasicControl("Decrease Timescale", BindableFunctions.TimescaleDown);
        AppendRow(1, 1);
        AppendBasicControl("Freeze Game", BindableFunctions.PauseGameNoUI);
        AppendBasicControl("Force Pause", BindableFunctions.ForcePause);
        AppendRow(1, 1);
        AppendBasicControl("Toggle Frame Advance", BindableFunctions.ToggleFrameAdvance);
        AppendBasicControl("Advance Frame", BindableFunctions.AdvanceFrame);
        AppendRow(1);
        AppendBasicControl("Reset Frame Counter", BindableFunctions.ResetFrameCounter);

        AppendSectionHeader("Visual");
        AppendRow(1, 1);
        AppendBasicControl("Toggle Hitboxes", BindableFunctions.ShowHitboxes);
        AppendBasicControl("Force Camera Follow", BindableFunctions.ForceCameraFollow);
        AppendRow(1, 1);
        AppendBasicControl("Preview Cocoon Position", BindableFunctions.PreviewCocoonPosition);
        AppendBasicControl("Hide Hero", BindableFunctions.HideHero);
        AppendRow(1, 1);
        AppendBasicControl("Toggle HUD", BindableFunctions.ToggleHUD);
        AppendBasicControl("Toggle Vignette", BindableFunctions.ToggleVignette);
        AppendRow(1, 1);
        AppendBasicControl("Toggle Hero Light", BindableFunctions.ToggleHeroLight);
        AppendBasicControl("Toggle Camera Shake", BindableFunctions.ToggleCameraShake);
        AppendRow(1, 1);
        AppendBasicControl("Deactivate Visual Masks", BindableFunctions.DoDeactivateVisualMasks);
        AppendBasicControl("Clear White Screen", BindableFunctions.ClearWhiteScreen);
        AppendRow(1, 1, 1);
        AppendBasicControl("Zoom In", BindableFunctions.ZoomIn);
        AppendBasicControl("Zoom Out", BindableFunctions.ZoomOut);
        AppendBasicControl("Reset Zoom", BindableFunctions.ResetZoom);


        AppendSectionHeader("Mod UI");
        AppendRow(1, 1);
        AppendBasicControl("Toggle All UI", BindableFunctions.ToggleAllPanels);
        AppendBasicControl("Toggle Main Panel", BindableFunctions.ToggleMainPanel);
        AppendRow(1, 1);
        AppendBasicControl("Toggle Enemies Panel", BindableFunctions.ToggleEnemiesPanel);
        AppendBasicControl("Toggle Console Panel", BindableFunctions.ToggleConsolePanel);
        AppendRow(1, 1);
        AppendBasicControl("Toggle Savestates Panel", BindableFunctions.ToggleSaveStatePanel);
        AppendBasicControl("Expand/Collapse Savestates", BindableFunctions.ToggleExpandedSaveStatePanel);
        AppendRow(1, 1);
        AppendBasicControl("Toggle Info Panel", BindableFunctions.ToggleInfoPanel);
        AppendBasicControl("Always Show Cursor", BindableFunctions.ToggleAlwaysShowCursor);

        AppendSectionHeader("Misc");
        AppendRow(1, 1);
        AppendBasicControl("Reset Current Scene Data", BindableFunctions.ResetCurrentScene);
        AppendBasicControl("Block Scene Data Changes", BindableFunctions.BlockCurrentSceneChanges);
        AppendRow(1, 1, 1);
        AppendBasicControl("Toggle Act 3", BindableFunctions.ToggleAct3);
        AppendBasicControl("Lock Keybinds", BindableFunctions.ToggleLockKeyBinds);
        AppendBasicControl("Reset All", BindableFunctions.Reset);

        AddTab("Items");

        AppendSectionHeader("Skills");
        AppendBasicControl("All Skills", BindableFunctions.GiveAllSkills);
        AppendIncrementControl("Silk Hearts", () => PlayerData.instance.silkRegenMax, BindableFunctions.IncrementSilkHeart);
        AppendToggleControl("Swift Step", () => PlayerData.instance.hasDash, BindableFunctions.ToggleSwiftStep);
        AppendToggleControl("Drifter's Cloak", () => PlayerData.instance.hasBrolly, BindableFunctions.ToggleDriftersCloak);
        AppendToggleControl("Cling Grip", () => PlayerData.instance.hasWalljump, BindableFunctions.ToggleClingGrip);
        AppendToggleControl("Needolin", () => PlayerData.instance.hasNeedolin, BindableFunctions.ToggleNeedolin);
        AppendToggleControl("Clawline", () => PlayerData.instance.hasHarpoonDash, BindableFunctions.ToggleClawline);
        AppendToggleControl("Faydown Cloak", () => PlayerData.instance.hasDoubleJump, BindableFunctions.ToggleFaydownCloak);
        AppendToggleControl("Silk Soar", () => PlayerData.instance.hasSuperJump, BindableFunctions.ToggleSilkSoar);
        AppendToggleControl("Beastling Call", () => PlayerData.instance.UnlockedFastTravelTeleport, BindableFunctions.ToggleBeastlingCall);
        AppendToggleControl("Elegy of the Deep", () => PlayerData.instance.hasNeedolinMemoryPowerup, BindableFunctions.ToggleElegyOfTheDeep);
        AppendToggleControl("Needle Strike", () => PlayerData.instance.hasChargeSlash, BindableFunctions.ToggleNeedleStrike);

        AppendSectionHeader("Tools");
        AppendBasicControl("Unlock All Tools", BindableFunctions.UnlockAllTools);
        AppendBasicControl("Unlock All Crests", BindableFunctions.UnlockAllCrests);
        AppendIncrementControl("Tool Pouches", () => PlayerData.instance.ToolPouchUpgrades, BindableFunctions.IncrementPouches);
        AppendIncrementControl("Crafting Kits", () => PlayerData.instance.ToolKitUpgrades, BindableFunctions.IncrementKits);
        AppendBasicControl("Craft Tools", BindableFunctions.CraftTools);

        AppendSectionHeader("Consumables");
        AppendBasicControl("Give Rosaries", BindableFunctions.GiveRosaries);
        AppendBasicControl("Give Shell Shards", BindableFunctions.GiveShellShards);
        AppendBasicControl("Give All Memory Lockets", BindableFunctions.GiveMemoryLockets);
        AppendBasicControl("Give All Craftmetal", BindableFunctions.GiveCraftmetal);
        AppendBasicControl("Give Silkeater", BindableFunctions.GiveSilkeater);

        AppendSectionHeader("Masks and Spools");
        AppendBasicControl("Give Mask", BindableFunctions.GiveMask);
        AppendBasicControl("Take Mask", BindableFunctions.TakeAwayMask);
        AppendBasicControl("Give Spool", BindableFunctions.GiveSpool);
        AppendBasicControl("Take Spool", BindableFunctions.TakeAwaySpool);
        AppendBasicControl("Give Health", BindableFunctions.AddHealth);
        AppendBasicControl("Take Health", BindableFunctions.TakeHealth);
        AppendBasicControl("Give Silk", BindableFunctions.AddSilk);
        AppendBasicControl("Take Silk", BindableFunctions.TakeSilk);
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
                keycode.OnUpdate += () => keycode.Text = KeybindContextPanel.GetKeycodeText(action.Name);

                CanvasButton edit = builder.AppendSquare(new CanvasButton("Edit"));
                edit.ImageOnly(UICommon.images["IconDotCircled"]);
                edit.OnClicked += () => DebugMod.settings.binds[action.Name] = KeyCode.None;

                builder.AppendPadding(UICommon.Margin);

                CanvasButton clear = builder.AppendSquare(new CanvasButton("Clear"));
                clear.ImageOnly(UICommon.images["IconX"]);
                clear.OnClicked += () => DebugMod.settings.binds.Remove(action.Name);

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

        currentRow?.Build();
        currentRow = builder;
        rowCounter++;
        relativeWidths = widths;
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

        int widthUnits = relativeWidths.Sum();
        float singleWidth = (row.Length() - UICommon.Margin * (widthUnits - 1)) / widthUnits;

        int units = relativeWidths[rowIndex];
        float width = Mathf.Ceil(singleWidth * units + UICommon.Margin * (units - 1));
        if (DebugMod.bindsByMethod.ContainsKey(effect.Method)) width -= UICommon.ControlHeight;

        CanvasButton button = row.AppendFixed(new CanvasButton(name), width);
        button.Text.Text = name;
        button.OnClicked += effect;
        if (update != null) button.OnUpdate += () => update(button);

        if (DebugMod.bindsByMethod.TryGetValue(effect.Method, out BindAction action))
        {
            CanvasButton keybindButton = row.AppendSquare(new CanvasButton($"{name}Keybind"));
            keybindButton.SetImage(UICommon.images["IconDotOutline"]);
            keybindButton.RemoveText();
            keybindButton.Border.Sides &= ~BorderSides.LEFT;
            keybindButton.OnUpdate += () =>
            {
                if (!DebugMod.settings.binds.ContainsKey(action.Name))
                {
                    keybindButton.SetImage(UICommon.images["IconDotOutline"]);
                }
                else if (DebugMod.settings.binds[action.Name] != KeyCode.None)
                {
                    keybindButton.SetImage(UICommon.images["IconDot"]);
                }
            };
            keybindButton.OnClicked += () => KeybindContextPanel.Instance.Toggle(keybindButton, action.Name);
        }

        rowIndex++;
        if (rowIndex == relativeWidths.Length)
        {
            currentRow.Build();
            currentRow = null;
            relativeWidths = null;
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

    public override void Update()
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

        base.Update();
    }
}
