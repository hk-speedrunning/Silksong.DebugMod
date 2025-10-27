using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DebugMod.UI.Canvas;
using UnityEngine;

namespace DebugMod.UI;

public static class UICommon
{
    public const int RIGHT_SIDE_WIDTH = 400;
    public const int MAIN_MENU_HEIGHT = 650;
    public const int SCREEN_MARGIN = 25;
    public const int MARGIN = 6;
    public const int CONTROL_HEIGHT = 25;

    public static readonly Color textColor = Color.white;
    public static readonly Color accentColor = RGB(137, 180, 250);

    public static Font trajanBold;
    public static Font trajanNormal;
    public static Font arial;
    public static readonly Dictionary<string, Texture2D> images = new();
    public static readonly Texture2D buttonBG = SolidColor(RGB(54, 58, 79));
    public static readonly Texture2D panelBG = SolidColor(RGB(36, 39, 58));

    public static CanvasButton AddStyledButton(this CanvasPanel panel, string name)
    {
        CanvasButton button = panel.AddButton(name);
        button.UpdateImage(buttonBG);

        CanvasText t = button.AddText();
        t.Font = arial;
        t.Alignment = TextAnchor.MiddleCenter;

        return button;
    }

    public static CanvasPanel AddStyledPanel(this CanvasPanel panel, string name, Vector2 size)
    {
        CanvasPanel p = panel.AddPanel(name);
        p.Size = size;

        CanvasImage background = p.AddImage("Background");
        background.Size = size;
        background.UpdateImage(panelBG);

        return p;
    }

    private static Color RGB(int r, int g, int b) => new Color(r / 255f, g / 255f, b / 255f);

    private static Texture2D SolidColor(Color color)
    {
        Texture2D tex = new(1, 1);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return tex;
    }

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