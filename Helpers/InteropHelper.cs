using BepInEx;
using BepInEx.Bootstrap;
using System;

namespace DebugMod.Helpers;

internal static class InteropHelper
{
    internal static string BenchwarpModId = "io.github.homothetyhk.benchwarp";
    internal static string I18NModId = "org.silksong-modding.i18n";

    internal static T GetPlugin<T>(string id)
        where T : BaseUnityPlugin
    {
        return (T)Chainloader.PluginInfos[id].Instance;
    }

    internal static bool IsModInstalled(string id)
    {
        return Chainloader.PluginInfos.ContainsKey(id);
    }

    internal static bool IsModInstalled(string id, Version minVersion)
    {
        if (Chainloader.PluginInfos.TryGetValue(id, out PluginInfo info))
        {
            return info.Metadata.Version >= minVersion;
        }

        return false;
    }

    internal static bool IsModInstalled(string id, string minVersion)
    {
        return IsModInstalled(id, new Version(minVersion));
    }
}