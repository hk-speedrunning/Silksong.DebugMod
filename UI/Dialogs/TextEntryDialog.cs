using DebugMod.Helpers;
using DebugMod.UI.Canvas;
using System;
using UnityEngine;

namespace DebugMod.UI.Dialogs;

public class TextEntryDialog : CanvasDialog
{
    public static int PanelWidth => UICommon.ScaleWidth(300);
    public static int ButtonWidth => UICommon.ScaleWidth(60);

    public static TextEntryDialog Instance { get; private set; }

    private string prompt;
    private Action<string> onSubmit;
    private string starterText;
    private Func<string, string> errorMessage;

    public static void BuildPanel()
    {
        Instance = new TextEntryDialog();
    }

    public TextEntryDialog() : base(nameof(TextEntryDialog)) { }

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
        promptText.Text = Localization.Get(prompt);

        CanvasButton textFieldButton = builder.AppendFixed(new CanvasButton("Button"), UICommon.ControlHeight);
        textFieldButton.SetImage(UICommon.panelDarkBG);

        CanvasTextField textField = textFieldButton.SetTextField();
        textField.Persistent = true;
        textField.Alignment = TextAnchor.MiddleLeft;
        textField.Text = starterText;

        CanvasPanel row = builder.AppendFixed(new CanvasPanel("Row"), UICommon.ControlHeight);
        using PanelBuilder rowBuilder = new(row);
        rowBuilder.Horizontal = true;

        if (errorMessage != null)
        {
            CanvasText errorText = rowBuilder.AppendFlex(new CanvasText("ErrorText"));
            errorText.Alignment = TextAnchor.MiddleLeft;
            errorText.OnUpdate += () =>
            {
                string error = errorMessage(textField.Text);
                errorText.Text = error == "" ? error : Localization.Get(error);
            };
        }
        else
        {
            rowBuilder.AppendFlexPadding();
        }

        CanvasButton submitButton = rowBuilder.AppendFixed(new CanvasButton("Submit"), ButtonWidth);
        submitButton.Text.Text = Localization.Get("TEXTENTRY_SUBMIT");
        submitButton.OnClicked += () =>
        {
            onSubmit(textField.Text);
            Hide();
        };

        rowBuilder.AppendPadding(UICommon.Margin);

        CanvasButton cancelButton = rowBuilder.AppendFixed(new CanvasButton("Cancel"), ButtonWidth);
        cancelButton.Text.Text = Localization.Get("TEXTENTRY_CANCEL");
        cancelButton.OnClicked += Hide;
    }

    public void Toggle(CanvasNode anchor, string prompt, Action<string> onSubmit, string starterText = "", Func<string, string> errorMessage = null)
    {
        if (TryStartToggle(anchor))
        {
            this.prompt = prompt;
            this.onSubmit = onSubmit;
            this.starterText = starterText;
            this.errorMessage = errorMessage;

            Show();
        }
    }
}
