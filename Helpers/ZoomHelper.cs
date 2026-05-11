using UnityEngine;

namespace DebugMod.Helpers;

public static class ZoomHelper
{
    public const float DEFAULT_FOV = 24f;
    public const float DEFAULT_ZOOM = 1f;

    public static void UpdateCameraFOV()
    {
        float zoomFactor = GameCameras.instance.tk2dCam.ZoomFactor;
        float newFOV = 2 * Mathf.Atan(Mathf.Tan(DEFAULT_FOV / 2 * Mathf.Deg2Rad) / zoomFactor) * Mathf.Rad2Deg;
        DebugMod.LBB.OnCameraFovChanged(newFOV);
    }
}