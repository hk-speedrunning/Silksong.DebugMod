using System.Collections.Generic;
using DebugMod.UI.Canvas;
using UnityEngine;

namespace DebugMod.UI;

public class MainPanel : CanvasPanel
{
    public const int TAB_BUTTON_HEIGHT = 20;

    public static MainPanel Instance { get; private set; }

    private readonly List<CanvasAutoPanel> tabs = [];
    private string currentTab;

    public static void BuildPanel()
    {
        Instance = new MainPanel();
        Instance.Build();
    }

    public MainPanel() : base(nameof(MainPanel), null)
    {
        LocalPosition = new Vector2(1920f - UICommon.SCREEN_MARGIN - UICommon.RIGHT_SIDE_WIDTH, UICommon.SCREEN_MARGIN);
        Size = new Vector2(UICommon.RIGHT_SIDE_WIDTH, UICommon.MAIN_MENU_HEIGHT);

        CanvasAutoPanel gameplay = AddTab("Gameplay");

        gameplay.AppendToggleControl("Noclip", () => DebugMod.noclip, BindableFunctions.ToggleNoclip);
        gameplay.AppendToggleControl("Invincibility", () => DebugMod.playerInvincible, BindableFunctions.ToggleInvincibility);
        gameplay.AppendToggleControl("Infinite HP", () => DebugMod.infiniteHP, BindableFunctions.ToggleInfiniteHP);
        gameplay.AppendToggleControl("Infinite Silk", () => DebugMod.infiniteSilk, BindableFunctions.ToggleInfiniteSilk);
        gameplay.AppendToggleControl("Infinite Tools", () => DebugMod.infiniteTools, BindableFunctions.ToggleInfiniteTools);
        gameplay.AppendToggleControl("Infinite Jump", () => PlayerData.instance.infiniteAirJump, BindableFunctions.ToggleInfiniteJump);
        gameplay.AppendControl("Disable Hero Collider", BindableFunctions.ToggleHeroCollider);

        CanvasAutoPanel items = AddTab("Items");

        items.AppendControl("All Skills", BindableFunctions.GiveAllSkills);

        /*
        //Skills panel buttons
        // TODO: use images instead of text?
        skillsPanel.AddButton("All Skills", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 30f), Vector2.zero, BindableFunctions.GiveAllSkills, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "All Skills", 10);
        skillsPanel.AddButton("Silk Heart", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 60f), Vector2.zero, BindableFunctions.IncrementSilkHeart, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Silk Hearts: " + PlayerData.instance.silkRegenMax, 10);
        skillsPanel.AddButton("Swift Step", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 90f), Vector2.zero, BindableFunctions.ToggleSwiftStep, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Swift Step", 10);
        skillsPanel.AddButton("Cloak", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 120f), Vector2.zero, BindableFunctions.IncrementCloak, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Drifter's", 10);
        skillsPanel.AddButton("Cling Grip", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 150f), Vector2.zero, BindableFunctions.ToggleClingGrip, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Cling Grip", 10);
        skillsPanel.AddButton("Needolin", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 180f), Vector2.zero, BindableFunctions.ToggleNeedolin, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Needolin", 10);
        skillsPanel.AddButton("Clawline", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 210f), Vector2.zero, BindableFunctions.ToggleClawline, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Clawline", 10);
        skillsPanel.AddButton("Silk Soar", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 240f), Vector2.zero, BindableFunctions.ToggleSilkSoar, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Silk Soar", 10);
        skillsPanel.AddButton("Beastling Call", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 270f), Vector2.zero, BindableFunctions.ToggleBeastlingCall, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Beastling", 10);
        skillsPanel.AddButton("Elegy of the Deep", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 300f), Vector2.zero, BindableFunctions.ToggleElegyOfTheDeep, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Elegy", 10);
        skillsPanel.AddButton("Needle Strike", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 330f), Vector2.zero, BindableFunctions.ToggleNeedleStrike, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Needle Strike", 10);

        //Tools panel
        toolsPanel.AddButton("All Tools", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 30f), Vector2.zero, BindableFunctions.UnlockAllTools, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "All Tools", 10);
        toolsPanel.AddButton("All Crests", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 60f), Vector2.zero, BindableFunctions.UnlockAllCrests, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "All Crests", 10);
        toolsPanel.AddButton("Tool Pouches", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 90f), Vector2.zero, BindableFunctions.IncrementPouches, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Pouches: " + PlayerData.instance.ToolPouchUpgrades, 10);
        toolsPanel.AddButton("Crafting Kits", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 120f), Vector2.zero, BindableFunctions.IncrementKits, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Kits: " + PlayerData.instance.ToolKitUpgrades, 10);
        toolsPanel.AddButton("Infinite Tools", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 150f), Vector2.zero, BindableFunctions.ToggleInfiniteTools, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Infinite Uses", 10);
        toolsPanel.AddButton("Craft Tools", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 180f), Vector2.zero, BindableFunctions.CraftTools, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Craft Tools", 10);

        //Items panel
        itemsPanel.AddButton("Rosaries", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 30f), Vector2.zero, BindableFunctions.GiveRosaries, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Rosaries", 10);
        itemsPanel.AddButton("Shell Shards", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 60f), Vector2.zero, BindableFunctions.GiveShellShards, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Shards", 10);
        itemsPanel.AddButton("Memory Lockets", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 90f), Vector2.zero, BindableFunctions.GiveMemoryLockets, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Lockets", 10);
        itemsPanel.AddButton("Craftmetal", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 120f), Vector2.zero, BindableFunctions.GiveCraftmetal, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Craftmetal", 10);
        itemsPanel.AddButton("Silkeater", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 150f), Vector2.zero, BindableFunctions.GiveSilkeater, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Silkeater", 10);

        Rect buttonRect = new Rect(0, 0, UICommon.images["ButtonRect"].width, UICommon.images["ButtonRect"].height);

        //Main buttons
        AddButton("Hide Menu", UICommon.images["ButtonRect"], new Vector2(46f, 28f), Vector2.zero, HideMenuClicked, buttonRect, UICommon.trajanBold, "Hide Menu");
        AddButton("Kill All", UICommon.images["ButtonRect"], new Vector2(146f, 28f), Vector2.zero, BindableFunctions.KillAll, buttonRect, UICommon.trajanBold, "Kill All");
        AddButton("Set Spawn", UICommon.images["ButtonRect"], new Vector2(246f, 28f), Vector2.zero, BindableFunctions.SetHazardRespawn, buttonRect, UICommon.trajanBold, "Set Spawn");
        AddButton("Respawn", UICommon.images["ButtonRect"], new Vector2(346f, 28f), Vector2.zero, BindableFunctions.Respawn, buttonRect, UICommon.trajanBold, "Respawn");
        AddButton("Other", UICommon.images["ButtonRect"], new Vector2(446f, 28f), Vector2.zero, () => TogglePanel("Other Panel"), buttonRect, UICommon.trajanBold, "Other");
        AddButton("Cheats", UICommon.images["ButtonRect"], new Vector2(46f, 68f), Vector2.zero, () => TogglePanel("Cheats Panel"), buttonRect, UICommon.trajanBold, "Cheats");
        AddButton("Tools", UICommon.images["ButtonRect"], new Vector2(146f, 68f), Vector2.zero, () => TogglePanel("Tools Panel"), buttonRect, UICommon.trajanBold, "Tools");
        AddButton("Skills", UICommon.images["ButtonRect"], new Vector2(246f, 68f), Vector2.zero, () => TogglePanel("Skills Panel"), buttonRect, UICommon.trajanBold, "Skills");
        AddButton("Items", UICommon.images["ButtonRect"], new Vector2(346f, 68f), Vector2.zero, () => TogglePanel("Items Panel"), buttonRect, UICommon.trajanBold, "Items");
        // AddButton("Bosses", UICommon.images["ButtonRect"], new Vector2(446f, 68f), Vector2.zero, () => TogglePanel("Bosses Panel"), buttonRect, UICommon.trajanBold, "Bosses");
        AddButton("SaveStates", UICommon.images["ButtonRect"], new Vector2(446f, 68f), Vector2.zero, () => TogglePanel("SaveStates Panel"), buttonRect, UICommon.trajanBold, "SaveStates");

        //Dropdown panels
        var cheatsPanel = AddPanel("Cheats Panel", UICommon.images["DropdownBG"], new Vector2(45f, 75f), Vector2.zero, new Rect(0, 0, UICommon.images["DropdownBG"].width, 240f), true);
        var toolsPanel = AddPanel("Tools Panel", UICommon.images["DropdownBG"], new Vector2(145f, 75f), Vector2.zero, new Rect(0, 0, UICommon.images["DropdownBG"].width, 210f), true);
        var skillsPanel = AddPanel("Skills Panel", UICommon.images["DropdownBG"], new Vector2(245f, 75f), Vector2.zero, new Rect(0, 0, UICommon.images["DropdownBG"].width, UICommon.images["DropdownBG"].height), true);
        var itemsPanel = AddPanel("Items Panel", UICommon.images["DropdownBG"], new Vector2(345f, 75f), Vector2.zero, new Rect(0, 0, UICommon.images["DropdownBG"].width, 180f), true);
        // var bossesPanel = AddPanel("Bosses Panel", UICommon.images["DropdownBG"], new Vector2(445f, 75f), Vector2.zero, new Rect(0, 0, UICommon.images["DropdownBG"].width, 200f), true);
        var otherPanel = AddPanel("Other Panel", UICommon.images["DropdownBG"], new Vector2(445f, 75f), Vector2.zero, new Rect(0, 0, UICommon.images["DropdownBG"].width, 120f), true);
        var saveStatesPanel = AddPanel("SaveStates Panel", UICommon.images["DropdownBG"], new Vector2(445f, 75f), Vector2.zero, new Rect(0, 0, UICommon.images["DropdownBG"].width, 170f), true);

        //Cheats panel
        cheatsPanel.AddButton("Infinite HP", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 30f), Vector2.zero, BindableFunctions.ToggleInfiniteHP, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Infinite HP", 10);
        cheatsPanel.AddButton("Infinite Silk", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 60f), Vector2.zero, BindableFunctions.ToggleInfiniteSilk, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Infinite Silk", 10);
        cheatsPanel.AddButton("Invincibility", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 90f), Vector2.zero, BindableFunctions.ToggleInvincibility, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Invincibility", 10);
        cheatsPanel.AddButton("Noclip", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 120f), Vector2.zero, BindableFunctions.ToggleNoclip, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Noclip", 10);
        cheatsPanel.AddButton("Infinite Jump", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 150f), Vector2.zero, BindableFunctions.ToggleInfiniteJump, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Infinite Jump", 10);
        cheatsPanel.AddButton("Kill Self", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 180f), Vector2.zero, BindableFunctions.KillSelf, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Kill Self", 10);
        cheatsPanel.AddButton("Lock KeyBinds", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 210f), Vector2.zero, BindableFunctions.ToggleLockKeyBinds, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Lock KeyBinds", 9);

        //Items panel button glow
        // itemsPanel.AddImage("Lantern Glow", UICommon.images["BlueGlow"], new Vector2(0f, 91f), new Vector2(47f, 51f), new Rect(0f, 0f, UICommon.images["BlueGlow"].width, UICommon.images["BlueGlow"].height));
        // itemsPanel.AddImage("Tram Pass Glow", UICommon.images["BlueGlow"], new Vector2(38f, 91f), new Vector2(47f, 37f), new Rect(0f, 0f, UICommon.images["BlueGlow"].width, UICommon.images["BlueGlow"].height));
        // itemsPanel.AddImage("Map & Quill Glow", UICommon.images["BlueGlow"], new Vector2(0f, 142f), new Vector2(47f, 40f), new Rect(0f, 0f, UICommon.images["BlueGlow"].width, UICommon.images["BlueGlow"].height));
        // itemsPanel.AddImage("City Crest Glow", UICommon.images["BlueGlow"], new Vector2(38f, 142f), new Vector2(47f, 60f), new Rect(0f, 0f, UICommon.images["BlueGlow"].width, UICommon.images["BlueGlow"].height));
        // itemsPanel.AddImage("Sly Key Glow", UICommon.images["BlueGlow"], new Vector2(0f, 202f), new Vector2(47f, 49f), new Rect(0f, 0f, UICommon.images["BlueGlow"].width, UICommon.images["BlueGlow"].height));
        // itemsPanel.AddImage("Elegant Key Glow", UICommon.images["BlueGlow"], new Vector2(38f, 202f), new Vector2(47f, 46f), new Rect(0f, 0f, UICommon.images["BlueGlow"].width, UICommon.images["BlueGlow"].height));
        // itemsPanel.AddImage("Love Key Glow", UICommon.images["BlueGlow"], new Vector2(0f, 251f), new Vector2(47f, 46f), new Rect(0f, 0f, UICommon.images["BlueGlow"].width, UICommon.images["BlueGlow"].height));
        // itemsPanel.AddImage("King's Brand Glow", UICommon.images["BlueGlow"], new Vector2(38f, 251f), new Vector2(47f, 45f), new Rect(0f, 0f, UICommon.images["BlueGlow"].width, UICommon.images["BlueGlow"].height));
        // itemsPanel.AddImage("Bullshit Flower Glow", UICommon.images["BlueGlow"], new Vector2(0f, 297f), new Vector2(47f, 45f), new Rect(0f, 0f, UICommon.images["BlueGlow"].width, UICommon.images["BlueGlow"].height));

        //Boss panel
        // bossesPanel.AddButton("Respawn Boss", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 30f), Vector2.zero, BindableFunctions.RespawnBoss, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Respawn Boss", 10);
        // bossesPanel.AddButton("Respawn Ghost", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 50f), Vector2.zero, RespawnGhostClicked, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Respawn Ghost", 9);

        // bossesPanel.AddButton("Failed Champ", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 110f), Vector2.zero, FailedChampClicked, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Failed Champ", 10);
        // bossesPanel.AddButton("Soul Tyrant", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 130f), Vector2.zero, SoulTyrantClicked, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Soul Tyrant", 10);
        // bossesPanel.AddButton("Lost Kin", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 150f), Vector2.zero, LostKinClicked, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Lost Kin", 10);
        // bossesPanel.AddButton("NK Grimm", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 180f), Vector2.zero, NKGrimmClicked, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "NK Grimm", 10);

        //Other Panel
        otherPanel.AddButton("Join Discord", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 30f), Vector2.zero, () => Application.OpenURL("https://discord.gg/VDsg3HmWuB"), new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Join Discord", 10);
        otherPanel.AddButton("Open Saves", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 60f), Vector2.zero, () => Process.Start(Application.persistentDataPath), new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Open Saves Files", 10);
        otherPanel.AddButton("Open Mods", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 90f), Vector2.zero, () => Process.Start(Directory.GetParent(Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().Location)).ToString()), new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Open Mods Folder", 10);

        //SaveStates Panel
        //TODO: Make the left/right page arrows, make them hidden when an option isn't up, integrate this into its own menu instead of top menu and combine it with the file panel itself
        saveStatesPanel.AddButton("QuickSlot Save", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 30f), Vector2.zero, BindableFunctions.SaveState, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "QuickSlot Save", 8);
        saveStatesPanel.AddButton("QuickSlot Load", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 50f), Vector2.zero, BindableFunctions.LoadState, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "QuickSlot Load", 8);
        saveStatesPanel.AddButton("File Save", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 80f), Vector2.zero, BindableFunctions.NewSaveStateToFile, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "File Save", 10);
        saveStatesPanel.AddButton("File Load", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 100f), Vector2.zero, BindableFunctions.LoadFromFile, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "File Load", 10);
        saveStatesPanel.AddButton("Scroll Left", UICommon.images["ButtonRectEmpty"], new Vector2(-15f, 115f), Vector2.zero, BindableFunctions.PrevStatePage, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Left", 8);
        saveStatesPanel.AddButton("Scroll Right", UICommon.images["ButtonRectEmpty"], new Vector2(20f, 115f), Vector2.zero, BindableFunctions.NextStatePage, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "Right", 8);
        saveStatesPanel.AddButton("Load State On Death", UICommon.images["ButtonRectEmpty"], new Vector2(5f, 145f), Vector2.zero, BindableFunctions.LoadStateOnDeath, new Rect(0f, 0f, 80f, 20f), UICommon.trajanNormal, "State On Death", 9);
        */
    }

