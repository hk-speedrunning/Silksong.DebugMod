using DebugMod.Helpers;
using Silksong.I18N;
using TeamCherry.Localization;

namespace DebugMod.Interop;

// Safely reference I18N without it needing to be installed
// (Mono will only load the class when one of its methods is called)
internal static class I18NInterop
{
    private static readonly I18NPlugin i18n = InteropHelper.GetPlugin<I18NPlugin>(InteropHelper.I18NModId);

    internal static LanguageCode GetLanguage()
    {
        return i18n.LanguageOverride ?? Language.CurrentLanguage();
    }
}