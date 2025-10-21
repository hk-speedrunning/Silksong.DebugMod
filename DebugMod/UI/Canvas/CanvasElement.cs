using UnityEngine;

namespace DebugMod.UI.Canvas;

public abstract class CanvasElement
{
    protected readonly GameObject obj;

    private Vector2 size;
    private CanvasElement parent;

    public string Name { get; }

    public CanvasElement Parent
    {
        get => parent;
        set
        {
            parent = value;
            obj.transform.parent = value.obj.transform;
        }
    }

    public Vector2 LocalPosition
    {
        get => obj.transform.localPosition;
        set
        {
            obj.transform.localPosition = value;
            PositionUpdate();
        }
    }

    public Vector2 Position
    {
        get => obj.transform.position;
        set
        {
            obj.transform.position = value;
            PositionUpdate();
        }
    }

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
        this.parent = parent;
        this.size = size;

        obj = new GameObject($"{GetType().Name} {name}");
        obj.transform.SetParent((parent?.obj ?? GUIController.Instance.canvas).transform, false);
        obj.transform.localPosition = position;

        obj.AddComponent<CanvasRenderer>();
        RectTransform imageTransform = obj.AddComponent<RectTransform>();
        imageTransform.sizeDelta = new Vector2(size.x, size.y);

        CanvasGroup group = obj.AddComponent<CanvasGroup>();
        group.interactable = false;
        group.blocksRaycasts = false;

        Object.DontDestroyOnLoad(obj);
    }

    public virtual void PositionUpdate() {}
    public virtual void SizeUpdate() {}

    public void ToggleActive() => Active = !Active;
    public bool ActiveInHierarchy() => Active && (Parent == null || Parent.ActiveInHierarchy());
    public void Destroy() => Object.Destroy(obj);
}