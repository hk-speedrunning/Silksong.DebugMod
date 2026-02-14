using System;
using System.Collections.Generic;
using UnityEngine;

namespace DebugMod.UI.Canvas;

// Helper class to automatically position a series of elements
// one after the other while filling the space along the other axis
public class PanelBuilder : IDisposable
{
    private readonly CanvasPanel panel;
    private readonly List<Entry> entries = [];

    public bool Horizontal { get; set; }
    public bool DynamicLength { get; set; }
    public float OuterPadding { get; set; }
    public float InnerPadding { get; set; }

    public float Padding
    {
        set
        {
            OuterPadding = value;
            InnerPadding = value;
        }
    }

    public PanelBuilder(CanvasPanel panel)
    {
        this.panel = panel;
    }

    private T Append<T>(T element, LengthType type, float length = default) where T : CanvasNode
    {
        entries.Add(new Entry(element, type, length));

        if (element != null)
        {
            if (type == LengthType.Fixed && length <= 0)
            {
                throw new Exception("Fixed elements must have a positive length");
            }

            panel.Add(element);

            // Set the breadth now since it is unlikely to change later and other code might rely on it
            float breadth = ChildBreadth();
            if (type == LengthType.Square)
            {
                length = breadth;
            }

            element?.Size = Horizontal ? new Vector2(length, breadth) : new Vector2(breadth, length);
        }

        return element;
    }

    public T AppendFixed<T>(T element, float length) where T : CanvasNode => Append(element, LengthType.Fixed, length);
    public T AppendSquare<T>(T element) where T : CanvasNode => Append(element, LengthType.Square);
    public T AppendFlex<T>(T element) where T : CanvasNode => Append(element, LengthType.Flex);
    public void AppendPadding(float length) => Append<CanvasNode>(null, LengthType.Fixed, length);
    public void AppendFlexPadding() => Append<CanvasNode>(null, LengthType.Flex);

    public void Build()
    {
        float totalFixedLength = OuterPadding * 2 + InnerPadding * (entries.Count - 1);
        int flexCount = 0;

        foreach (Entry entry in entries)
        {
            switch (entry.type)
            {
                case LengthType.Fixed:
                    totalFixedLength += entry.length;
                    break;
                case LengthType.Square:
                    totalFixedLength += ChildBreadth();
                    break;
                case LengthType.Flex:
                    flexCount++;
                    break;
            }
        }

        if (DynamicLength && flexCount != 0)
        {
            throw new Exception("Flex elements are not supported for a dynamic-length PanelBuilder");
        }

        if (Length() < totalFixedLength && flexCount != 0)
        {
            throw new Exception($"Overflow in PanelBuilder: no room for flex elements (need {totalFixedLength}, have {Length()})");
        }

        float flexLength = (Length() - totalFixedLength) / flexCount;
        float t = OuterPadding;

        foreach (Entry entry in entries)
        {
            float length = entry.type switch
            {
                LengthType.Square => ChildBreadth(),
                LengthType.Flex => flexLength,
                _ => entry.length
            };

            if (entry.element != null)
            {
                float x = OuterPadding;
                float y = t;
                float width = ChildBreadth();
                float height = length;
                if (Horizontal) (x, y, width, height) = (y, x, height, width);

                entry.element.LocalPosition = new Vector2(x, y);
                entry.element.Size = new Vector2(width, height);
            }

            t += length + InnerPadding;
        }

        t -= InnerPadding;
        t += OuterPadding;

        if (DynamicLength)
        {
            panel.Size = Horizontal ? new Vector2(t, panel.Size.y) : new Vector2(panel.Size.x, t);
        }
    }

    public float GetCurrentLength()
    {
        float length = OuterPadding * 2 + InnerPadding * (entries.Count - 1);

        foreach (Entry entry in entries)
        {
            switch (entry.type)
            {
                case LengthType.Fixed:
                    length += entry.length;
                    break;
                case LengthType.Square:
                    length += ChildBreadth();
                    break;
                case LengthType.Flex:
                    throw new Exception("Current length is meaningless if the panel contains flex elements");
            }
        }

        return length;
    }

    public float Length() => Horizontal ? panel.Size.x : panel.Size.y;
    public float Breadth() => Horizontal ? panel.Size.y : panel.Size.x;
    public float ChildBreadth() => Breadth() - OuterPadding * 2;

    public void Dispose()
    {
        Build();
    }

    private class Entry
    {
        public readonly CanvasNode element;
        public readonly LengthType type;
        public readonly float length;

        public Entry(CanvasNode element, LengthType type, float length)
        {
            this.element = element;
            this.type = type;
            this.length = length;
        }
    }

    private enum LengthType
    {
        Fixed,
        Square,
        Flex,
    }
}
