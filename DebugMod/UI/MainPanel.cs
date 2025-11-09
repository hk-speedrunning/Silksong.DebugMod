using System.Collections.Generic;
using DebugMod.UI.Canvas;
using UnityEngine;

namespace DebugMod.UI;

public class MainPanel : CanvasPanel
{
    public const int TAB_BUTTON_HEIGHT = 20;
    public const int KEYBIND_CATEGORY_HEADER_FONT_SIZE = 20;
    public const int KEYBIND_LISTING_HEIGHT = 15;

    public static MainPanel Instance { get; private set; }

    private static readonly List<string> keybindCategoryOrder =
    [
        "Cheats",
        "Player",
        "Time",
        "Skills",
        "Tools",
        "Items",
        "Consumables",
        "Masks & Spools",
        "Mod UI",
        "Visual",
        "Misc",
        "Savestates",
        "Enemy Panel",
    ];

    private readonly List<CanvasPanel> tabs = [];
    private string currentTab;

    public static void BuildPanel()
    {
        Instance = new MainPanel();
        Instance.Build();
    }

    public MainPanel() : base(nameof(MainPanel))
    {
        LocalPosition = new Vector2(1920f - UICommon.SCREEN_MARGIN - UICommon.RIGHT_SIDE_WIDTH, UICommon.SCREEN_MARGIN);
        Size = new Vector2(UICommon.RIGHT_SIDE_WIDTH, UICommon.MAIN_MENU_HEIGHT);

        CanvasAutoPanel gameplay = AddTab("Gameplay");

        gameplay.AppendSectionHeader("Cheats");
        gameplay.AppendToggleControl("Noclip", () => DebugMod.noclip, BindableFunctions.ToggleNoclip);
        gameplay.AppendToggleControl("Invincibility", () => DebugMod.playerInvincible, BindableFunctions.ToggleInvincibility);
        gameplay.AppendToggleControl("Infinite HP", () => DebugMod.infiniteHP, BindableFunctions.ToggleInfiniteHP);
        gameplay.AppendToggleControl("Infinite Silk", () => DebugMod.infiniteSilk, BindableFunctions.ToggleInfiniteSilk);
        gameplay.AppendToggleControl("Infinite Tools", () => DebugMod.infiniteTools, BindableFunctions.ToggleInfiniteTools);
        gameplay.AppendToggleControl("Infinite Jump", () => PlayerData.instance.infiniteAirJump, BindableFunctions.ToggleInfiniteJump);
        gameplay.AppendBasicControl("Toggle Hero Collider", BindableFunctions.ToggleHeroCollider);
        gameplay.AppendBasicControl("Kill All", BindableFunctions.KillAll);

        gameplay.AppendSectionHeader("Player");
        gameplay.AppendBasicControl("Increase Needle Damage", BindableFunctions.IncreaseNeedleDamage);
        gameplay.AppendBasicControl("Decrease Needle Damage", BindableFunctions.DecreaseNeedleDamage);
        gameplay.AppendBasicControl("Set Hazard Respawn", BindableFunctions.SetHazardRespawn);
        gameplay.AppendBasicControl("Hazard Respawn", BindableFunctions.Respawn);
        gameplay.AppendBasicControl("Damage Self", BindableFunctions.SelfDamage);
        gameplay.AppendBasicControl("Kill Self", BindableFunctions.KillSelf);
        gameplay.AppendBasicControl("Break Cocoon", BindableFunctions.BreakCocoon);

        gameplay.AppendSectionHeader("Time");
        gameplay.AppendBasicControl("Increase Timescale", BindableFunctions.TimescaleUp);
        gameplay.AppendBasicControl("Decrease Timescale", BindableFunctions.TimescaleDown);
        gameplay.AppendBasicControl("Freeze Game", BindableFunctions.PauseGameNoUI);
        gameplay.AppendBasicControl("Force Pause", BindableFunctions.ForcePause);
        gameplay.AppendBasicControl("Toggle Frame Advance", BindableFunctions.ToggleFrameAdvance);
        gameplay.AppendBasicControl("Advance Frame", BindableFunctions.AdvanceFrame);
        gameplay.AppendBasicControl("Reset Frame Counter", BindableFunctions.ResetFrameCounter);

        CanvasAutoPanel items = AddTab("Items");

        items.AppendSectionHeader("Skills");
        items.AppendBasicControl("All Skills", BindableFunctions.GiveAllSkills);
        items.AppendIncrementControl("Silk Hearts", () => PlayerData.instance.silkRegenMax, BindableFunctions.IncrementSilkHeart);
        items.AppendToggleControl("Swift Step", () => PlayerData.instance.hasDash, BindableFunctions.ToggleSwiftStep);
        items.AppendToggleControl("Drifter's Cloak", () => PlayerData.instance.hasBrolly, BindableFunctions.ToggleDriftersCloak);
        items.AppendToggleControl("Cling Grip", () => PlayerData.instance.hasWalljump, BindableFunctions.ToggleClingGrip);
        items.AppendToggleControl("Needolin", () => PlayerData.instance.hasNeedolin, BindableFunctions.ToggleNeedolin);
        items.AppendToggleControl("Clawline", () => PlayerData.instance.hasHarpoonDash, BindableFunctions.ToggleClawline);
        items.AppendToggleControl("Faydown Cloak", () => PlayerData.instance.hasDoubleJump, BindableFunctions.ToggleFaydownCloak);
        items.AppendToggleControl("Silk Soar", () => PlayerData.instance.hasSuperJump, BindableFunctions.ToggleSilkSoar);
        items.AppendToggleControl("Beastling Call", () => PlayerData.instance.UnlockedFastTravelTeleport, BindableFunctions.ToggleBeastlingCall);
        items.AppendToggleControl("Elegy of the Deep", () => PlayerData.instance.hasNeedolinMemoryPowerup, BindableFunctions.ToggleElegyOfTheDeep);
        items.AppendToggleControl("Needle Strike", () => PlayerData.instance.hasChargeSlash, BindableFunctions.ToggleNeedleStrike);

        items.AppendSectionHeader("Tools");
        items.AppendBasicControl("Unlock All Tools", BindableFunctions.UnlockAllTools);
        items.AppendBasicControl("Unlock All Crests", BindableFunctions.UnlockAllCrests);
        items.AppendIncrementControl("Tool Pouches", () => PlayerData.instance.ToolPouchUpgrades, BindableFunctions.IncrementPouches);
        items.AppendIncrementControl("Crafting Kits", () => PlayerData.instance.ToolKitUpgrades, BindableFunctions.IncrementKits);
        items.AppendBasicControl("Craft Tools", BindableFunctions.CraftTools);

        items.AppendSectionHeader("Consumables");
        items.AppendBasicControl("Give Rosaries", BindableFunctions.GiveRosaries);
        items.AppendBasicControl("Give Shell Shards", BindableFunctions.GiveShellShards);
        items.AppendBasicControl("Give All Memory Lockets", BindableFunctions.GiveMemoryLockets);
        items.AppendBasicControl("Give All Craftmetal", BindableFunctions.GiveCraftmetal);
        items.AppendBasicControl("Give Silkeater", BindableFunctions.GiveSilkeater);

        items.AppendSectionHeader("Masks and Spools");
        items.AppendBasicControl("Give Mask", BindableFunctions.GiveMask);
        items.AppendBasicControl("Take Mask", BindableFunctions.TakeAwayMask);
        items.AppendBasicControl("Give Spool", BindableFunctions.GiveSpool);
        items.AppendBasicControl("Take Spool", BindableFunctions.TakeAwaySpool);
        items.AppendBasicControl("Give Health", BindableFunctions.AddHealth);
        items.AppendBasicControl("Take Health", BindableFunctions.TakeHealth);
        items.AppendBasicControl("Give Silk", BindableFunctions.AddSilk);
        items.AppendBasicControl("Take Silk", BindableFunctions.TakeSilk);
        items.AppendBasicControl("Add Lifeblood", BindableFunctions.Lifeblood);

        CanvasAutoPanel other = AddTab("Other");

        other.AppendSectionHeader("Mod UI");
        other.AppendBasicControl("Toggle All UI", BindableFunctions.ToggleAllPanels);
        other.AppendBasicControl("Toggle Main Panel", BindableFunctions.ToggleMainPanel);
        other.AppendBasicControl("Toggle Console", BindableFunctions.ToggleConsole);
        other.AppendBasicControl("Toggle Enemies Panel", BindableFunctions.ToggleEnemyPanel);
        other.AppendBasicControl("Toggle Info Panel", BindableFunctions.ToggleInfoPanel);
        other.AppendBasicControl("Always Show Cursor", BindableFunctions.ToggleAlwaysShowCursor);

        other.AppendSectionHeader("Visual");
        other.AppendBasicControl("Toggle Hitboxes", BindableFunctions.ShowHitboxes);
        other.AppendBasicControl("Force Camera Follow", BindableFunctions.ForceCameraFollow);
        other.AppendBasicControl("Preview Cocoon Position", BindableFunctions.PreviewCocoonPosition);
        other.AppendBasicControl("Hide Hero", BindableFunctions.HideHero);
        other.AppendBasicControl("Toggle HUD", BindableFunctions.ToggleHUD);
        other.AppendBasicControl("Toggle Vignette", BindableFunctions.ToggleVignette);
        other.AppendBasicControl("Toggle Hero Light", BindableFunctions.ToggleHeroLight);
        other.AppendBasicControl("Toggle Camera Shake", BindableFunctions.ToggleCameraShake);
        other.AppendBasicControl("Deactivate Visual Masks", BindableFunctions.DoDeactivateVisualMasks);
        other.AppendBasicControl("Clear White Screen", BindableFunctions.ClearWhiteScreen);
        other.AppendBasicControl("Zoom In", BindableFunctions.ZoomIn);
        other.AppendBasicControl("Zoom Out", BindableFunctions.ZoomOut);
        other.AppendBasicControl("Reset Zoom", BindableFunctions.ResetZoom);

        other.AppendSectionHeader("Misc");
        other.AppendBasicControl("Toggle Act 3", BindableFunctions.ToggleAct3);
        other.AppendBasicControl("Reset Current Scene Data", BindableFunctions.ResetCurrentScene);
        other.AppendBasicControl("Block Scene Data Changes", BindableFunctions.BlockCurrentSceneChanges);
        other.AppendBasicControl("Lock Keybinds", BindableFunctions.ToggleLockKeyBinds);
        other.AppendBasicControl("Reset Cheats", BindableFunctions.Reset);

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

        CanvasAutoPanel keybinds = AddTab("Keybinds");

        foreach (string category in keybindCategoryOrder)
        {
            CanvasText header = keybinds.AppendSectionHeader(category);
            header.FontSize = KEYBIND_CATEGORY_HEADER_FONT_SIZE;
            header.Alignment = TextAnchor.MiddleLeft;

            foreach (BindAction action in keybindData[category])
            {
                CanvasControl control = keybinds.Append(new CanvasControl(action.Name), KEYBIND_LISTING_HEIGHT);

                CanvasText keybindName = control.AppendFlex(new CanvasText("KeybindName"));
                keybindName.Text = action.Name;
                keybindName.Alignment = TextAnchor.MiddleLeft;

                CanvasText keycode = control.AppendFlex(new CanvasText("Keycode"));
                keycode.Alignment = TextAnchor.MiddleLeft;
                keycode.OnUpdate += () => keycode.Text = KeybindContextPanel.GetKeycodeText(action.Name);

                CanvasButton edit = control.AppendSquare(new CanvasButton("Edit"));
                edit.ImageOnly(UICommon.images["Scrollbar_point"]);
                edit.OnClicked += () => DebugMod.settings.binds[action.Name] = KeyCode.None;

                CanvasButton clear = control.AppendSquare(new CanvasButton("Clear"));
                clear.ImageOnly(UICommon.images["ButtonDel"]);
                clear.OnClicked += () => DebugMod.settings.binds.Remove(action.Name);
            }
        }
    }

