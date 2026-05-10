using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TeamCherry.Localization;

namespace DebugMod.Helpers;

internal static class Localization
{
    internal static readonly List<string> sheets = [$"Mods.{DebugMod.Id}"];
    internal static Dictionary<string, string> fallbackSheet;

    internal static string LanguageSheetFallback(string key)
    {
        if (fallbackSheet == null)
        {
            try
            {
                DebugMod.LogWarn("Entry not found in language sheet, manually loading English translations...");
                DebugMod.LogWarn("(This can happen if Silksong.I18N is not installed.)");

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

        if (fallbackSheet.TryGetValue(key, out string value))
        {
            return value;
        }

        DebugMod.LogError($"'{key}' is not a valid key in the language sheet.");
        return key;
    }

    internal static string Get(string key)
    {
        foreach (string sheet in sheets)
        {
            string result = Language.Get(key, sheet);
            if (result != "" && !result.StartsWith("#!#"))
            {
                return result;
            }
        }

        return LanguageSheetFallback(key);
    }

    internal static void AddSheet(string sheet)
    {
        sheets.Insert(0, sheet);
    }
}