using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DebugMod.UI.Canvas;

public class CanvasPanel : CanvasNode
{
    protected readonly Dictionary<string, CanvasNode> elements = new();
    private readonly bool contextual;

    public CanvasPanel(string name, bool contextual = false) : base(name)
    {
        this.contextual = contextual;
        if (contextual)
        {
            ActiveSelf = false;
        }
    }

    public CanvasPanel(string name, CanvasNode parent, Vector2 position, Vector2 size, Texture2D tex, Rect subSprite = default, bool contextual = false)
        : this(name, contextual)
    {
        Parent = parent;
        LocalPosition = position;
        Size = size;
        AddImage("Background", tex, Vector2.zero, size, subSprite);
    }

    protected override IEnumerable<CanvasNode> ChildList() => elements.Values;

    public CanvasButton AddButton(string name, Texture2D tex, Vector2 pos, Vector2 sz, Action func, Rect subSprite = default, Font font = null, string text = null, int fontSize = 13)
    {
        if (subSprite.width == 0 || subSprite.height == 0)
        {
            subSprite = new Rect(0, 0, tex.width, tex.height);
        }

        sz = Size + sz;
        if (sz.x == 0 || sz.y == 0)
        {
            sz = new Vector2(subSprite.width, subSprite.height);
        }

        CanvasButton button = new CanvasButton(name);
        button.Parent = this;
        button.LocalPosition = pos;
        button.Size = sz;
        button.SetImage(tex, subSprite);
        button.OnClicked += func;

        if (text != null || font != null)
        {
            button.Text.Text = text;
            button.Text.Font = font;
            button.Text.FontSize = fontSize;
        }
        else
        {
            button.RemoveText();
        }

        button.RemoveBorder();

        elements.Add(name, button);
        return button;
    }

    public CanvasPanel AddPanel(string name, Texture2D tex, Vector2 pos, Vector2 sz, Rect subSprite, bool contextual = false)
    {
        CanvasPanel panel = new CanvasPanel(name, this, pos, sz, tex, subSprite, contextual);
        elements.Add(name, panel);
        return panel;
    }

    public CanvasImage AddImage(string name, Texture2D tex, Vector2 pos, Vector2 sz = default, Rect subSprite = default)
    {
        if (subSprite.width == 0 || subSprite.height == 0)
        {
            subSprite = new Rect(0, 0, tex.width, tex.height);
        }

        if (sz.x == 0 || sz.y == 0)
        {
            sz = new Vector2(subSprite.width, subSprite.height);
        }

        CanvasImage image = new CanvasImage(name);
        image.Parent = this;
        image.LocalPosition = pos;
        image.Size = sz;
        image.SetImage(tex, subSprite);

        elements.Add(name, image);
        return image;
    }

    public CanvasText AddText(string name, string text, Vector2 pos, Vector2 sz, Font font, int fontSize = 13, FontStyle style = FontStyle.Normal, TextAnchor alignment = TextAnchor.UpperLeft)
    {
        if (sz.x == 0 || sz.y == 0)
        {
            sz = new Vector2(1920f, 1080f);
        }

        CanvasText t = new CanvasText(name);
        t.Parent = this;
        t.LocalPosition = pos;
        t.Size = sz;
        t.Text = text;
        t.Font = font;
        t.FontSize = fontSize;
        t.FontStyle = style;
        t.Alignment = alignment;

        elements.Add(name, t);
        return t;
    }

    public T Add<T>(T element) where T : CanvasNode
    {
        element.Parent = this;
        elements.Add(element.Name, element);
        return element;
    }

    public CanvasButton GetButton(string name) => elements[name] as CanvasButton;
    public CanvasButton GetButton(string name, string panel) => GetPanel(panel).GetButton(name);
    public CanvasImage GetImage(string name) => elements[name] as CanvasImage;
    public CanvasImage GetImage(string name, string panel) => GetPanel(panel).GetImage(name);
    public CanvasPanel GetPanel(string name) => elements[name] as CanvasPanel;
    public CanvasText GetText(string name) => elements[name] as CanvasText;
    public CanvasText GetText(string name, string panel) => GetPanel(panel).GetText(name);

    protected IEnumerable<T> AllElementsOfType<T>() where T : CanvasNode
    {
        return elements.Values.Where(x => x is T).Cast<T>();
    }

    public void TogglePanel(string name)
    {
        if (ActiveInHierarchy)
        {
            CanvasPanel panel = GetPanel(name);
            panel.ActiveSelf = !panel.ActiveSelf;

            // Hide any other panels with the same position
            foreach (CanvasPanel other in AllElementsOfType<CanvasPanel>())
            {
                if (other != panel && other.ActiveInHierarchy && other.Position == panel.Position)
                {
                    other.ActiveSelf = false;
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
            foreach (CanvasPanel panel in AllElementsOfType<CanvasPanel>())
            {
                if (panel.contextual)
                {
                    panel.ActiveSelf = false;
                }
            }
        }
    }
}
