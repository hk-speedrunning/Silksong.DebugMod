using System;
using System.Collections;
using HarmonyLib;
using UnityEngine;

namespace DebugMod;

// Implementation of ModHooks via patches
[HarmonyPatch]
internal static class ModHooks
{
    #region AfterSavegameLoadHook
    public static event Action<SaveGameData> AfterSavegameLoadHook;

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.SetLoadedGameData))]
    [HarmonyPostfix]
    private static void GameManager_SetLoadedGameData(SaveGameData saveGameData)
    {
        AfterSavegameLoadHook?.Invoke(saveGameData);
    }
    #endregion

    #region ApplicationQuitHook
    public static event Action ApplicationQuitHook;

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.OnApplicationQuit))]
    [HarmonyPostfix]
    private static void GameManager_OnApplicationQuit()
    {
        ApplicationQuitHook?.Invoke();
    }
    #endregion

    #region BeforeSceneLoadHook
    public static event Func<string, string> BeforeSceneLoadHook;

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.BeginSceneTransition))]
    [HarmonyPrefix]
    private static void GameManager_BeginSceneTransition(GameManager.SceneLoadInfo info)
    {
        if (BeforeSceneLoadHook != null) info.SceneName = BeforeSceneLoadHook.Invoke(info.SceneName);
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.LoadScene))]
    [HarmonyPrefix]
    private static void GameManager_LoadScene(ref string destScene)
    {
        if (BeforeSceneLoadHook != null) destScene = BeforeSceneLoadHook.Invoke(destScene);
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.LoadSceneAdditive))]
    [HarmonyPrefix]
    private static void GameManager_LoadSceneAdditive(ref string destScene)
    {
        if (BeforeSceneLoadHook != null) destScene = BeforeSceneLoadHook.Invoke(destScene);
    }
    #endregion

    #region ColliderCreateHook
    public static event Action<GameObject> ColliderCreateHook;

    [HarmonyPatch(typeof(PlayMakerUnity2DProxy), nameof(PlayMakerUnity2DProxy.Start))]
    [HarmonyPostfix]
    private static void PlayMakerUnity2DProxy_Start(PlayMakerUnity2DProxy __instance)
    {
        ColliderCreateHook?.Invoke(__instance.gameObject);
    }
    #endregion

    #region FinishedLoadingModsHook
    private static event Action finishedLoadingModsHook;
    private static bool finishedLoadingModsInvoked;

    public static event Action FinishedLoadingModsHook
    {
        add
        {
            finishedLoadingModsHook += value;
            if (finishedLoadingModsInvoked)
            {
                value();
            }
        }
        remove
        {
            finishedLoadingModsHook -= value;
        }
    }

    // This is where the Hollow Knight modding API loads mods and where it would invoke this hook
    [HarmonyPatch(typeof(OnScreenDebugInfo), nameof(OnScreenDebugInfo.Awake))]
    [HarmonyPrefix]
    private static void OnScreenDebugInfo_Awake()
    {
        if (!finishedLoadingModsInvoked)
        {
            finishedLoadingModsHook?.Invoke();
        }
        finishedLoadingModsInvoked = true;
    }
    #endregion

    #region NewGameHook
    public static event Action NewGameHook;

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.LoadFirstScene))]
    [HarmonyPostfix]
    private static IEnumerator GameManager_LoadFirstScene(IEnumerator orig)
    {
        yield return orig;
        NewGameHook?.Invoke();
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.OnWillActivateFirstLevel))]
    [HarmonyPostfix]
    private static void GameManager_OnWillActivateFirstLevel()
    {
        NewGameHook?.Invoke();
    }
    #endregion

    #region TakeHealthHook
    public static event Func<int, int> TakeHealthHook;

    [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.TakeHealth))]
    private static void PlayerData_TakeHealth(ref int amount)
    {
        if (TakeHealthHook != null)
        {
            amount = TakeHealthHook.Invoke(amount);
        }
    }
    #endregion
}