using System;
using System.Collections.Generic;
using UnityEngine;

namespace DebugMod.UI.Canvas;

public class CanvasPanel : CanvasNode
{
    private readonly Dictionary<string, CanvasButton> buttons = new();
    private readonly Dictionary<string, CanvasPanel> panels = new();
    private readonly Dictionary<string, CanvasImage> images = new();
    private readonly Dictionary<string, CanvasText> texts = new();
    private readonly bool contextual;

    public CanvasPanel(string name, CanvasNode parent, bool contextual = false)
        : base(name, parent)
    {
        this.contextual = contextual;
        if (contextual)
        {
            ActiveSelf = false;
        }
    }

    public CanvasPanel(string name, CanvasNode parent, Vector2 position, Vector2 size, Texture2D tex, Rect subSprite = default, bool contextual = false)
        : this(name, parent, contextual)
    {
        LocalPosition = position;
        Size = size;
        AddImage("Background", tex, Vector2.zero, size, subSprite);
    }

    protected override IEnumerable<CanvasNode> ChildList()
    {
        foreach (CanvasImage image in images.Values)
        {
            yield return image;
        }

        foreach (CanvasButton button in buttons.Values)
        {
            yield return button;
        }

        foreach (CanvasText text in texts.Values)
        {
            yield return text;
        }

        foreach (CanvasPanel panel in panels.Values)
        {
            yield return panel;
        }
    }

    public void AddButton(string name, Texture2D tex, Vector2 pos, Vector2 sz, Action func, Rect subSprite, Font font = null, string text = null, int fontSize = 13)
    {
        sz = Size + sz;
        if (sz.x == 0 || sz.y == 0)
        {
            sz = new Vector2(subSprite.width, subSprite.height);
        }

        CanvasButton button = new CanvasButton(name, this);
        button.LocalPosition = pos;
        button.Size = sz;
        button.UpdateImage(tex, subSprite);
        button.OnClicked += func;

        if (text != null && font != null)
        {
            button.Text.Text = text;
            button.Text.Font = font;
            button.Text.FontSize = fontSize;
            button.Text.Alignment = TextAnchor.MiddleCenter;
        }

        buttons.Add(name, button);
    }

    public CanvasPanel AddPanel(string name, Texture2D tex, Vector2 pos, Vector2 sz, Rect subSprite, bool contextual = false)
    {
        CanvasPanel panel = new CanvasPanel(name, this, pos, sz, tex, subSprite, contextual);

        panels.Add(name, panel);
        return panel;
    }

    public void AddImage(string name, Texture2D tex, Vector2 pos, Vector2 sz = default, Rect subSprite = default)
    {
        if (subSprite.width == 0 || subSprite.height == 0)
        {
            subSprite = new Rect(0, 0, tex.width, tex.height);
        }

        if (sz.x == 0 || sz.y == 0)
        {
            sz = new Vector2(subSprite.width, subSprite.height);
        }

        CanvasImage image = new CanvasImage(name, this);
        image.LocalPosition = pos;
        image.Size = sz;
        image.UpdateImage(tex, subSprite);

        images.Add(name, image);
    }

    public void AddText(string name, string text, Vector2 pos, Vector2 sz, Font font, int fontSize = 13, FontStyle style = FontStyle.Normal, TextAnchor alignment = TextAnchor.UpperLeft)
    {
        if (sz.x == 0 || sz.y == 0)
        {
            sz = new Vector2(1920f, 1080f);
        }

        CanvasText t = new CanvasText(name, this);
        t.LocalPosition = pos;
        t.Size = sz;
        t.Text = text;
        t.Font = font;
        t.FontSize = fontSize;
        t.FontStyle = style;
        t.Alignment = alignment;

        texts.Add(name, t);
    }

    public CanvasButton GetButton(string buttonName, string panelName = null)
    {
        if (panelName != null && panels.ContainsKey(panelName))
        {
            return panels[panelName].GetButton(buttonName);
        }

        if (buttons.ContainsKey(buttonName))
        {
            return buttons[buttonName];
        }

        return null;
    }

    public CanvasImage GetImage(string imageName, string panelName = null)
    {
        if (panelName != null && panels.ContainsKey(panelName))
        {
            return panels[panelName].GetImage(imageName);
        }

        if (images.ContainsKey(imageName))
        {
            return images[imageName];
        }

        return null;
    }

    public CanvasPanel GetPanel(string panelName)
    {
        if (panels.ContainsKey(panelName))
        {
            return panels[panelName];
        }

        return null;
    }

    public CanvasText GetText(string textName, string panelName = null)
    {
        if (panelName != null && panels.ContainsKey(panelName))
        {
            return panels[panelName].GetText(textName);
        }

        if (texts.ContainsKey(textName))
        {
            return texts[textName];
        }

        return null;
    }

    public void TogglePanel(string name)
    {
        if (ActiveInHierarchy && panels.ContainsKey(name))
        {
            panels[name].ToggleActive();

            // Hide any other panels with the same position
            foreach (CanvasPanel panel in panels.Values)
            {
                if (panel != panels[name] && panel.ActiveInHierarchy && panel.Position == panels[name].Position)
                {
                    panel.ActiveSelf = false;
                }
            }
        }
    }

    protected override void OnUpdateActive()
    {
        base.OnUpdateActive();

        if (!ActiveSelf)
        {
            // Hide contextual panels
            foreach (CanvasPanel panel in panels.Values)
            {
                if (panel.contextual)
                {
                    panel.ActiveSelf = false;
                }
            }
        }
    }
}
