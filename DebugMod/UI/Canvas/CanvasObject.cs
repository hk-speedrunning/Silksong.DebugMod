using UnityEngine;

namespace DebugMod.UI.Canvas;

// Represents a node that maps to a Unity object in the UI
public abstract class CanvasObject : CanvasNode
{
    protected readonly GameObject obj;
    protected readonly RectTransform transform;

    protected virtual bool Interactable => false;

    protected CanvasObject(string name, CanvasNode parent, Vector2 position, Vector2 size) : base(name, parent, position, size)
    {
        obj = new GameObject(GetQualifiedName());
        obj.transform.SetParent(GUIController.Instance.canvas.transform, false);

        obj.AddComponent<CanvasRenderer>();

        transform = obj.AddComponent<RectTransform>();
        transform.sizeDelta = new Vector2(size.x, size.y);
        AnchorToCenter();

        if (!Interactable)
        {
            CanvasGroup group = obj.AddComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;
        }
    }

    protected override void OnUpdatePosition()
    {
        AnchorToCenter();
        base.OnUpdatePosition();
    }

    protected override void OnUpdateActive()
    {
        obj.SetActive(ActiveInHierarchy);
        base.OnUpdateActive();
    }

    private void AnchorToCenter()
    {
        Vector2 anchor = new((Position.x + Size.x / 2f) / 1920f, (1080f - (Position.y + Size.y / 2f)) / 1080f);
        transform.anchorMin = transform.anchorMax = anchor;
    }


    public override void Destroy()
    {
        base.Destroy();
        Object.Destroy(obj);
    }
}