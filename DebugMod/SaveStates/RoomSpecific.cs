using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace DebugMod.SaveStates;

[HarmonyPatch]
public static class RoomSpecific
{
    private static readonly List<string> scenesWithSpecialHandling =
    [
        "Bone_East_08" // Fourth Chorus room
    ];

    internal static string SaveRoomSpecific(string scene)
    {
        foreach (BattleScene battleScene in Object.FindObjectsByType<BattleScene>(FindObjectsSortMode.None))
        {
            if (battleScene.started)
            {
                return SaveBattleScene(battleScene);
            }
        }

        if (scenesWithSpecialHandling.Contains(scene))
        {
            return scene;
        }

        return "";
    }

    // Allow loading into the middle of an arena battle
    internal static string SaveBattleScene(BattleScene battleScene)
    {
        return $"BattleScene|{battleScene.gameObject.name}|{battleScene.currentWave}";
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

        switch (scene)
        {
            case "Bone_East_08":
                // Wait for lava platforms to load in so we don't fall through them
                yield return new WaitUntil(() => !GameManager.instance.isLoading);
                break;
            default:
                Console.AddLine("No Room Specific Function Found In: " + scene);
                break;
        }
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
}