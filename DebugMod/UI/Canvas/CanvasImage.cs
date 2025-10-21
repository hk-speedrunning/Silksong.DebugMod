using UnityEngine;
using UnityEngine.UI;

namespace DebugMod.UI.Canvas;

public sealed class CanvasImage : CanvasElement
{
    private Rect subSprite;

    public CanvasImage(string name, CanvasElement parent, Vector2 position, Texture2D tex, Rect subSprite)
        : base(name, parent, position, new Vector2(subSprite.width, subSprite.height))
    {
        this.subSprite = subSprite;

        obj.AddComponent<Image>();
        UpdateImage(tex, subSprite);

        PositionUpdate();
        SizeUpdate();
    }

    public void UpdateImage(Texture2D tex, Rect subSection)
    {
        obj.GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(subSection.x, tex.height - subSection.height, subSection.width, subSection.height), Vector2.zero);
    }

    public override void SizeUpdate()
    {
        obj.GetComponent<RectTransform>().SetScaleX(Size.x / subSprite.width);
        obj.GetComponent<RectTransform>().SetScaleY(Size.y / subSprite.height);
    }

    public void SetRenderIndex(int idx)
    {
        obj.transform.SetSiblingIndex(idx);
    }
}
