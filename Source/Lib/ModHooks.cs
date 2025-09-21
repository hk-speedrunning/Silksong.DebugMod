using System;
using System.Collections;
using HarmonyLib;

namespace DebugMod;

// Implementation of ModHooks via patches
[HarmonyPatch]
internal static class ModHooks
{
    #region AfterSavegameLoadHook
    public static event Action<SaveGameData> AfterSavegameLoadHook;

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.SetLoadedGameData))]
    [HarmonyPostfix]
    private static void SetLoadedGameData(SaveGameData saveGameData)
    {
        AfterSavegameLoadHook?.Invoke(saveGameData);
    }
    #endregion

    #region ApplicationQuitHook
    public static event Action ApplicationQuitHook;

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.OnApplicationQuit))]
    [HarmonyPostfix]
    private static void OnApplicationQuit()
    {
        ApplicationQuitHook?.Invoke();
    }
    #endregion

    #region BeforeSceneLoadHook
    public static event Func<string, string> BeforeSceneLoadHook;

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.BeginSceneTransition))]
    [HarmonyPrefix]
    private static void BeginSceneTransition(GameManager.SceneLoadInfo info)
    {
        if (BeforeSceneLoadHook != null) info.SceneName = BeforeSceneLoadHook.Invoke(info.SceneName);
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.LoadScene))]
    [HarmonyPrefix]
    private static void LoadScene(ref string destScene)
    {
        if (BeforeSceneLoadHook != null) destScene = BeforeSceneLoadHook.Invoke(destScene);
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.LoadSceneAdditive))]
    [HarmonyPrefix]
    private static void LoadSceneAdditive(ref string destScene)
    {
        if (BeforeSceneLoadHook != null) destScene = BeforeSceneLoadHook.Invoke(destScene);
    }
    #endregion

    #region NewGameHook
    public static event Action NewGameHook;

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.LoadFirstScene))]
    [HarmonyPostfix]
    private static IEnumerator LoadFirstScene(IEnumerator orig)
    {
        yield return orig;
        NewGameHook?.Invoke();
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.OnWillActivateFirstLevel))]
    [HarmonyPostfix]
    private static void OnWillActivateFirstLevel()
    {
        NewGameHook?.Invoke();
    }
    #endregion

    #region TakeHealthHook
    public static event Func<int, int> TakeHealthHook;

    [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.TakeHealth))]
    private static void TakeHealth(ref int amount)
    {
        if (TakeHealthHook != null)
        {
            amount = TakeHealthHook.Invoke(amount);
        }
    }
    #endregion
}