using DebugMod.MonoBehaviours;
using System;
using System.Collections;
using UnityEngine;

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
            frameCounter = 0;
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

    [BindableMethod(name = "Advance Frame", category = "Time")]
    public static void AdvanceFrame()
    {
        if (TimeScale.Frozen == false) TimeScale.Frozen = true;
        frameCounter++;
        GameManager.instance.StartCoroutine(AdvanceMyFrame());
    }

    private static IEnumerator AdvanceMyFrame()
    {
        TimeScale.Frozen = false;
        yield return new WaitForFixedUpdate();
        TimeScale.Frozen = true;
    }

    [BindableMethod(name = "Reset Frame Counter", category = "Time")]
    public static void ResetFrameCounter()
    {
        frameCounter = 0;
    }
}