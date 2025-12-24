using System;
using System.Collections;
using System.Reflection;
using DebugMod.MonoBehaviours;
using HarmonyLib;
using UnityEngine;

namespace DebugMod;

[HarmonyPatch]
public static partial class BindableFunctions
{
    private static readonly FieldInfo TimeSlowed = typeof(GameManager).GetField("timeSlowed", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
    private static readonly FieldInfo IgnoreUnpause = typeof(UIManager).GetField("ignoreUnpause", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
    internal static readonly FieldInfo cameraGameplayScene = typeof(CameraController).GetField("isGameplayScene", BindingFlags.Instance | BindingFlags.NonPublic);
    private static float TimeScaleDuringFrameAdvance = 0f;
    internal static int frameCounter = 0;

    [BindableMethod(name = "Force Pause", category = "Misc")]
    public static void ForcePause()
    {
        try
        {
            if ((PlayerData.instance.disablePause || (bool) TimeSlowed.GetValue(GameManager.instance) ||
                 (bool) IgnoreUnpause.GetValue(UIManager.instance)) && DebugMod.GetSceneName() != "Menu_Title" &&
                DebugMod.GM.IsGameplayScene())
            {
                TimeSlowed.SetValue(GameManager.instance, false);
                IgnoreUnpause.SetValue(UIManager.instance, false);
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

    [BindableMethod(name = "Hazard Respawn", category = "Misc")]
    public static void Respawn()
    {
        if (GameManager.instance.IsGameplayScene() && !HeroController.instance.cState.dead &&
            PlayerData.instance.health > 0)
        {
            if (UIManager.instance.uiState.ToString() == "PAUSED")
            {
                InputHandler.Instance.StartCoroutine(GameManager.instance.PauseGameToggle(false));
                GameManager.instance.HazardRespawn();
                DebugMod.LogConsole("Closing Pause Menu and respawning...");
                return;
            }

            if (UIManager.instance.uiState.ToString() == "PLAYING")
            {
                HeroController.instance.RelinquishControl();
                GameManager.instance.HazardRespawn();
                HeroController.instance.RegainControl();
                DebugMod.LogConsole("Respawn signal sent");
                return;
            }

            DebugMod.LogConsole("Respawn requested in some weird conditions, abort, ABORT");
        }
    }

    [BindableMethod(name = "Set Respawn", category = "Misc")]
    public static void SetHazardRespawn()
    {
        Vector3 manualRespawn = DebugMod.RefKnight.transform.position;
        HeroController.instance.SetHazardRespawn(manualRespawn, false);
        DebugMod.LogConsole("Manual respawn point on this map set to" + manualRespawn);
    }

    [BindableMethod(name = "Toggle Act 3", category = "Misc")]
    public static void ToggleAct3()
    {
        PlayerData.instance.blackThreadWorld = !PlayerData.instance.blackThreadWorld;
        DebugMod.LogConsole("Act 3 world is now " + (PlayerData.instance.blackThreadWorld ? "enabled" : "disabled"));
    }

    [BindableMethod(name = "Force Camera Follow", category = "Misc")]
    public static void ForceCameraFollow()
    {
        if (!DebugMod.cameraFollow)
        {
            DebugMod.LogConsole("Forcing camera follow");
            DebugMod.cameraFollow = true;
        }
        else
        {
            DebugMod.cameraFollow = false;
            cameraGameplayScene.SetValue(DebugMod.RefCamera, true);
            DebugMod.LogConsole("Returning camera to normal settings");
        }
    }

    [BindableMethod(name = "Clear White Screen", category = "Misc")]
    public static void ClearWhiteScreen()
    {
        //fix white screen 
        PlayMakerFSM wakeFSM = HeroController.instance.gameObject.LocateMyFSM("Dream Return");
        wakeFSM.SetState("GET UP");
        wakeFSM.SendEvent("FINISHED");
        GameObject.Find("Blanker White").LocateMyFSM("Blanker Control").SendEvent("FADE OUT");
        HeroController.instance.EnableRenderer();
    }

    private static string saveLevelStateAction;

    [BindableMethod(name = "Reset Scene Data", category = "Misc")]
    public static void ResetCurrentScene()
    {
        saveLevelStateAction = GameManager.instance.GetSceneNameString();
        DebugMod.LogConsole("Clearing scene data from this scene, re-enter scene or warp to apply changes");
    }

    [BindableMethod(name = "Block Scene Data Changes", category = "Misc")]
    public static void BlockCurrentSceneChanges()
    {
        saveLevelStateAction = "block";
        DebugMod.LogConsole("Scene data changes made since entering this scene will not be saved");
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.SaveLevelState))]
    [HarmonyPrefix]
    private static bool GameManager_SaveLevelState_Prefix()
    {
        if (saveLevelStateAction == "block")
        {
            saveLevelStateAction = null;
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.SaveLevelState))]
    [HarmonyPostfix]
    private static void GameManager_SaveLevelState_Postfix()
    {
        if (saveLevelStateAction != null && saveLevelStateAction != "block")
        {
            SceneData.instance.persistentBools.scenes.Remove(saveLevelStateAction);
            SceneData.instance.persistentInts.scenes.Remove(saveLevelStateAction);
            SceneData.instance.geoRocks.scenes.Remove(saveLevelStateAction);

            saveLevelStateAction = null;
        }
    }

    [BindableMethod(name = "Break Cocoon", category = "Misc")]
    public static void BreakCocoon()
    {
        HeroController.instance?.CocoonBroken();
        EventRegister.SendEvent("BREAK HERO CORPSE");
    }

    [BindableMethod(name = "Start/End Frame Advance", category = "Misc")]
    public static void ToggleFrameAdvance()
    {
        frameCounter = 0;
        if (Time.timeScale != 0)
        {
            if (GameManager.instance.GetComponent<TimeScale>() == null)
                GameManager.instance.gameObject.AddComponent<TimeScale>();
            Time.timeScale = 0f;
            TimeScaleDuringFrameAdvance = DebugMod.CurrentTimeScale;
            DebugMod.CurrentTimeScale = 0;
            DebugMod.TimeScaleActive = true;
            DebugMod.LogConsole("Starting frame by frame advance on keybind press");
        }
        else
        {
            DebugMod.CurrentTimeScale = TimeScaleDuringFrameAdvance;
            Time.timeScale = DebugMod.CurrentTimeScale;
            DebugMod.LogConsole("Stopping frame by frame advance on keybind press");
        }
    }

    [BindableMethod(name = "Advance Frame", category = "Misc")]
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

    [BindableMethod(name = "Reset Counter", category = "Misc")]
    public static void ResetCounter()
    {
        frameCounter = 0;
    }

    [BindableMethod(name = "Lock KeyBinds", category = "Misc")]
    public static void ToggleLockKeyBinds()
    {
        DebugMod.KeyBindLock = !DebugMod.KeyBindLock;
        DebugMod.LogConsole($"{(DebugMod.KeyBindLock ? "Removing" : "Adding")} the ability to use keybinds");
    }
}