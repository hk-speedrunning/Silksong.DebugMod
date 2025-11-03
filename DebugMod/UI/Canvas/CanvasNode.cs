using System;
using System.Collections.Generic;
using UnityEngine;

namespace DebugMod.UI.Canvas;

// Base class for all canvas elements
public abstract class CanvasNode
{
    internal static readonly HashSet<CanvasNode> rootNodes = [];

    private CanvasNode parent;
    private Vector2 localPosition;
    private Vector2 size;
    private bool activeSelf = true;

    public string Name { get; }

    public CanvasNode Parent
    {
        get => parent;
        set
        {
            if (parent != value)
            {
                parent = value;

                if (parent != null)
                {
                    rootNodes.Remove(this);
                }
                else
                {
                    rootNodes.Add(this);
                }

                OnUpdatePosition();
                OnUpdateActive();
            }
        }
    }

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

    protected CanvasNode(string name)
    {
        Name = name;
        rootNodes.Add(this);
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

    protected virtual bool GetClipRect(out Rect clipRect)
    {
        clipRect = default;
        return false;
    }

    protected bool ShouldClip(out Rect clipRect)
    {
        bool clip = GetClipRect(out clipRect);

        if (Parent != null && Parent.ShouldClip(out Rect parentRect))
        {
            clipRect = clip ? Intersect(clipRect, parentRect) : parentRect;
            return true;
        }

        return clip;
    }

    private static Rect Intersect(Rect a, Rect b)
    {
        float xMin = Mathf.Max(a.xMin, b.xMin);
        float yMin = Mathf.Max(a.yMin, b.yMin);
        float xMax = Mathf.Min(a.xMax, b.xMax);
        float yMax = Mathf.Min(a.yMax, b.yMax);
        if (xMin > xMax) (xMin, xMax) = (xMax, xMin);
        if (yMin > yMax) (yMin, yMax) = (yMax, yMin);
        return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
    }

    internal string GetQualifiedName() => $"{Parent?.GetQualifiedName()}:{Name}";
}