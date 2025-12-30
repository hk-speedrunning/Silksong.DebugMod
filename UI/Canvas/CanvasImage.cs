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
    public bool IsBackground { get; set; }

    public CanvasImage(string name) : base(name) { }

    public void SetImage(Texture2D tex, Rect subSprite = default)
    {
        if (this.tex == tex && this.subSprite == subSprite) return;

        if (subSprite.width == 0 || subSprite.height == 0)
        {
            subSprite = new Rect(0, 0, tex.width, tex.height);
        }

        this.tex = tex;
        this.subSprite = subSprite;

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

    protected override void OnUpdatePosition()
    {
        base.OnUpdatePosition();

        UpdateScale();
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
            gameObject.GetComponent<CanvasGroup>().blocksRaycasts = true;
        }

        gameObject.AddComponent<Image>();
        UpdateSprite();

        UpdateScale();
    }

    private void UpdateSprite()
    {
        Image image = gameObject.GetComponent<Image>();
        image.sprite = Sprite.Create(tex, new Rect(subSprite.x, tex.height - subSprite.height, subSprite.width, subSprite.height), Vector2.zero);
        image.color = UICommon.iconColor;
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
