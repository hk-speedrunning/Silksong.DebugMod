using DebugMod.Helpers;
using DebugMod.SaveStates;
using DebugMod.UI.Canvas;
using DebugMod.UI.Dialogs;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DebugMod.UI;

public class SaveStatesPanel : CanvasPanel
{
    public static int QuickSlotButtonWidth => UICommon.ScaleWidth(90);
    public static int FileSlotButtonWidth => UICommon.ScaleWidth(60);
    public static int IconPadding => UICommon.ScaleHeight(4);

    public static SaveStatesPanel Instance { get; private set; }
    public static bool loadedAnySavestate;

    public static bool ShouldBeVisible => DebugMod.settings.SaveStatePanelVisible || InSelectState;
    public static bool ShouldBeExpanded => DebugMod.settings.SaveStatePanelExpanded || InSelectState;
    public static bool InSelectState => Instance != null && Instance.selectStateOperation != SelectOperation.None;

    private SelectOperation selectStateOperation;
    private int currentPage;
    private bool editMode;

    static SaveStatesPanel()
    {
        SaveStateManager.PackChanged += () => Instance?.PackChanged();
        SaveState.AfterLoad += _ => loadedAnySavestate = true;
    }

    public static void BuildPanel()
    {
        Instance = new SaveStatesPanel();
        Instance.Build();
    }

