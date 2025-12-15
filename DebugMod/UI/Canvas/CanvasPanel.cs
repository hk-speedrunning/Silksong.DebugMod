using System.Collections.Generic;

namespace DebugMod.UI.Canvas;

public class CanvasPanel : CanvasNode
{
    protected readonly Dictionary<string, CanvasNode> elements = new();

    public CollapseMode CollapseMode { get; set; }

    public CanvasPanel(string name) : base(name) { }

    protected override IEnumerable<CanvasNode> ChildList() => elements.Values;

    public T Add<T>(T element) where T : CanvasNode
    {
        element.Parent = this;
        elements.Add(element.Name, element);
        return element;
    }

    public T Get<T>(string name) where T : CanvasNode => elements[name] as T;

    public void Remove(string name) => elements.Remove(name);

    public override void Build()
    {
        Collapse();
        base.Build();
    }

    public void Collapse()
    {
        List<CanvasPanel> panelsToCollapse = [];

        foreach (CanvasNode element in elements.Values)
        {
            if (element is CanvasPanel panel && panel.CollapseMode != CollapseMode.Deny)
            {
                panelsToCollapse.Add(panel);
            }
        }

        foreach (CanvasPanel panel in panelsToCollapse)
        {
            panel.Collapse();

            foreach (CanvasNode child in panel.elements.Values)
            {
                if (panel.CollapseMode == CollapseMode.Allow)
                {
                    child.Name = $"{panel.Name}{child.Name}";
                }

                Add(child);
                child.LocalPosition += panel.LocalPosition;
            }

            panel.elements.Clear();
            Remove(panel.Name);
        }
    }
}

public enum CollapseMode
{
    Allow,
    AllowNoRenaming,
    Deny
}