using DebugMod.UI;

namespace DebugMod;

public static partial class BindableFunctions
{
    [BindableMethod(name = "GAMEPLAY_MODUI_TOGGLEALLUI", category = "CATEGORY_MODUI", allowLock = false)]
    public static void ToggleAllPanels()
    {
        bool active = !(
            DebugMod.settings.InfoPanelVisible ||
            DebugMod.settings.EnemiesPanelVisible ||
            DebugMod.settings.MainPanelVisible ||
            DebugMod.settings.ConsoleVisible ||
            DebugMod.settings.SaveStatePanelVisible
            );

        DebugMod.settings.InfoPanelVisible = active;
        DebugMod.settings.MainPanelVisible = active;
        DebugMod.settings.EnemiesPanelVisible = active;
        DebugMod.settings.ConsoleVisible = active;
        DebugMod.settings.SaveStatePanelVisible = active;
    }

    [BindableMethod(name = "GAMEPLAY_MODUI_TOGGLEMAINPANEL", category = "CATEGORY_MODUI")]
    public static void ToggleMainPanel()
    {
        DebugMod.settings.MainPanelVisible = !DebugMod.settings.MainPanelVisible;
    }


    [BindableMethod(name = "GAMEPLAY_MODUI_TOGGLEENEMIESPANEL", category = "CATEGORY_MODUI")]
    public static void ToggleEnemiesPanel()
    {
        DebugMod.settings.EnemiesPanelVisible = !DebugMod.settings.EnemiesPanelVisible;
    }

    [BindableMethod(name = "GAMEPLAY_MODUI_TOGGLECONSOLEPANEL", category = "CATEGORY_MODUI")]
    public static void ToggleConsolePanel()
    {
        DebugMod.settings.ConsoleVisible = !DebugMod.settings.ConsoleVisible;
    }

    [BindableMethod(name = "GAMEPLAY_MODUI_TOGGLEINFOPANEL", category = "CATEGORY_MODUI")]
    public static void ToggleInfoPanel()
    {
        DebugMod.settings.InfoPanelVisible = !DebugMod.settings.InfoPanelVisible;
    }

    [BindableMethod(name = "GAMEPLAY_MODUI_TOGGLESAVESTATESPANEL", category = "CATEGORY_MODUI")]
    public static void ToggleSaveStatePanel()
    {
        DebugMod.settings.SaveStatePanelVisible = !DebugMod.settings.SaveStatePanelVisible;
        if (!DebugMod.settings.SaveStatePanelVisible)
        {
            SaveStatesPanel.Instance.CancelSelectState();
        }
    }

    [BindableMethod(name = "GAMEPLAY_MODUI_EXPANDCOLLAPSESAVESTATES", category = "CATEGORY_MODUI")]
    public static void ToggleExpandedSaveStatePanel()
    {
        SaveStatesPanel.Instance.ToggleView();
    }

    [BindableMethod(name = "GAMEPLAY_MODUI_ALWAYSSHOWCURSOR", category = "CATEGORY_MODUI")]
    public static void ToggleAlwaysShowCursor()
    {
        DebugMod.settings.ShowCursorWhileUnpaused = !DebugMod.settings.ShowCursorWhileUnpaused;

        if (DebugMod.settings.ShowCursorWhileUnpaused)
        {
            DebugMod.LogConsole("Showing cursor while unpaused");
        }
        else
        {
            DebugMod.LogConsole("Not showing cursor while unpaused");
        }
    }
}