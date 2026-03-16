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

    public CanvasText(string name,
        HorizontalWrapMode overflow = HorizontalWrapMode.Wrap) : base(name)
    {
        this.overflow = overflow;
    }

    protected override void OnUpdatePosition()
    {
        base.OnUpdatePosition();

        if (GameObject)
        {
            RoundPosition();
        }
    }

    // Absolute position needs to be integers or the text looks blurry
    private void RoundPosition()
    {
        Vector2 pos = transform.anchorMin;

        // No idea why this check is required but it is
        // (tested by dragging the scrollbar very slowly and watching
        // the spacing between the text and nearby borders)
        float x = (int)Size.x % 2 == 0
            ? Mathf.RoundToMultipleOf(pos.x, 1f / Screen.width)
            : (int)(pos.x * Screen.width) / (float)Screen.width;
        float y = (int)Size.y % 2 == 0
            ? Mathf.RoundToMultipleOf(pos.y, 1f / Screen.height)
            : (int)(pos.y * Screen.height) / (float)Screen.height;

        transform.anchorMin = transform.anchorMax = new(x, y);
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
