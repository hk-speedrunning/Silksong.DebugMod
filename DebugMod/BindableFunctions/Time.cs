using System;
using System.Collections;
using DebugMod.MonoBehaviours;
using GlobalEnums;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DebugMod;

public static partial class BindableFunctions
{
    private static float timeScaleDuringFrameAdvance = 0f;
    internal static int frameCounter = 0;

    [BindableMethod(name = "Increase Timescale", category = "Time")]
    public static void TimescaleUp()
    {
        if (DebugMod.GM.IsGamePaused())
        {
            DebugMod.LogConsole("Cannot change timescale when paused");
            return;
        }
        float oldScale = Time.timeScale;
        bool wasTimeScaleActive = DebugMod.TimeScaleActive;
        Time.timeScale = Time.timeScale = Mathf.Round(Time.timeScale * 10 + 1f) / 10f;
        DebugMod.CurrentTimeScale = Time.timeScale;

        DebugMod.TimeScaleActive = Math.Abs(DebugMod.CurrentTimeScale - 1f) > Mathf.Epsilon;

        switch (DebugMod.TimeScaleActive)
        {
            case true when wasTimeScaleActive == false:
                if (GameManager.instance.GetComponent<TimeScale>() == null) GameManager.instance.gameObject.AddComponent<TimeScale>();
                break;
            case false when wasTimeScaleActive:
                if (GameManager.instance.GetComponent<TimeScale>() != null)
                {
                    Object.Destroy(GameManager.instance.gameObject.GetComponent<TimeScale>());
                }

                break;
        }
        DebugMod.LogConsole("New TimeScale value: " + DebugMod.CurrentTimeScale + " Old value: " + oldScale);
    }

    [BindableMethod(name = "Decrease Timescale", category = "Time")]
    public static void TimescaleDown()
    {
        //This needs to be added because the game sets timescale to 0 when paused to pause the game if this is changed to a
        //non-zero value, the game continues to play even tho the pause menu is up which is scuffed so this makes it less skuffed
        if (DebugMod.GM.IsGamePaused())
        {
            DebugMod.LogConsole("Cannot change timescale when paused");
            return;
        }
        float oldScale = Time.timeScale;
        bool wasTimeScaleActive = DebugMod.TimeScaleActive;
        Time.timeScale = Time.timeScale = Mathf.Round(Time.timeScale * 10 - 1f) / 10f;
        DebugMod.CurrentTimeScale = Time.timeScale;

        DebugMod.TimeScaleActive = Math.Abs(DebugMod.CurrentTimeScale - 1f) > Mathf.Epsilon;

        switch (DebugMod.TimeScaleActive)
        {
            case true when wasTimeScaleActive == false:
                if (GameManager.instance.GetComponent<TimeScale>() == null) GameManager.instance.gameObject.AddComponent<TimeScale>();
                break;
            case false when wasTimeScaleActive:
                if (GameManager.instance.GetComponent<TimeScale>() != null)
                {
                    Object.Destroy(GameManager.instance.gameObject.GetComponent<TimeScale>());
                }

                break;
        }

        DebugMod.LogConsole("New TimeScale value: " + DebugMod.CurrentTimeScale + " Old value: " + oldScale);
    }

    [BindableMethod(name = "Freeze Game", category = "Time")]
    public static void PauseGameNoUI()
    {
        DebugMod.PauseGameNoUIActive = !DebugMod.PauseGameNoUIActive;

        if (DebugMod.PauseGameNoUIActive)
        {
            Time.timeScale = 0;
            GameCameras.instance.StopCameraShake();
            SetAlwaysShowCursor();
            DebugMod.LogConsole("Game was Frozen");
        }
        else
        {
            GameCameras.instance.ResumeCameraShake();
            GameManager.instance.isPaused = false;
            GameManager.instance.ui.SetState(UIState.PLAYING);
            GameManager.instance.SetState(GameState.PLAYING);
            if (HeroController.instance != null) HeroController.instance.UnPause();
            Time.timeScale = DebugMod.CurrentTimeScale;
            GameManager.instance.inputHandler.AllowPause();

            if (!DebugMod.settings.ShowCursorWhileUnpaused)
            {
                UnsetAlwaysShowCursor();
            }

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
        if (Time.timeScale != 0)
        {
            if (GameManager.instance.GetComponent<TimeScale>() == null)
                GameManager.instance.gameObject.AddComponent<TimeScale>();
            Time.timeScale = 0f;
            timeScaleDuringFrameAdvance = DebugMod.CurrentTimeScale;
            DebugMod.CurrentTimeScale = 0;
            DebugMod.TimeScaleActive = true;
            DebugMod.LogConsole("Starting frame by frame advance on keybind press");
        }
        else
        {
            DebugMod.CurrentTimeScale = timeScaleDuringFrameAdvance;
            Time.timeScale = DebugMod.CurrentTimeScale;
            DebugMod.LogConsole("Stopping frame by frame advance on keybind press");
        }
    }

    [BindableMethod(name = "Advance Frame", category = "Time")]
    public static void AdvanceFrame()
    {
        if (Time.timeScale != 0) ToggleFrameAdvance();
        frameCounter++;
        GameManager.instance.StartCoroutine(AdvanceMyFrame());
    }

    private static IEnumerator AdvanceMyFrame()
    {
        DebugMod.CurrentTimeScale = Time.timeScale = 1f;
        yield return new WaitForFixedUpdate();

        DebugMod.CurrentTimeScale = Time.timeScale = 0f;
    }

    [BindableMethod(name = "Reset Frame Counter", category = "Time")]
    public static void ResetFrameCounter()
    {
        frameCounter = 0;
    }
}