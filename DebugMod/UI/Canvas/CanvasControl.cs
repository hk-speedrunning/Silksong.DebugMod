using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DebugMod.UI.Canvas;

public class CanvasControl : CanvasNode
{
    private readonly List<CanvasNode> nodes = [];

    public CanvasControl(string name, CanvasNode parent)
        : base(name, parent) {}

    public CanvasButton AppendButton(string name, float width)
    {
        CanvasButton button = new CanvasButton(name, this);
        nodes.Add(button);

        UICommon.ApplyCommonStyle(button);
        button.Size = new Vector2(width, Size.y);

        return button;
    }

    public CanvasButton AppendFlexButton(string name) => AppendButton(name, 0);
    public CanvasButton AppendSquareButton(string name) => AppendButton(name, Size.y);

    protected override IEnumerable<CanvasNode> ChildList() => nodes;

    public T GetNode<T>(string name) where T : CanvasNode => nodes.FirstOrDefault(x => x.Name == name) as T;

    public override void Build()
    {
        float fixedSize = 0;
        int flexNodes = 0;

        foreach (CanvasNode node in nodes)
        {
            if (node.Size.x == 0)
            {
                flexNodes++;
            }
            else
            {
                fixedSize += node.Size.x;
            }
        }

        float flexWidth = (Size.x - fixedSize) / flexNodes;
        float x = 0;

        foreach (CanvasNode node in nodes)
        {
            if (node.Size.x == 0)
            {
                node.Size = new Vector2(flexWidth, node.Size.y);
            }

            node.LocalPosition = new Vector2(x, 0);
            x += node.Size.x;
        }

        base.Build();
    }
}