using System;
using UnityEngine;

namespace DebugMod.UI.Canvas;

public class CanvasAutoPanel : CanvasPanel
{
    public const int SECTION_END_PADDING = 20;
    public const int SECTION_HEADER_HEIGHT = 30;

    public float Offset { get; set; } = UICommon.MARGIN;

    public CanvasAutoPanel(string name, CanvasNode parent)
        : base(name, parent) {}

    public T Append<T>(string name, float height) where T : CanvasNode
    {
        T element = Add<T>(name);
        element.LocalPosition = new Vector2(UICommon.MARGIN, Offset);
        element.Size = new Vector2(Size.x - UICommon.MARGIN * 2, height);
        Offset += height + UICommon.MARGIN;
        return element;
    }

    public CanvasText AppendSectionHeader(string name)
    {
        Offset += SECTION_END_PADDING;

        TextGenerationSettings settings = new();
        TextGenerator generator = new TextGenerator();
        float height = generator.GetPreferredHeight(name, settings);

        CanvasText text = Append<CanvasText>(name, SECTION_HEADER_HEIGHT);
        text.Text = name;
        text.Font = UICommon.trajanNormal;
        text.FontSize = 30;
        text.Alignment = TextAnchor.MiddleCenter;

        return text;
    }

    private CanvasControl AppendButtonControl(string name, Action effect, Action<CanvasButton> update)
    {
        CanvasControl control = Append<CanvasControl>(name, UICommon.CONTROL_HEIGHT);

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

    public override void Build()
    {
        base.Build();

        Rect clipRect = new Rect(Position.x - 1920f / 2f + 1, 1080f / 2f - Position.y - Size.y + 1, Size.x - 2, Size.y - 2);

        foreach (CanvasNode child in Subtree())
        {
            if (child != GetImage("Background") && child != GetImage("Background").Border && child is CanvasObject canvasObject)
            {
                CanvasRenderer renderer = canvasObject.obj.GetComponent<CanvasRenderer>();
                renderer.EnableRectClipping(clipRect);
            }
        }
    }
}