    private CanvasAutoPanel AddTab(string name)
    {
        CanvasButton button = AddButton($"{name}TabButton");
        button.SetImage(UICommon.panelBG);
        button.Border.Sides &= ~BorderSides.BOTTOM;
        button.Text.Text = name;
        button.OnClicked += () => currentTab = name;

        CanvasAutoPanel panel = Add<CanvasAutoPanel>(name);
        panel.LocalPosition = new Vector2(0, TAB_BUTTON_HEIGHT);
        panel.Size = new Vector2(UICommon.RIGHT_SIDE_WIDTH, UICommon.MAIN_MENU_HEIGHT - TAB_BUTTON_HEIGHT);
        UICommon.AddBackground(panel);

        tabs.Add(panel);
        return panel;
    }

    public override void Build()
    {
        float tabButtonWidth = (Size.x - UICommon.MARGIN * (tabs.Count - 1)) / tabs.Count;
        float tabX = 0;

        foreach (CanvasAutoPanel tab in tabs)
        {
            CanvasButton tabButton = GetButton($"{tab.Name}TabButton");
            tabButton.LocalPosition = new Vector2(tabX, 0);
            tabButton.Size = new Vector2(tabButtonWidth, TAB_BUTTON_HEIGHT);
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
            foreach (CanvasAutoPanel tab in tabs)
            {
                tab.ActiveSelf = currentTab == tab.Name;
            }
        }

        /*
        if (GetPanel("Skills Panel").ActiveInHierarchy) RefreshSkillsMenu();

        if (GetPanel("Items Panel").ActiveInHierarchy) RefreshItemsMenu();

        if (GetPanel("Tools Panel").ActiveInHierarchy)
        {
            GetButton("Tool Pouches", "Tools Panel").Text.Text = "Pouches: " + PlayerData.instance.ToolPouchUpgrades;
            GetButton("Crafting Kits", "Tools Panel").Text.Text = "Kits: " + PlayerData.instance.ToolKitUpgrades;
            GetButton("Infinite Tools", "Tools Panel").Text.Color = DebugMod.infiniteTools ? selectedColor : Color.white;

        }

        if (GetPanel("Cheats Panel").ActiveInHierarchy)
        {
            GetButton("Infinite HP", "Cheats Panel").Text.Color = DebugMod.infiniteHP ? selectedColor : Color.white;
            GetButton("Infinite Silk", "Cheats Panel").Text.Color = DebugMod.infiniteSilk ? selectedColor : Color.white;
            GetButton("Invincibility", "Cheats Panel").Text.Color = PlayerData.instance.isInvincible ? selectedColor : Color.white;
            GetButton("Noclip", "Cheats Panel").Text.Color = DebugMod.noclip ? selectedColor : Color.white;
            GetButton("Infinite Jump", "Cheats Panel").Text.Color = PlayerData.instance.infiniteAirJump ? selectedColor : Color.white;
            GetButton("Lock KeyBinds", "Cheats Panel").Text.Color = DebugMod.KeyBindLock ? selectedColor : Color.white;

        }

        // if (GetPanel("Bosses Panel").active)
        // {
        //     GetButton("Failed Champ", "Bosses Panel").SetTextColor(PlayerData.instance.falseKnightDreamDefeated ? selectedColor : Color.white);
        //     GetButton("Soul Tyrant", "Bosses Panel").SetTextColor(PlayerData.instance.mageLordDreamDefeated ? selectedColor : Color.white);
        //     GetButton("Lost Kin", "Bosses Panel").SetTextColor(PlayerData.instance.infectedKnightDreamDefeated ? selectedColor : Color.white);
        //     GetButton("NK Grimm", "Bosses Panel").SetTextColor((PlayerData.instance.GetBoolInternal("killedNightmareGrimm") || PlayerData.instance.GetBoolInternal("destroyedNightmareLantern")) ? selectedColor : Color.white);
        // }
        
        //TODO fix naming so this doesnt require it to be setup like this (currently page panel is savestate panel so CC thinks its throwing errors not sure)
        if (GetPanel("SaveStates Panel").ActiveInHierarchy)
        {
            CanvasPanel savepanel = GetPanel("SaveStates Panel");
            savepanel.GetButton("Scroll Left").Text.Color = SaveStateManager.inSelectSlotState ? new Color(244f / 255f, 216f / 255f, 184f / 255f) : new Color(69f / 255f, 69f / 255f, 69f / 255f);
            savepanel.GetButton("Scroll Right").Text.Color = SaveStateManager.inSelectSlotState ? new Color(244f / 255f, 216f / 255f, 184f / 255f) : new Color(69f / 255f, 69f / 255f, 69f / 255f);
            savepanel.GetButton("File Save").Text.Color = SaveStateManager.currentStateOperation == "Save new state to file" ? selectedColor : Color.white;
            savepanel.GetButton("File Load").Text.Color = SaveStateManager.currentStateOperation == "Load new state from file" ? selectedColor : Color.white;
            savepanel.GetButton("Load State On Death").Text.Color = DebugMod.stateOnDeath ? selectedColor : Color.white;
        }
        */
    }

