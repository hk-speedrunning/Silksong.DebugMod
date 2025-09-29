using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DebugMod.Canvas;
using UnityEngine;

namespace DebugMod
{
    public static class TopMenu
    {
        private static CanvasPanel panel;

        private static List<Vector2> AllPossibleLocations = new();

        private static readonly Color SelectedColor = new Color(244f / 255f, 127f / 255f, 32f / 255f);

        public static void BuildMenu(GameObject canvas)
        {
            MakeList();
            panel = new CanvasPanel(canvas, GUIController.Instance.images["ButtonsMenuBG"], new Vector2(1070f , 25f), Vector2.zero, new Rect(0f, 0f, GUIController.Instance.images["ButtonsMenuBG"].width, GUIController.Instance.images["ButtonsMenuBG"].height));

            Rect buttonRect = new Rect(0, 0, GUIController.Instance.images["ButtonRect"].width, GUIController.Instance.images["ButtonRect"].height);
            
            //Main buttons
            panel.AddButton("Hide Menu", GUIController.Instance.images["ButtonRect"], new Vector2(46f, 28f), Vector2.zero, HideMenuClicked, buttonRect, GUIController.Instance.trajanBold, "Hide Menu");
            panel.AddButton("Kill All", GUIController.Instance.images["ButtonRect"], new Vector2(146f, 28f), Vector2.zero, BindableFunctions.KillAll, buttonRect, GUIController.Instance.trajanBold, "Kill All");
            panel.AddButton("Set Spawn", GUIController.Instance.images["ButtonRect"], new Vector2(246f, 28f), Vector2.zero, BindableFunctions.SetHazardRespawn, buttonRect, GUIController.Instance.trajanBold, "Set Spawn");
            panel.AddButton("Respawn", GUIController.Instance.images["ButtonRect"], new Vector2(346f, 28f), Vector2.zero, BindableFunctions.Respawn, buttonRect, GUIController.Instance.trajanBold, "Respawn");
            panel.AddButton("Other", GUIController.Instance.images["ButtonRect"], new Vector2(446f, 28f), Vector2.zero, () => panel.TogglePanel("Other Panel"), buttonRect, GUIController.Instance.trajanBold, "Other");
            panel.AddButton("Cheats", GUIController.Instance.images["ButtonRect"], new Vector2(46f, 68f), Vector2.zero, () => panel.TogglePanel("Cheats Panel"), buttonRect, GUIController.Instance.trajanBold, "Cheats");
            panel.AddButton("Tools", GUIController.Instance.images["ButtonRect"], new Vector2(146f, 68f), Vector2.zero, () => panel.TogglePanel("Tools Panel"), buttonRect, GUIController.Instance.trajanBold, "Tools");
            panel.AddButton("Skills", GUIController.Instance.images["ButtonRect"], new Vector2(246f, 68f), Vector2.zero, () => panel.TogglePanel("Skills Panel"), buttonRect, GUIController.Instance.trajanBold, "Skills");
            panel.AddButton("Items", GUIController.Instance.images["ButtonRect"], new Vector2(346f, 68f), Vector2.zero, () => panel.TogglePanel("Items Panel"), buttonRect, GUIController.Instance.trajanBold, "Items");
            panel.AddButton("Bosses", GUIController.Instance.images["ButtonRect"], new Vector2(446f, 68f), Vector2.zero, () => panel.TogglePanel("Bosses Panel"), buttonRect, GUIController.Instance.trajanBold, "Bosses");
            panel.AddButton("SaveStates", GUIController.Instance.images["ButtonRect"], new Vector2(546f, 68f), Vector2.zero, () => panel.TogglePanel("SaveStates Panel"), buttonRect, GUIController.Instance.trajanBold, "SaveStates");

            //Dropdown panels
            var cheatsPanel = panel.AddPanel("Cheats Panel", GUIController.Instance.images["DropdownBG"], new Vector2(45f, 75f), Vector2.zero, new Rect(0, 0, GUIController.Instance.images["DropdownBG"].width, 240f));
            var toolsPanel = panel.AddPanel("Tools Panel", GUIController.Instance.images["DropdownBG"], new Vector2(145f, 75f), Vector2.zero, new Rect(0, 0, GUIController.Instance.images["DropdownBG"].width, 270f));
            var skillsPanel = panel.AddPanel("Skills Panel", GUIController.Instance.images["DropdownBG"], new Vector2(245f, 75f), Vector2.zero, new Rect(0, 0, GUIController.Instance.images["DropdownBG"].width, GUIController.Instance.images["DropdownBG"].height));
            var itemsPanel = panel.AddPanel("Items Panel", GUIController.Instance.images["DropdownBG"], new Vector2(345f, 75f), Vector2.zero, new Rect(0, 0, GUIController.Instance.images["DropdownBG"].width, GUIController.Instance.images["DropdownBG"].height));
            var bossesPanel = panel.AddPanel("Bosses Panel", GUIController.Instance.images["DropdownBG"], new Vector2(445f, 75f), Vector2.zero, new Rect(0, 0, GUIController.Instance.images["DropdownBG"].width, 200f));
            var otherPanel = panel.AddPanel("Other Panel", GUIController.Instance.images["DropdownBG"], new Vector2(445f, 75f), Vector2.zero, new Rect(0, 0, GUIController.Instance.images["DropdownBG"].width, GUIController.Instance.images["DropdownBG"].height));
            var saveStatesPanel = panel.AddPanel("SaveStates Panel", GUIController.Instance.images["DropdownBG"], new Vector2(545f, 75f), Vector2.zero, new Rect(0, 0, GUIController.Instance.images["DropdownBG"].width, 170f));

            //Cheats panel
            cheatsPanel.AddButton("Infinite HP", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 30f), Vector2.zero, BindableFunctions.ToggleInfiniteHP, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "Infinite HP", 10);
            cheatsPanel.AddButton("Infinite Silk", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 60f), Vector2.zero, BindableFunctions.ToggleInfiniteSilk, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "Infinite Silk", 10);
            cheatsPanel.AddButton("Invincibility", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 90f), Vector2.zero, BindableFunctions.ToggleInvincibility, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "Invincibility", 10);
            cheatsPanel.AddButton("Noclip", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 120f), Vector2.zero, BindableFunctions.ToggleNoclip, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "Noclip", 10);
            cheatsPanel.AddButton("Infinite Jump", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 150f), Vector2.zero, BindableFunctions.ToggleInfiniteJump, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "Infinite Jump", 10);
            cheatsPanel.AddButton("Kill Self", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 180f), Vector2.zero, BindableFunctions.KillSelf, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "Kill Self", 10);
            cheatsPanel.AddButton("Lock KeyBinds", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 210f), Vector2.zero, BindableFunctions.ToggleLockKeyBinds, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "Lock KeyBinds", 9);

            //Skills panel buttons
            // TODO: use images instead of text?
            skillsPanel.AddButton("All Skills", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 30f), Vector2.zero, BindableFunctions.GiveAllSkills, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "All Skills", 10);
            skillsPanel.AddButton("Silk Heart", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 60f), Vector2.zero, BindableFunctions.IncrementSilkHeart, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "Silk Hearts: " + PlayerData.instance.silkRegenMax, 10);
            skillsPanel.AddButton("Swift Step", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 90f), Vector2.zero, BindableFunctions.ToggleSwiftStep, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "Swift Step", 10);
            skillsPanel.AddButton("Cloak", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 120f), Vector2.zero, BindableFunctions.IncrementCloak, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "Drifter's", 10);
            skillsPanel.AddButton("Cling Grip", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 150f), Vector2.zero, BindableFunctions.ToggleClingGrip, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "Cling Grip", 10);
            skillsPanel.AddButton("Needolin", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 180f), Vector2.zero, BindableFunctions.ToggleNeedolin, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "Needolin", 10);
            skillsPanel.AddButton("Clawline", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 210f), Vector2.zero, BindableFunctions.ToggleClawline, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "Clawline", 10);
            skillsPanel.AddButton("Silk Soar", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 240f), Vector2.zero, BindableFunctions.ToggleSilkSoar, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "Silk Soar", 10);
            skillsPanel.AddButton("Beastling Call", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 270f), Vector2.zero, BindableFunctions.ToggleBeastlingCall, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "Beastling", 10);
            skillsPanel.AddButton("Elegy of the Deep", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 300f), Vector2.zero, BindableFunctions.ToggleElegyOfTheDeep, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "Elegy", 10);
            skillsPanel.AddButton("Needle Strike", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 330f), Vector2.zero, BindableFunctions.ToggleNeedleStrike, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "Needle Strike", 10);

            //Tools panel
            toolsPanel.AddButton("All Tools", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 30f), Vector2.zero, BindableFunctions.UnlockAllTools, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "All Tools", 10);
            toolsPanel.AddButton("All Crests", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 60f), Vector2.zero, BindableFunctions.UnlockAllCrests, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "All Crests", 10);
            toolsPanel.AddButton("Tool Pouches", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 90f), Vector2.zero, BindableFunctions.IncrementPouches, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "Pouches: " + PlayerData.instance.ToolPouchUpgrades, 10);
            toolsPanel.AddButton("Crafting Kits", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 120f), Vector2.zero, BindableFunctions.IncrementKits, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "Kits: " + PlayerData.instance.ToolKitUpgrades, 10);
            toolsPanel.AddButton("Infinite Tools", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 150f), Vector2.zero, BindableFunctions.ToggleInfiniteTools, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "Infinite Uses", 10);
            toolsPanel.AddButton("Craft Tools", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 180f), Vector2.zero, BindableFunctions.CraftTools, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "Craft Tools", 10);

            //Items panel
            itemsPanel.AddButton("Rosaries", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 30f), Vector2.zero, BindableFunctions.GiveRosaries, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "Rosaries", 10);
            itemsPanel.AddButton("Shell Shards", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 60f), Vector2.zero, BindableFunctions.GiveShellShards, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "Shards", 10);
            
            // itemsPanel.AddButton("Pale Ore", GUIController.Instance.images["PaleOre"], new Vector2(5f, 30f), new Vector2(23f, 22f), PaleOreClicked, new Rect(0, 0, GUIController.Instance.images["PaleOre"].width, GUIController.Instance.images["PaleOre"].height));
            // itemsPanel.AddButton("Simple Key", GUIController.Instance.images["SimpleKey"], new Vector2(33f, 30f), new Vector2(23f, 23f), SimpleKeyClicked, new Rect(0, 0, GUIController.Instance.images["SimpleKey"].width, GUIController.Instance.images["SimpleKey"].height));
            // itemsPanel.AddButton("Rancid Egg", GUIController.Instance.images["RancidEgg"], new Vector2(61f, 30f), new Vector2(23f, 30f), RancidEggClicked, new Rect(0, 0, GUIController.Instance.images["RancidEgg"].width, GUIController.Instance.images["RancidEgg"].height));
            // itemsPanel.AddButton("Geo", GUIController.Instance.images["Geo"], new Vector2(5f, 63f), new Vector2(23f, 23f), GeoClicked, new Rect(0, 0, GUIController.Instance.images["Geo"].width, GUIController.Instance.images["Geo"].height));
            // itemsPanel.AddButton("Essence", GUIController.Instance.images["Essence"], new Vector2(33f, 63f), new Vector2(23f, 23f), EssenceClicked, new Rect(0, 0, GUIController.Instance.images["Essence"].width, GUIController.Instance.images["Essence"].height));
            // itemsPanel.AddButton("Lantern", GUIController.Instance.images["Lantern"], new Vector2(5f, 96f), new Vector2(37f, 41f), LanternClicked, new Rect(0, 0, GUIController.Instance.images["Lantern"].width, GUIController.Instance.images["Lantern"].height));
            // itemsPanel.AddButton("Tram Pass", GUIController.Instance.images["TramPass"], new Vector2(43f, 96f), new Vector2(37f, 27f), TramPassClicked, new Rect(0, 0, GUIController.Instance.images["TramPass"].width, GUIController.Instance.images["TramPass"].height));
            // itemsPanel.AddButton("Map & Quill", GUIController.Instance.images["MapQuill"], new Vector2(5f, 147f), new Vector2(37f, 30f), MapQuillClicked, new Rect(0, 0, GUIController.Instance.images["MapQuill"].width, GUIController.Instance.images["MapQuill"].height));
            // itemsPanel.AddButton("City Crest", GUIController.Instance.images["CityKey"], new Vector2(43f, 147f), new Vector2(37f, 50f), CityKeyClicked, new Rect(0, 0, GUIController.Instance.images["CityKey"].width, GUIController.Instance.images["CityKey"].height));
            // itemsPanel.AddButton("Sly Key", GUIController.Instance.images["SlyKey"], new Vector2(5f, 207f), new Vector2(37f, 39f), SlyKeyClicked, new Rect(0, 0, GUIController.Instance.images["SlyKey"].width, GUIController.Instance.images["SlyKey"].height));
            // itemsPanel.AddButton("Elegant Key", GUIController.Instance.images["ElegantKey"], new Vector2(43f, 207f), new Vector2(37f, 36f), ElegantKeyClicked, new Rect(0, 0, GUIController.Instance.images["ElegantKey"].width, GUIController.Instance.images["ElegantKey"].height));
            // itemsPanel.AddButton("Love Key", GUIController.Instance.images["LoveKey"], new Vector2(5f, 256f), new Vector2(37f, 36f), LoveKeyClicked, new Rect(0, 0, GUIController.Instance.images["LoveKey"].width, GUIController.Instance.images["LoveKey"].height));
            // itemsPanel.AddButton("King's Brand", GUIController.Instance.images["Kingsbrand"], new Vector2(43f, 256f), new Vector2(37f, 35f), KingsbrandClicked, new Rect(0, 0, GUIController.Instance.images["Kingsbrand"].width, GUIController.Instance.images["Kingsbrand"].height));
            // itemsPanel.AddButton("Bullshit Flower", GUIController.Instance.images["Flower"], new Vector2(5f, 302f), new Vector2(37f, 35f), FlowerClicked, new Rect(0, 0, GUIController.Instance.images["Flower"].width, GUIController.Instance.images["Flower"].height));
            // itemsPanel.AddButton("Stags", GUIController.Instance.images["LastStagFace"], new Vector2(43f, 302f), new Vector2(37f, 35f), StagsClicked, new Rect(0, 0, GUIController.Instance.images["LastStagFace"].width, GUIController.Instance.images["LastStagFace"].height));
            // itemsPanel.AddButton("Mask", GUIController.Instance.images["Mask"], new Vector2(5f, 351f), new Vector2(37f, 35f), MaskClicked, new Rect(0, 0, GUIController.Instance.images["Mask"].width, GUIController.Instance.images["Mask"].height));
            // itemsPanel.AddButton("Vessel", GUIController.Instance.images["Vessel"], new Vector2(43f, 351f), new Vector2(37f, 35f), VesselClicked, new Rect(0, 0, GUIController.Instance.images["Vessel"].width, GUIController.Instance.images["Vessel"].height));

            //Items panel button glow
            // itemsPanel.AddImage("Lantern Glow", GUIController.Instance.images["BlueGlow"], new Vector2(0f, 91f), new Vector2(47f, 51f), new Rect(0f, 0f, GUIController.Instance.images["BlueGlow"].width, GUIController.Instance.images["BlueGlow"].height));
            // itemsPanel.AddImage("Tram Pass Glow", GUIController.Instance.images["BlueGlow"], new Vector2(38f, 91f), new Vector2(47f, 37f), new Rect(0f, 0f, GUIController.Instance.images["BlueGlow"].width, GUIController.Instance.images["BlueGlow"].height));
            // itemsPanel.AddImage("Map & Quill Glow", GUIController.Instance.images["BlueGlow"], new Vector2(0f, 142f), new Vector2(47f, 40f), new Rect(0f, 0f, GUIController.Instance.images["BlueGlow"].width, GUIController.Instance.images["BlueGlow"].height));
            // itemsPanel.AddImage("City Crest Glow", GUIController.Instance.images["BlueGlow"], new Vector2(38f, 142f), new Vector2(47f, 60f), new Rect(0f, 0f, GUIController.Instance.images["BlueGlow"].width, GUIController.Instance.images["BlueGlow"].height));
            // itemsPanel.AddImage("Sly Key Glow", GUIController.Instance.images["BlueGlow"], new Vector2(0f, 202f), new Vector2(47f, 49f), new Rect(0f, 0f, GUIController.Instance.images["BlueGlow"].width, GUIController.Instance.images["BlueGlow"].height));
            // itemsPanel.AddImage("Elegant Key Glow", GUIController.Instance.images["BlueGlow"], new Vector2(38f, 202f), new Vector2(47f, 46f), new Rect(0f, 0f, GUIController.Instance.images["BlueGlow"].width, GUIController.Instance.images["BlueGlow"].height));
            // itemsPanel.AddImage("Love Key Glow", GUIController.Instance.images["BlueGlow"], new Vector2(0f, 251f), new Vector2(47f, 46f), new Rect(0f, 0f, GUIController.Instance.images["BlueGlow"].width, GUIController.Instance.images["BlueGlow"].height));
            // itemsPanel.AddImage("King's Brand Glow", GUIController.Instance.images["BlueGlow"], new Vector2(38f, 251f), new Vector2(47f, 45f), new Rect(0f, 0f, GUIController.Instance.images["BlueGlow"].width, GUIController.Instance.images["BlueGlow"].height));
            // itemsPanel.AddImage("Bullshit Flower Glow", GUIController.Instance.images["BlueGlow"], new Vector2(0f, 297f), new Vector2(47f, 45f), new Rect(0f, 0f, GUIController.Instance.images["BlueGlow"].width, GUIController.Instance.images["BlueGlow"].height));

            //Boss panel
            // bossesPanel.AddButton("Respawn Boss", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 30f), Vector2.zero, BindableFunctions.RespawnBoss, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "Respawn Boss", 10);
            // bossesPanel.AddButton("Respawn Ghost", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 50f), Vector2.zero, RespawnGhostClicked, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "Respawn Ghost", 9);

            // bossesPanel.AddButton("Failed Champ", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 110f), Vector2.zero, FailedChampClicked, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "Failed Champ", 10);
            // bossesPanel.AddButton("Soul Tyrant", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 130f), Vector2.zero, SoulTyrantClicked, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "Soul Tyrant", 10);
            // bossesPanel.AddButton("Lost Kin", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 150f), Vector2.zero, LostKinClicked, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "Lost Kin", 10);
            // bossesPanel.AddButton("NK Grimm", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 180f), Vector2.zero, NKGrimmClicked, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "NK Grimm", 10);

            //Other Panel
            otherPanel.AddButton("Join Discord", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 30f), Vector2.zero, _ => Application.OpenURL("https://discord.gg/VDsg3HmWuB"), new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "Join Discord", 10);
            otherPanel.AddButton("View Debug Additions", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 60f), Vector2.zero, _ => Application.OpenURL("https://docs.google.com/spreadsheets/d/1XLrikYGdLCVZ5YiD8i0ZuFNsY5k3Yi5IZmDTYPD-QVI/edit?usp=sharing"), new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "View Debug Additions", 10);
            otherPanel.AddButton("Open Saves", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 90f), Vector2.zero, _ => Process.Start(Application.persistentDataPath), new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "Open Saves Files", 10);
            otherPanel.AddButton("Open Mods", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 120f), Vector2.zero, _ => Process.Start(Directory.GetParent(Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().Location)).ToString()), new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "Open Mods Folder", 10);

            //SaveStates Panel
            //TODO: Make the left/right page arrows, make them hidden when an option isn't up, integrate this into its own menu instead of top menu and combine it with the file panel itself
            saveStatesPanel.AddButton("QuickSlot Save", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 30f), Vector2.zero, BindableFunctions.SaveState, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "QuickSlot Save", 8);
            saveStatesPanel.AddButton("QuickSlot Load", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 50f), Vector2.zero, BindableFunctions.LoadState, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "QuickSlot Load", 8);
            saveStatesPanel.AddButton("File Save", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 80f), Vector2.zero, BindableFunctions.NewSaveStateToFile, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "File Save", 10);
            saveStatesPanel.AddButton("File Load", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 100f), Vector2.zero, BindableFunctions.LoadFromFile, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "File Load", 10);
            saveStatesPanel.AddButton("Scroll Left", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(-15f, 115f), Vector2.zero, BindableFunctions.PrevStatePage, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "Left", 8);
            saveStatesPanel.AddButton("Scroll Right", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(20f, 115f), Vector2.zero, BindableFunctions.NextStatePage, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "Right", 8);
            saveStatesPanel.AddButton("Load State On Death", GUIController.Instance.images["ButtonRectEmpty"], new Vector2(5f, 145f), Vector2.zero, BindableFunctions.LoadStateOnDeath, new Rect(0f, 0f, 80f, 20f), GUIController.Instance.trajanNormal, "State On Death", 9);

            panel.FixRenderOrder();
        }

        private static void MakeList()
        {
            //currently only 5 slots available
            AllPossibleLocations.Add(new Vector2(546,68f));
            AllPossibleLocations.Add(new Vector2(646,28f));
            AllPossibleLocations.Add(new Vector2(646,68f));
            AllPossibleLocations.Add(new Vector2(746,28f));
            AllPossibleLocations.Add(new Vector2(746,68f));
        }

        //TODO: Set A Color Variable for highlighted so its easier to add new buttons
        public static void Update()
        {
            if (panel == null)
            {
                return;
            }

            if (GUIController.ForceHideUI())
            {
                if (panel.active)
                {
                    panel.SetActive(false, true);
                }

                return;
            }

            if (DebugMod.settings.TopMenuVisible && !panel.active)
            {
                panel.SetActive(true, false);
            }
            else if (!DebugMod.settings.TopMenuVisible && panel.active)
            {
                panel.SetActive(false, true);
            }

            if (panel.GetPanel("Skills Panel").active) RefreshSkillsMenu();

            if (panel.GetPanel("Items Panel").active) RefreshItemsMenu();

            if (panel.GetPanel("Tools Panel").active)
            {
                panel.GetButton("Tool Pouches", "Tools Panel").UpdateText("Pouches: " + PlayerData.instance.ToolPouchUpgrades);
                panel.GetButton("Crafting Kits", "Tools Panel").UpdateText("Kits: " + PlayerData.instance.ToolKitUpgrades);
                panel.GetButton("Infinite Tools", "Tools Panel").SetTextColor(DebugMod.infiniteTools ? SelectedColor : Color.white);

            }

            if (panel.GetPanel("Cheats Panel").active)
            {
                panel.GetButton("Infinite HP", "Cheats Panel").SetTextColor(DebugMod.infiniteHP ? SelectedColor : Color.white);
                panel.GetButton("Infinite Silk", "Cheats Panel").SetTextColor(DebugMod.infiniteSilk ? SelectedColor : Color.white);
                panel.GetButton("Invincibility", "Cheats Panel").SetTextColor(PlayerData.instance.isInvincible ? SelectedColor : Color.white);
                panel.GetButton("Noclip", "Cheats Panel").SetTextColor(DebugMod.noclip ? SelectedColor : Color.white);
                panel.GetButton("Infinite Jump", "Cheats Panel").SetTextColor(PlayerData.instance.infiniteAirJump ? SelectedColor : Color.white);
                panel.GetButton("Lock KeyBinds", "Cheats Panel").SetTextColor(DebugMod.KeyBindLock ? SelectedColor : Color.white);

            }

            if (panel.GetPanel("Bosses Panel").active)
            {
                // panel.GetButton("Failed Champ", "Bosses Panel").SetTextColor(PlayerData.instance.falseKnightDreamDefeated ? SelectedColor : Color.white);
                // panel.GetButton("Soul Tyrant", "Bosses Panel").SetTextColor(PlayerData.instance.mageLordDreamDefeated ? SelectedColor : Color.white);
                // panel.GetButton("Lost Kin", "Bosses Panel").SetTextColor(PlayerData.instance.infectedKnightDreamDefeated ? SelectedColor : Color.white);
                // panel.GetButton("NK Grimm", "Bosses Panel").SetTextColor((PlayerData.instance.GetBoolInternal("killedNightmareGrimm") || PlayerData.instance.GetBoolInternal("destroyedNightmareLantern")) ? SelectedColor : Color.white);
                
            }
            
            //TODO fix naming so this doesnt require it to be setup like this (currently page panel is savestate panel so CC thinks its throwing errors not sure)
            if (panel.GetPanel("SaveStates Panel").active)
            {
                CanvasPanel savepanel = panel.GetPanel("SaveStates Panel");
                savepanel.GetButton("Scroll Left").SetTextColor(SaveStateManager.inSelectSlotState ? new Color(244f / 255f, 216f / 255f, 184f / 255f) : new Color(69f / 255f, 69f / 255f, 69f / 255f));
                savepanel.GetButton("Scroll Right").SetTextColor(SaveStateManager.inSelectSlotState ? new Color(244f / 255f, 216f / 255f, 184f / 255f) : new Color(69f / 255f, 69f / 255f, 69f / 255f));
                savepanel.GetButton("File Save").SetTextColor(SaveStateManager.currentStateOperation == "Save new state to file" ? SelectedColor : Color.white);
                savepanel.GetButton("File Load").SetTextColor(SaveStateManager.currentStateOperation == "Load new state from file" ? SelectedColor : Color.white);
                savepanel.GetButton("Load State On Death").SetTextColor(DebugMod.stateOnDeath ? SelectedColor : Color.white);
            }
        }

        private static void RefreshItemsMenu()
        {
            // panel.GetImage("Lantern Glow", "Items Panel").SetActive(true);
            // panel.GetImage("Tram Pass Glow", "Items Panel").SetActive(true);
            // panel.GetImage("Map & Quill Glow", "Items Panel").SetActive(true);
            // panel.GetImage("City Crest Glow", "Items Panel").SetActive(true);
            // panel.GetImage("Sly Key Glow", "Items Panel").SetActive(true);
            // panel.GetImage("Elegant Key Glow", "Items Panel").SetActive(true);
            // panel.GetImage("Love Key Glow", "Items Panel").SetActive(true);
            // panel.GetImage("King's Brand Glow", "Items Panel").SetActive(true);
            // panel.GetImage("Bullshit Flower Glow", "Items Panel").SetActive(true);
            //
            // if (!PlayerData.instance.hasLantern) panel.GetImage("Lantern Glow", "Items Panel").SetActive(false);
            // if (!PlayerData.instance.hasTramPass) panel.GetImage("Tram Pass Glow", "Items Panel").SetActive(false);
            // if (!PlayerData.instance.hasQuill) panel.GetImage("Map & Quill Glow", "Items Panel").SetActive(false);
            // if (!PlayerData.instance.hasCityKey) panel.GetImage("City Crest Glow", "Items Panel").SetActive(false);
            // if (!PlayerData.instance.hasSlykey) panel.GetImage("Sly Key Glow", "Items Panel").SetActive(false);
            // if (!PlayerData.instance.hasWhiteKey) panel.GetImage("Elegant Key Glow", "Items Panel").SetActive(false);
            // if (!PlayerData.instance.hasLoveKey) panel.GetImage("Love Key Glow", "Items Panel").SetActive(false);
            // if (!PlayerData.instance.hasKingsBrand) panel.GetImage("King's Brand Glow", "Items Panel").SetActive(false);
            // if (!PlayerData.instance.hasXunFlower || PlayerData.instance.xunFlowerBroken) panel.GetImage("Bullshit Flower Glow", "Items Panel").SetActive(false);
        }

        private static void RefreshSkillsMenu()
        {
            panel.GetButton("Silk Heart", "Skills Panel").UpdateText("Silk Hearts: " + PlayerData.instance.silkRegenMax);

            panel.GetButton("Cloak", "Skills Panel").SetTextColor(PlayerData.instance.hasBrolly ? SelectedColor : Color.white);
            if (PlayerData.instance.hasDoubleJump) panel.GetButton("Cloak", "Skills Panel").UpdateText("Faydown");
            else panel.GetButton("Cloak", "Skills Panel").UpdateText("Drifter's");

            panel.GetButton("Swift Step", "Skills Panel").SetTextColor(PlayerData.instance.hasDash ? SelectedColor : Color.white);
            panel.GetButton("Cling Grip", "Skills Panel").SetTextColor(PlayerData.instance.hasWalljump ? SelectedColor : Color.white);
            panel.GetButton("Needolin", "Skills Panel").SetTextColor(PlayerData.instance.hasNeedolin ? SelectedColor : Color.white);
            panel.GetButton("Clawline", "Skills Panel").SetTextColor(PlayerData.instance.hasHarpoonDash ? SelectedColor : Color.white);
            panel.GetButton("Silk Soar", "Skills Panel").SetTextColor(PlayerData.instance.hasSuperJump ? SelectedColor : Color.white);
            panel.GetButton("Beastling Call", "Skills Panel").SetTextColor(PlayerData.instance.UnlockedFastTravelTeleport ? SelectedColor : Color.white);
            panel.GetButton("Elegy of the Deep", "Skills Panel").SetTextColor(PlayerData.instance.hasNeedolinMemoryPowerup ? SelectedColor : Color.white);
            panel.GetButton("Needle Strike", "Skills Panel").SetTextColor(PlayerData.instance.hasChargeSlash ? SelectedColor : Color.white);
        }

        private static void HideMenuClicked()
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

        internal static void AddTopMenuContent(string MenuName, List<TopMenuButton> ButtonList)
        {
            if (panel.GetPanel(MenuName) == null)
            {
                Rect buttonRect = new Rect(0, 0, GUIController.Instance.images["ButtonRect"].width, GUIController.Instance.images["ButtonRect"].height);
                Action action = () => panel.TogglePanel(MenuName);

                Vector2 Pos = AllPossibleLocations.First();
                AllPossibleLocations.Remove(Pos);

                panel.AddButton(MenuName, GUIController.Instance.images["ButtonRect"], Pos, Vector2.zero, action, buttonRect, GUIController.Instance.trajanBold, MenuName);
                panel.AddPanel(MenuName, GUIController.Instance.images["DropdownBG"], new Vector2(Pos.x, 75f), Vector2.zero, new Rect(0, 0, GUIController.Instance.images["DropdownBG"].width, GUIController.Instance.images["DropdownBG"].height));
            }

            CanvasPanel MyPanel = panel.GetPanel(MenuName);

            foreach (var button in ButtonList)
            {
                button.CreateButton(MyPanel);
            }
        }
    }
}
