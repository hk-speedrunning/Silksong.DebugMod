using System;
using UnityEngine;

namespace DebugMod.UI.Canvas;

public class CanvasAutoPanel : CanvasPanel
{
    public int Offset { get; set; } = UICommon.MARGIN;

    public CanvasAutoPanel(string name, CanvasNode parent)
        : base(name, parent) {}

    private CanvasControl AppendEmptyControl(string name)
    {
        CanvasControl control = new CanvasControl(name, this);
        AddElement(control);

        control.LocalPosition = new Vector2(UICommon.MARGIN, Offset);
        control.Size = new Vector2(Size.x - UICommon.MARGIN * 2, UICommon.CONTROL_HEIGHT);
        Offset += UICommon.CONTROL_HEIGHT + UICommon.MARGIN;

        return control;
    }

    public CanvasControl AppendControl(string name, Action effect)
    {
        CanvasControl control = AppendEmptyControl(name);

        CanvasButton button = control.AppendFlexButton("Button");
        button.Text.Text = name;
        button.OnClicked += effect;

        return control;
    }

    // TODO: replace this with checkbox
    public CanvasControl AppendBoolControl(string name, Func<bool> getter, Action updater)
    {
        CanvasControl control = AppendEmptyControl(name);

        CanvasButton button = control.AppendFlexButton("Button");
        button.Text.Text = name;
        button.OnUpdate += () => button.Text.Color = getter() ? UICommon.accentColor : UICommon.textColor;
        button.OnClicked += updater;

        return control;
    }
}