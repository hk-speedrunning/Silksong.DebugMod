using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DebugMod.UI.Canvas;

public class CanvasControl : CanvasNode
{
    private readonly List<CanvasNode> nodes = [];

    public CanvasControl(string name) : base(name) {}

    public T Append<T>(T element, float width) where T : CanvasNode
    {
        element.Parent = this;
        element.Size = new Vector2(width, Size.y);
        nodes.Add(element);
        return element;
    }

    public T AppendFlex<T>(T element) where T : CanvasNode => Append(element, 0);
    public T AppendSquare<T>(T element) where T : CanvasNode => Append(element, Size.y);

    public void AppendPadding(float width) => Append(new CanvasText($"Padding{nodes.Count}"), width);

    public CanvasButton AttachKeybind(string bindName)
    {
        CanvasButton button = AppendSquare(new CanvasButton("Keybind"));
        button.SetImage(UICommon.images["Scrollbar_point"]);
        button.RemoveText();

        button.OnClicked += () => KeybindContextPanel.Instance.Toggle(button, bindName);

        return button;
    }

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
                // Overlap borders on adjacent elements so the middle ones aren't twice as thick
                fixedSize += node.Size.x - UICommon.BORDER_THICKNESS;
            }
        }

        float flexWidth = (Size.x - UICommon.BORDER_THICKNESS - fixedSize) / flexNodes;
        float x = 0;

        foreach (CanvasNode node in nodes)
        {
            if (node.Size.x == 0)
            {
                node.Size = new Vector2(flexWidth, node.Size.y);
            }

            node.LocalPosition = new Vector2(x, 0);
            x += node.Size.x - UICommon.BORDER_THICKNESS;
        }

        base.Build();
    }
}