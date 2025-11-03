using System;
using System.Collections.Generic;
using UnityEngine;
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

        UICommon.AddBorder(this);
    }

    public void RemoveText()
    {
        text = null;
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

        gameObject.AddComponent<Button>().onClick.AddListener(() => OnClicked?.Invoke());
    }
}
