using System;
using System.Collections;
using System.Reflection;
using DebugMod.MonoBehaviours;
using UnityEngine;

namespace DebugMod
{
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
                    Console.AddLine("Forcing Pause Menu because pause is disabled");
                }
                else
                {
                    Console.AddLine("Game does not report that Pause is disabled, requesting it normally.");
                    UIManager.instance.TogglePauseGame();
                }
            }
            catch (Exception e)
            {
                Console.AddLine("Error while attempting to pause, check ModLog.txt");
                DebugMod.instance.Log("Error while attempting force pause:\n" + e);
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
                    Console.AddLine("Closing Pause Menu and respawning...");
                    return;
                }

                if (UIManager.instance.uiState.ToString() == "PLAYING")
                {
                    HeroController.instance.RelinquishControl();
                    GameManager.instance.HazardRespawn();
                    HeroController.instance.RegainControl();
                    Console.AddLine("Respawn signal sent");
                    return;
                }

                Console.AddLine("Respawn requested in some weird conditions, abort, ABORT");
            }
        }

        [BindableMethod(name = "Set Respawn", category = "Misc")]
        public static void SetHazardRespawn()
        {
            Vector3 manualRespawn = DebugMod.RefKnight.transform.position;
            HeroController.instance.SetHazardRespawn(manualRespawn, false);
            Console.AddLine("Manual respawn point on this map set to" + manualRespawn);
        }

        [BindableMethod(name = "Toggle Act 3", category = "Misc")]
        public static void ToggleAct3()
        {
            PlayerData.instance.blackThreadWorld = !PlayerData.instance.blackThreadWorld;
            Console.AddLine("Act 3 world is now " + (PlayerData.instance.blackThreadWorld ? "enabled" : "disabled"));
        }

        [BindableMethod(name = "Force Camera Follow", category = "Misc")]
        public static void ForceCameraFollow()
        {
            if (!DebugMod.cameraFollow)
            {
                Console.AddLine("Forcing camera follow");
                DebugMod.cameraFollow = true;
            }
            else
            {
                DebugMod.cameraFollow = false;
                cameraGameplayScene.SetValue(DebugMod.RefCamera, true);
                Console.AddLine("Returning camera to normal settings");
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

        /*
        internal static Action ClearSceneDataHook;

        [BindableMethod(name = "Refresh Scene Data", category = "Misc")]
        public static void HookResetCurrentScene()
        {
            string scene = GameManager.instance.sceneName;
            Console.AddLine("Clearing scene data from this scene, re-enter scene or warp");
            ClearSceneDataHook?.Invoke();
            On.GameManager.SaveLevelState += GameManager_SaveLevelState;
            ClearSceneDataHook = () => On.GameManager.SaveLevelState -= GameManager_SaveLevelState;

            void GameManager_SaveLevelState(On.GameManager.orig_SaveLevelState orig, GameManager self)
            {
                orig(self);
                SceneData.instance.persistentBoolItems =
                    SceneData.instance.persistentBoolItems.Where(x => x.sceneName != scene).ToList();
                SceneData.instance.persistentIntItems =
                    SceneData.instance.persistentIntItems.Where(x => x.sceneName != scene).ToList();
                SceneData.instance.geoRocks = SceneData.instance.geoRocks.Where(x => x.sceneName != scene).ToList();

                On.GameManager.SaveLevelState -= GameManager_SaveLevelState;
                ClearSceneDataHook = null;
            }
        }

        [BindableMethod(name = "Block Scene Data Changes", category = "Misc")]
        public static void HookBlockCurrentSceneChanges()
        {
            Console.AddLine("Scene data changes made since entering this scene will not be saved");
            ClearSceneDataHook?.Invoke();
            On.GameManager.SaveLevelState += GameManager_BlockLevelChanges;
            ClearSceneDataHook = () => On.GameManager.SaveLevelState -= GameManager_BlockLevelChanges;

            void GameManager_BlockLevelChanges(On.GameManager.orig_SaveLevelState orig, GameManager self)
            {
                On.GameManager.SaveLevelState -= GameManager_BlockLevelChanges;
                ClearSceneDataHook = null;
            }
        }
        */

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
                Console.AddLine("Starting frame by frame advance on keybind press");
            }
            else
            {
                DebugMod.CurrentTimeScale = TimeScaleDuringFrameAdvance;
                Time.timeScale = DebugMod.CurrentTimeScale;
                Console.AddLine("Stopping frame by frame advance on keybind press");
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
            Time.timeScale = 1f;
            yield return new WaitForFixedUpdate();

            Time.timeScale = 0;
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
            Console.AddLine($"{(DebugMod.KeyBindLock ? "Removing" : "Adding")} the ability to use keybinds");
        }

    }
}