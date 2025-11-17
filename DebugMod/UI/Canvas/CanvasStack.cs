using System;
using System.Collections.Generic;
using UnityEngine;

namespace DebugMod.UI.Canvas;

public class CanvasStack : CanvasNode
{
    private readonly List<Entry> entries = [];

    public bool Horizontal { get; set; }
    public bool DynamicLength { get; set; }
    public float Padding { get; set; }

    public CanvasStack(string name) : base(name) { }

    private T Append<T>(T element, LengthType type, float length = default) where T : CanvasNode
    {
        element?.Parent = this;
        entries.Add(new Entry(element, type, length));
        return element;
    }

    public T AppendFixed<T>(T element, float length) where T : CanvasNode => Append(element, LengthType.Fixed, length);
    public T AppendSquare<T>(T element) where T : CanvasNode => Append(element, LengthType.Square);
    public T AppendFlex<T>(T element) where T : CanvasNode => Append(element, LengthType.Flex);
    public void AppendPadding(float length) => Append<CanvasNode>(null, LengthType.Fixed, length);

    protected override IEnumerable<CanvasNode> ChildList()
    {
        foreach (Entry entry in entries)
        {
            if (entry.element != null) yield return entry.element;
        }
    }

    public override void Build()
    {
        float totalFixedLength = Padding * (entries.Count + 1);
        int flexCount = 0;

        foreach (Entry entry in entries)
        {
            switch (entry.type)
            {
                case LengthType.Fixed:
                    totalFixedLength += entry.length;
                    break;
                case LengthType.Square:
                    totalFixedLength += Breadth();
                    break;
                case LengthType.Flex:
                    flexCount++;
                    break;
            }
        }

        if (DynamicLength && flexCount != 0)
        {
            throw new Exception("Flex elements are not supported for a dynamic-length CanvasStack");
        }

        if (Length() < totalFixedLength && flexCount != 0)
        {
            throw new Exception("Overflow in CanvasStack: no room for flex elements");
        }

        float flexLength = (Length() - totalFixedLength) / flexCount;
        float t = Padding;

        foreach (Entry entry in entries)
        {
            float length = entry.type switch
            {
                LengthType.Fixed => entry.length,
                LengthType.Square => Breadth(),
                LengthType.Flex => flexLength
            };

            if (entry.element != null)
            {
                float x = Padding;
                float y = t;
                float width = Breadth() - Padding * 2;
                float height = length;
                if (Horizontal) (x, y, width, height) = (y, x, height, width);

                entry.element.LocalPosition = new Vector2(x, y);
                entry.element.Size = new Vector2(width, height);
            }

            t += length + Padding;
        }

        if (DynamicLength)
        {
            Size = Horizontal ? new Vector2(t, Size.y) : new Vector2(Size.x, t);
        }

        base.Build();
    }

    public float GetCurrentLength()
    {
        float length = Padding * (entries.Count + 1);

        foreach (Entry entry in entries)
        {
            switch (entry.type)
            {
                case LengthType.Fixed:
                    length += entry.length;
                    break;
                case LengthType.Square:
                    length += Breadth();
                    break;
                case LengthType.Flex:
                    throw new Exception("Current length is meaningless if the stack contains flex elements");
            }
        }

        return length;
    }

    private float Length() => Horizontal ? Size.x : Size.y;
    private float Breadth() => Horizontal ? Size.y : Size.x;

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