    private void RefreshItemsMenu()
    {
        // GetImage("Lantern Glow", "Items Panel").SetActive(true);
        // GetImage("Tram Pass Glow", "Items Panel").SetActive(true);
        // GetImage("Map & Quill Glow", "Items Panel").SetActive(true);
        // GetImage("City Crest Glow", "Items Panel").SetActive(true);
        // GetImage("Sly Key Glow", "Items Panel").SetActive(true);
        // GetImage("Elegant Key Glow", "Items Panel").SetActive(true);
        // GetImage("Love Key Glow", "Items Panel").SetActive(true);
        // GetImage("King's Brand Glow", "Items Panel").SetActive(true);
        // GetImage("Bullshit Flower Glow", "Items Panel").SetActive(true);
        //
        // if (!PlayerData.instance.hasLantern) GetImage("Lantern Glow", "Items Panel").SetActive(false);
        // if (!PlayerData.instance.hasTramPass) GetImage("Tram Pass Glow", "Items Panel").SetActive(false);
        // if (!PlayerData.instance.hasQuill) GetImage("Map & Quill Glow", "Items Panel").SetActive(false);
        // if (!PlayerData.instance.hasCityKey) GetImage("City Crest Glow", "Items Panel").SetActive(false);
        // if (!PlayerData.instance.hasSlykey) GetImage("Sly Key Glow", "Items Panel").SetActive(false);
        // if (!PlayerData.instance.hasWhiteKey) GetImage("Elegant Key Glow", "Items Panel").SetActive(false);
        // if (!PlayerData.instance.hasLoveKey) GetImage("Love Key Glow", "Items Panel").SetActive(false);
        // if (!PlayerData.instance.hasKingsBrand) GetImage("King's Brand Glow", "Items Panel").SetActive(false);
        // if (!PlayerData.instance.hasXunFlower || PlayerData.instance.xunFlowerBroken) GetImage("Bullshit Flower Glow", "Items Panel").SetActive(false);
    }

