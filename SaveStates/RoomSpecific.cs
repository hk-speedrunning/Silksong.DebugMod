using DebugMod.Helpers;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using TeamCherry.NestedFadeGroup;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace DebugMod.SaveStates;

[HarmonyPatch]
public static class RoomSpecific
{
    #region roomspecifics
    internal static string SaveRoomSpecific(string scene)
    {
        if (scene == "Cog_Dancers")
        {
            return SaveCogworkDancers();
        }

        if (scene == "Memory_Ant_Queen")
        {
            return SaveKarmelita();
        }

        if (scene == "Memory_Red")
        {
            return SaveRedMemory();
        }

        foreach (BattleScene battleScene in Object.FindObjectsByType<BattleScene>(FindObjectsSortMode.None))
        {
            if (battleScene.started)
            {
                return SaveBattleScene(battleScene);
            }
        }

        return "";
    }

    // Allow skipping phases
    internal static string SaveCogworkDancers()
    {
        PlayMakerFSM fsm = Utils.FindFSM("Dancer Control", "Control");

        int phase = fsm.FsmVariables.FindFsmInt("Phase").Value;
        if (phase > 1)
        {
            return phase.ToString();
        }

        return "";
    }

    // Allow skipping the arena
    internal static string SaveKarmelita()
    {
        BattleScene battleScene = Object.FindAnyObjectByType<BattleScene>();

        if (battleScene.completed)
        {
            return "SkipArena";
        }

        return "";
    }

    // Red memory has four parts which each get activated when you reach the preceeding cutscene
    internal static string SaveRedMemory()
    {
        GameObject sceneryRoot = Utils.FindGameObjectByPath("Scenery Groups");

        if (sceneryRoot.FindChildObject("End Scenery").activeSelf)
        {
            return "3";
        }
        else if (sceneryRoot.FindChildObject("Hive Scenery").activeSelf)
        {
            return "2";
        }
        else if (sceneryRoot.FindChildObject("Deepnest Scenery").activeSelf)
        {
            return "1";
        }

        // Assume first section
        return "0";
    }

    // Allow loading into the middle of an arena battle
    internal static string SaveBattleScene(BattleScene battleScene)
    {
        return $"BattleScene|{battleScene.gameObject.name}|{battleScene.currentWave}";
    }

