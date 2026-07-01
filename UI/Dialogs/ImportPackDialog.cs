using DebugMod.Helpers;
using DebugMod.SaveStates;
using DebugMod.UI.Canvas;
using System.Collections.Generic;
using UnityEngine;

namespace DebugMod.UI.Dialogs;

public class ImportPackDialog : CanvasDialog
{
    public static int PanelWidth => UICommon.ScaleWidth(300);
    public static int ButtonWidth => UICommon.ScaleWidth(60);

    public static ImportPackDialog Instance { get; private set; }

    private string selectedPack;

    public static void BuildPanel()
    {
        Instance = new ImportPackDialog();
    }

    public ImportPackDialog() : base(nameof(ImportPackDialog)) { }

    protected override void BuildDialog()
    {
        base.BuildDialog();

        Size = new Vector2(PanelWidth, 0f);

        using PanelBuilder builder = new(this);
        builder.OuterPadding = ContentMargin(UICommon.Margin);
        builder.InnerPadding = UICommon.Margin;
        builder.DynamicLength = true;

        CanvasText promptText = builder.AppendFixed(new CanvasText("Prompt"), UICommon.ControlHeight);
        promptText.Alignment = TextAnchor.MiddleCenter;
        promptText.Text = Localization.Get("SAVESTATES_IMPORTPACKPROMPT");

        CanvasPanel dropdown = builder.AppendFixed(new CanvasPanel("Dropdown"), UICommon.ControlHeight);

        CanvasButton dropdownButton = dropdown.Add(new CanvasButton("Current"));
        dropdownButton.Size = dropdown.Size;
        dropdownButton.Text.Alignment = TextAnchor.MiddleLeft;
        dropdownButton.Text.OnUpdate += () => dropdownButton.Text.Text = selectedPack ?? Localization.Get("SAVESTATES_NOPACKSELECTED");
        dropdownButton.OnClicked += () =>
        {
            List<string> packs = SaveStateManager.GetPackNames();
            string noPacksOption = Localization.Get("SAVESTATES_NOPACKSFOUND");

            if (packs.Count == 0)
            {
                packs.Add(noPacksOption);
            }

            DropdownDialog.Instance.Toggle(dropdownButton, packs,
                value =>
                {
                    if (value != noPacksOption)
                    {
                        selectedPack = value;
                    }
                    else
                    {
                        selectedPack = null;
                    }
                }
            );
        };

        CanvasImage dropdownIcon = dropdown.Add(new CanvasImage("DropdownIcon"));
        dropdownIcon.LocalPosition = new Vector2(dropdown.Size.x - dropdown.Size.y, 0f);
        dropdownIcon.Size = new Vector2(dropdown.Size.y, dropdown.Size.y);
        dropdownIcon.SetImage(UICommon.images["IconDownMin"]);
        dropdownIcon.OnUpdate += () =>
        {
            if (DropdownDialog.Instance.IsOpenFor(dropdownButton))
            {
                dropdownIcon.SetImage(UICommon.images["IconUpMin"]);
            }
            else
            {
                dropdownIcon.SetImage(UICommon.images["IconDownMin"]);
            }
        };

        CanvasText warningText = builder.AppendFixed(new CanvasText("Warning"), UICommon.ScaleHeight(54));
        warningText.Alignment = TextAnchor.MiddleCenter;
        warningText.Text = Localization.Get("SAVESTATES_IMPORTWARNING");

        CanvasPanel row = builder.AppendFixed(new CanvasPanel("Row"), UICommon.ControlHeight);
        using PanelBuilder rowBuilder = new(row);
        rowBuilder.InnerPadding = UICommon.Margin;
        rowBuilder.Horizontal = true;

        rowBuilder.AppendFlexPadding();

        CanvasButton submitButton = rowBuilder.AppendFixed(new CanvasButton("Submit"), ButtonWidth);
        submitButton.Text.Text = Localization.Get("SAVESTATES_IMPORTINDIALOG");
        submitButton.OnClicked += () =>
        {
            if (selectedPack != null)
            {
                SaveStateManager.ImportPack(selectedPack);
                Hide();
            }
        };

        CanvasButton cancelButton = rowBuilder.AppendFixed(new CanvasButton("Cancel"), ButtonWidth);
        cancelButton.Text.Text = Localization.Get("DIALOG_CANCEL");
        cancelButton.OnClicked += Hide;
    }

    public void Toggle(CanvasNode anchor)
    {
        if (TryStartToggle(anchor))
        {
            selectedPack = null;
            SaveStateManager.RefreshSavestatePacks();

            Show();
        }
    }
}