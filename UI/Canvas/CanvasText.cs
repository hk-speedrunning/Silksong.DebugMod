using UnityEngine;
using UnityEngine.UI;

namespace DebugMod.UI.Canvas;

public class CanvasText : CanvasNode
{
    protected Text t;

    private string text;
    private Font font = UICommon.arial;
    private int fontSize = UICommon.FontSize;
    private FontStyle fontStyle = FontStyle.Normal;
    private TextAnchor alignment = TextAnchor.UpperLeft;
    private Color color = UICommon.textColor;
    private HorizontalWrapMode overflow;

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

    protected override bool Interactable => false;

    public CanvasText(string name,
        HorizontalWrapMode overflow = HorizontalWrapMode.Wrap) : base(name)
    {
        this.overflow = overflow;
    }

    protected override void OnUpdateLocalPosition()
    {
        base.OnUpdateLocalPosition();

        if (GameObject)
        {
            RoundPosition();
        }
    }

    // Relative position needs to be integers or the text looks blurry
    private void RoundPosition()
    {
        Vector2 pos = transform.anchoredPosition;
        transform.anchoredPosition = new(Mathf.Round(pos.x), Mathf.Round(pos.y));
    }

    public override void Build()
    {
        base.Build();

        RoundPosition();

        t = gameObject.AddComponent<Text>();
        t.horizontalOverflow = overflow;
        t.text = text;
        t.font = font;
        t.fontSize = fontSize;
        t.fontStyle = fontStyle;
        t.alignment = alignment;
        t.color = color;
    }
}
