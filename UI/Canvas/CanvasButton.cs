using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DebugMod.UI.Canvas;

public class CanvasButton : CanvasImage
{
    private CanvasText text;
    private CanvasBorder hoverBorder;
    private bool toggled;

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
        text.Alignment = TextAlignmentOptions.Center;

        hoverBorder = new CanvasBorder("HoverBorder");
        hoverBorder.Parent = this;
        hoverBorder.Color = UICommon.blueColor;
        hoverBorder.ActiveSelf = false;
    }

    public void RemoveText()
    {
        text = null;
    }

    public void RemoveHoverBorder()
    {
        hoverBorder = null;
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

        if (hoverBorder != null) yield return hoverBorder;
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
        if (!ActiveInHierarchy)
        {
            hoverBorder?.ActiveSelf = false;
        }

        base.OnUpdateActive();
    }

    public override void Build()
    {
        // Expand hover border to cover borders of adjacent buttons if it wouldn't already
        if (Border != null && hoverBorder != null)
        {
            Vector2 pos = Vector2.zero;
            Vector2 sz = Size;

            if ((Border.Sides & BorderSides.LEFT) == 0) pos -= new Vector2(Border.Thickness, 0);
            if ((Border.Sides & BorderSides.RIGHT) == 0) sz += new Vector2(Border.Thickness, 0);
            if ((Border.Sides & BorderSides.TOP) == 0) pos -= new Vector2(0, Border.Thickness);
            if ((Border.Sides & BorderSides.BOTTOM) == 0) sz += new Vector2(0, Border.Thickness);

            sz -= pos;
            hoverBorder.LocalPosition = pos;
            hoverBorder.Size = sz;
        }

        base.Build();

        AddEventTrigger(EventTriggerType.PointerDown, _ => OnClicked?.Invoke());

        if (hoverBorder != null)
        {
            AddEventTrigger(EventTriggerType.PointerEnter, _ => hoverBorder.ActiveSelf = true);
            AddEventTrigger(EventTriggerType.PointerExit, _ => hoverBorder.ActiveSelf = false);
        }
    }

    public override void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == gameObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        base.Update();
    }

    private void UpdateToggled()
    {
        SetImage(toggled ? UICommon.panelStrongBG : UICommon.panelBG);
    }
}
