﻿using System;
using DebugMod.UI.Canvas;
using UnityEngine;

namespace DebugMod.UI;

/// <summary>
/// Creates an input panel which is a single column of info.
/// </summary>
public class SimpleInfoPanel : CustomInfoPanel
{
    private readonly float sep;
    private float y = -10f;

    public SimpleInfoPanel(string name, CanvasNode parent, Vector2 position, Vector2 size, float sep) : base(name, parent, position, size)
    {
        this.sep = sep;
    }

    public void AddInfo(string label, Func<string> textFunc)
    {
        if ((label, textFunc) == (null, null))
        {
            y += 20;
        }
        else
        {
            AddInfo(10, 10 + sep, y += 20, label, textFunc);
        }
    }
}
