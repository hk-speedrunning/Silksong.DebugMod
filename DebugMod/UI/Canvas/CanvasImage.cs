using UnityEngine;
using UnityEngine.UI;

namespace DebugMod.UI.Canvas
{
    public class CanvasImage
    {
        private GameObject imageObj;
        private Vector2 position;
        private Vector2 size;
        private Rect sub;

        public bool active;

        public CanvasImage(Vector2 pos, Vector2 sz, Texture2D tex, Rect subSprite)
        {
            if (sz.x == 0 || sz.y == 0)
            {
                sz = new Vector2(subSprite.width, subSprite.height);
            }

            position = pos;
            size = sz;
            sub = subSprite;

            imageObj = new GameObject();
            imageObj.AddComponent<CanvasRenderer>();
            RectTransform imageTransform = imageObj.AddComponent<RectTransform>();
            imageTransform.sizeDelta = new Vector2(subSprite.width, subSprite.height);
            imageObj.AddComponent<Image>().sprite = Sprite.Create(tex, new Rect(subSprite.x, tex.height - subSprite.height, subSprite.width, subSprite.height), Vector2.zero);

            CanvasGroup group = imageObj.AddComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;

            imageObj.transform.SetParent(GUIController.Instance.canvas.transform, false);

            Vector2 anchor = new Vector2((pos.x + sz.x / subSprite.width * subSprite.width / 2f) / 1920f, (1080f - (pos.y + sz.y / subSprite.height * subSprite.height / 2f)) / 1080f);
            imageTransform.anchorMin = anchor;
            imageTransform.anchorMax = anchor;
            imageTransform.SetScaleX(sz.x / subSprite.width);
            imageTransform.SetScaleY(sz.y / subSprite.height);

            Object.DontDestroyOnLoad(imageObj);

            active = true;
        }

        public Vector2 GetPosition()
        {
            return position;
        }

        public Vector2 GetSize()
        {
            return size;
        }

        public void UpdateImage(Texture2D tex, Rect subSection)
        {
            if (imageObj != null)
            {
                imageObj.GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(subSection.x, tex.height - subSection.height, subSection.width, subSection.height), Vector2.zero);
            }
        }

        public void SetWidth(float width)
        {
            if (imageObj != null)
            {
                size = new Vector2(width, size.y);
                imageObj.GetComponent<RectTransform>().SetScaleX(width / imageObj.GetComponent<RectTransform>().sizeDelta.x);
            }
        }

        public void SetHeight(float height)
        {
            if (imageObj != null)
            {
                size = new Vector2(size.x, height);
                imageObj.GetComponent<RectTransform>().SetScaleY(height / imageObj.GetComponent<RectTransform>().sizeDelta.y);
            }
        }

        public void SetPosition(Vector2 pos)
        {
            position = pos;

            if (imageObj != null)
            {
                Vector2 anchor = new Vector2((pos.x + size.x / sub.width * sub.width / 2f) / 1920f, (1080f - (pos.y + size.y / sub.height * sub.height / 2f)) / 1080f);
                imageObj.GetComponent<RectTransform>().anchorMin = anchor;
                imageObj.GetComponent<RectTransform>().anchorMax = anchor;
            }
        }

        public void SetActive(bool b)
        {
            if (imageObj != null)
            {
                imageObj.SetActive(b);
                active = b;
            }
        }

        public void SetRenderIndex(int idx)
        {
            imageObj.transform.SetSiblingIndex(idx);
        }

        public void Destroy()
        {
            Object.Destroy(imageObj);
        }
    }
}
