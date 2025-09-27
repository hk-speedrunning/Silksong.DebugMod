using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace DebugMod.MonoBehaviours
{
    [HarmonyPatch]
    public class TimeScale:MonoBehaviour
    {
        private static bool hooksActive;

        public void Awake()
        {
            DebugMod.TimeScaleActive = true;
            Time.timeScale = DebugMod.CurrentTimeScale;

            hooksActive = true;

            _coroutineHooks = new ILHook[FreezeCoroutines.Length];

            foreach ((MethodInfo coro, int idx) in FreezeCoroutines.Select((mi, idx) => (mi, idx)))
            {
                _coroutineHooks[idx] = new ILHook(coro, ScaleFreeze);
            }
            
        }
        
        public void OnDestroy()
        {
            
            foreach (ILHook hook in _coroutineHooks)
                hook.Dispose();

            Time.timeScale = 1;
            DebugMod.CurrentTimeScale = 1f;

            hooksActive = false;
        }
         
        private readonly MethodInfo[] FreezeCoroutines = (
            from method in typeof(GameManager).GetMethods()
            where method.Name.StartsWith("FreezeMoment")
            where method.ReturnType == typeof(IEnumerator)
            select method.GetCustomAttribute<IteratorStateMachineAttribute>() into attr
            select attr.StateMachineType into type
            select type.GetMethod("MoveNext", BindingFlags.NonPublic | BindingFlags.Instance)
        ).ToArray();

        private ILHook[] _coroutineHooks;
        private void ScaleFreeze(ILContext il)
        {
            var cursor = new ILCursor(il);

            cursor.GotoNext
            (
                MoveType.After,
                x => x.MatchLdfld(out _),
                x => x.MatchCall<Time>("get_unscaledDeltaTime")
            );

            cursor.EmitDelegate<Func<float>>(() => DebugMod.CurrentTimeScale);

            cursor.Emit(OpCodes.Mul);
        }

        [HarmonyPatch(typeof(QuitToMenu), nameof(QuitToMenu.Start))]
        [HarmonyPostfix]
        private static IEnumerator QuitToMenu_Start(IEnumerator orig)
        {
            bool shouldRun = hooksActive;
            yield return orig;

            if (shouldRun)
            {
                TimeManager.TimeScale = DebugMod.CurrentTimeScale;
            }
        }

        [HarmonyPatch(typeof(GameManager), nameof(GameManager.SetTimeScale), typeof(float))]
        [HarmonyPrefix]
        private static bool GameManager_SetTimeScale_1(GameManager __instance, float newTimeScale)
        {
            if (!hooksActive)
            {
                return true;
            }

            float temp = newTimeScale;
            if (__instance.timeSlowedCount > 1)
                temp = Math.Min(temp, TimeManager.TimeScale);
            
            TimeManager.TimeScale = (temp <= 0.01f ? 0f : temp) * DebugMod.CurrentTimeScale;
            return false;
        }
    }
}