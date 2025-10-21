using UnityEngine;

namespace DebugMod.UI.Canvas;

public abstract class CanvasElement
{
    protected readonly GameObject obj;
    protected readonly RectTransform transform;

    private Vector2 size;

    public string Name { get; }

    public CanvasElement Parent { get; }

    public Vector2 LocalPosition
    {
        get => FromUnityCoords(transform.anchoredPosition);
        set
        {
            transform.anchoredPosition = ToUnityCoords(value);
            PositionUpdate();
        }
    }

    public Vector2 Position => LocalPosition + (Parent?.Position ?? Vector2.zero);

    // uGUI has (0, 0) in the bottom left, we have it in the top left
    private Vector2 ToUnityCoords(Vector2 v)
    {
        float parentSize = Parent?.Size.y ?? 1080f;
        return new Vector2(v.x, parentSize - v.y - Size.y);
    }

    private Vector2 FromUnityCoords(Vector2 v) => ToUnityCoords(v);

    public Vector2 Size
    {
        get => size;
        set
        {
            size = value;
            SizeUpdate();
        }
    }

    public bool Active
    {
        get => obj.activeSelf;
        set => obj.SetActive(value);
    }

    public bool ActiveInHierarchy => Active && (Parent?.ActiveInHierarchy ?? true);

    public float Width
    {
        get => Size.x;
        set => Size = new Vector2(value, Size.y);
    }

    public float Height
    {
        get => Size.y;
        set => Size = new Vector2(Size.x, value);
    }

    protected CanvasElement(string name, CanvasElement parent, Vector2 position, Vector2 size)
    {
        Name = name;
        Parent = parent;
        this.size = size;

        obj = new GameObject($"{GetType().Name} {name}");
        obj.transform.SetParent((parent?.obj ?? GUIController.Instance.canvas).transform, true);

        obj.AddComponent<CanvasRenderer>();

        transform = obj.AddComponent<RectTransform>();
        transform.sizeDelta = new Vector2(size.x, size.y);
        transform.anchorMin = Vector2.zero;
        transform.anchorMax = Vector2.zero;
        transform.pivot = Vector2.zero;
        LocalPosition = position;

        CanvasGroup group = obj.AddComponent<CanvasGroup>();
        group.interactable = false;
        group.blocksRaycasts = false;

        Object.DontDestroyOnLoad(obj);
    }

    public virtual void PositionUpdate() {}
    public virtual void SizeUpdate() {}

    public void ToggleActive() => Active = !Active;
    public void Destroy() => Object.Destroy(obj);
}