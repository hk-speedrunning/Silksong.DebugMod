using System;
using DebugMod.MonoBehaviours;
using GlobalEnums;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DebugMod
{
    public static partial class BindableFunctions
    {
        [BindableMethod(name = "Increase Needle Damage", category = "Gameplay Altering")]
        public static void IncreaseNeedleDamage()
        {
            if (PlayerData.instance.nailDamage == 0)
            {
                PlayerData.instance.nailUpgrades = 0;
                DebugMod.extraNailDamage = 0;
                Console.AddLine("Resetting needle damage to 5");
            }
            else if (PlayerData.instance.nailUpgrades == 4 || DebugMod.extraNailDamage < 0)
            {
                DebugMod.extraNailDamage += 4;
                Console.AddLine("Adding 4 extra needle damage");
            }
            else
            {
                PlayerData.instance.nailUpgrades++;
                Console.AddLine("Adding needle upgrade");
            }

            PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
        }

        [BindableMethod(name = "Decrease Needle Damage", category = "Gameplay Altering")]
        public static void DecreaseNeedleDamage()
        {
            if (PlayerData.instance.nailUpgrades == 0 || DebugMod.extraNailDamage > 0)
            {
                DebugMod.extraNailDamage -= 4;
                if (DebugMod.extraNailDamage < -5)
                {
                    DebugMod.extraNailDamage = -5;
                    Console.AddLine("Setting needle damage to 0");
                }
                else
                {
                    Console.AddLine("Reducing nail damage by 4");
                }
            }
            else
            {
                PlayerData.instance.nailUpgrades--;
                Console.AddLine("Removing needle upgrade");
            }

            PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
        }
        
        [BindableMethod(name = "Decrease Timescale", category = "Gameplay Altering")]
        public static void TimescaleDown()
        {
            //This needs to be added because the game sets timescale to 0 when paused to pause the game if this is changed to a 
            //non-zero value, the game continues to play even tho the pause menu is up which is scuffed so this makes it less skuffed
            if (DebugMod.GM.IsGamePaused())
            {
                Console.AddLine("Cannot change timescale when paused");
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
            Console.AddLine("New TimeScale value: " + DebugMod.CurrentTimeScale + " Old value: " + oldScale);

        }

        [BindableMethod(name = "Increase Timescale", category = "Gameplay Altering")]
        public static void TimescaleUp()
        {
            if (DebugMod.GM.IsGamePaused())
            {
                Console.AddLine("Cannot change timescale when paused");
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
            Console.AddLine("New TimeScale value: " + DebugMod.CurrentTimeScale + " Old value: " + oldScale);
        }

        [BindableMethod(name = "Freeze Game", category = "Gameplay Altering")]
        public static void PauseGameNoUI()
        {
            DebugMod.PauseGameNoUIActive = !DebugMod.PauseGameNoUIActive;

            if (DebugMod.PauseGameNoUIActive)
            {
                Time.timeScale = 0;
                GameCameras.instance.StopCameraShake();
                SetAlwaysShowCursor();
                Console.AddLine("Game was Frozen");
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
                
                Console.AddLine("Game was Unfrozen");
            }
        }

        [BindableMethod(name = "Reset settings", category = "Gameplay Altering")]
        public static void Reset()
        {
            var pd = PlayerData.instance;
            var HC = HeroController.instance;
            var GC = GameCameras.instance;
            
            //nail damage
            DebugMod.extraNailDamage = 0;
            PlayerData.instance.nailUpgrades = 0;
            PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");

            //Hero Light
            GameObject gameObject = DebugMod.RefKnight.transform.Find("HeroLight").gameObject;
            Color color = gameObject.GetComponent<SpriteRenderer>().color;
            color.a = 0.7f;
            gameObject.GetComponent<SpriteRenderer>().color = color;
            
            //HUD
            // if (!GC.hudCanvas.gameObject.activeInHierarchy) 
            //     GC.hudCanvas.gameObject.SetActive(true);
            
            //Hide Hero
            tk2dSprite component = DebugMod.RefKnight.GetComponent<tk2dSprite>();
            color = component.color;  color.a = 1f;
            component.color = color;

            //rest all is self explanatory
            if (GameManager.instance.GetComponent<TimeScale>() != null)
            {
                Object.Destroy(GameManager.instance.gameObject.GetComponent<TimeScale>());
            }
            GC.tk2dCam.ZoomFactor = 1f;
            HC.vignette.enabled = false;
            EnemiesPanel.hpBars = false;
            pd.infiniteAirJump=false;
            DebugMod.infiniteSilk = false;
            DebugMod.infiniteHP = false; 
            pd.isInvincible=false; 
            DebugMod.noclip=false;
        }
    }
}