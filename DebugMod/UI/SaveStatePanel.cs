using System.Collections.Generic;
using DebugMod.SaveStates;
using DebugMod.UI.Canvas;
using UnityEngine;

namespace DebugMod.UI;

public class SaveStatesPanel : CanvasAutoPanel
{
    public const int SAVE_LOAD_BUTTON_WIDTH = 60;

    public static SaveStatesPanel Instance { get; private set; }

    public static bool ShouldBeVisible => DebugMod.settings.SaveStatePanelVisible || InSelectState;
    public static bool InSelectState => Instance?.selectStateOperation != SelectOperation.None;

    private SelectOperation selectStateOperation;
    private int currentPage;

    public static void BuildPanel()
    {
        Instance = new SaveStatesPanel();
        Instance.Build();
    }

    public SaveStatesPanel() : base(nameof(SaveStatesPanel))
    {
        LocalPosition = new Vector2(UICommon.SCREEN_MARGIN, 1080f - UICommon.SAVESTATES_HEIGHT - UICommon.CONSOLE_HEIGHT - UICommon.SCREEN_MARGIN * 2);
        Size = new Vector2(UICommon.LEFT_SIDE_WIDTH, UICommon.SAVESTATES_HEIGHT);

        UICommon.AddBackground(this);

        CanvasControl quickslot = Append(new CanvasControl("Quickslot"), UICommon.CONTROL_HEIGHT);

        CanvasText quickslotLabel = quickslot.AppendFlex(new CanvasText("Label"));
        quickslotLabel.Alignment = TextAnchor.MiddleLeft;
        quickslotLabel.OnUpdate += () => quickslotLabel.Text = $"Quickslot: {SaveStateManager.GetQuickState()}";

        CanvasButton quickslotSave = quickslot.Append(new CanvasButton("Save"), SAVE_LOAD_BUTTON_WIDTH);
        quickslotSave.Text.Text = "Save";
        quickslotSave.OnClicked += () => SaveStateManager.SetQuickState(SaveStateManager.SaveNewState());

        quickslot.AppendPadding(UICommon.MARGIN);

        CanvasButton quickslotLoad = quickslot.Append(new CanvasButton("Load"), SAVE_LOAD_BUTTON_WIDTH);
        quickslotLoad.Text.Text = "Load";
        quickslotLoad.OnClicked += () => SaveStateManager.LoadState(SaveStateManager.GetQuickState());

        CanvasControl pageControl = Append(new CanvasControl("Page"), 15f);

        CanvasText pageText = pageControl.Append(new CanvasText("CurrentPage"), 70f);
        pageText.Alignment = TextAnchor.MiddleLeft;
        pageText.OnUpdate += () => pageText.Text = $"Page {currentPage + 1}/{SaveStateManager.NumPages}";

        CanvasButton prevPage = pageControl.AppendSquare(new CanvasButton("Next"));
        prevPage.ImageOnly(UICommon.images["ButtonDel"]);
        prevPage.OnClicked += PrevPage;

        pageControl.AppendPadding(UICommon.MARGIN);

        CanvasButton nextPage = pageControl.AppendSquare(new CanvasButton("Next"));
        nextPage.ImageOnly(UICommon.images["ButtonPlus"]);
        nextPage.OnClicked += NextPage;

        for (int i = 0; i < SaveStateManager.STATES_PER_PAGE; i++)
        {
            int index = i; // lambda capturing reasons

            CanvasControl fileSlot = Append(new CanvasControl($"FileSlot{i}"), UICommon.CONTROL_HEIGHT);

            CanvasText label = fileSlot.AppendFlex(new CanvasText("Label"));
            label.Alignment = TextAnchor.MiddleLeft;
            label.OnUpdate += () => label.Text = $"{index}: {SaveStateManager.GetFileState(currentPage, index)}";

            CanvasButton rename = fileSlot.AppendSquare(new CanvasButton("Rename"));
            rename.ImageOnly(UICommon.images["ButtonRun"]);
            rename.OnClicked += () =>
            {
                int page = currentPage;
                SaveState state = SaveStateManager.GetFileState(page, index);

                if (state.IsSet())
                {
                    GUIController.Instance.TextBox(state.data.saveStateIdentifier,
                        name => SaveStateManager.RenameFileState(page, index, name));
                }
            };

            fileSlot.AppendPadding(UICommon.MARGIN);

            CanvasButton save = fileSlot.Append(new CanvasButton("Save"), SAVE_LOAD_BUTTON_WIDTH);
            save.Text.Text = "Save";
            save.OnClicked += () => SaveStateManager.SetFileState(SaveStateManager.GetQuickState(), currentPage, index);

            fileSlot.AppendPadding(UICommon.MARGIN);

            CanvasButton load = fileSlot.Append(new CanvasButton("Load"), SAVE_LOAD_BUTTON_WIDTH);
            load.Text.Text = "Load";
            load.OnClicked += () => SaveStateManager.SetQuickState(SaveStateManager.GetFileState(currentPage, index));
        }

        CanvasText currentOperation = Append(new CanvasText("CurrentOperation"), 15f);
        currentOperation.OnUpdate += () => currentOperation.Text = PrettyPrintSelectOperation(selectStateOperation);
    }

