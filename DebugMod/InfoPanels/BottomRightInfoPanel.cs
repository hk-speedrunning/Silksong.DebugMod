using System;
using DebugMod.Canvas;
using InControl;
using UnityEngine;

namespace DebugMod.InfoPanels
{
    public class BottomRightInfoPanel : InfoPanel
    {
        public override void BuildPanel(GameObject canvas)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadRawTextureData(new byte[] { 0x00, 0x00, 0x00, 0x00 });
            tex.Apply();

            // Puke
            panel = new CanvasPanel(
                canvas,
                tex,
                new Vector2(0f, 223f),
                Vector2.zero,
                new Rect(
                    0f,
                    0f,
                    1f,
                    1f));

            panel.AddText("Right1 Label", "Session Time\nLoad\nHero Pos\nMove Raw", new Vector2(1285, 747), Vector2.zero, GUIController.Instance.arial);
            panel.AddText("Right1", "", new Vector2(1385, 747), Vector2.zero, GUIController.Instance.trajanNormal);

            panel.AddText("Right2 Label", "Move Vector\nKey Pressed\nMove Pressed", new Vector2(1525, 747), Vector2.zero, GUIController.Instance.arial);
            panel.AddText("Right2", "", new Vector2(1625, 747), Vector2.zero, GUIController.Instance.trajanNormal);

            panel.AddText("Right3 Label", "Frame Counter\nGame Time Elapsed", new Vector2(1670, 747), Vector2.zero, GUIController.Instance.arial);
            panel.AddText("Right3", "", new Vector2(1800, 747), Vector2.zero, GUIController.Instance.trajanNormal);


            panel.FixRenderOrder();
        }

        public override void UpdatePanel()
        {
            var frameTime = TimeSpan.FromSeconds(BindableFunctions.frameCounter * 0.02);
            if (panel == null) return;

            int time1 = Mathf.FloorToInt(Time.realtimeSinceStartup / 60f);
            int time2 = Mathf.FloorToInt(Time.realtimeSinceStartup - (float)(time1 * 60));

            panel.GetText("Right1").UpdateText(string.Format("{0:00}:{1:00}", time1, time2) + "\n" + DebugMod.GetLoadTime() + "s\n" + InfoPanel.GetHeroPos() + "\n" + string.Format("L: {0} R: {1} U: {2} D: {3}", DebugMod.IH.inputActions.Left.RawValue, DebugMod.IH.inputActions.Right.RawValue, DebugMod.IH.inputActions.Up.RawValue, DebugMod.IH.inputActions.Down.RawValue));
            panel.GetText("Right2").UpdateText(DebugMod.IH.inputActions.MoveVector.Vector.x + ", " + DebugMod.IH.inputActions.MoveVector.Vector.y + "\n" + InfoPanel.GetStringForBool(InputManager.AnyKeyIsPressed) + "\n" + InfoPanel.GetStringForBool(DebugMod.IH.inputActions.Left.IsPressed || DebugMod.IH.inputActions.Right.IsPressed));
            panel.GetText("Right3").UpdateText(BindableFunctions.frameCounter + "\n" + frameTime.ToString("mm':'ss'.'ff"));
        }

        public override bool Active => true;
    }
}
