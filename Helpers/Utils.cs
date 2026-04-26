using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TeamCherry.Localization;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DebugMod.Helpers;

internal static class Utils
{
    internal static readonly string defaultTranslationSheet = $"Mods.{DebugMod.Id}";
    internal static string translationSheet = defaultTranslationSheet;
    internal static Dictionary<string, string> fallbackSheet;

    internal static PlayMakerFSM FindFSM(string goName, string fsmName)
    {
        return PlayMakerFSM.FindFsmOnGameObject(GameObject.Find(goName), fsmName);
    }

    internal static GameObject FindChildObject(this GameObject parent, string name)
    {
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            GameObject child = parent.transform.GetChild(i).gameObject;
            if (child.name == name)
            {
                return child;
            }
        }

        return null;
    }

    internal static GameObject FindGameObjectByPath(string path)
    {
        string[] parts = path.Split('/');

        GameObject go = SceneManager.GetActiveScene().GetRootGameObjects().First(x => x.name == parts[0]);

        for (int i = 1; i < parts.Length; i++)
        {
            go = go.FindChildObject(parts[i]);
        }

        return go;
    }

    internal static string LanguageSheetFallback(string key)
    {
        if (fallbackSheet == null)
        {
            try
            {
                DebugMod.LogWarn("I18N seems to not be installed, manually loading English translations...");

                string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "languages", "en.json");
                Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(path));

                fallbackSheet = [];
                foreach (KeyValuePair<string, object> pair in dictionary)
                {
                    fallbackSheet.Add(pair.Key, (string)pair.Value);
                }

                DebugMod.Log($"Loaded English translations with {fallbackSheet.Count} keys");
            }
            catch (Exception e)
            {
                DebugMod.LogError($"Could not manually load translations: {e}");
                fallbackSheet = [];
            }
        }

        return fallbackSheet.GetValueOrDefault(key, key);
    }

    internal static string Localize(string key)
    {
        string result = Language.Get(key, translationSheet);

        if (result == "" || result.StartsWith("#!#"))
        {
            return LanguageSheetFallback(key);
        }

        return result;
    }
}