    public SaveStatesPanel() : base(nameof(SaveStatesPanel))
    {
        LocalPosition = new Vector2(Screen.width / 2f - UICommon.SaveStatePanelWidth / 2f, UICommon.ScreenMargin);
        Size = new Vector2(UICommon.SaveStatePanelWidth, 0);
        OnUpdate += DoUpdate;

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
            quickslotLabel.OnUpdate += () => quickslotLabel.Text = string.Format(
                Localization.Get("SAVESTATEPANEL_QUICKSLOTFORMAT"), SaveStateManager.GetQuickState());

            CanvasButton load = quickslot.AppendFixed(new CanvasButton("Load"), QuickSlotButtonWidth - UICommon.ControlHeight);
            load.Text.Text = Localization.Get("SAVESTATEPANEL_QUICKSLOTLOAD");
            load.OnClicked += () =>
            {
                CancelSelectState(true);

                if (PlayerData.instance.playTime >= 3600 * 2 && !loadedAnySavestate)
                {
                    ConfirmDialog.Instance.Toggle(load, "SAVESTATEPANEL_LOADWARNING",
                        () =>
                        {
                            SaveStateManager.LoadState(SaveStateManager.GetQuickState());
                        },
                        width: UICommon.ScaleWidth(250), lines: 3);
                }
                else
                {
                    SaveStateManager.LoadState(SaveStateManager.GetQuickState());
                }
            };

            UICommon.AppendKeybindButton(quickslot, DebugMod.bindActions["SAVESTATES_QUICKSLOTLOAD"]);

            quickslot.AppendPadding(UICommon.Margin);

            CanvasButton save = quickslot.AppendFixed(new CanvasButton("Save"), QuickSlotButtonWidth - UICommon.ControlHeight);
            save.Text.Text = Localization.Get("SAVESTATEPANEL_QUICKSLOTSAVE");
            save.OnClicked += () =>
            {
                CancelSelectState(true);
                SaveStateManager.SetQuickState(SaveStateManager.SaveNewState());
            };

            UICommon.AppendKeybindButton(quickslot, DebugMod.bindActions["SAVESTATES_QUICKSLOTSAVE"]);

            quickslot.AppendPadding(UICommon.Margin);

            CanvasPanel toggleViewWrapper = quickslot.AppendSquare(new CanvasPanel("ToggleViewWrapper"));
            toggleViewWrapper.CollapseMode = CollapseMode.AllowNoRenaming;
            using PanelBuilder wrapper = new(toggleViewWrapper);
            wrapper.Padding = IconPadding;

            CanvasButton toggleViewButton = wrapper.AppendFlex(new CanvasButton("ToggleView"));
            toggleViewButton.ImageOnly(UICommon.images["IconDown"]);
            toggleViewButton.OnUpdate += () => toggleViewButton.SetImage(
                ShouldBeExpanded ? UICommon.images["IconUp"] : UICommon.images["IconDown"]);
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
        builder.InnerPadding = UICommon.Margin;

        // Positions builder at the bottom of the collapsed elements
        builder.AppendPadding((int)collapsed.Size.y - builder.OuterPadding * 2);

        {
            PanelBuilder pageRow = new(builder.AppendFixed(new CanvasPanel("PageRow"), UICommon.ScaleHeight(16)));
            pageRow.InnerPadding = UICommon.Margin;
            pageRow.Horizontal = true;

            using PanelBuilder leftSide = new(pageRow.AppendFlex(new CanvasPanel("Left")));
            leftSide.InnerPadding = UICommon.Margin;
            leftSide.Horizontal = true;
            
            // Edit button + Page Edit cluster
            
            CanvasButton editButton = leftSide.AppendSquare(new CanvasButton("Edit"));
            editButton.ImageOnly(UICommon.images["IconEditOff"]);
            editButton.OnUpdate += () => editButton.SetImage(UICommon.images[editMode ? "IconEditOn" : "IconEditOff"]);
            editButton.OnClicked += () => editMode = !editMode;
            
            leftSide.AppendPadding(leftSide.ChildBreadth());

            CanvasButton addPageButton = leftSide.AppendSquare(new CanvasButton("AddPage"));
            OnUpdate += () => addPageButton.ActiveSelf = editMode;
            addPageButton.ImageOnly(UICommon.images["IconPlus"]);
            addPageButton.OnClicked += () =>
            {
                SaveStateManager.AddPage(currentPage + 1);
                currentPage++;
            };

            CanvasButton removePageButton = leftSide.AppendSquare(new CanvasButton("RemovePage"));
            OnUpdate += () => removePageButton.ActiveSelf = editMode;
            removePageButton.ImageOnly(UICommon.images["IconX"]);
            removePageButton.OnClicked += () =>
            {
                if (!SaveStateManager.RemovePage(currentPage, false))
                {
                    ConfirmDialog.Instance.Toggle(removePageButton, "SAVESTATEPANEL_DELETEPAGEPROMPT",
                        () => SaveStateManager.RemovePage(currentPage, true), width: 250);
                }

                if (currentPage >= SaveStateManager.NumPages)
                {
                    currentPage = SaveStateManager.NumPages - 1;
                }
            };
            
            CanvasButton movePageLeft = leftSide.AppendSquare(new CanvasButton("MovePageLeft"));
            OnUpdate += () => movePageLeft.ActiveSelf = editMode;
            movePageLeft.ImageOnly(UICommon.images["IconLeft"]);
            movePageLeft.OnClicked += () =>
            {
                SaveStateManager.SwapPages(currentPage, WrapPageNumber(currentPage - 1));
                currentPage = WrapPageNumber(currentPage - 1);
            };
            
            CanvasButton movePageRight = leftSide.AppendSquare(new CanvasButton("MovePageRight"));
            OnUpdate += () => movePageRight.ActiveSelf = editMode;
            movePageRight.ImageOnly(UICommon.images["IconRight"]);
            movePageRight.OnClicked += () =>
            {
                SaveStateManager.SwapPages(currentPage, WrapPageNumber(currentPage + 1));
                currentPage = WrapPageNumber(currentPage + 1);
            };
            
            // Page nav cluster
            
            CanvasButton prevPage = pageRow.AppendSquare(new CanvasButton("Prev"));
            prevPage.ImageOnly(UICommon.images["IconLeftMin"]);
            prevPage.OnClicked += PrevPage;

            CanvasText pageText = pageRow.AppendFixed(new CanvasText("Page Number"), UICommon.ScaleWidth(80));
            pageText.Alignment = TextAnchor.MiddleCenter;
            pageText.OnUpdate += () => pageText.Text = string.Format(
                Localization.Get("SAVESTATEPANEL_PAGEFORMAT"), currentPage + 1, SaveStateManager.NumPages);

            CanvasButton nextPage = pageRow.AppendSquare(new CanvasButton("Next"));
            nextPage.ImageOnly(UICommon.images["IconRightMin"]);
            nextPage.OnClicked += NextPage;

            using PanelBuilder rightSide = new(pageRow.AppendFlex(new CanvasPanel("Right")));
            rightSide.Horizontal = true;

            pageRow.Build();

            CanvasText currentOperation = rightSide.AppendFlex(new CanvasText("CurrentOperation"));
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
            number.OnUpdate += () => number.Color = InSelectState ? UICommon.highlightColor : UICommon.textColor;

            CanvasTextField name = fileSlot.AppendFlex(new CanvasTextField("Name"));
            name.Alignment = TextAnchor.MiddleLeft;
            name.Overflow = HorizontalWrapMode.Overflow;
            name.OnUpdate += () => name.UpdateDefaultText(SaveStateManager.GetFileState(currentPage, index).ToString());
            name.OnSubmit += text => SaveStateManager.RenameFileState(currentPage, index, text);

            {
                CanvasPanel iconsWrapper = fileSlot.AppendLazy(new CanvasPanel("IconsWrapper"));
                using PanelBuilder wrapper = new(iconsWrapper);
                wrapper.OuterPadding = IconPadding;
                wrapper.InnerPadding = UICommon.Margin;
                wrapper.Horizontal = true;
                wrapper.DynamicLength = true;

                CanvasButton moveUp = wrapper.AppendSquare(new CanvasButton("MoveUp"));
                OnUpdate += () => moveUp.ActiveSelf = editMode;
                moveUp.ImageOnly(UICommon.images["IconUpMin"]);
                moveUp.OnClicked += () =>
                {
                    int otherPage;
                    int otherIndex;

                    if (index > 0)
                    {
                        otherPage = currentPage;
                        otherIndex = index - 1;
                    }
                    else
                    {
                        otherPage = currentPage - 1;
                        otherIndex = SaveStateManager.STATES_PER_PAGE - 1;

                        if (otherPage < 0)
                        {
                            return;
                        }
                    }

                    SaveStateManager.SwapFileStates(currentPage, index, otherPage, otherIndex);
                };

                CanvasButton moveDown = wrapper.AppendSquare(new CanvasButton("MoveDown"));
                OnUpdate += () => moveDown.ActiveSelf = editMode;
                moveDown.ImageOnly(UICommon.images["IconDownMin"]);
                moveDown.OnClicked += () =>
                {
                    int otherPage;
                    int otherIndex;

                    if (index < SaveStateManager.STATES_PER_PAGE - 1)
                    {
                        otherPage = currentPage;
                        otherIndex = index + 1;
                    }
                    else
                    {
                        otherPage = currentPage + 1;
                        otherIndex = 0;

                        if (otherPage >= SaveStateManager.NumPages)
                        {
                            return;
                        }
                    }

                    SaveStateManager.SwapFileStates(currentPage, index, otherPage, otherIndex);
                };

                CanvasButton delete = wrapper.AppendSquare(new CanvasButton("Delete"));
                OnUpdate += () => delete.ActiveSelf = editMode;
                delete.ImageOnly(UICommon.images["IconX"]);
                delete.OnClicked += () =>
                {
                    ConfirmDialog.Instance.Toggle(delete, "SAVESTATEPANEL_DELETESTATEPROMPT",
                        () => SaveStateManager.DeleteFileState(currentPage, index));
                };

                CanvasButton rename = wrapper.AppendSquare(new CanvasButton("Rename"));
                rename.ImageOnly(UICommon.images["IconEditText"]);
                rename.OnClicked += () =>
                {
                    CancelSelectState(true);
                    name.Activate();
                };

                // Removes padding from right edge
                wrapper.AppendPadding(-wrapper.OuterPadding - wrapper.InnerPadding);
            }

            fileSlot.AppendPadding(UICommon.Margin);

            CanvasButton read = fileSlot.AppendFixed(new CanvasButton("Read"), FileSlotButtonWidth);
            read.Text.Text = Localization.Get("SAVESTATEPANEL_FILESLOTREAD");
            read.OnUpdate += () => read.Border.Color = IsReadOperation() ? UICommon.highlightColor : UICommon.borderColor;
            read.OnClicked += () =>
            {
                if (IsReadOperation())
                {
                    DoSelectOperation(index);
                }
                else if (InSelectState)
                {
                    CancelSelectState(true);
                }
                else
                {
                    SaveStateManager.SetQuickState(SaveStateManager.GetFileState(currentPage, index));
                }
            };

            fileSlot.AppendPadding(UICommon.Margin);

            CanvasButton write = fileSlot.AppendFixed(new CanvasButton("Write"), FileSlotButtonWidth);
            write.Text.Text = Localization.Get("SAVESTATEPANEL_FILESLOTWRITE");
            write.OnUpdate += () => write.Border.Color = IsWriteOperation() ? UICommon.highlightColor : UICommon.borderColor;
            write.OnClicked += () =>
            {
                if (InSelectState && !IsWriteOperation())
                {
                    CancelSelectState(true);
                    return;
                }

                if (selectStateOperation != SelectOperation.SaveToFile && !SaveStateManager.GetQuickState().IsSet())
                {
                    DebugMod.LogConsole("Save a state to the quickslot before copying to a file slot");
                    return;
                }

                Action action = InSelectState
                    ? () => DoSelectOperation(index)
                    : () => SaveStateManager.SetFileState(SaveStateManager.GetQuickState(), currentPage, index);

                if (SaveStateManager.GetFileState(currentPage, index).IsSet())
                {
                    ConfirmDialog.Instance.Toggle(write, "SAVESTATEPANEL_OVERWRITEPROMPT", action, () => CancelSelectState(true));
                }
                else
                {
                    action();
                }
            };
        }

        builder.Build();
        Size = expanded.Size;
    }

