using System.Collections.Generic;
using UnityEngine;

namespace DebugMod.UI.Canvas;

public abstract class CanvasElement
{
    protected readonly GameObject obj;
    protected readonly RectTransform transform;

    private Vector2 localPosition;
    private bool activeSelf = true;

    public string Name { get; }

    public CanvasElement Parent { get; }

    public Vector2 LocalPosition
    {
        get => localPosition;
        set
        {
            localPosition = value;
            OnUpdatePosition();
        }
    }

    public Vector2 Position => LocalPosition + (Parent?.Position ?? Vector2.zero);

    public Vector2 Size { get; }

    public bool ActiveSelf
    {
        get => activeSelf;
        set
        {
            activeSelf = value;
            OnUpdateActive();
        }
    }

    public bool ActiveInHierarchy => ActiveSelf && (Parent?.ActiveInHierarchy ?? true);

    protected CanvasElement(string name, CanvasElement parent, Vector2 position, Vector2 size)
    {
        Name = name;
        Parent = parent;
        Size = size;

        obj = new GameObject(GetObjectName());
        obj.transform.SetParent(GUIController.Instance.canvas.transform, true);

        obj.AddComponent<CanvasRenderer>();

        transform = obj.AddComponent<RectTransform>();
        transform.sizeDelta = new Vector2(size.x, size.y);
        LocalPosition = position;

        CanvasGroup group = obj.AddComponent<CanvasGroup>();
        group.interactable = false;
        group.blocksRaycasts = false;
    }

    protected virtual IEnumerable<CanvasElement> ChildList()
    {
        yield break;
    }

    private void OnUpdatePosition()
    {
        Vector2 anchor = new((Position.x + Size.x / 2f) / 1920f, (1080f - (Position.y + Size.y / 2f)) / 1080f);
        transform.anchorMin = transform.anchorMax = anchor;

        foreach (CanvasElement child in ChildList())
        {
            child.OnUpdatePosition();
        }
    }

    private void OnUpdateActive()
    {
        obj.SetActive(ActiveInHierarchy);

        foreach (CanvasElement child in ChildList())
        {
            child.OnUpdateActive();
        }
    }

    private string GetObjectName() => $"{Parent?.GetObjectName()}:{Name}";

    public void ToggleActive() => ActiveSelf = !ActiveSelf;

    public void Destroy()
    {
        foreach (CanvasElement element in ChildList())
        {
            element.Destroy();
        }

        Object.Destroy(obj);
    }
}