    private void RefreshSkillsMenu()
    {
        // GetButton("Silk Heart", "Skills Panel").Text.Text = "Silk Hearts: " + PlayerData.instance.silkRegenMax;
        //
        // GetButton("Cloak", "Skills Panel").Text.Color = PlayerData.instance.hasBrolly ? selectedColor : Color.white;
        // if (PlayerData.instance.hasDoubleJump) GetButton("Cloak", "Skills Panel").Text.Text = "Faydown";
        // else GetButton("Cloak", "Skills Panel").Text.Text = "Drifter's";
        //
        // GetButton("Swift Step", "Skills Panel").Text.Color = PlayerData.instance.hasDash ? selectedColor : Color.white;
        // GetButton("Cling Grip", "Skills Panel").Text.Color = PlayerData.instance.hasWalljump ? selectedColor : Color.white;
        // GetButton("Needolin", "Skills Panel").Text.Color = PlayerData.instance.hasNeedolin ? selectedColor : Color.white;
        // GetButton("Clawline", "Skills Panel").Text.Color = PlayerData.instance.hasHarpoonDash ? selectedColor : Color.white;
        // GetButton("Silk Soar", "Skills Panel").Text.Color = PlayerData.instance.hasSuperJump ? selectedColor : Color.white;
        // GetButton("Beastling Call", "Skills Panel").Text.Color = PlayerData.instance.UnlockedFastTravelTeleport ? selectedColor : Color.white;
        // GetButton("Elegy of the Deep", "Skills Panel").Text.Color = PlayerData.instance.hasNeedolinMemoryPowerup ? selectedColor : Color.white;
        // GetButton("Needle Strike", "Skills Panel").Text.Color = PlayerData.instance.hasChargeSlash ? selectedColor : Color.white;
    }

    private void HideMenuClicked()
    {
        // Text text = CanvasUtil.CreateTextPanel(GUIController.Instance.canvas, "", 27, TextAnchor.MiddleCenter,
        //     new CanvasUtil.RectData(
        //         new Vector2(0, 50),
        //         new Vector2(0, 45),
        //         new Vector2(0, 0),
        //         new Vector2(1, 0),
        //         new Vector2(0.5f, 0.5f))).GetComponent<Text>();
        // text.font = CanvasUtil.TrajanBold;
        // text.text = $"Press {Enum.GetName(typeof(KeyCode), DebugMod.settings.binds["Toggle All UI"])} to unhide the menu!";
        // text.fontSize = 42;
        // text.CrossFadeAlpha(1f, 0f, false);
        // text.CrossFadeAlpha(0f, 6f, false);
        BindableFunctions.ToggleAllPanels();
    }
}
