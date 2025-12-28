using TMPro;
using UnityEngine;

namespace DebugMod.UI.Canvas;

public class CanvasText : CanvasObject
{
    protected TMP_Text t;

    private string text;
    private TMP_FontAsset font = UICommon.arial;
    private float fontSize = UICommon.FontSize;
    private FontStyles fontStyle = FontStyles.Normal;
    private TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft;
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

    public TMP_FontAsset Font
    {
        get => font;
        set
        {
            font = value;
            if (t) t.font = font;
        }
    }

    public float FontSize
    {
        get => fontSize;
        set
        {
            fontSize = value;
            if (t) t.fontSize = fontSize;
        }
    }

    public FontStyles FontStyle
    {
        get => fontStyle;
        set
        {
            fontStyle = value;
            if (t) t.fontStyle = fontStyle;
        }
    }

    public TextAlignmentOptions Alignment
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

    public override void Build()
    {
        base.Build();

        t = gameObject.AddComponent<TMP_Text>();
        t.text = text;
        t.font = font;
        t.fontSize = fontSize;
        t.fontStyle = fontStyle;
        t.alignment = alignment;
        t.color = color;
    }
}