using UnityEngine;
using UnityEngine.UI;

namespace DebugMod.UI.Canvas;

public class CanvasText : CanvasObject
{
    public CanvasText(string name, CanvasNode parent, Vector2 position, Vector2 size, string text, Font font,
        int fontSize = 13, FontStyle style = FontStyle.Normal, TextAnchor alignment = TextAnchor.UpperLeft)
        : base(name, parent, position, size)
    {
        Text t = obj.AddComponent<Text>();
        t.text = text;
        t.font = font;
        t.fontSize = fontSize;
        t.fontStyle = style;
        t.alignment = alignment;
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
