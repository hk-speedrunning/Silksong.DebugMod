using DebugMod.Helpers;
using DebugMod.UI.Canvas;
using System.Collections.Generic;
using UnityEngine;

namespace DebugMod.UI;

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
        Instance.Build();
    }

    public KeybindDialog() : base(nameof(KeybindDialog))
    {
        Get<CanvasImage>("Background").RemoveBorder();

        float y = 0f;

        for (int i = 0; i < 3; i++)
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
        titleText.Text = Localization.Get("KEYBINDDIALOG_TITLE");

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
        if (TryToggle(anchor))
        {
            this.actions = actions;
            for (int i = 0; i < singlePanels.Count; i++)
            {
                singlePanels[i].ActiveSelf = actions.Length > i;
            }

            float height = singlePanels[0].Size.y * actions.Length + UICommon.Margin * (actions.Length - 1);
            Get<CanvasImage>("Background").Size = new Vector2(PanelWidth, height);

            // If the dialog was pushed back onto the screen, it might be too high up now
            // (the calculation uses Size.y which might be larger than the visible size)
            if (Mathf.Approximately(Position.y + Size.y + UICommon.Margin, Screen.height))
            {
                LocalPosition = new Vector2(LocalPosition.x, LocalPosition.y - Size.y + height);
            }
        }
    }
}