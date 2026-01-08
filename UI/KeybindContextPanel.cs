using DebugMod.UI.Canvas;
using UnityEngine;

namespace DebugMod.UI;

public class KeybindContextPanel : CanvasContextPanel
{
    public static int PanelWidth => UICommon.ScaleWidth(150);
    public static int RowHeight => UICommon.ScaleHeight(18);

    public static KeybindContextPanel Instance { get; private set; }

    private readonly CanvasText nameText;
    private readonly CanvasText keycodeText;

    private BindAction bindAction;

    public static void BuildPanel()
    {
        Instance = new KeybindContextPanel();
        Instance.Build();
    }

    public KeybindContextPanel() : base(nameof(KeybindContextPanel))
    {
        Size = new Vector2(PanelWidth, 0);

        using PanelBuilder builder = new(this);
        builder.Padding = UICommon.Margin;
        builder.DynamicLength = true;

        nameText = builder.AppendFixed(new CanvasText("BindName"), RowHeight);
        nameText.Alignment = TextAnchor.MiddleCenter;

        using PanelBuilder row = new(builder.AppendFixed(new CanvasPanel("KeycodeRow"), RowHeight));
        row.Horizontal = true;
        row.InnerPadding = UICommon.Margin;

        // TODO: replace this with uneditable text field
        keycodeText = row.AppendFlex(new CanvasText("Keycode"));
        keycodeText.Alignment = TextAnchor.MiddleLeft;
        keycodeText.OnUpdate += () => keycodeText.Text = GetKeycodeText(bindAction.Name);

        CanvasButton editButton = row.AppendSquare(new CanvasButton("Edit"));
        editButton.ImageOnly(UICommon.images["IconDotCircled"]);
        editButton.OnClicked += () => DebugMod.UpdateBind(bindAction.Name, KeyCode.None);

        CanvasButton clearButton = row.AppendSquare(new CanvasButton("Clear"));
        clearButton.ImageOnly(UICommon.images["IconX"]);
        clearButton.OnClicked += () => DebugMod.UpdateBind(bindAction.Name, null);
    }

    public static string GetKeycodeText(string action)
    {
        if (DebugMod.settings.binds.TryGetValue(action, out KeyCode keycode))
        {
            return keycode == KeyCode.None ? "Press a key..." : keycode.ToString();
        }
        else
        {
            return "Unbound";
        }
    }

    public void Toggle(CanvasNode anchor, string action)
    {
        if (TryToggle(anchor, action))
        {
            bindAction = DebugMod.bindActions[action];
            nameText.Text = bindAction.Name;
        }
    }
}