using UnityEngine;
using UnityEngine.UI;

namespace DebugMod.UI.Canvas;

public class CanvasImage : CanvasObject
{
    private Texture2D tex;
    private Rect subSprite;

    public CanvasImage(string name, CanvasNode parent)
        : base(name, parent) {}

    public void UpdateImage(Texture2D tex, Rect subSprite = default)
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

    private void UpdateSprite()
    {
        obj.GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(subSprite.x, tex.height - subSprite.height, subSprite.width, subSprite.height), Vector2.zero);
    }

    public override void Build()
    {
        base.Build();

        obj.AddComponent<Image>();
        UpdateSprite();

        transform.sizeDelta = new Vector2(subSprite.width, subSprite.height);
        transform.SetScaleX(Size.x / subSprite.width);
        transform.SetScaleY(Size.y / subSprite.height);
    }
}