    private CanvasAutoPanel AddTab(string name)
    {
        CanvasPanel tab = Add(new CanvasPanel(name));
        tab.LocalPosition = new Vector2(0, TAB_BUTTON_HEIGHT);
        tab.Size = new Vector2(UICommon.RIGHT_SIDE_WIDTH, UICommon.MAIN_MENU_HEIGHT - TAB_BUTTON_HEIGHT);
        UICommon.AddBackground(tab);
        tabs.Add(tab);

        CanvasScrollView scroll = tab.Add(new CanvasScrollView("ScrollView"));
        scroll.Margin = new Vector2(UICommon.BORDER_THICKNESS, UICommon.BORDER_THICKNESS);
        scroll.Size = tab.Size;

        CanvasAutoPanel panel = scroll.SetContent(new CanvasAutoPanel("Panel"));
        panel.Size = tab.Size;

        return panel;
    }

    public override void Build()
    {
        float tabButtonWidth = (Size.x - UICommon.MARGIN * (tabs.Count - 1)) / tabs.Count;
        float tabX = 0;

        foreach (CanvasPanel tab in tabs)
        {
            // Created after the tabs themselves so they get input priority over offscreen controls
            CanvasButton button = Add(new CanvasButton($"{tab.Name}TabButton"));
            button.LocalPosition = new Vector2(tabX, 0);
            button.Size = new Vector2(tabButtonWidth, TAB_BUTTON_HEIGHT);
            button.SetImage(UICommon.panelBG);
            button.Border.Sides &= ~BorderSides.BOTTOM;
            button.Text.Text = tab.Name;
            button.OnClicked += () => currentTab = tab.Name;

            tabX += tabButtonWidth + UICommon.MARGIN;
        }

        currentTab = tabs[0].Name;

        base.Build();
    }

    public override void Update()
    {
        foreach (CanvasPanel tab in tabs)
        {
            tab.ActiveSelf = currentTab == tab.Name;
        }

        base.Update();
    }
}
