using DebugMod.UI;

namespace DebugMod;

public static partial class BindableFunctions
{
    [BindableMethod(name = "Toggle All UI", category = "Mod UI", allowLock = false)]
    public static void ToggleAllPanels()
    {
        bool active = !(
            DebugMod.settings.HelpPanelVisible ||
            DebugMod.settings.InfoPanelVisible ||
            DebugMod.settings.EnemiesPanelVisible ||
            DebugMod.settings.TopMenuVisible ||
            DebugMod.settings.ConsoleVisible ||
            DebugMod.settings.SaveStatePanelVisible
            );

        DebugMod.settings.InfoPanelVisible = active;
        DebugMod.settings.TopMenuVisible = active;
        DebugMod.settings.EnemiesPanelVisible = active;
        DebugMod.settings.ConsoleVisible = active;
        DebugMod.settings.HelpPanelVisible = active;

        if (!active)
        {
            DebugMod.settings.ClearSaveStatePanel = true;
        }
    }

    [BindableMethod(name = "Toggle Binds", category = "Mod UI")]
    public static void ToggleHelpPanel()
    {
        DebugMod.settings.HelpPanelVisible = !DebugMod.settings.HelpPanelVisible;
    }

    [BindableMethod(name = "Toggle Info", category = "Mod UI")]
    public static void ToggleInfoPanel()
    {
        DebugMod.settings.InfoPanelVisible = !DebugMod.settings.InfoPanelVisible;
    }

    [BindableMethod(name = "Toggle Top Menu", category = "Mod UI")]
    public static void ToggleTopRightPanel()
    {
        DebugMod.settings.TopMenuVisible = !DebugMod.settings.TopMenuVisible;
    }

    [BindableMethod(name = "Toggle Console", category = "Mod UI")]
    public static void ToggleConsole()
    {
        DebugMod.settings.ConsoleVisible = !DebugMod.settings.ConsoleVisible;
    }

    [BindableMethod(name = "Toggle Enemy Panel", category = "Mod UI")]
    public static void ToggleEnemyPanel()
    {
        DebugMod.settings.EnemiesPanelVisible = !DebugMod.settings.EnemiesPanelVisible;
    }

    // View handled in the InfoPanel classes
    [BindableMethod(name = "Info Panel Switch", category = "Mod UI")]
    public static void SwitchActiveInfoPanel()
    {
        InfoPanel.ToggleActivePanel();
    }
}