using DebugMod.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace DebugMod.UI.Canvas;

// Base class for all canvas elements
public abstract class CanvasNode
{
    internal static readonly HashSet<CanvasNode> rootNodes = [];
    private static readonly OrderedHashSet<CanvasNode> activeNodes = [];

    private static readonly List<CanvasNode> _activeNodesList = [];
    public static void UpdateAll()
    {
        _activeNodesList.Clear();
        _activeNodesList.AddRange(activeNodes);
        foreach (var node in _activeNodesList) node.Update();
    }

    private CanvasNode parent;
    private Vector2 localPosition;
    private Vector2 size;
    private bool activeSelf = true;

    private event Action onUpdate;
    private bool updateHooked;

    protected GameObject gameObject;
    protected RectTransform transform;
    protected EventTrigger eventTrigger;

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
                OnUpdateSize();
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
                OnUpdateLocalPosition();
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
                OnUpdateSize();
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

    public GameObject GameObject => gameObject;

    protected virtual bool Interactable => false;

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

    protected virtual void OnUpdateLocalPosition()
    {
        if (gameObject)
        {
            UpdateAnchoredPosition();
        }

        OnUpdatePosition();
    }

    protected virtual void OnUpdatePosition()
    {
        foreach (CanvasNode child in ChildList())
        {
            child.OnUpdatePosition();
        }
    }

    protected virtual void OnUpdateSize()
    {
        if (gameObject)
        {
            UpdateAnchoredPosition();
            UpdateSizeDelta();
        }
    }

    protected virtual void OnUpdateActive()
    {
        if (gameObject)
        {
            gameObject.SetActive(ActiveSelf);
        }

        CheckUpdateHook();
        foreach (CanvasNode child in ChildList())
        {
            child.OnUpdateActive();
        }
    }

    protected virtual void OnUpdateParent()
    {
        if (gameObject)
        {
            gameObject.transform.SetParent(GetParentTransform());
            UpdateClipRect();
        }

        foreach (CanvasNode child in ChildList())
        {
            child.OnUpdatePosition();
        }
    }

    public virtual void Build()
    {
        gameObject = new GameObject(Name);
        gameObject.transform.SetParent(GetParentTransform());

        gameObject.AddComponent<CanvasRenderer>();
        transform = gameObject.AddComponent<RectTransform>();

        transform.anchorMin = transform.anchorMax = new Vector2(0f, 1f);
        transform.pivot = new Vector2(0f, 1f);

        if (!Interactable)
        {
            // CanvasGroup group = gameObject.AddComponent<CanvasGroup>();
            // group.interactable = false;
            // group.blocksRaycasts = false;
        }

        gameObject.SetActive(ActiveSelf);

        UpdateAnchoredPosition();
        UpdateSizeDelta();
        UpdateClipRect();

        foreach (CanvasNode child in ChildList())
        {
            child.Build();
        }
    }

    private Transform GetParentTransform()
    {
        if (Parent != null)
        {
            return Parent.GameObject.transform;
        }
        else
        {
            return GUIController.Instance.canvas.transform;
        }
    }

    private void UpdateAnchoredPosition()
    {
        transform.anchoredPosition = new Vector2(LocalPosition.x, -LocalPosition.y);
    }

    protected virtual void UpdateSizeDelta()
    {
        transform.sizeDelta = Size;
    }

    private void UpdateClipRect()
    {
        CanvasRenderer renderer = gameObject.GetComponent<CanvasRenderer>();
        renderer.DisableRectClipping();
        if (ShouldClip(out Rect clipRect))
        {
            renderer.EnableRectClipping(clipRect);
        }
    }

    private void CheckUpdateHook()
    {
        bool shouldUpdate = onUpdate != null && ActiveInHierarchy;
        if (shouldUpdate && !updateHooked)
        {
            activeNodes.Add(this);
            updateHooked = true;
        }
        else if (!shouldUpdate && updateHooked)
        {
            activeNodes.Remove(this);
            updateHooked = false;
        }
    }

    private void Update()
    {
        try
        {
            onUpdate?.Invoke();
        }
        catch (Exception e)
        {
            DebugMod.LogError($"Error updating node {GetQualifiedName()}: {e}");
        }
    }

    public virtual void Destroy()
    {
        foreach (CanvasNode element in ChildList())
        {
            element.Destroy();
        }

        if (gameObject)
        {
            Object.Destroy(gameObject);
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

    public void AddEventTrigger<T>(EventTriggerType type, Action<T> callback) where T : BaseEventData
    {
        if (!gameObject)
        {
            DebugMod.LogError("Cannot add event triggers before building");
            return;
        }

        if (eventTrigger == null)
        {
            eventTrigger = gameObject.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry entry = new();
        entry.eventID = type;
        entry.callback.AddListener(data => callback((T)data));
        eventTrigger.triggers.Add(entry);
    }

    public void AddEventTrigger(EventTriggerType type, Action<PointerEventData> callback)
        => AddEventTrigger<PointerEventData>(type, callback);

    internal string GetQualifiedName() => $"{Parent?.GetQualifiedName()}:{Name}";
}