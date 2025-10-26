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
    public static Font trajanBold;
    public static Font trajanNormal;
    public static Font arial;
    public static readonly Dictionary<string, Texture2D> images = new();
    public static readonly Texture2D buttonBG = SolidColor(54, 58, 79);
    public static readonly Texture2D panelBG = SolidColor(36, 39, 58);

    public static CanvasButton AddStyledButton(this CanvasPanel panel, string name, Vector2 position, Vector2 size, string text, Action clicked)
    {
        CanvasButton button = panel.AddButton(name);
        button.LocalPosition = position;
        button.Size = size;
        button.UpdateImage(buttonBG);
        button.OnClicked += clicked;

        CanvasText t = button.AddText();
        t.Text = text;
        t.Font = arial;
        t.Alignment = TextAnchor.MiddleCenter;

        return button;
    }

    public static CanvasPanel AddStyledPanel(this CanvasPanel panel, string name, Vector2 position, Vector2 size)
    {
        CanvasPanel p = panel.AddPanel(name);
        p.LocalPosition = position;
        p.Size = size;

        CanvasImage background = p.AddImage("Background");
        background.Size = size;
        background.UpdateImage(panelBG);

        return p;
    }

    private static Texture2D SolidColor(int r, int g, int b)
    {
        Texture2D tex = new(1, 1);
        tex.SetPixel(0, 0, new Color(r / 255f, g / 255f, b / 255f));
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