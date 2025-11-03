using System.Collections.Generic;
using DebugMod.UI.Canvas;
using UnityEngine;

namespace DebugMod.UI;

public class MainPanel : CanvasPanel
{
    public const int TAB_BUTTON_HEIGHT = 20;

    public static MainPanel Instance { get; private set; }

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
        gameplay.AppendControl("Toggle Hero Collider", BindableFunctions.ToggleHeroCollider);

        CanvasAutoPanel items = AddTab("Items");

        items.AppendSectionHeader("Skills");
        items.AppendControl("All Skills", BindableFunctions.GiveAllSkills);
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
        items.AppendControl("Unlock All Tools", BindableFunctions.UnlockAllTools);
        items.AppendControl("Unlock All Crests", BindableFunctions.UnlockAllCrests);
        items.AppendIncrementControl("Tool Pouches", () => PlayerData.instance.ToolPouchUpgrades, BindableFunctions.IncrementPouches);
        items.AppendIncrementControl("Crafting Kits", () => PlayerData.instance.ToolKitUpgrades, BindableFunctions.IncrementKits);
        items.AppendControl("Craft Tools", BindableFunctions.CraftTools);

        items.AppendSectionHeader("Consumables");
        items.AppendControl("Give Rosaries", BindableFunctions.GiveRosaries);
        items.AppendControl("Give Shell Shards", BindableFunctions.GiveShellShards);
        items.AppendControl("Give All Memory Lockets", BindableFunctions.GiveMemoryLockets);
        items.AppendControl("Give All Craftmetal", BindableFunctions.GiveCraftmetal);
        items.AppendControl("Give Silkeater", BindableFunctions.GiveSilkeater);
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
        base.Update();

        ActiveSelf = DebugMod.settings.MainPanelVisible;

        if (ActiveInHierarchy)
        {
            foreach (CanvasPanel tab in tabs)
            {
                tab.ActiveSelf = currentTab == tab.Name;
            }
        }
    }
}
