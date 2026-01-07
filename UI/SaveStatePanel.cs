using DebugMod.SaveStates;
using DebugMod.UI.Canvas;
using System.Collections.Generic;
using UnityEngine;

namespace DebugMod.UI;

public class SaveStatesPanel : CanvasPanel
{
    public static int QuickSlotButtonWidth => UICommon.ScaleWidth(90);
    public static int FileSlotButtonWidth => UICommon.ScaleWidth(60);
    public static int IconPadding => UICommon.ScaleHeight(4);

    public static SaveStatesPanel Instance { get; private set; }

    public static bool ShouldBeVisible => DebugMod.settings.SaveStatePanelVisible || InSelectState;
    public static bool ShouldBeExpanded => DebugMod.settings.SaveStatePanelExpanded || InSelectState;
    public static bool InSelectState => Instance != null && Instance.selectStateOperation != SelectOperation.None;

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
        Size = new Vector2(UICommon.SaveStatePanelWidth, 0);
        OnUpdate += Update;

        CanvasImage expandedBackground = UICommon.AddBackground(this);
        OnUpdate += () => expandedBackground.ActiveSelf = ShouldBeExpanded;

        CanvasPanel collapsed = Add(new CanvasPanel("Collapsed"));
        collapsed.Size = Size;
        collapsed.CollapseMode = CollapseMode.Deny;

        PanelBuilder builder = new(collapsed);
        builder.DynamicLength = true;
        builder.OuterPadding = ContentMargin(UICommon.Margin);
        builder.InnerPadding = UICommon.Margin;

        {
            using PanelBuilder quickslot = new(builder.AppendFixed(new CanvasPanel("Quickslot"), UICommon.ControlHeight));
            quickslot.Horizontal = true;

            CanvasText quickslotLabel = quickslot.AppendFlex(new CanvasText("Label"));
            quickslotLabel.Alignment = TextAnchor.MiddleLeft;
            quickslotLabel.OnUpdate += () => quickslotLabel.Text = $"Quickslot: {SaveStateManager.GetQuickState()}";

            CanvasButton load = quickslot.AppendFixed(new CanvasButton("Load"), QuickSlotButtonWidth - UICommon.ControlHeight);
            load.Text.Text = "Load";
            load.OnClicked += BindableFunctions.LoadState;

            UICommon.AppendKeybindButton(quickslot, DebugMod.bindActions["Quickslot (load)"]);

            quickslot.AppendPadding(UICommon.Margin);

            CanvasButton save = quickslot.AppendFixed(new CanvasButton("Save"), QuickSlotButtonWidth - UICommon.ControlHeight);
            save.Text.Text = "Save";
            save.OnClicked += BindableFunctions.SaveState;

            UICommon.AppendKeybindButton(quickslot, DebugMod.bindActions["Quickslot (save)"]);

            quickslot.AppendPadding(UICommon.Margin);

            CanvasPanel toggleViewWrapper = quickslot.AppendSquare(new CanvasPanel("ToggleViewWrapper"));
            toggleViewWrapper.CollapseMode = CollapseMode.AllowNoRenaming;
            using PanelBuilder wrapper = new(toggleViewWrapper);
            wrapper.Padding = IconPadding;

            CanvasButton toggleViewButton = wrapper.AppendFlex(new CanvasButton("ToggleView"));
            toggleViewButton.ImageOnly(UICommon.images["IconPlus"]);
            toggleViewButton.OnUpdate += () => toggleViewButton.SetImage(
                ShouldBeExpanded ? UICommon.images["IconMinus"] : UICommon.images["IconPlus"]);
            toggleViewButton.OnClicked += ToggleView;
        }

        builder.Build();

        CanvasImage collapsedBackground = UICommon.AddBackground(collapsed);
        OnUpdate += () => collapsedBackground.ActiveSelf = !ShouldBeExpanded;

        CanvasPanel expanded = Add(new CanvasPanel("Expanded"));
        expanded.Size = Size;
        expanded.CollapseMode = CollapseMode.Deny;
        OnUpdate += () => expanded.ActiveSelf = ShouldBeExpanded;

        builder = new(expanded);
        builder.DynamicLength = true;
        builder.OuterPadding = ContentMargin(UICommon.Margin);
        builder.Padding = UICommon.Margin;

        // Positions builder at the bottom of the collapsed elements
        builder.AppendPadding(collapsed.Size.y - builder.OuterPadding * 2);

