using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TeamCherry.Localization;

namespace DebugMod.Helpers;

internal static class Localization
{
    private static readonly List<string> sheets = [$"Mods.{DebugMod.Id}"];
    private static Dictionary<string, string> fallbackSheet;
    private static bool warnedEntryMissing;

    internal static Dictionary<string, string> FallbackSheet
    {
        get
        {
            if (fallbackSheet == null)
            {
                LoadFallbackSheet();
            }

            return fallbackSheet;
        }
    }

    private static void LoadFallbackSheet()
    {
        try
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "languages", "en.json");
            Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(path));

            fallbackSheet = [];
            foreach (KeyValuePair<string, object> pair in dictionary)
            {
                fallbackSheet.Add(pair.Key, (string)pair.Value);
            }
        }
        catch (Exception e)
        {
            DebugMod.LogError($"Could not load fallback sheet: {e}");
            fallbackSheet = [];
        }
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

        if (!warnedEntryMissing)
        {
            DebugMod.LogWarn("Entry not found in language sheet (is Silksong.I18N installed?)");
            warnedEntryMissing = true;
        }

        if (FallbackSheet.TryGetValue(key, out string value))
        {
            return value;
        }

        DebugMod.LogError($"'{key}' is not a valid key in the language sheet.");
        return key;
    }

    internal static void AddSheet(string sheet)
    {
        sheets.Add(sheet);
    }
}