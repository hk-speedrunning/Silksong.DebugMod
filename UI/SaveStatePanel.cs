using DebugMod.SaveStates;
using DebugMod.UI.Canvas;
using System.Collections.Generic;
using TMProOld;
using UnityEngine;

namespace DebugMod.UI;

public class SaveStatesPanel : CanvasPanel
{
    public static int SaveLoadButtonWidth => UICommon.ScaleWidth(60);

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
        LocalPosition = new Vector2(Screen.width / 2f - UICommon.SaveStatePanelWidth / 2f, UICommon.ScreenMargin);
        Size = new Vector2(UICommon.SaveStatePanelWidth, UICommon.SaveStatePanelHeight);

        UICommon.AddBackground(this);

        using PanelBuilder builder = new(this);
        builder.DynamicLength = true;
        builder.Padding = UICommon.Margin;

        using PanelBuilder quickslot = new(builder.AppendFixed(new CanvasPanel("Quickslot"), UICommon.ControlHeight));
        quickslot.Horizontal = true;

        CanvasText quickslotLabel = quickslot.AppendFlex(new CanvasText("Label"));
        quickslotLabel.Alignment = TextAlignmentOptions.Left;
        quickslotLabel.OnUpdate += () => quickslotLabel.Text = $"Quickslot: {SaveStateManager.GetQuickState()}";

        CanvasButton quickslotSave = quickslot.AppendFixed(new CanvasButton("Save"), SaveLoadButtonWidth);
        quickslotSave.Text.Text = "Save";
        quickslotSave.OnClicked += () => SaveStateManager.SetQuickState(SaveStateManager.SaveNewState());

        quickslot.AppendPadding(UICommon.Margin);

        CanvasButton quickslotLoad = quickslot.AppendFixed(new CanvasButton("Load"), SaveLoadButtonWidth);
        quickslotLoad.Text.Text = "Load";
        quickslotLoad.OnClicked += () => SaveStateManager.LoadState(SaveStateManager.GetQuickState());

        using PanelBuilder pageControl = new(builder.AppendFixed(new CanvasPanel("Page"), UICommon.ScaleHeight(15)));
        pageControl.Horizontal = true;

        CanvasText pageText = pageControl.AppendFixed(new CanvasText("Current"), UICommon.ScaleWidth(70));
        pageText.Alignment = TextAlignmentOptions.Left;
        pageText.OnUpdate += () => pageText.Text = $"Page {currentPage + 1}/{SaveStateManager.NumPages}";

        CanvasButton prevPage = pageControl.AppendSquare(new CanvasButton("Prev"));
        prevPage.ImageOnly(UICommon.images["ButtonDel"]);
        prevPage.OnClicked += PrevPage;

        pageControl.AppendPadding(UICommon.Margin);

        CanvasButton nextPage = pageControl.AppendSquare(new CanvasButton("Next"));
        nextPage.ImageOnly(UICommon.images["ButtonPlus"]);
        nextPage.OnClicked += NextPage;

        for (int i = 0; i < SaveStateManager.STATES_PER_PAGE; i++)
        {
            int index = i; // lambda capturing reasons

            using PanelBuilder fileSlot = new(builder.AppendFixed(new CanvasPanel(i.ToString()), UICommon.ControlHeight));
            fileSlot.Horizontal = true;

            CanvasText number = fileSlot.AppendFixed(new CanvasText("Number"), 30f);
            number.Alignment = TextAlignmentOptions.Left;
            number.Text = $"{index}:";

            CanvasTextField name = fileSlot.AppendFlex(new CanvasTextField("Name"));
            name.Alignment = TextAlignmentOptions.Left;
            name.OnUpdate += () => name.UpdateDefaultText(SaveStateManager.GetFileState(currentPage, index).ToString());
            name.OnSubmit += text => SaveStateManager.RenameFileState(currentPage, index, text);

            CanvasButton rename = fileSlot.AppendSquare(new CanvasButton("Rename"));
            rename.ImageOnly(UICommon.images["ButtonRun"]);
            rename.OnClicked += () => name.Activate();

            fileSlot.AppendPadding(UICommon.Margin);

            CanvasButton save = fileSlot.AppendFixed(new CanvasButton("Save"), SaveLoadButtonWidth);
            save.Text.Text = "Save";
            save.OnClicked += () => SaveStateManager.SetFileState(SaveStateManager.GetQuickState(), currentPage, index);

            fileSlot.AppendPadding(UICommon.Margin);

            CanvasButton load = fileSlot.AppendFixed(new CanvasButton("Load"), SaveLoadButtonWidth);
            load.Text.Text = "Load";
            load.OnClicked += () => SaveStateManager.SetQuickState(SaveStateManager.GetFileState(currentPage, index));
        }

        CanvasText currentOperation = builder.AppendFixed(new CanvasText("CurrentOperation"), UICommon.ScaleWidth(15));
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