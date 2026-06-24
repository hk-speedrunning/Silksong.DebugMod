using DebugMod.UI.Canvas;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DebugMod.UI;

public class DropdownDialog : CanvasDialog
{
    public static DropdownDialog Instance { get; private set; }

    public static void BuildPanel()
    {
        Instance = new DropdownDialog();
    }

    private readonly List<CanvasButton> buttons = [];

    private List<string> options;
    private Action<string> onSelect;

    protected override bool CustomPositioning => true;

    public DropdownDialog() : base(nameof(DropdownDialog))
    {
    }

    protected override void BuildDialog()
    {
        base.BuildDialog();
        Get<CanvasImage>("Background").RemoveBorder();

        LocalPosition = anchor.Position + new Vector2(0f, anchor.Size.y);
        Size = new Vector2(anchor.Size.x, 0f);

        PanelBuilder builder = new(this);
        builder.DynamicLength = true;

        buttons.Clear();

        for (int i = 0; i < options.Count; i++)
        {
            string option = options[i];

            CanvasButton button = builder.AppendFixed(new CanvasButton($"Option{i}"), UICommon.ControlHeight);
            button.Text.Alignment = TextAnchor.MiddleLeft;
            button.Text.Text = option;
            button.SetImage(UICommon.clearBG);
            button.RemoveBorder();
            button.RemoveHoverBorder();
            button.OnClicked += () =>
            {
                onSelect(option);
                Hide();
            };

            buttons.Add(button);
        }

        builder.Build();

        // Needs to go on top of button backgrounds
        CanvasBorder border = Add(new CanvasBorder("TopBorder"));
        border.Sides &= ~BorderSides.TOP;
    }

    public override void Build()
    {
        base.Build();

        foreach (CanvasButton button in buttons)
        {
            button.AddEventTrigger(EventTriggerType.PointerEnter, _ => button.SetImage(UICommon.panelStrongBG));
            button.AddEventTrigger(EventTriggerType.PointerExit, _ => button.SetImage(UICommon.clearBG));
        }
    }

    public void Toggle(CanvasNode anchor, List<string> options, Action<string> onSelect)
    {
        if (options == null || options.Count == 0)
        {
            return;
        }

        if (TryStartToggle(anchor))
        {
            this.options = options;
            this.onSelect = onSelect;

            Show();
        }
    }
}
