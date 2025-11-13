using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DebugMod.UI.Canvas;

public class CanvasScrollbar : CanvasNode
{
    public static int GripHeight => UICommon.ScaleHeight(80);

    private readonly CanvasButton grip;

    public CanvasScrollView ScrollView { get; set; }

    public CanvasScrollbar(string name) : base(name)
    {
        grip = new CanvasButton("Grip");
        grip.Parent = this;
        grip.RemoveText();
    }

    protected override IEnumerable<CanvasNode> ChildList()
    {
        yield return grip;
    }

    public override void Build()
    {
        grip.Size = new Vector2(Size.x, GripHeight);

        base.Build();

        grip.AddEventTrigger(EventTriggerType.Drag, eventData =>
        {
            float y = grip.LocalPosition.y - eventData.delta.y;
            y = Mathf.Clamp(y, 0, Size.y - grip.Size.y);
            grip.LocalPosition = new Vector2(grip.LocalPosition.x, y);
            ScrollView.SetScrollPosition(y / (Size.y - grip.Size.y));
        });
    }
}