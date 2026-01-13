using DebugMod.MonoBehaviours;
using DebugMod.UI;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DebugMod;

[HarmonyPatch]
public static partial class BindableFunctions
{
    [BindableMethod(name = "Toggle Act 3", category = "Misc")]
    public static void ToggleAct3()
    {
        PlayerData.instance.blackThreadWorld = !PlayerData.instance.blackThreadWorld;
        DebugMod.LogConsole("Act 3 world is now " + (PlayerData.instance.blackThreadWorld ? "enabled" : "disabled"));
    }

    [BindableMethod(name = "Set Hazard Respawn", category = "Misc")]
    public static void SetHazardRespawn()
    {
        Vector3 manualRespawn = DebugMod.RefKnight.transform.position;
        HeroController.instance.SetHazardRespawn(manualRespawn, false);
        DebugMod.LogConsole("Manual respawn point on this map set to" + manualRespawn);
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

    [BindableMethod(name = "Damage Self", category = "Misc")]
    public static void SelfDamage()
    {
        if (PlayerData.instance.health <= 0)
        {
            DebugMod.LogConsole("Cannot damage self: health <= 0");
        }
        else if (HeroController.instance.cState.dead)
        {
            DebugMod.LogConsole("Cannot damage self: player is dead");
        }
        else if (!GameManager.instance.IsGameplayScene())
        {
            DebugMod.LogConsole("Cannot damage self: not a gameplay scene");
        }
        else if (HeroController.instance.cState.recoiling)
        {
            DebugMod.LogConsole("Cannot damage self: player is recoiling");
        }
        else if (HeroController.instance.cState.invulnerable)
        {
            DebugMod.LogConsole("Cannot damage self: player is invulnerable");
        }
        else
        {
            HeroController.instance.DamageSelf(1);
            DebugMod.LogConsole("Attempting self damage");
        }
    }

    [BindableMethod(name = "Kill Self", category = "Misc")]
    public static void KillSelf()
    {
        if (!HeroController.instance.cState.dead && !HeroController.instance.cState.transitioning)
        {
            HeroController.instance.StartCoroutine(HeroController.instance.Die(false, false));
            DebugMod.LogConsole("Killed player");
        }
    }

    [BindableMethod(name = "Break Cocoon", category = "Misc")]
    public static void BreakCocoon()
    {
        HeroController.instance?.CocoonBroken();
        EventRegister.SendEvent("BREAK HERO CORPSE");
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

    [BindableMethod(name = "Lock Keybinds", category = "Misc")]
    public static void ToggleLockKeyBinds()
    {
        DebugMod.KeyBindLock = !DebugMod.KeyBindLock;
        DebugMod.LogConsole($"{(DebugMod.KeyBindLock ? "Removing" : "Adding")} the ability to use keybinds");
    }

    [BindableMethod(name = "Reset Cheats", category = "Misc")]
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

        //Hide Hero
        tk2dSprite component = DebugMod.RefKnight.GetComponent<tk2dSprite>();
        color = component.color; color.a = 1f;
        component.color = color;

        //rest all is self explanatory
        TimeScale.Reset();
        GC.tk2dCam.ZoomFactor = 1f;
        HC.vignette.enabled = false;
        EnemiesPanel.hpBars = false;
        pd.infiniteAirJump = false;
        DebugMod.infiniteSilk = false;
        DebugMod.infiniteHP = false;
        pd.isInvincible = false;
        DebugMod.noclip = false;
    }
}