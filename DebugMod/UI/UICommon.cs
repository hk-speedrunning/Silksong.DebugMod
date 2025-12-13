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
    // Values that should not scale with the resolution
    public const int BORDER_THICKNESS = 1;

    // Values that scale with either width or height, unscaled value is for 1080p
    public static int RightSideWidth => ScaleWidth(450);
    public static int LeftSideWidth => ScaleWidth(450);
    public static int MainPanelHeight => ScaleHeight(650);
    public static int ConsoleHeight => ScaleHeight(250);
    public static int InfoPanelHeight => ScaleHeight(400);
    public static int SaveStatePanelWidth => ScaleWidth(500);
    public static int SaveStatePanelHeight => ScaleHeight(400);
    public static int Margin => ScaleHeight(6);
    public static int ScreenMargin => ScaleHeight(10);
    public static int ControlHeight => ScaleHeight(25);
    public static int FontSize => ScaleHeight(13);

    // Catppuccin Mocha
    public static readonly Color baseColor = RGB(36, 39, 58);
    public static readonly Color strongColor = RGB(54, 58, 79);
    public static readonly Color borderColor = RGB(205, 214, 244);
    public static readonly Color blueColor = RGB(137, 180, 250);

    public static readonly Color textColor = Color.white;

    public static readonly Texture2D panelBG = SolidColor(baseColor, 100);
    public static readonly Texture2D buttonBG = SolidColor(strongColor);
    public static readonly Texture2D contextPanelBG = SolidColor(baseColor);

    public static Font trajanBold;
    public static Font trajanNormal;
    public static Font arial;

    public static readonly Dictionary<string, Texture2D> images = new();

    public static int ScaleWidth(int unscaled) => (int)(unscaled * Screen.width / 1920f);
    public static int ScaleHeight(int unscaled) => (int)(unscaled * Screen.height / 1080f);

    private static Color RGB(int r, int g, int b) => new(r / 255f, g / 255f, b / 255f);
    private static Color RGBA(int r, int g, int b, int a) => new(r / 255f, g / 255f, b / 255f, a / 255f);
    private static Color WithAlpha(Color color, int a) => color with { a = a / 255f };

    private static Texture2D SolidColor(Color color, int a = 255)
    {
        color = WithAlpha(color, a);
        Texture2D tex = new(1, 1);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return tex;
    }

    public static CanvasImage AddBackground(CanvasPanel panel)
    {
        CanvasImage background = panel.Add(new CanvasImage("Background"));
        background.IsBackground = true;
        background.SetImage(panelBG);
        background.AddBorder();
        return background;
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

        arial?.RequestCharactersInTexture("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890`~!@#$%^&*()-_=+[{]}\\|;:'\",<.>/? ", 13);

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