using UnityEngine;

namespace DebugMod.UI.Canvas;

// Represents a node that maps to a Unity object in the UI
public abstract class CanvasObject : CanvasNode
{
    protected GameObject obj;
    protected RectTransform transform;

    protected virtual bool Interactable => false;

    protected CanvasObject(string name, CanvasNode parent)
        : base(name, parent) {}

    protected override void OnUpdatePosition()
    {
        if (obj)
        {
            AnchorToCenter();
        }

        base.OnUpdatePosition();
    }

    private void AnchorToCenter()
    {
        Vector2 anchor = new((Position.x + Size.x / 2f) / 1920f, (1080f - (Position.y + Size.y / 2f)) / 1080f);
        transform.anchorMin = transform.anchorMax = anchor;
        transform.sizeDelta = Size;
    }

    protected override void OnUpdateActive()
    {
        if (obj)
        {
            obj.SetActive(ActiveInHierarchy);
        }

        base.OnUpdateActive();
    }

    public override void Build()
    {
        obj = new GameObject(GetQualifiedName());
        obj.transform.SetParent(GUIController.Instance.canvas.transform, false);

        obj.AddComponent<CanvasRenderer>();

        transform = obj.AddComponent<RectTransform>();
        AnchorToCenter();

        if (!Interactable)
        {
            CanvasGroup group = obj.AddComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;
        }

        obj.SetActive(ActiveInHierarchy);

        base.Build();
    }


    public override void Destroy()
    {
        base.Destroy();

        if (obj)
        {
            Object.Destroy(obj);
        }
    }
}