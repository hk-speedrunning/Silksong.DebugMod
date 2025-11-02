using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DebugMod.UI.Canvas;

public class CanvasBorder : CanvasObject
{
    private static readonly Dictionary<(Vector2, int, Color, BorderSides), Sprite> spriteCache = new();

    public int Thickness { get; set; }
    public Color Color { get; set; }
    public BorderSides Sides { get; set; } = BorderSides.ALL;

    public CanvasBorder(string name, CanvasNode parent)
        : base(name, parent) {}

    public override void Build()
    {
        base.Build();

        Vector2 truncated = new((int)Position.x, (int)Position.y);
        if (Position != truncated)
        {
            LocalPosition += truncated - Position;
        }

        if (Size.x == 0 || Size.y == 0)
        {
            Size = Parent.Size;
        }

        if (!spriteCache.TryGetValue((Size, Thickness, Color, Sides), out Sprite sprite))
        {
            Texture2D tex = new Texture2D((int)Size.x, (int)Size.y, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            // Make the entire texture transparent
            Color[] colors = new Color[tex.width * tex.height];
            tex.SetPixels(colors);

            void FillRect(int x, int y, int width, int height)
            {
                colors = new Color[width * height];
                Array.Fill(colors, Color);
                tex.SetPixels(x, y, width, height, colors);
            }

            if ((Sides & BorderSides.LEFT) != 0) FillRect(0, 0, Thickness, tex.height);
            if ((Sides & BorderSides.RIGHT) != 0) FillRect(tex.width - Thickness, 0, Thickness, tex.height);
            if ((Sides & BorderSides.TOP) != 0) FillRect(0, tex.height - Thickness, tex.width, Thickness);
            if ((Sides & BorderSides.BOTTOM) != 0) FillRect(0, 0, tex.width, Thickness);

            tex.Apply();

            sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
            spriteCache.Add((Size, Thickness, Color, Sides), sprite);
        }

        obj.AddComponent<Image>().sprite = sprite;
    }
}

[Flags]
public enum BorderSides
{
    ALL = LEFT | RIGHT | TOP | BOTTOM,
    LEFT = 1 << 0,
    RIGHT = 1 << 1,
    TOP = 1 << 2,
    BOTTOM = 1 << 3,
}