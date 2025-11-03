using System.Collections.Generic;
using UnityEngine;

namespace DebugMod.UI.Canvas;

public class CanvasScrollView : CanvasNode
{
    public const float SCROLL_SPEED = 20f;

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

    public override void Update()
    {
        if (!Mathf.Approximately(Input.mouseScrollDelta.y, 0f) && IsMouseOver())
        {
            float y = content.LocalPosition.y + Input.mouseScrollDelta.y * SCROLL_SPEED;
            y = Mathf.Clamp(y, Size.y - content.Size.y, 0f);
            content.LocalPosition = new Vector2(content.LocalPosition.x, y);
        }

        base.Update();
    }
}