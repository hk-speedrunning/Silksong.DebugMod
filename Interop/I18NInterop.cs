using BepInEx;
using DebugMod.Helpers;
using System;
using TeamCherry.Localization;

namespace DebugMod.Interop;

#nullable enable

// Safely reference I18N without it needing to be installed
// (Mono will only load the class when one of its methods is called)
internal static class I18NInterop
{
    private static readonly Func<LanguageCode?>? getLanguageOverride;

    static I18NInterop()
    {
        if (!InteropHelper.IsModInstalled(InteropHelper.I18NModId, "1.1.0")) return;

        BaseUnityPlugin i18NPlugin = InteropHelper.GetPlugin(InteropHelper.I18NModId)!;
        getLanguageOverride = () => (LanguageCode?)i18NPlugin.GetType().GetProperty("LanguageOverride")?.GetValue(i18NPlugin);
    }

    internal static LanguageCode GetLanguage()
    {
        if (getLanguageOverride == null) return Language.CurrentLanguage();
        return getLanguageOverride() ?? Language.CurrentLanguage();
    }
}