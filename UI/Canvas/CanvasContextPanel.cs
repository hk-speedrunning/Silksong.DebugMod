using DebugMod.UI.Canvas;
using UnityEngine;

namespace DebugMod.UI;

public class CanvasContextPanel : CanvasPanel
{
    private CanvasNode anchor;
    private string key;
    private Vector2 anchorPos;
    private bool initialClickEnded;

    public CanvasContextPanel(string name) : base(name)
    {
        ActiveSelf = false;
        OnUpdate += Update;

        UICommon.AddBackground(this);
        Get<CanvasImage>("Background").SetImage(UICommon.contextPanelBG);
    }

    private void Update()
    {
        if (initialClickEnded && !IsMouseOver() && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
        {
            Hide();
        }
        else if (anchor != null && (anchor.Position != anchorPos || !anchor.ActiveInHierarchy))
        {
            Hide();
        }

        if (Input.GetMouseButtonUp(0))
        {
            initialClickEnded = true;
        }
    }

    protected bool TryToggle(CanvasNode anchor, string key)
    {
        if (ActiveInHierarchy && this.key == key)
        {
            Hide();
            return false;
        }

        this.anchor = anchor;
        this.key = key;
        anchorPos = anchor.Position;
        initialClickEnded = false;

        float x = (int)(anchor.Position.x + anchor.Size.x * 0.8f);
        float xOver = x + Size.x - (Screen.width - UICommon.Margin);
        if (xOver > 0)
        {
            x -= xOver;
        }

        float y = (int)(anchor.Position.y + anchor.Size.y * 0.8f);
        float yOver = y + Size.y - (Screen.height - UICommon.Margin);
        if (yOver > 0)
        {
            y -= yOver;
        }

        LocalPosition = new Vector2(x, y);
        ActiveSelf = true;

        return true;
    }

    public virtual void Hide()
    {
        anchor = null;
        ActiveSelf = false;
    }
}