using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
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

        return "";
    }

    // Allow loading into the middle of an arena battle
    internal static string SaveBattleScene(BattleScene battleScene)
    {
        return $"BattleScene|{battleScene.gameObject.name}|{battleScene.currentWave}";
    }

    internal static PlayMakerFSM FindFSM(string goName, string fsmName)
    {
        return PlayMakerFSM.FindFsmOnGameObject(GameObject.Find(goName), fsmName);
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
        FindFSM("Memory Control", "Memory Control").SendEvent("DOOR GET UP FINISHED");

        // TODO: find a way to remove the 1 second wait
        yield return new WaitUntil(() => GameCameras.instance.hudCanvasSlideOut.gameObject);
        yield return new WaitForSecondsRealtime(1f);
        GameCameras.instance.hudCanvasSlideOut.SendEvent("OUT");
    }
    #endregion
}