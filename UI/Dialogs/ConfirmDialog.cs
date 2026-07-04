using DebugMod.Helpers;
using DebugMod.UI.Canvas;
using System;
using UnityEngine;

namespace DebugMod.UI.Dialogs;

public class ConfirmDialog : CanvasDialog
{
    public static int PanelWidth => UICommon.ScaleWidth(150);
    public static int PromptHeight => UICommon.ScaleHeight(18);

    public static ConfirmDialog Instance { get; private set; }

    private string prompt;
    private Action onAccept;
    private Action onReject;
    private int width;
    private int lines;

    public static void BuildPanel()
    {
        Instance = new ConfirmDialog();
    }

    public ConfirmDialog() : base(nameof(ConfirmDialog)) { }

    protected override void BuildDialog()
    {
        base.BuildDialog();

        Size = new Vector2(width, 0);

        using PanelBuilder builder = new(this);
        builder.OuterPadding = ContentMargin(UICommon.Margin);
        builder.InnerPadding = UICommon.Margin;
        builder.DynamicLength = true;

        CanvasText promptText = builder.AppendFixed(new CanvasText("Prompt"), PromptHeight * lines);
        promptText.Alignment = TextAnchor.MiddleCenter;
        promptText.Text = Localization.Get(prompt);

        using PanelBuilder row = new(builder.AppendFixed(new CanvasPanel("Row"), UICommon.ControlHeight));
        row.Horizontal = true;
        row.InnerPadding = UICommon.Margin;

        CanvasButton yes = row.AppendFlex(new CanvasButton("Yes"));
        yes.Text.Text = Localization.Get("DIALOG_YES");
        yes.OnClicked += () =>
        {
            onAccept();
            Hide();
        };

        CanvasButton no = row.AppendFlex(new CanvasButton("No"));
        no.Text.Text = Localization.Get("DIALOG_NO");
        no.OnClicked += () =>
        {
            onReject?.Invoke();
            Hide();
        };
    }

    public void Toggle(CanvasNode anchor, string prompt, Action onAccept, Action onReject = null, int? width = null, int lines = 1)
    {
        if (TryStartToggle(anchor))
        {
            this.prompt = prompt;
            this.onAccept = onAccept;
            this.onReject = onReject;
            this.width = width ?? PanelWidth;
            this.lines = lines;

            Show();
        }
    }
}