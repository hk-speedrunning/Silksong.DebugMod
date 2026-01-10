using DebugMod.UI.Canvas;
using System;
using UnityEngine;

namespace DebugMod.UI;

public class ConfirmDialog : CanvasDialog
{
    public static int PanelWidth => UICommon.ScaleWidth(150);
    public static int PromptHeight => UICommon.ScaleHeight(18);

    public static ConfirmDialog Instance { get; private set; }

    private readonly CanvasText prompt;

    private Action onAccept;
    private Action onReject;

    public static void BuildPanel()
    {
        Instance = new ConfirmDialog();
        Instance.Build();
    }

    public ConfirmDialog() : base(nameof(ConfirmDialog))
    {
        Size = new Vector2(PanelWidth, 0);

        using PanelBuilder builder = new(this);
        builder.DynamicLength = true;
        builder.Padding = UICommon.Margin;

        prompt = builder.AppendFixed(new CanvasText("Prompt"), PromptHeight);
        prompt.Alignment = TextAnchor.MiddleCenter;

        using PanelBuilder row = new(builder.AppendFixed(new CanvasPanel("Row"), UICommon.ControlHeight));
        row.Horizontal = true;
        row.InnerPadding = UICommon.Margin;

        CanvasButton yes = row.AppendFlex(new CanvasButton("Yes"));
        yes.Text.Text = "Yes";
        yes.OnClicked += () =>
        {
            onAccept();
            Hide();
        };

        CanvasButton no = row.AppendFlex(new CanvasButton("No"));
        no.Text.Text = "No";
        no.OnClicked += () =>
        {
            onReject();
            Hide();
        };
    }

    public void Toggle(CanvasNode anchor, string prompt, Action onAccept, Action onReject)
    {
        if (TryToggle(anchor))
        {
            this.prompt.Text = prompt;
            this.onAccept = onAccept;
            this.onReject = onReject;
        }
    }
}