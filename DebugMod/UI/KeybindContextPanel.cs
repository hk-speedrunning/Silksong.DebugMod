using DebugMod.UI.Canvas;
using UnityEngine;

namespace DebugMod.UI;

public class KeybindContextPanel : CanvasPanel
{
    public const int KEYCODE_TEXT_WIDTH = 80;
    public const int ROW_HEIGHT = 20;
    public const int PANEL_WIDTH = KEYCODE_TEXT_WIDTH + ROW_HEIGHT * 2 + UICommon.MARGIN * 2;
    public const int PANEL_HEIGHT = ROW_HEIGHT * 2 + UICommon.MARGIN;

    public static KeybindContextPanel Instance { get; private set; }

    private readonly CanvasText nameText;
    private readonly CanvasText keycodeText;

    private BindAction bindAction;
    private CanvasNode anchor;
    private Vector2 anchorPos;

    public static void BuildPanel()
    {
        Instance = new KeybindContextPanel();
        Instance.Build();
    }

    public KeybindContextPanel() : base(nameof(KeybindContextPanel), null)
    {
        ActiveSelf = false;
        Size = new Vector2(PANEL_WIDTH + UICommon.MARGIN * 2, PANEL_HEIGHT + UICommon.MARGIN * 2);

        UICommon.ApplyCommonStyle(this);

        nameText = AddText("BindName");
        nameText.LocalPosition = new Vector2(UICommon.MARGIN, UICommon.MARGIN);
        nameText.Size = new Vector2(PANEL_WIDTH, ROW_HEIGHT);
        UICommon.ApplyCommonStyle(nameText);

        // TODO: replace this with uneditable text field
        keycodeText = AddText("Keycode");
        keycodeText.LocalPosition = new Vector2(UICommon.MARGIN, ROW_HEIGHT + UICommon.MARGIN * 2);
        keycodeText.Size = new Vector2(KEYCODE_TEXT_WIDTH, ROW_HEIGHT);
        UICommon.ApplyCommonStyle(keycodeText);
        keycodeText.Alignment = TextAnchor.MiddleLeft;

        CanvasButton editButton = AddButton("Edit");
        editButton.LocalPosition = new Vector2(KEYCODE_TEXT_WIDTH + UICommon.MARGIN * 2, ROW_HEIGHT + UICommon.MARGIN * 2);
        editButton.Size = new Vector2(ROW_HEIGHT, ROW_HEIGHT);
        editButton.SetImage(UICommon.images["Scrollbar_point"]);

        CanvasButton clearButton = AddButton("Clear");
        clearButton.LocalPosition = new Vector2(KEYCODE_TEXT_WIDTH + ROW_HEIGHT + UICommon.MARGIN * 3, ROW_HEIGHT + UICommon.MARGIN * 2);
        clearButton.Size = new Vector2(ROW_HEIGHT, ROW_HEIGHT);
        clearButton.SetImage(UICommon.images["ButtonDel"]);
    }

    public override void Update()
    {
        base.Update();

        if (ActiveInHierarchy)
        {
            keycodeText.Text = GetKeycodeText(bindAction.Name);

            if (!new Rect(Position.x, 1080f - Position.y - Size.y, Size.x, Size.y).Contains(Input.mousePosition)
                && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
            {
                Hide();
            }
            else if (anchor != null && (anchor.Position != anchorPos || !anchor.ActiveInHierarchy))
            {
                Hide();
            }
        }
    }

    private string GetKeycodeText(string action)
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

        bindAction = DebugMod.bindActions[action];
        nameText.Text = bindAction.Name;

        float x = anchor.Position.x + anchor.Size.x / 2;
        float xOver = x + Size.x - (1920f - UICommon.SCREEN_MARGIN);
        if (xOver > 0)
        {
            x -= xOver;
        }

        float y = anchor.Position.y + anchor.Size.y / 2;
        float yOver = y + Size.y - (1080f - UICommon.SCREEN_MARGIN);
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