using System;
using UnityEngine;

namespace DebugMod.UI.Canvas;

public class CanvasAutoPanel : CanvasPanel
{
    public const int SECTION_END_PADDING = 20;
    public const int SECTION_HEADER_FONT_SIZE = 30;
    public const int SECTION_HEADER_HEIGHT = 30;

    public float Offset { get; set; } = UICommon.MARGIN;

    public CanvasAutoPanel(string name) : base(name) {}

    public T Append<T>(T element, float height) where T : CanvasNode
    {
        Add(element);
        element.LocalPosition = new Vector2(UICommon.MARGIN, Offset);
        element.Size = new Vector2(Size.x - UICommon.MARGIN * 2, height);
        Offset += height + UICommon.MARGIN;
        return element;
    }

    public CanvasText AppendSectionHeader(string name)
    {
        Offset += SECTION_END_PADDING;

        CanvasText text = Append(new CanvasText(name), SECTION_HEADER_HEIGHT);
        text.Text = name;
        text.Font = UICommon.trajanNormal;
        text.FontSize = SECTION_HEADER_FONT_SIZE;
        text.Alignment = TextAnchor.MiddleCenter;

        return text;
    }

    private CanvasControl AppendButtonControl(string name, Action effect, Action<CanvasButton> update)
    {
        CanvasControl control = Append(new CanvasControl(name), UICommon.ControlHeight);

        CanvasButton button = control.AppendFlex(new CanvasButton("Button"));
        button.Text.Text = name;
        button.OnClicked += effect;
        if (update != null) button.OnUpdate += () => update(button);

        if (DebugMod.bindsByMethod.TryGetValue(effect.Method, out BindAction action))
        {
            control.AttachKeybind(action.Name);
        }

        return control;
    }

    public CanvasControl AppendBasicControl(string name, Action effect) => AppendButtonControl(name, effect, null);

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

    public override void Build()
    {
        Size = new Vector2(Size.x, Mathf.Max(Size.y, Offset));
        base.Build();
    }
}