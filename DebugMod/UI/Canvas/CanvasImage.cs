using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DebugMod.UI.Canvas;

public class CanvasImage : CanvasObject
{
    private Texture2D tex;
    private Rect subSprite;
    private CanvasBorder border;

    public CanvasBorder Border => border;

    public CanvasImage(string name, CanvasNode parent)
        : base(name, parent) {}

    public void SetImage(Texture2D tex, Rect subSprite = default)
    {
        if (subSprite.width == 0 || subSprite.height == 0)
        {
            subSprite = new Rect(0, 0, tex.width, tex.height);
        }

        this.tex = tex;
        this.subSprite = subSprite;

        if (obj)
        {
            UpdateSprite();
        }
    }

    public void AddBorder(int thickness, Color color)
    {
        border = new CanvasBorder("Border", this);
        border.Thickness = thickness;
        border.Color = color;
    }

    protected override IEnumerable<CanvasNode> ChildList()
    {
        if (border != null) yield return border;
    }

    protected override void OnUpdatePosition()
    {
        base.OnUpdatePosition();
        UpdateScale();
    }

    public override void Build()
    {
        base.Build();

        obj.AddComponent<Image>();
        UpdateSprite();

        UpdateScale();
    }

    private void UpdateSprite()
    {
        if (tex)
        {
            obj.GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(subSprite.x, tex.height - subSprite.height, subSprite.width, subSprite.height), Vector2.zero);
        }
        else
        {
            DebugMod.LogWarn($"CanvasImage {GetQualifiedName()} has no texture");
        }
    }

    private void UpdateScale()
    {
        if (transform)
        {
            transform.sizeDelta = new Vector2(subSprite.width, subSprite.height);
            transform.SetScaleX(Size.x / subSprite.width);
            transform.SetScaleY(Size.y / subSprite.height);
        }
    }
}
