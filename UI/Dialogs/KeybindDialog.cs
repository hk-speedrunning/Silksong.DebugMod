using DebugMod.Helpers;
using DebugMod.UI.Canvas;
using System.Collections.Generic;
using UnityEngine;

namespace DebugMod.UI.Dialogs;

public class KeybindDialog : CanvasDialog
{
    public static int PanelWidth => UICommon.ScaleWidth(150);
    public static int RowHeight => UICommon.ScaleHeight(18);

    public static KeybindDialog Instance { get; private set; }

    private readonly List<CanvasPanel> singlePanels = [];
    private BindAction[] actions;

    public static void BuildPanel()
    {
        Instance = new KeybindDialog();
    }

    public KeybindDialog() : base(nameof(KeybindDialog)) { }

    protected override void BuildDialog()
    {
        base.BuildDialog();
        Get<CanvasImage>("Background").RemoveBorder();

        float y = 0f;

        for (int i = 0; i < actions.Length; i++)
        {
            CanvasPanel panel = BuildSinglePanel(i);
            panel.LocalPosition = new Vector2(0f, y);
            y += panel.Size.y + UICommon.Margin;
            singlePanels.Add(panel);
        }

        y -= UICommon.Margin;
        Size = new Vector2(PanelWidth, y);
    }

    private CanvasPanel BuildSinglePanel(int index)
    {
        CanvasPanel panel = Add(new CanvasPanel(index.ToString()));
        panel.Size = new Vector2(PanelWidth, 0);
        panel.CollapseMode = CollapseMode.Deny;
        UICommon.AddBackground(panel);
        panel.Get<CanvasImage>("Background").SetImage(UICommon.dialogBG);

        PanelBuilder builder = new(panel);
        builder.OuterPadding = ContentMargin(UICommon.Margin);
        builder.InnerPadding = UICommon.Margin;
        builder.DynamicLength = true;

        CanvasText titleText = builder.AppendFixed(new CanvasText("BindName"), RowHeight);
        titleText.Alignment = TextAnchor.MiddleCenter;

        if (actions.Length > 1)
        {
            titleText.Text = Localization.Get(actions[index].Name);
        }
        else
        {
            // Makes it more obvious what the dialog is for
            titleText.Text = Localization.Get("KEYBINDDIALOG_TITLE");
        }

        using PanelBuilder row = new(builder.AppendFixed(new CanvasPanel("KeycodeRow"), RowHeight));
        row.Horizontal = true;
        row.InnerPadding = UICommon.Margin;

        CanvasText keycodeText = row.AppendFlex(new CanvasText("Keycode"));
        keycodeText.Alignment = TextAnchor.MiddleLeft;
        keycodeText.OnUpdate += () => keycodeText.Text = GetKeycodeText(actions[index].Name);

        CanvasButton editButton = row.AppendSquare(new CanvasButton("Edit"));
        editButton.ImageOnly(UICommon.images["IconDotCircled"]);
        editButton.OnClicked += () => DebugMod.UpdateBind(actions[index].Name, KeyCode.None);

        CanvasButton clearButton = row.AppendSquare(new CanvasButton("Clear"));
        clearButton.ImageOnly(UICommon.images["IconX"]);
        clearButton.OnClicked += () => DebugMod.UpdateBind(actions[index].Name, null);

        builder.Build();
        return panel;
    }

    public static string GetKeycodeText(string action)
    {
        if (DebugMod.settings.binds.TryGetValue(action, out KeyCode keycode))
        {
            return keycode == KeyCode.None ? Localization.Get("KEYBIND_REBINDPROMPT") : keycode.ToString();
        }
        else
        {
            return Localization.Get("KEYBIND_UNBOUND");
        }
    }

    public void Toggle(CanvasNode anchor, params BindAction[] actions)
    {
        if (TryStartToggle(anchor))
        {
            this.actions = actions;

            Show();
        }
    }
}