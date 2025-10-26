using UnityEngine;
using UnityEngine.UI;

namespace DebugMod.UI.Canvas;

public class CanvasText : CanvasObject
{
    private Text t;

    private string text;
    private Font font;
    private int fontSize = 13;
    private FontStyle fontStyle = FontStyle.Normal;
    private TextAnchor alignment = TextAnchor.UpperLeft;
    private Color color = Color.white;

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

    public CanvasText(string name, CanvasNode parent)
        : base(name, parent) {}

    public override void Build()
    {
        base.Build();

        t = obj.AddComponent<Text>();
        t.text = text;
        t.font = font;
        t.fontSize = fontSize;
        t.fontStyle = fontStyle;
        t.alignment = alignment;
        t.color = color;
    }
}
