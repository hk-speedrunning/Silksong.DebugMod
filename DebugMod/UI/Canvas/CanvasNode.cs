using System;
using System.Collections.Generic;
using UnityEngine;

namespace DebugMod.UI.Canvas;

// Base class for all canvas elements
public abstract class CanvasNode
{
    internal static readonly List<CanvasNode> rootNodes = [];

    private Vector2 localPosition;
    private Vector2 size;
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

    public Vector2 Size
    {
        get => size;
        set
        {
            if (size != value)
            {
                size = value;
                OnUpdatePosition();
            }
        }
    }

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

    public event Action OnUpdate;

    protected CanvasNode(string name, CanvasNode parent)
    {
        Name = name;
        Parent = parent;

        if (parent == null)
        {
            rootNodes.Add(this);
        }
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

    public virtual void Build()
    {
        foreach (CanvasNode child in ChildList())
        {
            child.Build();
        }
    }

    public virtual void Update()
    {
        OnUpdate?.Invoke();

        foreach (CanvasNode child in ChildList())
        {
            child.Update();
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

        if (Parent == null)
        {
            rootNodes.Remove(this);
        }
    }
}