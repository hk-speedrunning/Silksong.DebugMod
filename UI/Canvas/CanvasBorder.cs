using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DebugMod.UI.Canvas;

public class CanvasBorder : CanvasObject
{
    private static readonly Dictionary<Args, Sprite> spriteCache = new();

    private Args args = new(Vector2.zero, UICommon.BORDER_THICKNESS, UICommon.borderColor, BorderSides.ALL);

    public int Thickness
    {
        get => args.thickness;
        set
        {
            if (args.thickness != value)
            {
                args.thickness = value;
                DrawTexture();
            }
        }
    }

    public Color Color
    {
        get => args.color;
        set
        {
            if (args.color != value)
            {
                args.color = value;
                DrawTexture();
            }
        }
    }

    public BorderSides Sides
    {
        get => args.sides;
        set
        {
            if (args.sides != value)
            {
                args.sides = value;
                DrawTexture();
            }
        }
    }

    public CanvasBorder(string name) : base(name) { }

    protected override void OnUpdatePosition()
    {
        base.OnUpdatePosition();

        if (args.size != Size)
        {
            args.size = Size;
            DrawTexture();
        }
    }

    public override void Build()
    {
        // Absolute coords need to be whole numbers or the border will not render correctly
        Vector2 truncated = new((int)Position.x, (int)Position.y);
        if (Position != truncated)
        {
            LocalPosition += truncated - Position;
        }

        if (Size.x == 0 || Size.y == 0)
        {
            Size = Parent.Size;
        }

        if (Size.x <= 0 || Size.y <= 0)
        {
            throw new Exception($"Border size must be positive: {GetQualifiedName()}");
        }

        base.Build();
        DrawTexture();
    }

    private void DrawTexture()
    {
        if (!gameObject)
        {
            return;
        }

        if (!spriteCache.TryGetValue(args, out Sprite sprite))
        {
            Texture2D tex = new((int)Size.x, (int)Size.y, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            // Make the entire texture transparent to start
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
            spriteCache.Add(args, sprite);
        }

        Image image = gameObject.GetComponent<Image>() ?? gameObject.AddComponent<Image>();
        image.sprite = sprite;
    }

    private record struct Args(Vector2 size, int thickness, Color color, BorderSides sides);
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