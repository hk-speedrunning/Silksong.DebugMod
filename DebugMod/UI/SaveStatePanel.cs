using System.Collections.Generic;
using DebugMod.SaveStates;
using DebugMod.UI.Canvas;
using UnityEngine;

namespace DebugMod.UI;

public static class SaveStatesPanel
{
    private static CanvasPanel statePanel;

    public static void BuildMenu(GameObject canvas)
    {
        statePanel = new CanvasPanel(
            nameof(SaveStatesPanel),
            null,
            new Vector2(720f, 40f),
            Vector2.zero,
            GUIController.Instance.images["BlankVertical"],
            new Rect(
                0f,
                0f,
                GUIController.Instance.images["BlankVertical"].width,
                GUIController.Instance.images["BlankVertical"].height
            )
        );
        statePanel.AddText("CurrentFolder", "Page: " + (SaveStateManager.currentStateFolder + 1).ToString(), new Vector2(8, 0), Vector2.zero, GUIController.Instance.arial, 15);
        statePanel.AddText("Mode", "mode: ", new Vector2(8, 20), Vector2.zero, GUIController.Instance.arial, 15);
        statePanel.AddText("currentmode", "-", new Vector2(60, 20), Vector2.zero, GUIController.Instance.arial, 15);

        for (int i = 0; i < SaveStateManager.maxSaveStates; i++)
        {
            int index = i; // lambda capturing reasons

            statePanel.AddButton($"Rename{i}", GUIController.Instance.images["ButtonRun"], new Vector2(-5, i * 20 + 42), new Vector2(12, 12),
                () => SaveStateManager.RenameSaveState(index), new Rect(0, 0, GUIController.Instance.images["ButtonRun"].width, GUIController.Instance.images["ButtonRun"].height));

            //Labels - these shouldn't be modified
            statePanel.AddText($"Slot{i}", i.ToString(), new Vector2(10, i * 20 + 40), Vector2.zero, GUIController.Instance.arial, 15);

            //Values
            statePanel.AddText(i.ToString(), "", new Vector2(50, i * 20 + 40), Vector2.zero, GUIController.Instance.arial, 15);
        }
    }

    public static void Update()
    {
        if (statePanel == null)
        {
            return;
        }

        if (GUIController.ForceHideUI())
        {
            statePanel.ActiveSelf = false;
            return;
        }

        statePanel.ActiveSelf = DebugMod.settings.SaveStatePanelVisible;

        if (statePanel.ActiveInHierarchy)
        {
            statePanel.GetText("currentmode").UpdateText(SaveStateManager.currentStateOperation);
            statePanel.GetText("CurrentFolder").UpdateText($"Page: {SaveStateManager.currentStateFolder+1}/{SaveStateManager.savePages}");

            Dictionary<int, string[]> info = SaveStateManager.GetSaveStatesInfo();

            for (int i = 0; i < SaveStateManager.maxSaveStates; i++)
            {
                if (info.TryGetValue(i, out string[] array))
                {
                    statePanel.GetText(i.ToString()).UpdateText($"{array[0]} - {array[1]}");
                    statePanel.GetButton($"Rename{i}").ActiveSelf = true;
                }
                else
                {
                    statePanel.GetText(i.ToString()).UpdateText("open");
                    statePanel.GetButton($"Rename{i}").ActiveSelf = false;
                }
            }
        }
    }
}