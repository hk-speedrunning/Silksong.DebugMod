using DebugMod.UI.Canvas;
using TMProOld;
using UnityEngine;

namespace DebugMod.UI;

public class KeybindContextPanel : CanvasPanel
{
    public static int KeycodeTextWidth => UICommon.ScaleWidth(90);
    public static int RowHeight => UICommon.ScaleHeight(20);
    public static int PanelWidth => KeycodeTextWidth + RowHeight * 2 + UICommon.Margin * 2;
    public static int PanelHeight => RowHeight * 2 + UICommon.Margin;

    public static KeybindContextPanel Instance { get; private set; }

    private readonly CanvasText nameText;
    private readonly CanvasText keycodeText;

    private BindAction bindAction;
    private CanvasNode anchor;
    private Vector2 anchorPos;
    private bool initialClickEnded;

    public static void BuildPanel()
    {
        Instance = new KeybindContextPanel();
        Instance.Build();
    }

    public KeybindContextPanel() : base(nameof(KeybindContextPanel))
    {
        ActiveSelf = false;
        Size = new Vector2(PanelWidth + UICommon.Margin * 2, PanelHeight + UICommon.Margin * 2);

        UICommon.AddBackground(this);
        Get<CanvasImage>("Background").SetImage(UICommon.contextPanelBG);

        nameText = Add(new CanvasText("BindName"));
        nameText.LocalPosition = new Vector2(UICommon.Margin, UICommon.Margin);
        nameText.Size = new Vector2(PanelWidth, RowHeight);
        nameText.Alignment = TextAlignmentOptions.Center;

        // TODO: replace this with uneditable text field
        keycodeText = Add(new CanvasText("Keycode"));
        keycodeText.LocalPosition = new Vector2(UICommon.Margin, RowHeight + UICommon.Margin * 2);
        keycodeText.Size = new Vector2(KeycodeTextWidth, RowHeight);
        keycodeText.Alignment = TextAlignmentOptions.Left;

        CanvasButton editButton = Add(new CanvasButton("Edit"));
        editButton.LocalPosition = new Vector2(KeycodeTextWidth + UICommon.Margin * 2, RowHeight + UICommon.Margin * 2);
        editButton.Size = new Vector2(RowHeight, RowHeight);
        editButton.ImageOnly(UICommon.images["Scrollbar_point"]);
        editButton.OnClicked += () => DebugMod.settings.binds[bindAction.Name] = KeyCode.None;

        CanvasButton clearButton = Add(new CanvasButton("Clear"));
        clearButton.LocalPosition = new Vector2(KeycodeTextWidth + RowHeight + UICommon.Margin * 3, RowHeight + UICommon.Margin * 2);
        clearButton.Size = new Vector2(RowHeight, RowHeight);
        clearButton.ImageOnly(UICommon.images["ButtonDel"]);
        clearButton.OnClicked += () => DebugMod.settings.binds.Remove(bindAction.Name);
    }

    public override void Update()
    {
        keycodeText.Text = GetKeycodeText(bindAction.Name);

        if (initialClickEnded && !IsMouseOver() && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
        {
            Hide();
        }
        else if (anchor != null && (anchor.Position != anchorPos || !anchor.ActiveInHierarchy))
        {
            Hide();
        }

        if (Input.GetMouseButtonUp(0))
        {
            initialClickEnded = true;
        }

        base.Update();
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
        if (ActiveInHierarchy && bindAction != null && bindAction.Name == action)
        {
            Hide();
            return;
        }

        this.anchor = anchor;
        anchorPos = anchor.Position;
        initialClickEnded = false;

        bindAction = DebugMod.bindActions[action];
        nameText.Text = bindAction.Name;

        float x = (int)(anchor.Position.x + anchor.Size.x / 2);
        float xOver = x + Size.x - (Screen.width - UICommon.Margin);
        if (xOver > 0)
        {
            x -= xOver;
        }

        float y = (int)(anchor.Position.y + anchor.Size.y / 2);
        float yOver = y + Size.y - (Screen.height - UICommon.Margin);
        if (yOver > 0)
        {
            y -= yOver;
        }

        LocalPosition = new Vector2(x, y);
        ActiveSelf = true;
    }

    public void Hide()
    {
        anchor = null;
        ActiveSelf = false;
    }
}