using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DebugMod.UI.Canvas;

public class CanvasButton : CanvasImage
{
    private CanvasText text;

    protected override bool Interactable => true;

    public CanvasButton(string name, CanvasNode parent, Vector2 position, Vector2 size, Action clicked, Texture2D tex, Rect subSprite)
        : base(name, parent, position, size, tex, subSprite)
    {
        obj.AddComponent<Button>().onClick.AddListener(() => clicked());
    }

    protected override IEnumerable<CanvasNode> ChildList()
    {
        if (text != null) yield return text;
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
        text?.MoveToTop();
    }
}
