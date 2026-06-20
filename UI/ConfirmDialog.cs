using DebugMod.Helpers;
using DebugMod.UI.Canvas;
using System;
using UnityEngine;

namespace DebugMod.UI;

public class ConfirmDialog : CanvasDialog
{
    public static int PanelWidth => UICommon.ScaleWidth(150);
    public static int PromptHeight => UICommon.ScaleHeight(18);

    public static ConfirmDialog Instance { get; private set; }

    private CanvasText prompt;

    private string promptText;
    private Action onAccept;
    private Action onReject;

    public static void BuildPanel()
    {
        Instance = new ConfirmDialog();
    }

    public ConfirmDialog() : base(nameof(ConfirmDialog))
    {
    }

    protected override void BuildDialog()
    {
        base.BuildDialog();

        Size = new Vector2(PanelWidth, 0);

        using PanelBuilder builder = new(this);
        builder.DynamicLength = true;
        builder.Padding = UICommon.Margin;

        prompt = builder.AppendFixed(new CanvasText("Prompt"), PromptHeight);
        prompt.Alignment = TextAnchor.MiddleCenter;
        prompt.Text = promptText;

        using PanelBuilder row = new(builder.AppendFixed(new CanvasPanel("Row"), UICommon.ControlHeight));
        row.Horizontal = true;
        row.InnerPadding = UICommon.Margin;

        CanvasButton yes = row.AppendFlex(new CanvasButton("Yes"));
        yes.Text.Text = Localization.Get("CONFIRM_YES");
        yes.OnClicked += () =>
        {
            onAccept();
            Hide();
        };

        CanvasButton no = row.AppendFlex(new CanvasButton("No"));
        no.Text.Text = Localization.Get("CONFIRM_NO");
        no.OnClicked += () =>
        {
            onReject();
            Hide();
        };
    }

    public void Toggle(CanvasNode anchor, string prompt, Action onAccept, Action onReject)
    {
        if (TryStartToggle(anchor))
        {
            promptText = prompt;
            this.onAccept = onAccept;
            this.onReject = onReject;

            Show();
        }
    }
}