using System.Collections.Generic;
using UnityEngine;

namespace DebugMod.UI.Canvas;

// Base class for all canvas elements
public abstract class CanvasNode
{
    private Vector2 localPosition;
    private bool activeSelf = true;

    public string Name { get; }

    public CanvasNode Parent { get; }

    public Vector2 LocalPosition
    {
        get => localPosition;
        set
        {
            if (localPosition != value)
            {
                localPosition = value;
                OnUpdatePosition();
            }
        }
    }

    public Vector2 Position => LocalPosition + (Parent?.Position ?? Vector2.zero);

    public Vector2 Size { get; }

    public bool ActiveSelf
    {
        get => activeSelf;
        set
        {
            if (activeSelf != value)
            {
                activeSelf = value;
                OnUpdateActive();
            }
        }
    }

    public bool ActiveInHierarchy => ActiveSelf && (Parent?.ActiveInHierarchy ?? true);

    protected CanvasNode(string name, CanvasNode parent, Vector2 position, Vector2 size)
    {
        Name = name;
        Parent = parent;
        Size = size;
        localPosition = position;
    }

    protected virtual IEnumerable<CanvasNode> ChildList()
    {
        yield break;
    }

    protected virtual void OnUpdatePosition()
    {
        foreach (CanvasNode child in ChildList())
        {
            child.OnUpdatePosition();
        }
    }

    protected virtual void OnUpdateActive()
    {
        foreach (CanvasNode child in ChildList())
        {
            child.OnUpdateActive();
        }
    }

    protected string GetQualifiedName() => $"{Parent?.GetQualifiedName()}:{Name}";

    public void ToggleActive() => ActiveSelf = !ActiveSelf;

    public virtual void Destroy()
    {
        foreach (CanvasNode element in ChildList())
        {
            element.Destroy();
        }
    }
}