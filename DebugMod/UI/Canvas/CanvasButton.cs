using System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace DebugMod.UI.Canvas;

public class CanvasButton
{
    private readonly GameObject buttonObj;
    private readonly CanvasText textElement;
    private readonly CanvasImage imageElement;
    private Vector2 position;
    private Vector2 size;

    public CanvasButton(Vector2 pos, Vector2 sz, Action clicked, CanvasText text = null, CanvasImage image = null)
    {
        if (image != null && (sz.x == 0 || sz.y == 0))
        {
            sz = image.GetSize();
        }

        textElement = text;
        imageElement = image;
        position = pos;
        size = sz;

        buttonObj = new GameObject();
        buttonObj.AddComponent<CanvasRenderer>();
        RectTransform buttonTransform = buttonObj.AddComponent<RectTransform>();
        buttonTransform.sizeDelta = size;

        buttonObj.AddComponent<Button>().onClick.AddListener(() => clicked());

        buttonObj.transform.SetParent(GUIController.Instance.canvas.transform, false);
        UpdateAnchor();

        Object.DontDestroyOnLoad(buttonObj);
    }

    public Vector2 GetPosition()
    {
        return position;
    }

    public void SetPosition(Vector2 pos)
    {
        Vector2 deltaPos = pos - position;
        position = pos;

        textElement.SetPosition(textElement.GetPosition() + deltaPos);
        imageElement.SetPosition(imageElement.GetPosition() + deltaPos);

        UpdateAnchor();
    }

    private void UpdateAnchor()
    {
        if (buttonObj)
        {
            Vector2 anchor = new Vector2((position.x + size.x / 2f) / 1920f, (1080f - (position.y + size.y / 2f)) / 1080f);

            RectTransform buttonTransform = buttonObj.GetComponent<RectTransform>();
            buttonTransform.anchorMin = anchor;
            buttonTransform.anchorMax = anchor;
        }
    }

    public void UpdateText(string text) => textElement.UpdateText(text);
    public void SetTextColor(Color color) => textElement.SetTextColor(color);

    public void SetActive(bool b)
    {
        buttonObj?.SetActive(b);
        textElement?.SetActive(b);
        imageElement?.SetActive(b);
    }

    public void MoveToTop()
    {
        buttonObj?.transform.SetAsLastSibling();
        textElement?.MoveToTop();
        imageElement?.SetRenderIndex(0);
    }

    public void Destroy()
    {
        Object.Destroy(buttonObj);
        textElement?.Destroy();
        imageElement?.Destroy();
    }
}
