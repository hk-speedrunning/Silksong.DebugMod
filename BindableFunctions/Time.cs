using DebugMod.MonoBehaviours;
using System;
using System.Collections;
using UnityEngine;

namespace DebugMod;

public static partial class BindableFunctions
{
    internal static int frameCounter = 0;

    [BindableMethod(name = "ACTION_INCREASETIMESCALE", category = "CATEGORY_TIME")]
    public static void TimescaleUp() => TimeScale.CustomTimeScale = Mathf.Round(TimeScale.CustomTimeScale * 10 + 1f) / 10f;

    [BindableMethod(name = "ACTION_DECREASETIMESCALE", category = "CATEGORY_TIME")]
    public static void TimescaleDown() => TimeScale.CustomTimeScale = Mathf.Round(TimeScale.CustomTimeScale * 10 - 1f) / 10f;

    [BindableMethod(name = "ACTION_RESETTIMESCALE", category = "CATEGORY_TIME")]
    public static void TimescaleReset() => TimeScale.CustomTimeScale = 1f;

    [BindableMethod(name = "GAMEPLAY_TIME_FREEZEGAME", category = "CATEGORY_TIME")]
    public static void PauseGameNoUI()
    {
        TimeScale.Frozen = !TimeScale.Frozen; // <- this will set timescale accordingly
        // We need to log here since TimeScale.Frozen is used on Advance Frame
        if (TimeScale.Frozen)
        {
            frameCounter = 0;
            DebugMod.LogConsole("Game frozen");
        }
        else
        {
            DebugMod.LogConsole("Game unfrozen");
        }
    }

    [BindableMethod(name = "GAMEPLAY_TIME_FORCEPAUSE", category = "CATEGORY_TIME")]
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
                DebugMod.LogConsole("Forcing pause menu because pause is disabled");
            }
            else
            {
                DebugMod.LogConsole("Pausing game");
                UIManager.instance.TogglePauseGame();
            }

            DebugMod.forcePaused = !GameManager.instance.isPaused;
        }
        catch (Exception e)
        {
            DebugMod.LogConsole("Error while attempting to pause, please create a bug report");
            DebugMod.Log("Error while attempting force pause:\n" + e);
        }
    }

    [BindableMethod(name = "GAMEPLAY_TIME_ADVANCEFRAME", category = "CATEGORY_TIME")]
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

    [BindableMethod(name = "GAMEPLAY_TIME_RESETFRAMECOUNTER", category = "CATEGORY_TIME")]
    public static void ResetFrameCounter()
    {
        DebugMod.LogConsole($"Frame counter reset (was {frameCounter})");
        frameCounter = 0;
    }
}