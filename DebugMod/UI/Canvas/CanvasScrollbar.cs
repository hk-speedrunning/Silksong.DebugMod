using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DebugMod.UI.Canvas;

public class CanvasScrollbar : CanvasNode
{
    public static int MIN_GRIP_HEIGHT => UICommon.ScaleHeight(30);

    private readonly CanvasButton grip;
    private readonly CanvasImage trackUpper;
    private readonly CanvasImage trackLower;

    public CanvasScrollView ScrollView { get; set; }

    public CanvasScrollbar(string name) : base(name)
    {
        grip = new CanvasButton("Grip");
        grip.Parent = this;
        grip.RemoveText();
        grip.RemoveHoverBorder();

        trackUpper = new CanvasImage("TrackUpper");
        trackUpper.Parent = this;

        trackLower = new CanvasImage("TrackLower");
        trackLower.Parent = this;
    }

    protected override IEnumerable<CanvasNode> ChildList()
    {
        yield return trackUpper;
        yield return trackLower;
        yield return grip;
    }

    public override void Build()
    {
        // Round width down to nearest odd integer
        Size = new Vector2((int)((Size.x - 1) / 2) * 2 + 1, Size.y);
        trackUpper.Size = trackLower.Size = new Vector2(Size.x, 0);

        Texture2D trackTex = new Texture2D((int)Size.x, 1);
        Color[] colors = new Color[trackTex.width];
        Array.Fill(colors, Color.clear);
        colors[trackTex.width / 2] = UICommon.borderColor;
        trackTex.SetPixels(colors);
        trackTex.Apply();

        trackUpper.SetImage(trackTex);
        trackLower.SetImage(trackTex);

        // grip height / scrollbar height = scroll view height / content height
        float gripHeight = Size.y * ScrollView.Size.y / ScrollView.Content.Size.y;
        gripHeight = Mathf.Clamp(gripHeight, MIN_GRIP_HEIGHT, Size.y);
        grip.Size = new Vector2(Size.x, (int)gripHeight);

        base.Build();

        grip.AddEventTrigger(EventTriggerType.Drag, eventData =>
        {
            float y = grip.LocalPosition.y - eventData.delta.y;
            y = Mathf.Clamp(y, 0, Size.y - grip.Size.y);
            ScrollView.SetScrollPercentage(y / (Size.y - grip.Size.y));
            UpdateGripPosition(y);
        });
    }

    public override void Update()
    {
        float percentage = -ScrollView.Content.LocalPosition.y / ScrollView.GetScrollableHeight();
        percentage = Mathf.Clamp01(percentage);
        float y = percentage * (Size.y - grip.Size.y);
        UpdateGripPosition(y);

        base.Update();
    }

    private void UpdateGripPosition(float y)
    {
        grip.LocalPosition = new Vector2(grip.LocalPosition.x, y);
        trackUpper.Size = new Vector2(trackUpper.Size.x, y);
        trackLower.LocalPosition = new Vector2(trackLower.LocalPosition.x, y + grip.Size.y);
        trackLower.Size = new Vector2(trackLower.Size.x, Size.y - y - grip.Size.y);
    }
}