using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DebugMod.MonoBehaviours;

[HarmonyPatch]
public static class TimeScale
{
    static TimeManager.TimeControlInstance tFreeze = new(1, TimeManager.TimeControlInstance.Type.Multiplicative);
    static TimeManager.TimeControlInstance tScale = new(1, TimeManager.TimeControlInstance.Type.Multiplicative);
    static float CustomTimeScaleIncludingFreeze => CustomTimeScale * tFreeze.TimeScale;

    /// <summary>Sets a custom time scale multiplier for the game. This is relative to the game's existing,
    /// time scale, which means it scales the time of all gameplay, rather than setting <c>Time.timeScale</c> directly. 
    /// Note: use <c>TimeScale.Frozen</c> for freezing/unfreezing</summary>
    public static float CustomTimeScale
    {
        get => tScale.timeScale;
        set
        {
            float prevValue = CustomTimeScale;
            if (value == prevValue || value < 0f)
                return; // unchanged
            
            tScale.TimeScale = value;
            string adjustmentText = value > prevValue ? "increased" : "decreased";
            DebugMod.LogConsole($"Timescale {adjustmentText}: {CustomTimeScale:0.0} (was {prevValue:0.0})");
            CheckHookRequirement();
        }
    }
    static bool IsScalingTime => Mathf.Approximately(CustomTimeScaleIncludingFreeze, 1f) == false;
    static bool IsUsingSpedUpTime => CustomTimeScaleIncludingFreeze > 1f;

    /// <summary>Will freeze time in the game, independently of any other time scale settings</summary>
    public static bool Frozen
    {
        get => tFreeze.TimeScale == 0f;
        set
        {
            if (value == Frozen)
                return; // no change
            tFreeze.TimeScale = value ? 0f : 1f;

            bool cameraExistsAndIsntPaused = GameManager.UnsafeInstance != null && GameCameras.instance != null && GameManager.instance.isPaused == false;

            if (value) // freeze
            {
                if (cameraExistsAndIsntPaused)
                    GameCameras.instance.StopCameraShake();
            }
            else // unfreeze
            {
                if (cameraExistsAndIsntPaused)
                    GameCameras.instance.ResumeCameraShake();
            }

            CheckHookRequirement();
        }
    }

    /// <summary>Unfreezes and resets timescale to 1</summary>
    public static void Reset()
    {
        tFreeze?.TimeScale = 1f;
        tScale?.TimeScale = 1f;
    }

    internal static void Initialize()
    {
        tFreeze ??= new(1, TimeManager.TimeControlInstance.Type.Multiplicative);
        tScale ??= new(1, TimeManager.TimeControlInstance.Type.Multiplicative);
    }

    internal static void Release()
    {
        tFreeze?.Release();
        tScale?.Release();
    }

    // This allows DebugMod to control timescale independently of Silksong,
    // and prevents fighting over who wants to control Time.timeScale.
    //
    // One caveat is that CheatManager.IsCheatsEnabled needs to return true during UpdateTimeScale,
    // in order for it to allow timescales > 1
    static bool overrideIsCheatsEnabled = false;

    [HarmonyPrefix] [HarmonyPatch(typeof(TimeManager), "UpdateTimeScale")]
    public static void UpdateTimeScalePrefix() => overrideIsCheatsEnabled = IsUsingSpedUpTime;

    [HarmonyPostfix] [HarmonyPatch(typeof(TimeManager), "UpdateTimeScale")]
    public static void UpdateTimeScalePostfix() => overrideIsCheatsEnabled = false;

    [HarmonyPostfix] [HarmonyPatch(typeof(CheatManager), "IsCheatsEnabled")] [HarmonyPatch(MethodType.Getter)]
    public static void IsCheatsEnabledPostfix(ref bool __result)
    {
        if (overrideIsCheatsEnabled)
            __result = true;
    }

    // FreezeMoment and SetTimeScale (with duration) does not take time scale into account,
    // so this will force those method to scale unscaledDeltaTime
    // by custom time scale we're using
    private static bool hooksActive = false;
    private static ILHook[] _coroutineHooks;

    static void CheckHookRequirement()
    {
        if (hooksActive == false && IsScalingTime)
            HookFreezeCoroutines();
        else if (hooksActive && IsScalingTime == false)
            UnhookFreezeCoroutines();
    }

    static void HookFreezeCoroutines()
    {
        hooksActive = true;
        _coroutineHooks = new ILHook[FreezeCoroutines.Length];
        foreach ((MethodInfo coro, int idx) in FreezeCoroutines.Select((mi, idx) => (mi, idx)))
            _coroutineHooks[idx] = new ILHook(coro, ScaleFreeze);
    }

    static void UnhookFreezeCoroutines()
    {
        if (hooksActive == false)
            return;
        hooksActive = false;
        foreach (ILHook hook in _coroutineHooks)
            hook.Dispose();
    }

    private static readonly MethodInfo[] FreezeCoroutines = (
        from method in typeof(GameManager).GetMethods()
        where method.Name.StartsWith("FreezeMoment") || method.Name.StartsWith("SetTimeScale")
        where method.ReturnType == typeof(IEnumerator)
        select method.GetCustomAttribute<IteratorStateMachineAttribute>()
        into attr
        select attr.StateMachineType
        into type
        select type.GetMethod("MoveNext", BindingFlags.NonPublic | BindingFlags.Instance)
    ).ToArray();


    private static void ScaleFreeze(ILContext il)
    {
        var cursor = new ILCursor(il);

        cursor.GotoNext
        (
            MoveType.After,
            x => x.MatchLdfld(out _),
            x => x.MatchCall<Time>("get_unscaledDeltaTime")
        );

        cursor.EmitDelegate<Func<float>>(() => CustomTimeScaleIncludingFreeze);

        cursor.Emit(OpCodes.Mul);
    }

}