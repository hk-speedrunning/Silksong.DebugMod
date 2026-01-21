using DebugMod.Helpers;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using TeamCherry.NestedFadeGroup;
using UnityEngine;

namespace DebugMod.SaveStates;

[HarmonyPatch]
public static class RoomSpecific
{
    #region roomspecifics
    internal static string SaveRoomSpecific(string scene)
    {
        foreach (BattleScene battleScene in Object.FindObjectsByType<BattleScene>(FindObjectsSortMode.None))
        {
            if (battleScene.started)
            {
                return SaveBattleScene(battleScene);
            }
        }

        if (scene == "Memory_Red")
        {
            return SaveRedMemory();
        }

        return "";
    }

    // Allow loading into the middle of an arena battle
    internal static string SaveBattleScene(BattleScene battleScene)
    {
        return $"BattleScene|{battleScene.gameObject.name}|{battleScene.currentWave}";
    }

    // Red memory has four parts which each get activated when you reach the preceeding cutscene
    internal static string SaveRedMemory()
    {
        GameObject sceneryRoot = Utils.FindGameObjectByPath("Scenery Groups");

        if (Utils.FindChildObject(sceneryRoot, "End Scenery").activeSelf)
        {
            return "3";
        }
        else if (Utils.FindChildObject(sceneryRoot, "Hive Scenery").activeSelf)
        {
            return "2";
        }
        else if (Utils.FindChildObject(sceneryRoot, "Deepnest Scenery").activeSelf)
        {
            return "1";
        }
        else if (Utils.FindChildObject(sceneryRoot, "Entry Scenery").activeSelf)
        {
            return "0";
        }

        return "";
    }

    internal static IEnumerator DoRoomSpecific(string scene, string options)
    {
        string[] parts = options.Split('|');
        switch (parts[0])
        {
            case "BattleScene":
                DoBattleScene(scene, parts[1], int.Parse(parts[2]));
                yield break;
        }

        if (scene == "Memory_Red")
        {
            DoRedMemory(options);
            yield break;
        }

        if (options == scene)
        {
            // Legacy way of running generic fixes, no need to warn
            yield break;
        }

        DebugMod.LogConsole($"Invalid room-specific options for {scene}: {options}");
    }

    internal static void DoBattleScene(string scene, string objectName, int wave)
    {
        void StartBattle()
        {
            GameManager.instance.OnLoadedBoss -= StartBattle;

            BattleScene battleScene = GameObject.Find(objectName).GetComponent<BattleScene>();
            battleScene.currentWave = wave;

            // Sometimes the FSM that calls StartBattle() does a (not just first time) roar/scream that needs to be played first
            switch (scene)
            {
                case "Library_02":
                    PlayMakerFSM.BroadcastEvent("DO BATTLE START");
                    break;
                default:
                    battleScene.StartBattle();
                    break;
            }
        }

        if (SceneAdditiveLoadConditional.ShouldLoadBoss)
        {
            GameManager.instance.OnLoadedBoss += StartBattle;
        }
        else
        {
            StartBattle();
        }
    }

    [HarmonyPatch(typeof(BattleScene), nameof(BattleScene.DoStartBattle), MethodType.Enumerator)]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> DoStartBattle(IEnumerable<CodeInstruction> instructions)
    {
        CodeMatcher matcher = new CodeMatcher(instructions);

        // StartWave(0) -> StartWave(currentWave)
        matcher.MatchForward(false,
            new CodeMatch(OpCodes.Ldc_I4_0),
            new CodeMatch(OpCodes.Call, typeof(BattleScene).GetMethod(nameof(BattleScene.StartWave))));
        matcher.RemoveInstruction();
        matcher.InsertAndAdvance(
            new CodeInstruction(OpCodes.Ldloc_1),
            new CodeInstruction(OpCodes.Ldfld, typeof(BattleScene).GetField(nameof(BattleScene.currentWave))));

        return matcher.InstructionEnumeration();
    }

    internal static void DoRedMemory(string options)
    {
        int stage = int.Parse(options);

        GameObject sceneryRoot = Utils.FindGameObjectByPath("Scenery Groups");

        string childName = stage switch
        {
            0 => "Entry Scenery",
            1 => "Deepnest Scenery",
            2 => "Hive Scenery",
            3 => "End Scenery",
            _ => null
        };

        Utils.FindChildObject(sceneryRoot, childName).SetActive(true);

        static void FadeOutFadeGroup(string path)
        {
            Utils.FindGameObjectByPath(path).GetComponent<NestedFadeGroup>().FadeTo(0f, 0f);
        }

        FadeOutFadeGroup("Memory Control/Weaver_Memory_Lighting");
        FadeOutFadeGroup("Memory Control/Beast_Memory_Lighting");
        FadeOutFadeGroup("Memory Control/Hive_Memory_Lighting");
    }
    #endregion

    #region genericfixes
    // Fixes specific to a room that do not require additional options
    internal static IEnumerator DoGenericFixes(string scene)
    {
        switch (scene)
        {
            case "Bone_East_08":
                // Wait for lava platforms to load in so we don't fall through them
                yield return new WaitUntil(() => !GameManager.instance.isLoading);
                break;
            case "Memory_Silk_Heart_BellBeast":
            case "Memory_Silk_Heart_LaceTower":
            case "Memory_Silk_Heart_WardBoss":
            case "Memory_Needolin":
            case "Memory_Red":
                GameManager.instance.StartCoroutine(MemoryFixes());
                break;
        }
    }

    internal static IEnumerator MemoryFixes()
    {
        HeroController.instance.SetSilkRegenBlocked(true);
        DarknessRegion.SetDarknessLevel(0);
        Utils.FindFSM("Memory Control", "Memory Control").SendEvent("DOOR GET UP FINISHED");

        // TODO: find a way to remove the 1 second wait
        yield return new WaitUntil(() => GameCameras.instance.hudCanvasSlideOut.gameObject);
        yield return new WaitForSecondsRealtime(1f);
        GameCameras.instance.hudCanvasSlideOut.SendEvent("OUT");
    }
    #endregion
}