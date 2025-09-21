using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HarmonyLib;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace DebugMod
{
    [HarmonyPatch]
    public static class SceneWatcher
    {
        private static List<LoadedSceneInfo> scenes;
        public static ReadOnlyCollection<LoadedSceneInfo> LoadedScenes => scenes.AsReadOnly();

        private static Dictionary<Scene, int> scenesWithManager;

        private static void AddScene(Scene scene, LoadSceneMode mode, bool checkSceneManager = true)
        {
            if (mode == LoadSceneMode.Single)
                scenes.Clear();

            LoadedSceneInfo d = new LoadedSceneInfo(scene.name, scene.name);
            scenes.Add(d);

            if (checkSceneManager && Object.FindObjectsOfType<CustomSceneManager>().Any(m => m.gameObject.scene == scene))
                scenesWithManager.Add(scene, d.id);
        }

        public static void Init()
        {
            scenesWithManager = new();
            scenes = new();

            for (int i = 0; i < USceneManager.sceneCount; i++)
                AddScene(USceneManager.GetSceneAt(i), LoadSceneMode.Additive, false);
            
            USceneManager.sceneLoaded += (scene, mode) => AddScene(scene, mode);
            USceneManager.sceneUnloaded += s => scenes.RemoveAt(scenes.FindIndex(d => d.name == s.name));
        }

        [HarmonyPatch(typeof(CustomSceneManager), nameof(CustomSceneManager.Start))]
        [HarmonyPostfix]
        private static void OnCustomSceneManagerStart(CustomSceneManager __instance)
        {
            if (!scenesWithManager.ContainsKey(__instance.gameObject.scene))
                return;

            int id = scenesWithManager[__instance.gameObject.scene];
            LoadedSceneInfo lsi = scenes.FirstOrDefault(i => i.id == id);
            scenesWithManager.Remove(__instance.gameObject.scene);

            if (lsi != null)
                lsi.activeSceneWhenLoaded = GameManager.instance.sceneName;
        }

        [HarmonyPatch]
        public class LoadedSceneInfo
        {
            private static int counter = 0;
            private static LoadedSceneInfo activeInfo;
            
            public readonly string name;
            public string activeSceneWhenLoaded { get; internal set; }
            public readonly int id;

            public LoadedSceneInfo(string name, string activeSceneName)
            {
                this.name = name;
                this.id = counter++;
                this.activeSceneWhenLoaded = activeSceneName;
            }

            public void LoadHook()
            {
                activeInfo = this;
                GameManager.instance.OnFinishedSceneTransition += FinishedSceneTransitionHook;
            }

            private void FinishedSceneTransitionHook()
            {
                GameManager.instance.OnFinishedSceneTransition -= FinishedSceneTransitionHook;
                activeInfo = null;
            }

            [HarmonyPatch(typeof(GameManager), nameof(GameManager.UpdateSceneName))]
            [HarmonyPrefix]
            public static bool UpdateSceneNameOverride(GameManager __instance)
            {
                if (activeInfo != null)
                {
                    if (activeInfo.activeSceneWhenLoaded != __instance.sceneName)
                    {
                        __instance.lastSceneName = __instance.sceneName;
                    }
                    __instance.sceneName = activeInfo.activeSceneWhenLoaded;
                    __instance.rawSceneName = activeInfo.activeSceneWhenLoaded;
                    __instance.sceneNameHash = activeInfo.activeSceneWhenLoaded.GetHashCode();

                    return false;
                }

                return true;
            }
        }
    }
}