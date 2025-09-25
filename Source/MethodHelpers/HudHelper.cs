using UnityEngine;

namespace DebugMod.MethodHelpers;

public static class HudHelper
{
    public static void RefreshMasks()
    {
        GameObject health = GameObject.Find("Health");

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