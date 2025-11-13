using System;
using System.Collections.Generic;
using DebugMod.MonoBehaviours;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DebugMod.UI.Canvas;

public class CanvasButton : CanvasImage
{
    private CanvasText text;

    public CanvasText Text => text;

    public event Action OnClicked;

    protected override bool Interactable => true;

    public CanvasButton(string name) : base(name)
    {
        SetImage(UICommon.buttonBG);

        text = new CanvasText("ButtonText");
        text.Parent = this;
        text.Alignment = TextAnchor.MiddleCenter;

        AddBorder();
    }

    public void RemoveText()
    {
        text = null;
    }

    public void ImageOnly(Texture2D tex, Rect subSprite = default)
    {
        SetImage(tex, subSprite);
        RemoveText();
        RemoveBorder();
    }

    protected override IEnumerable<CanvasNode> ChildList()
    {
        foreach (CanvasNode child in base.ChildList())
        {
            yield return child;
        }

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

    public override void Build()
    {
        base.Build();

        AddEventTrigger(EventTriggerType.PointerDown, _ => OnClicked?.Invoke());
    }

    public override void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == gameObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        base.Update();
    }
}
