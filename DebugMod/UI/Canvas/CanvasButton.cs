using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace DebugMod.UI.Canvas;

public class CanvasButton : CanvasImage
{
    private CanvasText text;

    public CanvasText Text
    {
        get
        {
            if (text == null)
            {
                text = new CanvasText("ButtonText", this);
                text.Size = Size;
            }

            return text;
        }
    }

    public event Action OnClicked;

    protected override bool Interactable => true;

    public CanvasButton(string name, CanvasNode parent)
        : base(name, parent) {}

    protected override IEnumerable<CanvasNode> ChildList()
    {
        if (text != null) yield return text;
    }

    public override void Build()
    {
        base.Build();

        obj.AddComponent<Button>().onClick.AddListener(() => OnClicked?.Invoke());
    }
}
