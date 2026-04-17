using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DebugMod.UI.Canvas;

public class CanvasImage : CanvasNode
{
    private Texture2D tex;
    private Rect subSprite;
    private Sprite sprite;
    private CanvasBorder border;

    public CanvasBorder Border => border;
    public bool IsBackground { get; set; }

    public CanvasImage(string name) : base(name) { }

    public void SetImage(Texture2D tex, Rect subSprite = default)
    {
        if (subSprite.width == 0 || subSprite.height == 0)
        {
            subSprite = new Rect(0, 0, tex.width, tex.height);
        }

        if (this.tex == tex && this.subSprite == subSprite) return;

        this.tex = tex;
        this.subSprite = subSprite;
        sprite = null;

        if (gameObject)
        {
            UpdateSprite();
        }
    }

    public void SetImage(Sprite sprite)
    {
        if (this.sprite == sprite) return;

        this.sprite = sprite;
        tex = null;
        subSprite = sprite.rect;

        if (gameObject)
        {
            UpdateSprite();
        }
    }

    public CanvasBorder AddBorder()
    {
        border ??= new CanvasBorder("Border");
        border.Parent = this;
        return border;
    }

    public void RemoveBorder()
    {
        border = null;
    }

    protected override IEnumerable<CanvasNode> ChildList()
    {
        if (border != null) yield return border;
    }

    public override void Build()
    {
        if (IsBackground && Parent != null)
        {
            Size = Parent.Size;
        }

        base.Build();

        if (IsBackground && !Interactable)
        {
            // gameObject.GetComponent<CanvasGroup>().blocksRaycasts = true;
        }

        gameObject.AddComponent<Image>();
        UpdateSprite();
        UpdateSizeDelta();
    }

    private void UpdateSprite()
    {
        Image image = gameObject.GetComponent<Image>();

        image.sprite = sprite ? sprite : Sprite.Create(tex, new Rect(subSprite.x, tex.height - subSprite.height - subSprite.y, subSprite.width, subSprite.height), Vector2.zero);
        image.color = UICommon.iconColor;
    }

    protected override void UpdateSizeDelta()
    {
        transform.sizeDelta = new Vector2(subSprite.width, subSprite.height);
        transform.SetScaleX(Size.x / subSprite.width);
        transform.SetScaleY(Size.y / subSprite.height);
    }
}
