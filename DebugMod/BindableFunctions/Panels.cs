using DebugMod.UI;
using UnityEngine;

namespace DebugMod;

public static partial class BindableFunctions
{
    [BindableMethod(name = "Toggle All UI", category = "Mod UI", allowLock = false)]
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

    [BindableMethod(name = "Toggle Main Panel", category = "Mod UI")]
    public static void ToggleMainPanel()
    {
        DebugMod.settings.MainPanelVisible = !DebugMod.settings.MainPanelVisible;
    }


    [BindableMethod(name = "Toggle Enemies Panel", category = "Mod UI")]
    public static void ToggleEnemiesPanel()
    {
        DebugMod.settings.EnemiesPanelVisible = !DebugMod.settings.EnemiesPanelVisible;
    }

    [BindableMethod(name = "Toggle Console Panel", category = "Mod UI")]
    public static void ToggleConsolePanel()
    {
        DebugMod.settings.ConsoleVisible = !DebugMod.settings.ConsoleVisible;
    }

    [BindableMethod(name = "Toggle Savestates Panel", category = "Mod UI")]
    public static void ToggleSaveStatePanel()
    {
        DebugMod.settings.SaveStatePanelVisible = !DebugMod.settings.SaveStatePanelVisible;
    }

    [BindableMethod(name = "Toggle Info Panel", category = "Mod UI")]
    public static void ToggleInfoPanel()
    {
        DebugMod.settings.InfoPanelVisible = !DebugMod.settings.InfoPanelVisible;
    }

    // View handled in the InfoPanel classes
    [BindableMethod(name = "Info Panel Switch", category = "Mod UI")]
    public static void SwitchActiveInfoPanel()
    {
        InfoPanel.ToggleActivePanel();
    }

    [BindableMethod(name = "Toggle Cursor", category = "Mod UI")]
    public static void ToggleAlwaysShowCursor()
    {
        DebugMod.settings.ShowCursorWhileUnpaused = !DebugMod.settings.ShowCursorWhileUnpaused;

        if (DebugMod.settings.ShowCursorWhileUnpaused)
        {
            SetAlwaysShowCursor();
            DebugMod.LogConsole("Showing cursor while unpaused");
        }
        else
        {
            UnsetAlwaysShowCursor();
            DebugMod.LogConsole("Not showing cursor while unpaused");
        }
    }

    internal static void SetAlwaysShowCursor()
    {
        ModHooks.CursorHook -= CursorDisplayActive;
        ModHooks.CursorHook += CursorDisplayActive;
    }

    internal static void UnsetAlwaysShowCursor()
    {
        ModHooks.CursorHook -= CursorDisplayActive;
    }

    private static void CursorDisplayActive()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}