    private static string PrettyPrintSelectOperation(SelectOperation operation)
    {
        return operation switch
        {
            SelectOperation.QuickslotToFile => "Saving quickslot to file slot",
            SelectOperation.FileToQuickslot => "Loading file slot to quickslot",
            SelectOperation.SaveToFile => "Saving to file slot",
            SelectOperation.LoadFromFile => "Loading from file slot",
            _ => null
        };
    }

    public override void Build()
    {
        float targetHeight = Offset + UICommon.MARGIN;
        if (Size.y > targetHeight)
        {
            LocalPosition -= new Vector2(0, targetHeight - Size.y);
            Size = new Vector2(Size.x, targetHeight);
        }

        base.Build();
    }

    public override void Update()
    {
        if (InSelectState)
        {
            foreach (KeyValuePair<KeyCode, int> pair in DebugMod.alphaKeyDict)
            {
                if (Input.GetKeyDown(pair.Key))
                {
                    DoSelectOperation(pair.Value);
                    break;
                }
            }
        }

        base.Update();
    }

    private void DoSelectOperation(int index)
    {
        switch (selectStateOperation)
        {
            case SelectOperation.QuickslotToFile:
                SaveStateManager.SetFileState(SaveStateManager.GetQuickState(), currentPage, index);
                break;
            case SelectOperation.FileToQuickslot:
                SaveStateManager.SetQuickState(SaveStateManager.GetFileState(currentPage, index));
                break;
            case SelectOperation.SaveToFile:
                SaveStateManager.SetFileState(SaveStateManager.SaveNewState(), currentPage, index);
                break;
            case SelectOperation.LoadFromFile:
                SaveStateManager.LoadState(SaveStateManager.GetFileState(currentPage, index));
                break;
        }

        selectStateOperation = SelectOperation.None;
    }

    public void NextPage()
    {
        if (ActiveInHierarchy)
        {
            currentPage++;
            if (currentPage >= SaveStateManager.NumPages)
            {
                currentPage -= SaveStateManager.NumPages;
            }
        }
    }

    public void PrevPage()
    {
        if (ActiveInHierarchy)
        {
            currentPage--;
            if (currentPage < 0)
            {
                currentPage += SaveStateManager.NumPages;
            }
        }
    }

    public void EnterSelectState(SelectOperation operation)
    {
        if (selectStateOperation == operation)
        {
            selectStateOperation = SelectOperation.None;
            return;
        }

        selectStateOperation = operation;
    }
}

public enum SelectOperation
{
    None,
    QuickslotToFile,
    FileToQuickslot,
    SaveToFile,
    LoadFromFile
}