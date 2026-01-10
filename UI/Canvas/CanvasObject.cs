using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace DebugMod.UI.Canvas;

// Represents a node that maps to a Unity object in the UI
public abstract class CanvasObject : CanvasNode
{
    protected GameObject gameObject;
    protected RectTransform transform;
    protected EventTrigger eventTrigger;

    public GameObject GameObject => gameObject;

    protected virtual bool Interactable => false;

    protected CanvasObject(string name) : base(name) { }

    protected override void OnUpdatePosition()
    {
        if (gameObject)
        {
            AnchorToCenter();
        }

        base.OnUpdatePosition();
    }

    protected override void OnUpdateActive()
    {
        if (gameObject)
        {
            gameObject.SetActive(ActiveInHierarchy);
        }

        base.OnUpdateActive();
    }

    protected override void OnUpdateParent()
    {
        if (gameObject)
        {
            UpdateClipRect();
        }

        base.OnUpdateParent();
    }

    public override void Build()
    {
        gameObject = new GameObject(GetQualifiedName());
        gameObject.transform.SetParent(GUIController.Instance.canvas.transform, false);

        gameObject.AddComponent<CanvasRenderer>();
        transform = gameObject.AddComponent<RectTransform>();

        if (!Interactable)
        {
            CanvasGroup group = gameObject.AddComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;
        }

        gameObject.SetActive(ActiveInHierarchy);

        AnchorToCenter();
        UpdateClipRect();

        base.Build();
    }

    private void AnchorToCenter()
    {
        Vector2 anchor = new((Position.x + Size.x / 2f) / Screen.width, 1f - (Position.y + Size.y / 2f) / Screen.height);
        transform.anchorMin = transform.anchorMax = anchor;
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

    public override void Destroy()
    {
        base.Destroy();

        if (gameObject)
        {
            Object.Destroy(gameObject);
        }
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
}