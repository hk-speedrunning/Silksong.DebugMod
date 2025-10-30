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
        CanvasControl control = Add<CanvasControl>(name);

        control.LocalPosition = new Vector2(UICommon.MARGIN, Offset);
        control.Size = new Vector2(Size.x - UICommon.MARGIN * 2, UICommon.CONTROL_HEIGHT);
        Offset += UICommon.CONTROL_HEIGHT + UICommon.MARGIN;

        return control;
    }

    private CanvasControl AppendButtonControl(string name, Action effect, Action<CanvasButton> update)
    {
        CanvasControl control = AppendEmptyControl(name);

        CanvasButton button = control.AppendFlexButton("Button");
        button.Text.Text = name;
        button.OnClicked += effect;
        if (update != null) button.OnUpdate += () => update(button);

        if (DebugMod.bindsByMethod.TryGetValue(effect.Method, out BindAction action))
        {
            control.AttachKeybind(action.Name);
        }

        return control;
    }

    public CanvasControl AppendControl(string name, Action effect) => AppendButtonControl(name, effect, null);

    // TODO: replace this with checkbox
    public CanvasControl AppendToggleControl(string name, Func<bool> getter, Action effect)
    {
        return AppendButtonControl(name, effect, button =>
        {
            button.Text.Color = getter() ? UICommon.accentColor : UICommon.textColor;
        });
    }

    // TODO: replace this with a slider or increment/decrement buttons
    public CanvasControl AppendIncrementControl(string name, Func<int> getter, Action effect)
    {
        return AppendButtonControl(name, effect, button =>
        {
            button.Text.Text = $"{name}: {getter()}";
        });
    }
}