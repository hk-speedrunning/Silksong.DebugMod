using UnityEngine;
using UnityEngine.UI;

namespace DebugMod.UI.Canvas
{
    public class CanvasText
    {
        private GameObject textObj;
        private Vector2 position;
        private Vector2 size;

        public bool active;

        public CanvasText(Vector2 pos, Vector2 sz, string text, Font font, int fontSize = 13, FontStyle style = FontStyle.Normal, TextAnchor alignment = TextAnchor.UpperLeft)
        {
            position = pos;
            if (sz.x == 0 || sz.y == 0)
            {
                size = new Vector2(1920f, 1080f);
            }
            else
            {
                size = sz;
            }

            textObj = new GameObject();
            textObj.AddComponent<CanvasRenderer>();
            RectTransform textTransform = textObj.AddComponent<RectTransform>();
            textTransform.sizeDelta = size;

            CanvasGroup group = textObj.AddComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;

            Text t = textObj.AddComponent<Text>();
            t.text = text;
            t.font = font;
            t.fontSize = fontSize;
            t.fontStyle = style;
            t.alignment = alignment;

            textObj.transform.SetParent(GUIController.Instance.canvas.transform, false);
            UpdateAnchor();

            Object.DontDestroyOnLoad(textObj);

            active = true;
        }

        public Vector2 GetPosition()
        {
            return position;
        }

        public void SetPosition(Vector2 pos)
        {
            position = pos;
            UpdateAnchor();
        }

        private void UpdateAnchor()
        {
            if (textObj)
            {
                Vector2 anchor = new Vector2((position.x + size.x / 2f) / 1920f, (1080f - (position.y + size.y / 2f)) / 1080f);

                RectTransform textTransform = textObj.GetComponent<RectTransform>();
                textTransform.anchorMin = anchor;
                textTransform.anchorMax = anchor;
            }
        }

        public void UpdateText(string text)
        {
            if (textObj != null)
            {
                textObj.GetComponent<Text>().text = text;
            }
        }

        public void SetActive(bool a)
        {
            active = a;

            if (textObj != null)
            {
                textObj.SetActive(active);
            }
        }

        public void MoveToTop()
        {
            if (textObj != null)
            {
                textObj.transform.SetAsLastSibling();
            }
        }

        public void SetTextColor(Color color)
        {
            if (textObj != null)
            {
                Text t = textObj.GetComponent<Text>();
                t.color = color;
            }
        }

        public void Destroy()
        {
            Object.Destroy(textObj);
        }
    }
}
