using System.Collections.Generic;

namespace DebugMod.UI.Canvas;

public class CanvasPanel : CanvasNode
{
    protected readonly List<CanvasNode> elements = [];
    protected readonly Dictionary<string, CanvasNode> byName = [];

    public CollapseMode CollapseMode { get; set; }

    public CanvasPanel(string name) : base(name) { }

    protected override IEnumerable<CanvasNode> ChildList() => elements;

    public T Add<T>(T element) where T : CanvasNode
    {
        element.Parent = this;
        elements.Add(element);
        byName.Add(element.Name, element);
        return element;
    }

    public T Get<T>(string name) where T : CanvasNode => byName.GetValueOrDefault(name) as T;

    public void Remove(string name)
    {
        elements.RemoveAll(node => node.Name == name);
        byName.Remove(name);
    }

    public int ContentMargin(int baseMargin = 0)
    {
        CanvasImage background = Get<CanvasImage>("Background");
        if (background != null && background.IsBackground && background.Border != null)
        {
            return baseMargin + background.Border.Thickness;
        }

        return baseMargin;
    }

    public override void Build()
    {
        Collapse();
        base.Build();
    }

    public void Collapse()
    {
        List<CanvasPanel> panelsToCollapse = [];

        foreach (CanvasNode element in elements)
        {
            if (element is CanvasPanel panel && panel.CollapseMode != CollapseMode.Deny)
            {
                panelsToCollapse.Add(panel);
            }
        }

        foreach (CanvasPanel panel in panelsToCollapse)
        {
            panel.Collapse();

            foreach (CanvasNode child in panel.elements)
            {
                if (panel.CollapseMode == CollapseMode.Allow)
                {
                    child.Name = $"{panel.Name}{child.Name}";
                }

                Add(child);
                child.LocalPosition += panel.LocalPosition;
            }

            panel.elements.Clear();
            panel.byName.Clear();
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