    private static string PrettyPrintSelectOperation(SelectOperation operation)
    {
        return operation switch
        {
            SelectOperation.QuickslotToFile => Localization.Get("SAVESTATEPANEL_OPERATION_QUICKSLOTTOFILE"),
            SelectOperation.FileToQuickslot => Localization.Get("SAVESTATEPANEL_OPERATION_FILETOQUICKSLOT"),
            SelectOperation.SaveToFile => Localization.Get("SAVESTATEPANEL_OPERATION_SAVETOFILE"),
            SelectOperation.LoadFromFile => Localization.Get("SAVESTATEPANEL_OPERATION_LOADFROMFILE"),
            _ => null
        };
    }

    private bool IsReadOperation()
    {
        return selectStateOperation is SelectOperation.FileToQuickslot or SelectOperation.LoadFromFile;
    }

    private bool IsWriteOperation()
    {
        return selectStateOperation is SelectOperation.QuickslotToFile or SelectOperation.SaveToFile;
    }

    private void DoUpdate()
    {
        if (InSelectState && !CanvasTextField.AnyFieldFocused)
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
            CancelSelectState();
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

        CancelSelectState();
    }

    public void NextPage()
    {
        if (ShouldBeVisible && ShouldBeExpanded)
        {
            currentPage = WrapPageNumber(currentPage + 1);
        }
    }

    public void PrevPage()
    {
        if (ShouldBeVisible && ShouldBeExpanded)
        {
            currentPage = WrapPageNumber(currentPage - 1);
        }
    }

    private int WrapPageNumber(int x)
    {
        if (x < 0)
        {
            x += SaveStateManager.NumPages;
        }
        else if (x >= SaveStateManager.NumPages)
        {
            x -= SaveStateManager.NumPages;
        }

        return x;
    }

    public void EnterSelectState(SelectOperation operation)
    {
        if (selectStateOperation == operation)
        {
            CancelSelectState();
        }
        else
        {
            selectStateOperation = operation;
        }
    }

    public void CancelSelectState(bool leavePanelOpen = false)
    {
        if (InSelectState && leavePanelOpen)
        {
            DebugMod.settings.SaveStatePanelVisible = true;
            DebugMod.settings.SaveStatePanelExpanded = true;
        }

        selectStateOperation = SelectOperation.None;
    }

    private void PackChanged()
    {
        currentPage = 0;
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