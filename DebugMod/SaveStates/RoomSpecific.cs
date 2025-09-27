using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace DebugMod
{
    [HarmonyPatch]
    public static class RoomSpecific
    {
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

        internal static void DoRoomSpecific(string scene, string options)
        {
            string[] parts = options.Split('|');
            switch (parts[0])
            {
                case "BattleScene":
                    DoBattleScene(parts[1], int.Parse(parts[2]));
                    return;
            }

            switch (scene)
            {
                // TODO: add cases
                default:
                    Console.AddLine("No Room Specific Function Found In: " + scene);
                    break;
            }
        }

        internal static void DoBattleScene(string objectName, int wave)
        {
            void StartBattle()
            {
                GameManager.instance.OnLoadedBoss -= StartBattle;

                DebugMod.instance.LogDebug($"{objectName} {GameObject.Find(objectName)} {GameObject.FindWithTag("Battle Scene")}");
                BattleScene battleScene = GameObject.Find(objectName).GetComponent<BattleScene>();
                battleScene.currentWave = wave;
                battleScene.StartBattle();
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
}