    internal static IEnumerator DoRoomSpecific(string scene, string options)
    {
        if (scene == "Cog_Dancers")
        {
            DoCogworkDancers(int.Parse(options));
            yield break;
        }

        if (scene == "Memory_Ant_Queen")
        {
            DoKarmelita();
            yield break;
        }

        if (scene == "Memory_Red")
        {
            DoRedMemory(options);
            yield break;
        }

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

    private static void ModifyFSM(string goName, string fsmName, Action<PlayMakerFSM> action)
    {
        IEnumerator Routine()
        {
            // Needed if the scene was already active before loading the savestate, not sure why
            yield return new WaitUntil(() => !GameManager.instance.isLoading);

            PlayMakerFSM fsm = Utils.FindFSM(goName, fsmName);
            action(fsm);
        }

        DebugMod.instance.StartCoroutine(Routine());
    }

    internal static void DoCogworkDancers(int phase)
    {
        ModifyFSM("Dancer Control", "Control", fsm =>
        {
            FsmState state = fsm.GetState("Beat Start");
            FsmTransition transition = state.GetTransition(0);

            string target = phase switch
            {
                2 => "Set Phase 2",
                3 => "Set Phase 3",
                4 => "Dancer Death",
                _ => transition.ToState
            };

            transition.ToState = target;
            transition.ToFsmState = fsm.GetState(transition.ToState);

            if (phase == 4)
            {
                // Dancers will swap their places during the phase, so the surviving dancer
                // could end up on either side even if you always target the same dancer at the start
                string dancerToKill = Random.value < 0.5f ? "Dancer A" : "Dancer B";
                fsm.FsmVariables.GetFsmGameObject("Stunned Dancer").Value = GameObject.Find(dancerToKill);
            }
        });

        ModifyFSM("Dancer Control", "Music Control", fsm =>
        {
            FsmState phase1 = fsm.GetState("P1 Music");
            List<FsmStateAction> initActions = phase1.Actions.ToList().GetRange(0, 2);

            string stateName = phase switch
            {
                2 => "P2 Music",
                3 => "P3 Music",
                4 => "P4 Music",
                _ => ""
            };

            FsmState state = fsm.GetState(stateName);
            List<FsmStateAction> actions = state.Actions.ToList();
            actions.InsertRange(0, initActions);
            state.Actions = actions.ToArray();
        });

        void ModifyDancer(PlayMakerFSM fsm)
        {
            fsm.FsmVariables.GetFsmBool("Did First Windup").Value = true;
            fsm.FsmVariables.GetFsmBool("Did Roar").Value = true;
        }

        ModifyFSM("Dancer A", "Control", ModifyDancer);
        ModifyFSM("Dancer B", "Control", ModifyDancer);
    }

    internal static void DoKarmelita()
    {
        // Based off randomscorp's Skip Karmelita Arena mod

        ModifyFSM("Hunter Queen Boss", "Control", fsm =>
        {
            FsmState state = fsm.GetState("Battle Dance");

            foreach (FsmStateAction action in state.Actions)
            {
                if (action is SendMessage || action is Wait)
                {
                    action.Enabled = false;
                }
            }

            foreach (FsmTransition transition in state.Transitions)
            {
                if (transition.EventName == "BATTLE END")
                {
                    transition.FsmEvent = FsmEvent.Finished;
                }
            }
        });
    }

    internal static void DoRedMemory(string options)
    {
        int stage = int.Parse(options);

        IEnumerator Routine()
        {
            yield return new WaitUntil(() => !GameManager.instance.isLoading);

            GameObject sceneryRoot = Utils.FindGameObjectByPath("Scenery Groups");

            string childName = stage switch
            {
                0 => "Entry Scenery",
                1 => "Deepnest Scenery",
                2 => "Hive Scenery",
                3 => "End Scenery",
                _ => null
            };

            sceneryRoot.FindChildObject(childName).SetActive(true);

            static void FadeOutFadeGroup(string path)
            {
                Utils.FindGameObjectByPath(path).GetComponent<NestedFadeGroup>().FadeTo(0f, 0f);
            }

            FadeOutFadeGroup("Memory Control/Weaver_Memory_Lighting");
            FadeOutFadeGroup("Memory Control/Beast_Memory_Lighting");
            FadeOutFadeGroup("Memory Control/Hive_Memory_Lighting");
        }

        DebugMod.instance.StartCoroutine(Routine());
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
        CodeMatcher matcher = new(instructions);

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
            case "Hang_04":
            case "Hang_06":
                // Hornet always turns towards this object when the roar plays, but for HHA its position
                // depends on previous roars done in other rooms. By default it will be at x=0 (face left),
                // but in practice Hornet will always face right when playing casually or in a speedrun.
                // TODO: if this can happen in other places, consider tracking it in savestates
                GameCameras.instance.gameObject.FindChildObject("Roar Wave Emitter").transform.SetPositionX(100f);
                break;
            case "Memory_Silk_Heart_BellBeast":
            case "Memory_Silk_Heart_LaceTower":
            case "Memory_Silk_Heart_WardBoss":
            case "Memory_Needolin":
            case "Memory_Red":
                GameManager.instance.StartCoroutine(MemoryFixes());
                break;
        }

        yield break;
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

    #region backwardscompat
    // Fill in missing roomspecifics / change format if needed and possible
    internal static void BackwardsCompat(string scene, ref string options)
    {
        switch (scene)
        {
            case "Memory_Red":
                // Savestate was most likely created in the first section
                if (string.IsNullOrEmpty(options))
                {
                    options = "0";
                }
                break;
        }
    }
    #endregion
}