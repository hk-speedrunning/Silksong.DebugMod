using DebugMod.UI.Canvas;
using UnityEngine;

namespace DebugMod.UI;

public class CanvasDialog : CanvasPanel
{
    private CanvasNode anchor;
    private Vector2 anchorPos;
    private bool initialClickEnded;

    public CanvasDialog(string name) : base(name)
    {
    }

    // Dialogs are more dynamic than panels, so they are rebuilt every time they are shown
    protected virtual void BuildDialog()
    {
        // Start from a clean slate
        Destroy();

        OnUpdate += DoUpdate;

        UICommon.AddBackground(this);
        Get<CanvasImage>("Background").SetImage(UICommon.dialogBG);
    }

    private void DoUpdate()
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

    protected bool TryStartToggle(CanvasNode anchor)
    {
        if (ActiveInHierarchy && this.anchor == anchor)
        {
            Hide();
            return false;
        }

        this.anchor = anchor;
        anchorPos = anchor.Position;
        initialClickEnded = false;

        return true;
    }

    protected void Show()
    {
        BuildDialog();

        float x = (int)(anchor.Position.x + anchor.Size.x - UICommon.Margin);
        float xOver = x + Size.x - (Screen.width - UICommon.Margin);
        if (xOver > 0)
        {
            x -= xOver;
        }

        float y = (int)(anchor.Position.y + anchor.Size.y - UICommon.Margin);
        float yOver = y + Size.y - (Screen.height - UICommon.Margin);
        if (yOver > 0)
        {
            y -= yOver;
        }

        LocalPosition = new Vector2(x, y);
        ActiveSelf = true;

        Build();
    }

    public void Hide()
    {
        anchor = null;
        initialClickEnded = false;

        Destroy();
    }
}