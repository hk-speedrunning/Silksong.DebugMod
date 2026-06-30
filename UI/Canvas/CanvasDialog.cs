using DebugMod.UI.Canvas;
using UnityEngine;

namespace DebugMod.UI;

public class CanvasDialog : CanvasPanel
{
    protected CanvasNode anchor;
    private Vector2 anchorPos;
    private bool initialClickEnded;

    protected virtual bool CustomPositioning => false;

    public CanvasDialog(string name) : base(name) { }

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
        if (initialClickEnded && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && !ClickedOnDialogOrChild())
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

    // Return true if the mouse is over this dialog or another dialog anchored to this one
    private bool ClickedOnDialogOrChild()
    {
        for (int i = 0; i < allNodes.Count;)
        {
            CanvasNode node = allNodes[i];

            if (node is CanvasDialog dialog && dialog.ActiveSelf && dialog.IsMouseOver())
            {
                CanvasNode current = dialog;

                while (current != null)
                {
                    if (current is CanvasDialog dialog2)
                    {
                        if (current == this)
                        {
                            return true;
                        }

                        current = dialog2.anchor;
                    }
                    else
                    {
                        current = node.Parent;
                    }
                }
            }

            i += node.childCount;
        }

        return false;
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

        if (!CustomPositioning)
        {
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
        }

        ActiveSelf = true;

        Build();
    }

    public void Hide()
    {
        anchor = null;
        initialClickEnded = false;

        Destroy();
    }

    public bool IsOpenFor(CanvasNode node)
    {
        return anchor == node;
    }
}