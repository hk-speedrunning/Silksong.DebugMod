using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DebugMod.UI;

public static class UICommon
{
    public static Font trajanBold;
    public static Font trajanNormal;
    public static Font arial;
    public static readonly Dictionary<string, Texture2D> images = new();

    public static void LoadResources()
    {
        foreach (Font f in Resources.FindObjectsOfTypeAll<Font>())
        {
            if (f != null && f.name == "TrajanPro-Bold")
            {
                trajanBold = f;
            }

            if (f != null && f.name == "TrajanPro-Regular")
            {
                trajanNormal = f;
            }

            //Just in case for some reason the computer doesn't have arial
            if (f != null && f.name == "Perpetua")
            {
                arial = f;
            }
        }

        foreach (string font in Font.GetOSInstalledFontNames())
        {
            if (font.ToLower().Contains("arial"))
            {
                arial = Font.CreateDynamicFontFromOSFont(font, 13);
                break;
            }
        }

        if (trajanBold == null || trajanNormal == null || arial == null) DebugMod.LogError("Could not find game fonts");

        string[] resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();

        foreach (string res in resourceNames)
        {
            if (res.StartsWith("DebugMod.Images."))
            {
                try
                {
                    Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(res);
                    byte[] buffer = new byte[imageStream.Length];
                    imageStream.Read(buffer, 0, buffer.Length);

                    Texture2D tex = new Texture2D(1, 1);
                    tex.LoadImage(buffer.ToArray());

                    string[] split = res.Split('.');
                    string internalName = split[split.Length - 2];
                    images.Add(internalName, tex);

                    DebugMod.LogDebug($"Loaded image: {internalName}");
                }
                catch (Exception e)
                {
                    DebugMod.LogError($"Failed to load image: {res}\n{e}");
                }
            }
        }
    }
}