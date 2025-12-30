using UnityEngine;
using UnityEngine.UI;

namespace DebugMod.UI.Canvas;

public class CanvasText : CanvasObject
{
    protected Text t;

    private string text;
    private Font font = UICommon.arial;
    private int fontSize = UICommon.FontSize;
    private FontStyle fontStyle = FontStyle.Normal;
    private TextAnchor alignment = TextAnchor.UpperLeft;
    private Color color = UICommon.textColor;

    public string Text
    {
        get => text;
        set
        {
            text = value;
            if (t) t.text = text;
        }
    }

    public Font Font
    {
        get => font;
        set
        {
            font = value;
            if (t) t.font = font;
        }
    }

    public int FontSize
    {
        get => fontSize;
        set
        {
            fontSize = value;
            if (t) t.fontSize = fontSize;
        }
    }

    public FontStyle FontStyle
    {
        get => fontStyle;
        set
        {
            fontStyle = value;
            if (t) t.fontStyle = fontStyle;
        }
    }

    public TextAnchor Alignment
    {
        get => alignment;
        set
        {
            alignment = value;
            if (t) t.alignment = alignment;
        }
    }

    public Color Color
    {
        get => color;
        set
        {
            color = value;
            if (t) t.color = color;
        }
    }

    public CanvasText(string name) : base(name) { }

    protected override void OnUpdatePosition()
    {
        // Absolute position needs to be integers or the text looks blurry
        Vector2 current = new(Position.x + Size.x / 2f, Position.y + Size.y / 2f);
        Vector2 desired = new(Mathf.Round(current.x), Mathf.Round(current.y));

        if (current != desired)
        {
            // Recursively calls base method
            LocalPosition += desired - current;
            return;
        }

        base.OnUpdatePosition();
    }

    public override void Build()
    {
        base.Build();

        t = gameObject.AddComponent<Text>();
        t.text = text;
        t.font = font;
        t.fontSize = fontSize;
        t.fontStyle = fontStyle;
        t.alignment = alignment;
        t.color = color;
    }
}
