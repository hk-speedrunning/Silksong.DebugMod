using System;
using System.Collections.Generic;
using UnityEngine;

namespace DebugMod.UI.Canvas;

// Base class for all canvas elements
public abstract class CanvasNode
{
    internal static readonly HashSet<CanvasNode> rootNodes = [];
    private static event Action allUpdates;

    public static void UpdateAll()
    {
        allUpdates?.Invoke();
    }

    private CanvasNode parent;
    private Vector2 localPosition;
    private Vector2 size;
    private bool activeSelf = true;
    private event Action onUpdate;
    private bool updateHooked;

    public string Name { get; set; }

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
                OnUpdateParent();
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

    public event Action OnUpdate
    {
        add
        {
            onUpdate += value;
            CheckUpdateHook();
        }
        remove
        {
            onUpdate -= value;
            CheckUpdateHook();
        }
    }

    protected CanvasNode(string name)
    {
        Name = name;
        rootNodes.Add(this);
    }

    protected virtual IEnumerable<CanvasNode> ChildList()
    {
        yield break;
    }

    public IEnumerable<CanvasNode> Subtree()
    {
        yield return this;

        foreach (CanvasNode child in ChildList())
        {
            foreach (CanvasNode node in child.Subtree())
            {
                yield return node;
            }
        }
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
        CheckUpdateHook();
        foreach (CanvasNode child in ChildList())
        {
            child.OnUpdateActive();
        }
    }

    protected virtual void OnUpdateParent()
    {
        foreach (CanvasNode child in ChildList())
        {
            child.OnUpdateParent();
        }
    }

    public virtual void Build()
    {
        foreach (CanvasNode child in ChildList())
        {
            child.Build();
        }
    }

    private void CheckUpdateHook()
    {
        bool shouldUpdate = onUpdate != null && ActiveInHierarchy;
        if (shouldUpdate && !updateHooked)
        {
            allUpdates += Update;
            updateHooked = true;
        }
        else if (!shouldUpdate && updateHooked)
        {
            allUpdates -= Update;
            updateHooked = false;
        }
    }

    private void Update()
    {
        onUpdate?.Invoke();
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

        ActiveSelf = false;
        CheckUpdateHook();
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

    public bool IsMouseOver()
    {
        Rect bounds = new Rect(Position.x, Screen.height - Position.y - Size.y, Size.x, Size.y);
        if (ShouldClip(out Rect clipRect))
        {
            clipRect = new Rect(clipRect.position + new Vector2(Screen.width / 2f, Screen.height / 2f), clipRect.size);
            bounds = Intersect(bounds, clipRect);
        }

        // Add some tolerance, Unity seems to do the same with some of its pointer events
        bounds.x--;
        bounds.y--;
        bounds.width += 2;
        bounds.height += 2;

        return bounds.Contains(Input.mousePosition);
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