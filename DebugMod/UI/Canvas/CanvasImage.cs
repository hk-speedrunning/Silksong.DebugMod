using UnityEngine;
using UnityEngine.UI;

namespace DebugMod.UI.Canvas;

public class CanvasImage : CanvasObject
{
    public CanvasImage(string name, CanvasNode parent, Vector2 position, Vector2 size, Texture2D tex, Rect subSprite)
        : base(name, parent, position, size)
    {
        transform.sizeDelta = new Vector2(subSprite.width, subSprite.height);

        obj.AddComponent<Image>();
        UpdateImage(tex, subSprite);

        transform.SetScaleX(Size.x / subSprite.width);
        transform.SetScaleY(Size.y / subSprite.height);
    }

    public void UpdateImage(Texture2D tex, Rect subSprite)
    {
        obj.GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(subSprite.x, tex.height - subSprite.height, subSprite.width, subSprite.height), Vector2.zero);
    }

    public void SetRenderIndex(int idx)
    {
        obj.transform.SetSiblingIndex(idx);
    }
}
