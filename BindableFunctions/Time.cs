using DebugMod.MonoBehaviours;
using GlobalEnums;
using System;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DebugMod;

public static partial class BindableFunctions
{
    internal static int frameCounter = 0;

    [BindableMethod(name = "Increase Timescale", category = "Time")]
    public static void TimescaleUp() => TimeScale.CustomTimeScale = Mathf.Round(TimeScale.CustomTimeScale * 10 + 1f) / 10f;

    [BindableMethod(name = "Decrease Timescale", category = "Time")]
    public static void TimescaleDown() => TimeScale.CustomTimeScale = Mathf.Round(TimeScale.CustomTimeScale * 10 - 1f) / 10f;

    [BindableMethod(name = "Freeze Game", category = "Time")]
    public static void PauseGameNoUI()
    {
        TimeScale.Frozen = !TimeScale.Frozen; // <- this will set timescale accordingly

        if (TimeScale.Frozen)
        {
            GameCameras.instance.StopCameraShake();
            DebugMod.LogConsole("Game was Frozen");
        }
        else
        {
            GameCameras.instance.ResumeCameraShake();
            GameManager.instance.isPaused = false;
            GameManager.instance.ui.SetState(UIState.PLAYING);
            GameManager.instance.SetState(GameState.PLAYING);
            if (HeroController.instance != null) HeroController.instance.UnPause();
            GameManager.instance.inputHandler.AllowPause();
            DebugMod.LogConsole("Game was Unfrozen");
        }
    }

    [BindableMethod(name = "Force Pause", category = "Time")]
    public static void ForcePause()
    {
        try
        {
            if (PlayerData.instance.disablePause || GameManager.instance.TimeSlowed ||
                 UIManager.instance.ignoreUnpause && DebugMod.GetSceneName() != "Menu_Title" &&
                DebugMod.GM.IsGameplayScene())
            {
                GameManager.instance.timeSlowedCount = 0;
                UIManager.instance.ignoreUnpause = false;
                PlayerData.instance.disablePause = false;
                UIManager.instance.TogglePauseGame();
                DebugMod.LogConsole("Forcing Pause Menu because pause is disabled");
            }
            else
            {
                DebugMod.LogConsole("Game does not report that Pause is disabled, requesting it normally.");
                UIManager.instance.TogglePauseGame();
            }

            DebugMod.forcePaused = !GameManager.instance.isPaused;
        }
        catch (Exception e)
        {
            DebugMod.LogConsole("Error while attempting to pause, check ModLog.txt");
            DebugMod.Log("Error while attempting force pause:\n" + e);
        }
    }

    [BindableMethod(name = "Start/End Frame Advance", category = "Time")]
    public static void ToggleFrameAdvance()
    {
        frameCounter = 0;
        if (TimeScale.Frozen == false || DebugMod.frameAdvanceActive == false)
        {
            DebugMod.frameAdvanceActive = true;
            TimeScale.Frozen = true;
            DebugMod.LogConsole("Starting frame by frame advance on keybind press");
        }
        else
        {
            DebugMod.frameAdvanceActive = false;
            TimeScale.Frozen = false;
            DebugMod.LogConsole("Stopping frame by frame advance on keybind press");
        }
    }

    [BindableMethod(name = "Advance Frame", category = "Time")]
    public static void AdvanceFrame()
    {
        if (TimeScale.Frozen == false) ToggleFrameAdvance();
        frameCounter++;
        GameManager.instance.StartCoroutine(AdvanceMyFrame());
    }

    private static IEnumerator AdvanceMyFrame()
    {
        TimeScale.Frozen = false;
        DebugMod.advancingFrame = true;
        yield return new WaitForFixedUpdate();
        TimeScale.Frozen = true;
        DebugMod.advancingFrame = false;
    }

    [BindableMethod(name = "Reset Frame Counter", category = "Time")]
    public static void ResetFrameCounter()
    {
        frameCounter = 0;
    }
}