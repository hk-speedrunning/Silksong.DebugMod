using UnityEngine;

namespace DebugMod.MethodHelpers;

public static class HudHelper
{
    public static void RefreshMasks()
    {
        Transform hudCanvas = GameCameras.instance.hudCanvasSlideOut.transform;

        // The health object might be inactive if we are loading from a save state
        GameObject health = null;
        for (int i = 0; i < hudCanvas.childCount; i++)
        {
            GameObject child = hudCanvas.GetChild(i).gameObject;
            if (child.name == "Health")
            {
                health = child;
                break;
            }
        }

        foreach (PlayMakerFSM fsm in health.GetComponentsInChildren<PlayMakerFSM>())
        {
            if (fsm.FsmName == "health_display")
            {
                if (fsm.gameObject.activeSelf)
                {
                    fsm.OnEnable();
                }
                else
                {
                    fsm.gameObject.SetActive(true);
                }

                fsm.FsmVariables.FindFsmBool("Initialised").Value = true;
                fsm.SetState("Check Max HP");
            }
        }
    }
}