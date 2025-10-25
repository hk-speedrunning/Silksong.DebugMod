using System.Collections.Generic;
using DebugMod.SaveStates;
using DebugMod.UI.Canvas;
using UnityEngine;

namespace DebugMod.UI;

public class SaveStatesPanel : CanvasPanel
{
    public static SaveStatesPanel Instance { get; private set; }

    public static void Build()
    {
        Instance = new SaveStatesPanel();
    }

    public SaveStatesPanel() : base(nameof(SaveStatesPanel), null, new Vector2(720f, 40f), Vector2.zero, GUIController.Instance.images["BlankVertical"])
    {
        AddText("CurrentFolder", "Page: " + (SaveStateManager.currentStateFolder + 1).ToString(), new Vector2(8, 0), Vector2.zero, GUIController.Instance.arial, 15);
        AddText("Mode", "mode: ", new Vector2(8, 20), Vector2.zero, GUIController.Instance.arial, 15);
        AddText("currentmode", "-", new Vector2(60, 20), Vector2.zero, GUIController.Instance.arial, 15);

        for (int i = 0; i < SaveStateManager.maxSaveStates; i++)
        {
            int index = i; // lambda capturing reasons

            AddButton($"Rename{i}", GUIController.Instance.images["ButtonRun"], new Vector2(-5, i * 20 + 42), new Vector2(12, 12),
                () => SaveStateManager.RenameSaveState(index), new Rect(0, 0, GUIController.Instance.images["ButtonRun"].width, GUIController.Instance.images["ButtonRun"].height));

            //Labels - these shouldn't be modified
            AddText($"Slot{i}", i.ToString(), new Vector2(10, i * 20 + 40), Vector2.zero, GUIController.Instance.arial, 15);

            //Values
            AddText(i.ToString(), "", new Vector2(50, i * 20 + 40), Vector2.zero, GUIController.Instance.arial, 15);
        }
    }

    public override void Update()
    {
        if (GUIController.ForceHideUI())
        {
            ActiveSelf = false;
            return;
        }

        ActiveSelf = DebugMod.settings.SaveStatePanelVisible;

        if (ActiveInHierarchy)
        {
            GetText("currentmode").UpdateText(SaveStateManager.currentStateOperation);
            GetText("CurrentFolder").UpdateText($"Page: {SaveStateManager.currentStateFolder+1}/{SaveStateManager.savePages}");

            Dictionary<int, string[]> info = SaveStateManager.GetSaveStatesInfo();

            for (int i = 0; i < SaveStateManager.maxSaveStates; i++)
            {
                if (info.TryGetValue(i, out string[] array))
                {
                    GetText(i.ToString()).UpdateText($"{array[0]} - {array[1]}");
                    GetButton($"Rename{i}").ActiveSelf = true;
                }
                else
                {
                    GetText(i.ToString()).UpdateText("open");
                    GetButton($"Rename{i}").ActiveSelf = false;
                }
            }
        }
    }
}