        {
            using PanelBuilder pageControl = new(builder.AppendFixed(new CanvasPanel("Page"), UICommon.ScaleHeight(15)));
            pageControl.Horizontal = true;

            CanvasText pageText = pageControl.AppendFixed(new CanvasText("Current"), UICommon.ScaleWidth(70));
            pageText.Alignment = TextAnchor.MiddleLeft;
            pageText.OnUpdate += () => pageText.Text = $"Page {currentPage + 1}/{SaveStateManager.NumPages}";

            CanvasButton prevPage = pageControl.AppendSquare(new CanvasButton("Prev"));
            prevPage.ImageOnly(UICommon.images["IconMinus"]);
            prevPage.OnClicked += PrevPage;

            pageControl.AppendPadding(UICommon.Margin);

            CanvasButton nextPage = pageControl.AppendSquare(new CanvasButton("Next"));
            nextPage.ImageOnly(UICommon.images["IconPlus"]);
            nextPage.OnClicked += NextPage;

            CanvasText currentOperation = pageControl.AppendFlex(new CanvasText("CurrentOperation"));
            currentOperation.Alignment = TextAnchor.MiddleRight;
            currentOperation.OnUpdate += () => currentOperation.Text = PrettyPrintSelectOperation(selectStateOperation);
        }

        for (int i = 0; i < SaveStateManager.STATES_PER_PAGE; i++)
        {
            int index = i; // Need immutable variable to capture in lambda functions

            using PanelBuilder fileSlot = new(builder.AppendFixed(new CanvasPanel(index.ToString()), UICommon.ControlHeight));
            fileSlot.Horizontal = true;

            CanvasText number = fileSlot.AppendFixed(new CanvasText("Number"), UICommon.ScaleWidth(30));
            number.Alignment = TextAnchor.MiddleLeft;
            number.Text = index.ToString();
            number.OnUpdate += () => number.Color = InSelectState ? UICommon.yellowColor : UICommon.textColor;

            CanvasTextField name = fileSlot.AppendFlex(new CanvasTextField("Name"));
            name.Alignment = TextAnchor.MiddleLeft;
            name.OnUpdate += () => name.UpdateDefaultText(SaveStateManager.GetFileState(currentPage, index).ToString());
            name.OnSubmit += text => SaveStateManager.RenameFileState(currentPage, index, text);

            CanvasPanel renameWrapper = fileSlot.AppendSquare(new CanvasPanel("RenameWrapper"));
            renameWrapper.CollapseMode = CollapseMode.AllowNoRenaming;
            using PanelBuilder wrapper = new(renameWrapper);
            wrapper.Padding = IconPadding;

            CanvasButton rename = wrapper.AppendFlex(new CanvasButton("Rename"));
            rename.ImageOnly(UICommon.images["IconEditText"]);
            rename.OnClicked += () => name.Activate();

            fileSlot.AppendPadding(UICommon.Margin);

            CanvasButton read = fileSlot.AppendFixed(new CanvasButton("Read"), FileSlotButtonWidth);
            read.Text.Text = "Read";
            read.OnClicked += () => SaveStateManager.SetQuickState(SaveStateManager.GetFileState(currentPage, index));

            fileSlot.AppendPadding(UICommon.Margin);

            CanvasButton write = fileSlot.AppendFixed(new CanvasButton("Write"), FileSlotButtonWidth);
            write.Text.Text = "Write";
            write.OnClicked += () => SaveStateManager.SetFileState(SaveStateManager.GetQuickState(), currentPage, index);
        }

        builder.Build();
        Size = expanded.Size;
    }

    private static string PrettyPrintSelectOperation(SelectOperation operation)
    {
        return operation switch
        {
            SelectOperation.QuickslotToFile => "Writing quickslot to file slot",
            SelectOperation.FileToQuickslot => "Reading file slot to quickslot",
            SelectOperation.SaveToFile => "Saving directly to file slot",
            SelectOperation.LoadFromFile => "Loading directly from file slot",
            _ => null
        };
    }

    private void Update()
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
    }

    public void ToggleView()
    {
        if (InSelectState)
        {
            DebugMod.settings.SaveStatePanelExpanded = false;
            selectStateOperation = SelectOperation.None;
        }
        else
        {
            DebugMod.settings.SaveStatePanelExpanded = !DebugMod.settings.SaveStatePanelExpanded;
        }
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
        }
        else
        {
            selectStateOperation = operation;
        }
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