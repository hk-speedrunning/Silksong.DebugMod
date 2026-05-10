using DebugMod.Helpers;
using DebugMod.MonoBehaviours;
using HarmonyLib;
using System;
using UnityEngine;

namespace DebugMod;

[HarmonyPatch]
public static partial class BindableFunctions
{
    const float defaultFOV = 24f;
    const float defaultZoom = 1f;

    [BindableMethod(name = "GAMEPLAY_VISUAL_TOGGLEHITBOXES", category = "CATEGORY_VISUAL")]
    public static void ShowHitboxes()
    {
        if (++DebugMod.settings.ShowHitBoxes > 2) DebugMod.settings.ShowHitBoxes = 0;

        switch (DebugMod.settings.ShowHitBoxes)
        {
            case 0:
                DebugMod.LogConsole("Not showing hitboxes");
                break;
            case 1:
                DebugMod.LogConsole("Showing hitboxes");
                break;
            case 2:
                DebugMod.LogConsole("Showing all hitboxes");
                break;
        }
    }

    [BindableMethod(name = "GAMEPLAY_VISUAL_FORCECAMERAFOLLOW", category = "CATEGORY_VISUAL")]
    public static void ForceCameraFollow()
    {
        if (!DebugMod.cameraFollow)
        {
            DebugMod.LogConsole("Forcing camera follow");
            DebugMod.cameraFollow = true;
        }
        else
        {
            DebugMod.cameraFollow = false;
            DebugMod.RefCamera.isGameplayScene = true;
            DebugMod.LogConsole("Returning camera to normal settings");
        }
    }

    [BindableMethod(name = "GAMEPLAY_VISUAL_PREVIEWCOCOONPOSITION", category = "CATEGORY_VISUAL")]
    public static void PreviewCocoonPosition()
    {
        CocoonPreviewer component = GameManager.instance.GetComponent<CocoonPreviewer>()
            ?? GameManager.instance.gameObject.AddComponent<CocoonPreviewer>();

        if (!component.previewEnabled)
        {
            component.previewEnabled = true;
            DebugMod.LogConsole("Enabled cocoon spawn point preview");
        }
        else
        {
            component.previewEnabled = false;
            DebugMod.LogConsole("Disabled cocoon spawn point preview");
        }
    }

    [BindableMethod(name = "GAMEPLAY_VISUAL_TOGGLEVIGNETTE", category = "CATEGORY_VISUAL")]
    public static void ToggleVignette()
    {
        VisualMaskHelper.ToggleVignette();
    }

    [BindableMethod(name = "GAMEPLAY_VISUAL_DEACTIVATEVISUALMASKS", category = "CATEGORY_VISUAL")]
    public static void DoDeactivateVisualMasks()
    {
        VisualMaskHelper.ToggleAllMasks();
    }

    [BindableMethod(name = "GAMEPLAY_VISUAL_TOGGLEHEROLIGHT", category = "CATEGORY_VISUAL")]
    public static void ToggleHeroLight()
    {
        GameObject gameObject = DebugMod.RefKnight.transform.Find("HeroLight").gameObject;
        Color color = gameObject.GetComponent<SpriteRenderer>().color;
        if (Math.Abs(color.a) > 0f)
        {
            color.a = 0f;
            gameObject.GetComponent<SpriteRenderer>().color = color;
            DebugMod.LogConsole("Making hero light invisible");
        }
        else
        {
            color.a = 0.7f;
            gameObject.GetComponent<SpriteRenderer>().color = color;
            DebugMod.LogConsole("Making hero light visible");
        }
    }

    [BindableMethod(name = "GAMEPLAY_VISUAL_TOGGLEHUD", category = "CATEGORY_VISUAL")]
    public static void ToggleHUD()
    {
        if (GameCameras.instance.hudCanvasSlideOut.gameObject.activeInHierarchy)
        {
            GameCameras.instance.hudCanvasSlideOut.gameObject.SetActive(false);
            DebugMod.LogConsole("Disabling HUD");
        }
        else
        {
            GameCameras.instance.hudCanvasSlideOut.gameObject.SetActive(true);
            HudHelper.RefreshSpool();
            DebugMod.LogConsole("Enabling HUD");
        }
    }

    // Don't try to spawn new silk chunks if the HUD is disabled
    [HarmonyPatch(typeof(SilkSpool), nameof(SilkSpool.ChangeSilk))]
    [HarmonyPrefix]
    private static bool SilkSpool_ChangeSilk()
    {
        return GameCameras.instance.hudCanvasSlideOut.gameObject.activeInHierarchy;
    }

    [BindableMethod(name = "ACTION_ZOOMIN", category = "CATEGORY_VISUAL")]
    public static void ZoomIn()
    {
        GameCameras.instance.tk2dCam.zoomFactor *= 1.1f;
        float currentFOV = CalculateNewFOV(defaultFOV, GameCameras.instance.tk2dCam.ZoomFactor);
        DebugMod.LBB.OnCameraFovChanged(currentFOV);
        DebugMod.LogConsole($"Zoom level increased to {GameCameras.instance.tk2dCam.ZoomFactor}");
    }

    [BindableMethod(name = "ACTION_ZOOMOUT", category = "CATEGORY_VISUAL")]
    public static void ZoomOut()
    {
        GameCameras.instance.tk2dCam.zoomFactor *= 0.9f;
        float currentFOV = CalculateNewFOV(defaultFOV, GameCameras.instance.tk2dCam.ZoomFactor);
        DebugMod.LBB.OnCameraFovChanged(currentFOV);
        DebugMod.LogConsole($"Zoom level increased to {GameCameras.instance.tk2dCam.ZoomFactor}");
    }

    [BindableMethod(name = "ACTION_RESETZOOM", category = "CATEGORY_VISUAL")]
    public static void ResetZoom()
    {
        GameCameras.instance.tk2dCam.ZoomFactor = defaultZoom;
        DebugMod.LBB.OnCameraFovChanged(defaultFOV);
        DebugMod.LogConsole("Zoom level was reset");
    }

    static float CalculateNewFOV(float baseFOV, float zoomFactor)
    {
        return 2 * Mathf.Atan(Mathf.Tan(baseFOV / 2 * Mathf.Deg2Rad) / zoomFactor) * Mathf.Rad2Deg;
    }

    [BindableMethod(name = "GAMEPLAY_VISUAL_HIDEHERO", category = "CATEGORY_VISUAL")]
    public static void HideHero()
    {
        tk2dSprite component = DebugMod.RefKnight.GetComponent<tk2dSprite>();
        Color color = component.color;
        if (Math.Abs(color.a) > 0f)
        {
            color.a = 0f;
            component.color = color;
            DebugMod.LogConsole("Making hero sprite invisible");
        }
        else
        {
            color.a = 1f;
            component.color = color;
            DebugMod.LogConsole("Making hero sprite visible");
        }
    }

    [BindableMethod(name = "GAMEPLAY_VISUAL_TOGGLECAMERASHAKE", category = "CATEGORY_VISUAL")]
    public static void ToggleCameraShake()
    {
        bool newValue = !GameCameras.instance.cameraShakeFSM.enabled;
        GameCameras.instance.cameraShakeFSM.enabled = newValue;
        DebugMod.LogConsole($"{(newValue ? "Enabling" : "Disabling")} camera shake");
    }
}