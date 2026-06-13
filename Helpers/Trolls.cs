using DebugMod.UI;
using System;
using System.Linq;
using UnityEngine;

namespace DebugMod.Helpers;

internal static class Trolls
{
    public static void OnResourcesLoaded()
    {
        DateTime time = DateTime.Now;

        if (time.Month == 4 && time.Day == 1)
        {
            string fontName = Font.GetOSInstalledFontNames().FirstOrDefault(x => x.Contains("Comic Sans"));

            if (fontName != null)
            {
                UICommon.arial = Font.CreateDynamicFontFromOSFont(fontName, UICommon.FontSize);
            }
        }
    }
}

