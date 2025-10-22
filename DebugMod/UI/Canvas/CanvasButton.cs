using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DebugMod.UI.Canvas;

public class CanvasButton : CanvasObject
{
    private CanvasImage image;
    private CanvasText text;

    public CanvasButton(string name, CanvasNode parent, Vector2 position, Vector2 size, Action clicked)
        : base(name, parent, position, size)
    {
        obj.AddComponent<Button>().onClick.AddListener(() => clicked());
    }

    protected override IEnumerable<CanvasNode> ChildList()
    {
        if (image != null) yield return image;
        if (text != null) yield return text;
    }

    public void SetImage(Texture2D tex, Rect subSprite)
    {
        image = new CanvasImage("ButtonImage", this, Vector2.zero, Size, tex, subSprite);
        transform.sizeDelta = new Vector2(subSprite.width, subSprite.height);
    }

    public void SetText(string t, Font font, int fontSize = 13,
        FontStyle style = FontStyle.Normal, TextAnchor alignment = TextAnchor.UpperLeft)
    {
        text = new CanvasText("ButtonText", this, Vector2.zero, Size, t, font, fontSize, style, alignment);
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
