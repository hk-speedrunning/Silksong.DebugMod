using HutongGames.PlayMaker;
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
    internal static string fallbackLanguage;
    internal static Dictionary<string, string> englishKeyLookup;

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
        string currentLanguage = CurrentDebugModLanguage();
        if (fallbackSheet == null || fallbackLanguage != currentLanguage)
        {
            fallbackLanguage = currentLanguage;
            fallbackSheet = [];

            foreach (string languageFile in FallbackLanguageFiles(currentLanguage))
            {
                LoadFallbackFile(languageFile, fallbackSheet);
            }
        }

        if (fallbackSheet.TryGetValue(key, out string value))
        {
            return value;
        }

        DebugMod.LogError($"'{key}' is not a valid key in the language sheet.");
        return key;
    }

    internal static void ClearLanguageCache()
    {
        fallbackSheet = null;
        fallbackLanguage = null;
    }

    internal static string CurrentDebugModLanguage()
    {
        string language = NormalizeDebugModLanguage(DebugMod.settings?.DebugModLanguage);
        return language == "auto" ? GameLanguageFile(Language.CurrentLanguage()) : language;
    }

    internal static string CurrentDebugModLanguageNameKey()
    {
        string language = NormalizeDebugModLanguage(DebugMod.settings?.DebugModLanguage);
        if (language == "auto")
        {
            return "LANGUAGE_AUTO";
        }

        return CurrentDebugModLanguage() == "zh" ? "LANGUAGE_ZH" : "LANGUAGE_EN";
    }

    internal static string NextDebugModLanguage(string language)
    {
        return NormalizeDebugModLanguage(language) switch
        {
            "en" => "zh",
            "zh" => "auto",
            _ => "en"
        };
    }

    internal static string NormalizeDebugModLanguage(string language)
    {
        language = (language ?? "en").Trim().ToLowerInvariant().Replace('_', '-');
        return language switch
        {
            "" => "en",
            "english" => "en",
            "zh-cn" => "zh",
            "zh-hans" => "zh",
            "simplified-chinese" => "zh",
            "chinese" => "zh",
            "follow-game" => "auto",
            "game" => "auto",
            _ => language
        };
    }

    private static string GameLanguageFile(LanguageCode language)
    {
        return language.ToString().ToLowerInvariant().Replace('_', '-');
    }

    private static IEnumerable<string> FallbackLanguageFiles(string languageFile)
    {
        languageFile = NormalizeDebugModLanguage(languageFile);

        yield return "en";

        if (languageFile == "en")
        {
            yield break;
        }

        yield return languageFile;

        int separator = languageFile.IndexOf('-');
        string baseLanguage = separator >= 0 ? languageFile[..separator] : languageFile;
        if (baseLanguage != languageFile && baseLanguage != "en")
        {
            yield return baseLanguage;
        }
    }

    private static void LoadFallbackFile(string languageFile, Dictionary<string, string> target)
    {
        try
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "languages", $"{languageFile}.json");
            if (!File.Exists(path))
            {
                DebugMod.LogWarn($"Bundled translation file not found: {path}");
                return;
            }

            Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path));
            if (dictionary == null)
            {
                return;
            }

            foreach (KeyValuePair<string, string> pair in dictionary)
            {
                target[pair.Key] = pair.Value;
            }

            DebugMod.Log($"Loaded {languageFile} translations with {dictionary.Count} keys");
        }
        catch (Exception e)
        {
            DebugMod.LogError($"Could not manually load {languageFile} translations: {e}");
        }
    }

    internal static string Localize(string key)
    {
        if (translationSheet == defaultTranslationSheet)
        {
            return LanguageSheetFallback(key);
        }

        string result = Language.Get(key, translationSheet);

        if (result == "" || result == key || result.StartsWith("#!#"))
        {
            result = LanguageSheetFallback(key);
        }

        return result;
    }

    internal static string LocalizeAction(string actionName)
    {
        englishKeyLookup ??= LoadEnglishKeyLookup();
        return englishKeyLookup.TryGetValue(actionName, out string key) ? Localize(key) : actionName;
    }

    private static Dictionary<string, string> LoadEnglishKeyLookup()
    {
        Dictionary<string, string> lookup = [];

        try
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "languages", "en.json");
            Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path));
            if (dictionary == null)
            {
                return lookup;
            }

            foreach (KeyValuePair<string, string> pair in dictionary)
            {
                lookup.TryAdd(pair.Value, pair.Key);
            }
        }
        catch (Exception e)
        {
            DebugMod.LogError($"Could not load action localization lookup: {e}");
        }

        return lookup;
    }
#nullable enable


    internal static FsmState? GetState(this PlayMakerFSM fsm, string name)
    {
        return fsm.FsmStates.FirstOrDefault(state => state.Name == name);
    }

    internal static PlayMakerFSM? GetTemplatedFsm(this GameObject go, string name)
    {
        return go.GetComponents<PlayMakerFSM>()?.FirstOrDefault(fsm => fsm.FsmTemplate && fsm.FsmTemplate.name == name);
    }

    internal static PlayMakerFSM? GetNamedFsm(this GameObject go, string name)
    {
        return go.GetComponents<PlayMakerFSM>()?.FirstOrDefault(fsm => fsm.FsmTemplate && fsm.FsmTemplate.name == name);
    }

    internal static T? GetFirstActionOrDefault<T>(this FsmState state) where T : FsmStateAction
    {
        return state.Actions.FirstOrDefault(act => act is T) as T;
    }
}
