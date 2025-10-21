using System;
using UnityEngine;
using UnityEngine.UI;

namespace DebugMod.UI.Canvas;

public sealed class CanvasButton : CanvasElement
{
    private CanvasImage image;
    private CanvasText text;

    public CanvasButton(string name, CanvasElement parent, Vector2 position, Vector2 size, Action clicked)
        : base(name, parent, position, size)
    {
        obj.AddComponent<Button>().onClick.AddListener(() => clicked());
        PositionUpdate();
    }

    public override void PositionUpdate()
    {
        image?.PositionUpdate();
        text?.PositionUpdate();
    }

    public void SetImage(Texture2D tex, Rect subSprite)
    {
        image = new CanvasImage(Name, this, Vector2.zero, tex, subSprite);
    }

    public void SetText(string t, Font font, int fontSize = 13,
        FontStyle style = FontStyle.Normal, TextAnchor alignment = TextAnchor.UpperLeft)
    {
        text = new CanvasText(Name, this, Vector2.zero, Size, t, font, fontSize, style, alignment);
    }

    public void UpdateText(string t) => text.UpdateText(t);
    public void SetTextColor(Color color) => text.SetTextColor(color);

    public void MoveToTop()
    {
        obj.transform.SetAsLastSibling();
        image?.SetRenderIndex(0);
        text?.MoveToTop();
    }
}
