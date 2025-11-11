using System;
using InControl;
using UnityEngine;

namespace DebugMod.UI;

public class BottomRightInfoPanel : InfoPanel
{
    public BottomRightInfoPanel() : base(nameof(BottomRightInfoPanel))
    {
        LocalPosition = new Vector2(25f, 300f);

        AddText("Right1 Label", "Load\nHero Pos\nMove Raw", new Vector2(0f, 0f), Vector2.zero, UICommon.arial);
        AddText("Right1", "", new Vector2(100f, 0f), Vector2.zero, UICommon.trajanNormal);

        AddText("Right2 Label", "Move Vector\nKey Pressed\nMove Pressed", new Vector2(240f, 0f), Vector2.zero, UICommon.arial);
        AddText("Right2", "", new Vector2(340f, 0f), Vector2.zero, UICommon.trajanNormal);

        AddText("Right3 Label", "Frame Counter\nGame Time Elapsed\nSession Time", new Vector2(385f, 0f), Vector2.zero, UICommon.arial);
        AddText("Right3", "", new Vector2(515f, 0f), Vector2.zero, UICommon.trajanNormal);
    }

    public override void Update()
    {
        var frameTime = TimeSpan.FromSeconds(BindableFunctions.frameCounter * 0.02);

        int time1 = Mathf.FloorToInt(Time.realtimeSinceStartup / 60f);
        int time2 = Mathf.FloorToInt(Time.realtimeSinceStartup - time1 * 60);

        GetText("Right1").Text = DebugMod.GetLoadTime() + "s\n" + GetHeroPos() + "\n" + string.Format("L: {0} R: {1} U: {2} D: {3}", DebugMod.IH.inputActions.Left.RawValue, DebugMod.IH.inputActions.Right.RawValue, DebugMod.IH.inputActions.Up.RawValue, DebugMod.IH.inputActions.Down.RawValue);
        GetText("Right2").Text = DebugMod.IH.inputActions.MoveVector.Vector.x + ", " + DebugMod.IH.inputActions.MoveVector.Vector.y + "\n" + GetStringForBool(InputManager.AnyKeyIsPressed) + "\n" + GetStringForBool(DebugMod.IH.inputActions.Left.IsPressed || DebugMod.IH.inputActions.Right.IsPressed);
        GetText("Right3").Text = BindableFunctions.frameCounter + "\n" + frameTime.ToString("mm':'ss'.'ff") + "\n" + string.Format("{0:00}:{1:00}", time1, time2);

        base.Update();
    }
}