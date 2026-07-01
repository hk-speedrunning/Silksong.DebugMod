using DebugMod.Helpers;
using DebugMod.SaveStates;
using DebugMod.UI.Canvas;
using UnityEngine;

namespace DebugMod.UI.Dialogs;

public class ExportPackDialog : CanvasDialog
{
    public static int PanelWidth => UICommon.ScaleWidth(300);
    public static int ButtonWidth => UICommon.ScaleWidth(60);

    public static ExportPackDialog Instance { get; private set; }

    public static void BuildPanel()
    {
        Instance = new ExportPackDialog();
    }

    public ExportPackDialog() : base(nameof(ExportPackDialog)) { }

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
        promptText.Text = Localization.Get("SAVESTATES_EXPORTPACKPROMPT");

        CanvasButton textFieldButton = builder.AppendFixed(new CanvasButton("Button"), UICommon.ControlHeight);
        textFieldButton.SetImage(UICommon.panelDarkBG);
        textFieldButton.RemoveHoverBorder();

        CanvasTextField textField = textFieldButton.SetTextField();
        textField.Persistent = true;
        textField.Alignment = TextAnchor.MiddleLeft;
        textField.Text = DebugMod.settings.LastLoadedPack;

        CanvasPanel row = builder.AppendFixed(new CanvasPanel("Row"), UICommon.ControlHeight);
        using PanelBuilder rowBuilder = new(row);
        rowBuilder.InnerPadding = UICommon.Margin;
        rowBuilder.Horizontal = true;

        CanvasText errorText = rowBuilder.AppendFlex(new CanvasText("ErrorText"));
        errorText.Alignment = TextAnchor.MiddleLeft;
        errorText.OnUpdate += () =>
        {
            string error = GetErrorMessage(textField.Text, out Color? color);

            errorText.Text = error == "" ? error : Localization.Get(error);

            if (color != null)
            {
                textFieldButton.Border.Color = color.Value;
            }
            else if (textFieldButton.IsMouseOver())
            {
                // Fake the hover border for this control so the error border gets priority
                textFieldButton.Border.Color = UICommon.accentColor;
            }
            else
            {
                textFieldButton.Border.Color = UICommon.borderColor;
            }
        };

        CanvasButton submitButton = rowBuilder.AppendFixed(new CanvasButton("Submit"), ButtonWidth);
        submitButton.Text.Text = Localization.Get("SAVESTATES_EXPORTINDIALOG");
        submitButton.OnClicked += () =>
        {
            if (SaveStateManager.ValidateNewPackName(textField.Text) == "")
            {
                SaveStateManager.ExportPack(textField.Text);
                Hide();
            }
        };

        CanvasButton cancelButton = rowBuilder.AppendFixed(new CanvasButton("Cancel"), ButtonWidth);
        cancelButton.Text.Text = Localization.Get("DIALOG_CANCEL");
        cancelButton.OnClicked += Hide;
    }

    private string GetErrorMessage(string text, out Color? color)
    {
        color = null;

        string error = SaveStateManager.ValidateNewPackName(text);
        if (error != "")
        {
            color = UICommon.redColor;
            return error;
        }

        if (SaveStateManager.GetPackNames().Contains(text))
        {
            return "SAVESTATES_WARNPACKALREADYEXISTS";
        }

        return "";
    }

    public void Toggle(CanvasNode anchor)
    {
        if (TryStartToggle(anchor))
        {
            SaveStateManager.RefreshSavestatePacks();

            Show();
        }
    }
}
