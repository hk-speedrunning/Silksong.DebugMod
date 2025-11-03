using System.Collections.Generic;
using UnityEngine;

namespace DebugMod.UI.Canvas;

public class CanvasScrollView : CanvasNode
{
    private CanvasNode content;

    public Vector2 Margin { get; set; } = new(UICommon.BORDER_THICKNESS, UICommon.BORDER_THICKNESS);

    public CanvasScrollView(string name) : base(name) {}

    public T SetContent<T>(T content) where T : CanvasNode
    {
        this.content = content;
        content.Parent = this;
        return content;
    }

    protected override IEnumerable<CanvasNode> ChildList()
    {
        if (content != null) yield return content;
    }

    protected override bool GetClipRect(out Rect clipRect)
    {
        clipRect = new Rect(Position.x - 1920f / 2f + Margin.x, 1080f / 2f - Position.y - Size.y + Margin.y,
            Size.x - Margin.x * 2, Size.y - Margin.y * 2);
        return true;
    }
}