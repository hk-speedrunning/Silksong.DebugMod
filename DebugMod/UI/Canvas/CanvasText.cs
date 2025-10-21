using UnityEngine;
using UnityEngine.UI;

namespace DebugMod.UI.Canvas;

public sealed class CanvasText : CanvasElement
{
    public CanvasText(string name, CanvasElement parent, Vector2 position, Vector2 size, string text, Font font,
        int fontSize = 13, FontStyle style = FontStyle.Normal, TextAnchor alignment = TextAnchor.UpperLeft)
        : base(name, parent, position, size)
    {
        Text t = obj.AddComponent<Text>();
        t.text = text;
        t.font = font;
        t.fontSize = fontSize;
        t.fontStyle = style;
        t.alignment = alignment;

        PositionUpdate();
    }

    public CanvasText(string name, CanvasElement parent, Vector2 position, string text, Font font,
        int fontSize = 13, FontStyle style = FontStyle.Normal, TextAnchor alignment = TextAnchor.UpperLeft)
        : this(name, parent, position, new Vector2(1920f, 1080f), text, font, fontSize, style, alignment) {}

    public override void PositionUpdate()
    {
        Vector2 anchor = new Vector2((Position.x + Size.x / 2f) / 1920f, (1080f - (Position.y + Size.y / 2f)) / 1080f);
        obj.GetComponent<RectTransform>().anchorMin = anchor;
        obj.GetComponent<RectTransform>().anchorMax = anchor;
    }

    public void UpdateText(string text)
    {
        obj.GetComponent<Text>().text = text;
    }

    public void MoveToTop()
    {
        obj.transform.SetAsLastSibling();
    }

    public void SetTextColor(Color color)
    {
        obj.GetComponent<Text>().color = color;
    }
}
