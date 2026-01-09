using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DebugMod.UI.Canvas;

public class CanvasButton : CanvasImage
{
    private static CanvasBorder hoverBorder;

    internal static void BuildHoverBorder()
    {
        hoverBorder = new CanvasBorder("HoverBorder");
        hoverBorder.Size = new Vector2(1, 1);
        hoverBorder.Color = UICommon.accentColor;
        hoverBorder.ActiveSelf = false;
        hoverBorder.Build();
    }

    private CanvasText text;
    private bool toggled;
    private bool useHoverBorder = true;
    private Vector2 hoverBorderPosition;
    private Vector2 hoverBorderSize;

    public CanvasText Text => text;

    public bool Toggled
    {
        get => toggled;
        set
        {
            if (toggled != value)
            {
                toggled = value;
                UpdateToggled();
            }
        }
    }

    public event Action OnClicked;

    protected override bool Interactable => true;

    public CanvasButton(string name) : base(name)
    {
        SetImage(UICommon.panelBG);
        AddBorder();

        text = new CanvasText("ButtonText");
        text.Parent = this;
        text.Alignment = TextAnchor.MiddleCenter;
    }

    public void RemoveText()
    {
        text = null;
    }

    public void RemoveHoverBorder()
    {
        useHoverBorder = false;
    }

    public void ImageOnly(Texture2D tex, Rect subSprite = default)
    {
        SetImage(tex, subSprite);
        RemoveText();
        RemoveBorder();
        RemoveHoverBorder();
    }

    protected override IEnumerable<CanvasNode> ChildList()
    {
        foreach (CanvasNode child in base.ChildList())
        {
            yield return child;
        }

        if (hoverBorder?.Parent == this) yield return hoverBorder;
        if (text != null) yield return text;
    }

    protected override void OnUpdatePosition()
    {
        if (text != null)
        {
            text.Size = Size;
        }

        base.OnUpdatePosition();
    }

    protected override void OnUpdateActive()
    {
        if (!ActiveInHierarchy && hoverBorder?.Parent == this)
        {
            hoverBorder.ActiveSelf = false;
        }

        base.OnUpdateActive();
    }

    public override void Build()
    {
        // Expand hover border to cover borders of adjacent buttons if it wouldn't already
        if (Border != null && useHoverBorder)
        {
            hoverBorderPosition = Vector2.zero;
            hoverBorderSize = Size;

            if ((Border.Sides & BorderSides.LEFT) == 0) hoverBorderPosition -= new Vector2(Border.Thickness, 0);
            if ((Border.Sides & BorderSides.RIGHT) == 0) hoverBorderSize += new Vector2(Border.Thickness, 0);
            if ((Border.Sides & BorderSides.TOP) == 0) hoverBorderPosition -= new Vector2(0, Border.Thickness);
            if ((Border.Sides & BorderSides.BOTTOM) == 0) hoverBorderSize += new Vector2(0, Border.Thickness);

            hoverBorderSize -= hoverBorderPosition;
        }

        base.Build();

        AddEventTrigger(EventTriggerType.PointerDown, _ => OnClicked?.Invoke());

        if (useHoverBorder)
        {
            AddEventTrigger(EventTriggerType.PointerEnter, _ =>
            {
                hoverBorder.Parent = this;
                hoverBorder.LocalPosition = hoverBorderPosition;
                hoverBorder.Size = hoverBorderSize;
                hoverBorder.ActiveSelf = IsMouseOver();

                GameObject sibling = Border != null ? Border.GameObject : GameObject;
                hoverBorder.GameObject.transform.SetSiblingIndex(sibling.transform.GetSiblingIndex() + 1);

                OnUpdate += UpdateHoverBorder;
            });
            AddEventTrigger(EventTriggerType.PointerExit, _ =>
            {
                if (hoverBorder.Parent == this)
                {
                    hoverBorder.ActiveSelf = false;
                }

                OnUpdate -= UpdateHoverBorder;
            });
        }
    }

    private void UpdateToggled()
    {
        SetImage(toggled ? UICommon.panelStrongBG : UICommon.panelBG);
    }

    private void UpdateHoverBorder()
    {
        if (hoverBorder.Parent == this)
        {
            hoverBorder.ActiveSelf = IsMouseOver();
        }
    }
}
