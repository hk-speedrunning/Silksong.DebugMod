using System.Collections.Generic;

namespace DebugMod.UI.Canvas;

public class CanvasPanel : CanvasNode
{
    protected readonly Dictionary<string, CanvasNode> elements = new();

    public CanvasPanel(string name) : base(name) {}

    protected override IEnumerable<CanvasNode> ChildList() => elements.Values;

    public T Add<T>(T element) where T : CanvasNode
    {
        element.Parent = this;
        elements.Add(element.Name, element);
        return element;
    }

    public T Get<T>(string name) where T : CanvasNode => elements[name] as T;
}
